using System;
using System.Collections.Generic;
using System.IO;
using EasySave.Models;
using Newtonsoft.Json;
using EasyLog;
using System.Diagnostics;

namespace EasySave
{
    class Program
    {
        static string configFile = "jobs.json";
        static List<ModelJob> myJobs = new List<ModelJob>();

        static void Main(string[] args)
        {
            LoadData();

            if (args.Length > 0)
            {
                Lang.Init("en");
                ExecuterSauvegarde();
                return;
            }

            Console.WriteLine("\nSelect Language / Choisissez la langue :");
            Console.WriteLine("1. English");
            Console.WriteLine("2. Français");
            Console.Write("Choice / Choix : ");

            ConsoleKeyInfo key = Console.ReadKey();
            Console.WriteLine();

            if (key.KeyChar == '1')
            {
                Lang.Init("en"); 
            }
            else
            {
                Lang.Init("fr"); 
            }

            bool continuer = true;
            while (continuer)
            {
                Console.WriteLine(Lang.Msg["MenuTitle"]);
                Console.WriteLine(Lang.Msg["List"]);
                Console.WriteLine(Lang.Msg["Add"]);
                Console.WriteLine(Lang.Msg["Run"]);
                Console.WriteLine(Lang.Msg["Quit"]);
                Console.Write(Lang.Msg["Choice"]);

                string choix = Console.ReadLine();

                if (choix == "1") { ListerTravaux(); }
                else if (choix == "2") { AjouterTravail(); }
                else if (choix == "3") { ExecuterSauvegarde(); }
                else if (choix == "4") { continuer = false; }
            }
        }

        static void LoadData()
        {
            if (File.Exists(configFile))
            {
                string json = File.ReadAllText(configFile);
                myJobs = JsonConvert.DeserializeObject<List<ModelJob>>(json);
            }
            if (myJobs == null) myJobs = new List<ModelJob>();
        }

        static void ListerTravaux()
        {
            Console.WriteLine(Lang.Msg["MenuTitle"]);

            if (myJobs.Count == 0)
            {
                Console.WriteLine(Lang.Msg["NoJob"]);
            }
            else
            {
                for (int i = 0; i < myJobs.Count; i++)
                {
                    Console.WriteLine((i + 1) + ". " + myJobs[i].Name + " | " + myJobs[i].Source);
                }

                Console.Write(Lang.Msg["DeletePrompt"]);
                string choix = Console.ReadLine();

                if (int.TryParse(choix, out int index) && index > 0 && index <= myJobs.Count)
                {
                    myJobs.RemoveAt(index - 1);

                    string json = JsonConvert.SerializeObject(myJobs, Formatting.Indented);
                    File.WriteAllText(configFile, json);

                    Console.WriteLine(Lang.Msg["Deleted"]);
                }
            }
        }

        static void AjouterTravail()
        {
            if (myJobs.Count >= 5)
            {
                Console.WriteLine(Lang.Msg["MaxJobs"]);
                return;
            }

            ModelJob nouveau = new ModelJob();
            Console.Write(Lang.Msg["EnterName"]); nouveau.Name = Console.ReadLine();

            Console.Write(Lang.Msg["EnterSource"]);
            nouveau.Source = Console.ReadLine().Replace("\"", "").Trim();

            Console.Write(Lang.Msg["EnterTarget"]);
            nouveau.Target = Console.ReadLine().Replace("\"", "").Trim();

            Console.Write(Lang.Msg["EnterType"]);
            string type = Console.ReadLine();
            nouveau.IsFull = (type == "1");

            myJobs.Add(nouveau);

            string json = JsonConvert.SerializeObject(myJobs, Formatting.Indented);
            File.WriteAllText(configFile, json);
            Console.WriteLine(Lang.Msg["Saved"]);
        }

        static void UpdateEtat(string jobName, string src, string dest, string state, int totalF, long totalS, int leftF, long leftS)
        {
            int prog = 0;
            if (totalF > 0)
            {
                prog = 100 - (leftF * 100 / totalF);
            }

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
            File.WriteAllText("state.json", json);
        }

        static void ExecuterSauvegarde()
        {
            foreach (var job in myJobs)
            {
                try
                {
                    Console.WriteLine(Lang.Msg["Start"] + job.Name + " (" + (job.IsFull ? "Full" : "Diff") + ") ---");

                    if (!Directory.Exists(job.Source))
                    {
                        Console.WriteLine(Lang.Msg["SourceMissing"]);
                        continue;
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
                            EasyLog.LogManager.SaveLog(job.Name, file, dest, fileSize, sw.Elapsed.TotalMilliseconds);
                        }
                        catch (Exception)
                        {
                            sw.Stop();
                            EasyLog.LogManager.SaveLog(job.Name, file, dest, fileSize, -1);
                            throw;
                        }

                        filesLeft--;
                        sizeLeft -= fileSize;

                        Console.WriteLine(Lang.Msg["Copy"] + relatif);
                    }

                    UpdateEtat(job.Name, "", "", "INACTIF", totalFiles, totalSize, 0, 0);
                    Console.WriteLine(Lang.Msg["Success"] + job.Name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(Lang.Msg["Error"] + ex.Message);
                }
            }
            Console.WriteLine(Lang.Msg["PressKey"]);
            Console.ReadKey();
        }
    }
}