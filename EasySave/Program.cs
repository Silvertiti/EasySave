using System;
using System.Collections.Generic;
using System.IO;
using EasySave.Models; // Assure-toi que ton ModelJob.cs est bien dans ce namespace
using Newtonsoft.Json;

namespace EasySave
{
    class Program
    {
        // On déclare la liste et le fichier ici pour qu'ils soient accessibles partout
        static string configFile = "jobs.json";
        static List<ModelJob> myJobs = new List<ModelJob>();

        static void Main(string[] args)
        {
            // 1. Charger les données au démarrage
            LoadData();

            bool continuer = true;
            while (continuer)
            {
                Console.WriteLine("\n--- MENU EASY SAVE ---");
                Console.WriteLine("1. Lister les travaux");
                Console.WriteLine("2. Ajouter un travail");
                Console.WriteLine("3. Quitter");
                Console.Write("Votre choix : ");

                string choix = Console.ReadLine();

                if (choix == "1")
                {
                    ListerTravaux();
                }
                else if (choix == "2")
                {
                    AjouterTravail();
                }
                else if (choix == "3")
                {
                    continuer = false;
                }
                else
                {
                    Console.WriteLine("Choix invalide !");
                }
            }
        }

        static void LoadData()
        {
            if (File.Exists(configFile))
            {
                string json = File.ReadAllText(configFile);
                myJobs = JsonConvert.DeserializeObject<List<ModelJob>>(json);
            }
            
            // Si le fichier est vide ou inexistant, on initialise une liste vide
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
                foreach (var job in myJobs)
                {
                    Console.WriteLine("Nom: " + job.Name + " | Source: " + job.Source);
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
            Console.Write("Source : "); nouveau.Source = Console.ReadLine();
            Console.Write("Cible : "); nouveau.Target = Console.ReadLine();

            myJobs.Add(nouveau);

            // Sauvegarde immédiate
            string json = JsonConvert.SerializeObject(myJobs, Formatting.Indented);
            File.WriteAllText(configFile, json);
            Console.WriteLine("Travail enregistré avec succès !");
        }
    }
}