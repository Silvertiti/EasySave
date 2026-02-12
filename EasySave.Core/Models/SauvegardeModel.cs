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
        string configFile = "jobs.json";

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

        public void AddJob(string name, string src, string dest, bool isFull)
        {
            myJobs.Add(new ModelJob { Name = name, Source = src, Target = dest, IsFull = isFull });
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

        public void ExecuterSauvegarde(Action<string> uiCallback)
        {
            foreach (var job in myJobs)
            {
                ExecuterUnSeulJob(job, uiCallback);
            }
        }

        public void ExecuterUnSeulJob(ModelJob job, Action<string> uiCallback)
        {
            try
            {
                uiCallback(Lang.Msg["Start"] + job.Name);

                if (!Directory.Exists(job.Source))
                {
                    uiCallback(Lang.Msg["SourceMissing"]);
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

                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    UpdateEtat(job.Name, file, dest, "ACTIF", totalFiles, totalSize, filesLeft, sizeLeft);

                    Stopwatch sw = Stopwatch.StartNew();
                    try
                    {
                        File.Copy(file, dest, true);
                        sw.Stop();
                        LogManager.SaveLog(job.Name, file, dest, fileSize, sw.Elapsed.TotalMilliseconds);
                    }
                    catch
                    {
                        sw.Stop();
                        LogManager.SaveLog(job.Name, file, dest, fileSize, -1);
                        throw;
                    }

                    filesLeft--;
                    sizeLeft -= fileSize;
                    uiCallback(Lang.Msg["Copy"] + relatif);
                }

                UpdateEtat(job.Name, "", "", "INACTIF", totalFiles, totalSize, 0, 0);
                uiCallback(Lang.Msg["Success"] + job.Name);
            }
            catch (Exception ex)
            {
                uiCallback(Lang.Msg["Error"] + ex.Message);
            }
        }

        private void UpdateEtat(string jobName, string src, string dest, string state, int totalF, long totalS, int leftF, long leftS)
        {
            int prog = (totalF > 0) ? 100 - (leftF * 100 / totalF) : 0;

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
            File.WriteAllText("state.json", JsonConvert.SerializeObject(etat, Formatting.Indented));
        }
    }
}