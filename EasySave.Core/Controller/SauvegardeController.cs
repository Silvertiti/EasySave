using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using EasySave.Core.Models;
using EasySave.Core.Services;

namespace EasySave.Core.Controller
{
    public class SauvegardeController
    {
        public List<ModelJob> myJobs;
        SettingsManager settingsManager = new SettingsManager();
        BusinessSoftwareService businessSoftwareService = new BusinessSoftwareService();
        JobManager jobManager = new JobManager();
        private string stateFile = "state.json";

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
                foreach (string f in files) totalSize += new FileInfo(f).Length;

                int filesLeft = totalFiles;
                long sizeLeft = totalSize;
                UpdateEtat(job.Name, job.Source, job.Target, "ACTIF", totalFiles, totalSize, filesLeft, sizeLeft);
                List<string> extensionsToEncrypt = settings.ExtensionsToEncrypt.Split(',').Select(e => e.Trim().ToLower()).ToList();

                foreach (string file in files)
                {
                    if (businessSoftwareService.IsBusinessSoftRunning())
                    {
                        uiCallback("INTERRUPTION : Logiciel métier détecté.");
                        LogManager.SaveLog(job.Name, "STOP_METIER", "STOP_METIER", 0, 0, 0);
                        break;
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

                    try
                    {
                        File.Copy(file, dest, true);
                        string ext = Path.GetExtension(dest).ToLower().Replace(".", "");
                        if (extensionsToEncrypt.Contains(ext))
                        {
                            encryptionTime = ExecuteCryptoSoft(dest);
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