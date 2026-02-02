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

            bool continuer = true;
            while (continuer)
            {

                Console.WriteLine("\n--- MENU EASY SAVE ---");
                Console.WriteLine("1. Lister les travaux");
                Console.WriteLine("2. Ajouter un travail");
                Console.WriteLine("3. LANCER TOUTES LES SAUVEGARDES");
                Console.WriteLine("4. Quitter");
                Console.Write("Votre choix : ");

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
            Console.WriteLine("\n--- LISTE DES TRAVAUX ---");
            if (myJobs.Count == 0)
            {
                Console.WriteLine("Aucun travail trouvé.");
            }
            else
            {
                for (int i = 0; i < myJobs.Count; i++)
                {
                    Console.WriteLine((i + 1) + ". Nom: " + myJobs[i].Name + " | Source: " + myJobs[i].Source);
                }

                Console.Write("\nEntrez le numéro du travail à supprimer (ou Entrée pour annuler) : ");
                string choix = Console.ReadLine();

                if (int.TryParse(choix, out int index) && index > 0 && index <= myJobs.Count)
                {
                    myJobs.RemoveAt(index - 1); 

                    // Sauvegarde du changement dans le JSON
                    string json = JsonConvert.SerializeObject(myJobs, Formatting.Indented);
                    File.WriteAllText(configFile, json);

                    Console.WriteLine("Travail supprimé !");
                }
            }
        }

        static void AjouterTravail()
        {
            if (myJobs.Count >= 5)
            {
                Console.WriteLine("Erreur : Maximum 5 travaux !");
                return;
            }

            ModelJob nouveau = new ModelJob();
            Console.Write("Nom : "); nouveau.Name = Console.ReadLine();

            Console.Write("Source : ");
            nouveau.Source = Console.ReadLine().Replace("\"", "").Trim();

            Console.Write("Cible : ");
            nouveau.Target = Console.ReadLine().Replace("\"", "").Trim();

            Console.Write("Type (1 pour Complet, 2 pour Différentiel) : ");
            string type = Console.ReadLine();
            nouveau.IsFull = (type == "1"); 

            myJobs.Add(nouveau);

            string json = JsonConvert.SerializeObject(myJobs, Formatting.Indented);
            File.WriteAllText(configFile, json);
            Console.WriteLine("Travail enregistré !");
        }

        static void ExecuterSauvegarde()
        {
            foreach (var job in myJobs)
            {
                try
                {
                    Console.WriteLine("\n--- Travail : " + job.Name + " (" + (job.IsFull ? "Complet" : "Différentiel") + ") ---");

                    if (!Directory.Exists(job.Source))
                    {
                        Console.WriteLine("Erreur : Source introuvable.");
                        continue;
                    }
                    string[] files = Directory.GetFiles(job.Source, "*.*", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        string relatif = file.Replace(job.Source, "").TrimStart('\\');
                        string dest = Path.Combine(job.Target, relatif);

                        if (!job.IsFull && File.Exists(dest))
                        {
                            if (File.GetLastWriteTime(file) <= File.GetLastWriteTime(dest)) continue;
                        }

                        Directory.CreateDirectory(Path.GetDirectoryName(dest));

                        long fileSize = new FileInfo(file).Length; 
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

                        Console.WriteLine(" Fichier copié : " + relatif);
                    }
                    Console.WriteLine("Succès pour " + job.Name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur sur ce travail : " + ex.Message);
                }
            }
            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }
    }
}