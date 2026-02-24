using System;
using EasySave.Core.Controller;
using EasySave.Core.Models;
using EasySave.Core.Services;
using EasySave.View;

namespace EasySave.Controller
{
    public class SauvegardeView
    {
        SauvegardeController controller = new SauvegardeController();
        ConsoleTools consoleTools = new ConsoleTools();
        BackupServer server = new BackupServer();

        public void Start(string[] args)
        {
            if (args.Length > 0) { controller.ExecuterSauvegarde(Console.WriteLine); return; }

            consoleTools.ChoixLangue();
            var k = Console.ReadKey();
            Console.WriteLine();
            LangConsole.Init(k.KeyChar == '1' ? "en" : "fr");
            Console.Clear();
            consoleTools.AfficherLogo();

            // Configuration des logs du serveur
            server.OnLog += msg =>
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\n[Server] {msg}");
                Console.ResetColor();
            };

            bool continuer = true;
            while (continuer)
            {
                consoleTools.AfficherMenu(server.IsRunning);
                switch (consoleTools.LireSaisie())
                {
                    case "1": Lister(); break;
                    case "2": Ajouter(); break;
                    case "3":
                        controller.ExecuterSauvegarde(consoleTools.AfficherMessage);
                        Console.WriteLine("\nAppuyez sur une touche..."); Console.ReadKey();
                        Console.Clear(); consoleTools.AfficherLogo();
                        break;
                    case "4": continuer = false; break;
                    case "5":
                        if (server.IsRunning) server.Stop();
                        else server.Start(controller);
                        Console.Clear(); consoleTools.AfficherLogo();
                        break;
                    case "6": Settings(); break;
                }
            }

            server.Stop();
        }

        void Settings()
        {
            var settingsManager = new SettingsManager();
            bool inSettings = true;
            while (inSettings)
            {
                var settings = settingsManager.GetSettings();
                consoleTools.AfficherSettings(settings.LogFormat.ToLower() == "xml");
                switch (consoleTools.LireSaisie())
                {
                    case "1":
                        settings.LogFormat = (settings.LogFormat.ToLower() == "xml") ? "json" : "xml";
                        settingsManager.SaveSettings(settings);
                        break;
                    case "2": inSettings = false; break;
                }
                Console.Clear();
                consoleTools.AfficherLogo();
            }
        }

        void Lister()
        {
            consoleTools.AfficherListe(controller.myJobs);
            consoleTools.AfficherMessage("\n(Entrez un numéro pour supprimer, ou 0 pour retour)");
            if (int.TryParse(consoleTools.LireSaisie(), out int i) && i > 0 && i <= controller.myJobs.Count)
            { controller.DeleteJob(i - 1); consoleTools.AfficherMessage("Supprimé !"); }
        }

        void Ajouter()
        {
            consoleTools.AfficherMessage("Nom :");       string nom  = consoleTools.LireSaisie();
            consoleTools.AfficherMessage("Source :");    string src  = consoleTools.LireSaisie().Replace("\"", "").Trim();
            consoleTools.AfficherMessage("Cible :");     string dest = consoleTools.LireSaisie().Replace("\"", "").Trim();
            consoleTools.AfficherMessage("Type (1=Complet, 2=Différentiel) :"); string type = consoleTools.LireSaisie();
            controller.AddJob(new ModelJob { Name = nom, Source = src, Target = dest, IsFull = (type == "1") });
            consoleTools.AfficherMessage("Sauvegardé !");
        }
    }
}