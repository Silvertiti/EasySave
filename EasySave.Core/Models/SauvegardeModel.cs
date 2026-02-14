using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using EasyLog.Core.Logs;

namespace EasySave.Core.Models
{
    public class SauvegardeModel
    {
        public List<ModelJob> myJobs = new List<ModelJob>();
        private string configFile = "jobs.json";
        private string stateFile = "state.json";

        public void LoadData()
        {
            if (File.Exists(configFile))
            {
                string json = File.ReadAllText(configFile);
                myJobs = JsonConvert.DeserializeObject<List<ModelJob>>(json) ?? new List<ModelJob>();
            }
        }

        public void SaveData()
        {
            string json = JsonConvert.SerializeObject(myJobs, Formatting.Indented);
            File.WriteAllText(configFile, json);
        }

        public void AddJob(ModelJob newJob)
        {
            myJobs.Add(newJob);
            SaveData();
        }

        public void DeleteJob(int index)
        {
            if (index >= 0 && index < myJobs.Count)
            {
                myJobs.RemoveAt(index);
                SaveData();
            }
        }

        private bool IsBusinessSoftRunning()
        {
            try
            {
                var settings = Settings.Load();
                string targetName = settings.BusinessSoftware;

                if (string.IsNullOrEmpty(targetName)) return false;

                if (targetName.ToLower().EndsWith(".exe"))
                {
                    targetName = targetName.Substring(0, targetName.Length - 4);
                }

                Process[] processes = Process.GetProcesses();
                foreach (var p in processes)
                {
                    if (string.Equals(p.ProcessName, targetName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public void ExecuterSauvegarde(Action<string> uiCallback)
        {
            foreach (var job in myJobs)
            {
                ExecuterUnSeulJob(job, uiCallback);
            }
        }

        public void ExecuterUnSeulJob(ModelJob job, Action<string> uiCallback)
        {
            if (IsBusinessSoftRunning())
            {
                uiCallback($"STOP : Logiciel métier détecté ({Settings.Load().BusinessSoftware}). Sauvegarde annulée.");
                return;
            }

            try
            {
                uiCallback("Lancement : " + job.Name);

                if (!Directory.Exists(job.Source))
                {
                    uiCallback("ERREUR : Source introuvable -> " + job.Source);
                    return;
                }

                string[] files = Directory.GetFiles(job.Source, "*.*", SearchOption.AllDirectories);
                int totalFiles = files.Length;
                long totalSize = 0;
                foreach (string f in files) totalSize += new FileInfo(f).Length;

                int filesLeft = totalFiles;
                long sizeLeft = totalSize;

                UpdateEtat(job.Name, job.Source, job.Target, "ACTIF", totalFiles, totalSize, filesLeft, sizeLeft);

                foreach (string file in files)
                {
                    if (IsBusinessSoftRunning())
                    {
                        uiCallback("INTERRUPTION : Logiciel métier détecté pendant l'exécution.");
                        LogManager.SaveLog(job.Name, "STOP_METIER", "STOP_METIER", 0, 0);
                        break;
                    }

                    string relatif = file.Replace(job.Source, "").TrimStart('\\');
                    string dest = Path.Combine(job.Target, relatif);
                    long fileSize = new FileInfo(file).Length;

                    if (!job.IsFull && File.Exists(dest))
                    {
                        if (File.GetLastWriteTime(file) <= File.GetLastWriteTime(dest))
                        {
                            filesLeft--;
                            sizeLeft -= fileSize;
                            UpdateEtat(job.Name, file, dest, "ACTIF", totalFiles, totalSize, filesLeft, sizeLeft);
                            continue;
                        }
                    }

                    string? dirDest = Path.GetDirectoryName(dest);
                    if (!string.IsNullOrEmpty(dirDest) && !Directory.Exists(dirDest))
                        Directory.CreateDirectory(dirDest);

                    UpdateEtat(job.Name, file, dest, "ACTIF", totalFiles, totalSize, filesLeft, sizeLeft);

                    Stopwatch sw = Stopwatch.StartNew();
                    try
                    {
                        File.Copy(file, dest, true);
                        sw.Stop();
                        LogManager.SaveLog(job.Name, file, dest, fileSize, sw.Elapsed.TotalMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        LogManager.SaveLog(job.Name, file, dest, fileSize, -1);
                        uiCallback("Erreur copie : " + ex.Message);
                    }

                    filesLeft--;
                    sizeLeft -= fileSize;
                    uiCallback("Copié : " + relatif);
                }

                UpdateEtat(job.Name, "", "", "INACTIF", totalFiles, totalSize, 0, 0);
                uiCallback("Succès : " + job.Name);
            }
            catch (Exception ex)
            {
                uiCallback("ERREUR CRITIQUE : " + ex.Message);
            }
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

                string json = JsonConvert.SerializeObject(etat, Formatting.Indented);
                File.WriteAllText(stateFile, json);
            }
            catch
            {
            }
        }
    }
}