using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using EasySave.Core.Models;
using EasySave.Core.Services;
using System.Threading;

namespace EasySave.Core.Controller
{
    public class SauvegardeController
    {
        private static readonly object _largeFileLock = new object();
        public List<ModelJob> myJobs;
        SettingsManager settingsManager = new SettingsManager();
        BusinessSoftwareService businessSoftwareService = new BusinessSoftwareService();
        JobManager jobManager = new JobManager();
        private string stateFile = "state.json";
        public bool IsPausedRequested { get; set; } = false;

        public SauvegardeController() { myJobs = jobManager.LoadData(); }
        public void AddJob(ModelJob newJob) { myJobs.Add(newJob); jobManager.SaveData(myJobs); }

        public void DeleteJob(int index)
        {
            if (index >= 0 && index < myJobs.Count) { myJobs.RemoveAt(index); jobManager.SaveData(myJobs); }
        }

        public void DeleteAllJobs() { myJobs.Clear(); jobManager.SaveData(myJobs); }

        private double ExecuteCryptoSoft(string sourceFilePath)
        {
            try
            {
                var settings = settingsManager.GetSettings();
                if (string.IsNullOrEmpty(settings.CryptoSoftPath) || !File.Exists(settings.CryptoSoftPath))
                    return -1;

                Stopwatch sw = Stopwatch.StartNew();

                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = settings.CryptoSoftPath;
                startInfo.Arguments = $"\"{sourceFilePath}\" \"EasyKey\"";
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;

                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }

                sw.Stop();
                return sw.Elapsed.TotalMilliseconds;
            }
            catch
            {
                return -1;
            }
        }

        public void ExecuterSauvegarde(Action<string> uiCallback)
        {
            foreach (var job in myJobs) ExecuterUnSeulJob(job, uiCallback);
        }

        public void ExecuterUnSeulJob(ModelJob job, Action<string> uiCallback)
        {
            var settings = settingsManager.GetSettings();
            if (businessSoftwareService.IsBusinessSoftRunning())
            {
                uiCallback($"STOP : Logiciel métier détecté ({settings.BusinessSoftware}).");
                return;
            }

            try
            {
                uiCallback("Lancement : " + job.Name);
                if (!Directory.Exists(job.Source)) { uiCallback("ERREUR : Source introuvable."); return; }

                string[] files = Directory.GetFiles(job.Source, "*.*", SearchOption.AllDirectories);
                int totalFiles = files.Length;
                
                long totalSize = 0;
                int filesLeft = totalFiles;
                long sizeLeft = 0;
                string resumeFile = null;

                if (File.Exists(stateFile))
                {
                    try {
                        var etat = JsonConvert.DeserializeObject<ModelEtat>(File.ReadAllText(stateFile));
                        if (etat != null && etat.Name == job.Name && etat.State == "PAUSE")
                        {
                            resumeFile = etat.SourceFile;
                            filesLeft = etat.FilesLeft;
                            totalSize = etat.TotalSize;
                            sizeLeft = etat.SizeLeft;
                        }
                    } catch { }
                }

                bool skip = !string.IsNullOrEmpty(resumeFile);

                if (!skip) 
                {
                    foreach (string f in files) totalSize += new FileInfo(f).Length;
                    sizeLeft = totalSize;
                    UpdateEtat(job.Name, job.Source, job.Target, "ACTIF", totalFiles, totalSize, filesLeft, sizeLeft);
                }

                List<string> extensionsToEncrypt = settings.ExtensionsToEncrypt.Split(',').Select(e => e.Trim().ToLower()).ToList();

                foreach (string file in files)
                {
                    if (IsPausedRequested)
                    {
                        UpdateEtat(job.Name, file, "", "PAUSE", totalFiles, totalSize, filesLeft, sizeLeft);
                        uiCallback("PAUSE : " + job.Name);
                        return; // Arrêt complet, la vue pourra reprendre plus tard
                    }

                    if (businessSoftwareService.IsBusinessSoftRunning())
                    {
                        uiCallback("INTERRUPTION : Logiciel métier détecté.");
                        LogManager.SaveLog(job.Name, "STOP_METIER", "STOP_METIER", 0, 0, 0);
                        break;
                    }

                    if (skip)
                    {
                        if (file == resumeFile) skip = false;
                        if (skip) continue;
                    }

                    string relatif = file.Replace(job.Source, "").TrimStart('\\');
                    string dest = Path.Combine(job.Target, relatif);
                    long fileSize = new FileInfo(file).Length;
                    if (!job.IsFull && File.Exists(dest))
                    {
                        if (File.GetLastWriteTime(file) <= File.GetLastWriteTime(dest))
                        {
                            filesLeft--; sizeLeft -= fileSize;
                            UpdateEtat(job.Name, file, dest, "ACTIF", totalFiles, totalSize, filesLeft, sizeLeft);
                            continue;
                        }
                    }

                    string? dirDest = Path.GetDirectoryName(dest);
                    if (!string.IsNullOrEmpty(dirDest) && !Directory.Exists(dirDest)) Directory.CreateDirectory(dirDest);

                    UpdateEtat(job.Name, file, dest, "ACTIF", totalFiles, totalSize, filesLeft, sizeLeft);

                    Stopwatch sw = Stopwatch.StartNew();
                    double encryptionTime = 0;

                    bool isLargeFile = (settings.MaxParallelFileSizeKb > 0) && (fileSize > settings.MaxParallelFileSizeKb * 1024);

                    try
                    {
                        Action processFile = () => 
                        {
                            File.Copy(file, dest, true);
                            string ext = Path.GetExtension(dest).ToLower().Replace(".", "");
                            if (extensionsToEncrypt.Contains(ext))
                            {
                                encryptionTime = ExecuteCryptoSoft(dest);
                            }
                        };

                        if (isLargeFile)
                        {
                            lock (_largeFileLock)
                            {
                                processFile();
                            }
                        }
                        else
                        {
                            processFile();
                        }

                        sw.Stop();
                        LogManager.SaveLog(job.Name, file, dest, fileSize, sw.Elapsed.TotalMilliseconds, encryptionTime);
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        LogManager.SaveLog(job.Name, file, dest, fileSize, -1, -1);
                        uiCallback("Erreur : " + ex.Message);
                    }

                    filesLeft--; sizeLeft -= fileSize;
                    uiCallback("Copié : " + relatif);
                }

                UpdateEtat(job.Name, "", "", "INACTIF", totalFiles, totalSize, 0, 0);
                uiCallback("Succès : " + job.Name);
            }
            catch (Exception ex) { uiCallback("ERREUR CRITIQUE : " + ex.Message); }
        }

        private void UpdateEtat(string jobName, string src, string dest, string state, int totalF, long totalS, int leftF, long leftS)
        {
            try
            {
                int prog = (totalF > 0) ? 100 - (leftF * 100 / totalF) : 100;
                ModelEtat etat = new ModelEtat()
                {
                    Name = jobName,
                    SourceFile = src,
                    TargetFile = dest,
                    State = state,
                    TotalFiles = totalF,
                    TotalSize = totalS,
                    FilesLeft = leftF,
                    SizeLeft = leftS,
                    Progression = prog,
                    Timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                };
                File.WriteAllText(stateFile, JsonConvert.SerializeObject(etat, Formatting.Indented));
            }
            catch { }
        }
    }
}