using System;
using EasySave.Core.Controller;
using EasySave.Core.Models;
using EasySave.View;

namespace EasySave.Controller
{
    public class SauvegardeView
    {
        SauvegardeController controller = new SauvegardeController();
        ConsoleTools consoleTools = new ConsoleTools();

        public void Start(string[] args)
        {
            if (args.Length > 0)
            {
                controller.ExecuterSauvegarde(Console.WriteLine);
                return;
            }
            consoleTools.ChoixLangue();
            var k = Console.ReadKey();
            Console.WriteLine();
            if (Lang.Msg == null || Lang.Msg.Count == 0) Lang.Init(k.KeyChar == '1' ? "en" : "fr");
            else Lang.Init(k.KeyChar == '1' ? "en" : "fr");

            Console.Clear();
            consoleTools.AfficherLogo();

            bool continuer = true;
            while (continuer)
            {
                consoleTools.AfficherMenu();
                string choix = consoleTools.LireSaisie();

                switch (choix)
                {
                    case "1": Lister(); break;
                    case "2": Ajouter(); break;
                    case "3":
                        controller.ExecuterSauvegarde(consoleTools.AfficherMessage);

                        consoleTools.AfficherMessage("\nAppuyez sur une touche...");
                        Console.ReadKey();
                        Console.Clear();
                        consoleTools.AfficherLogo();
                        break;
                    case "4": continuer = false; break;
                }
            }
        }

        void Lister()
        {
            consoleTools.AfficherListe(controller.myJobs);
            consoleTools.AfficherMessage("\n(Entrez un numéro pour supprimer, ou 0 pour retour)");
            string choix = consoleTools.LireSaisie();

            if (int.TryParse(choix, out int index) && index > 0 && index <= controller.myJobs.Count)
            {
                controller.DeleteJob(index - 1);
                consoleTools.AfficherMessage("Travail supprimé !");
            }
        }

        void Ajouter()
        {

            consoleTools.AfficherMessage("Nom du travail :");
            string nom = consoleTools.LireSaisie();

            consoleTools.AfficherMessage("Source :");
            string src = consoleTools.LireSaisie().Replace("\"", "").Trim();

            consoleTools.AfficherMessage("Cible :");
            string dest = consoleTools.LireSaisie().Replace("\"", "").Trim();

            consoleTools.AfficherMessage("Type (1=Complet, 2=Différentiel) :");
            string type = consoleTools.LireSaisie();
            var newJob = new ModelJob
            {
                Name = nom,
                Source = src,
                Target = dest,
                IsFull = (type == "1")
            };

            controller.AddJob(newJob);

            consoleTools.AfficherMessage("Sauvegardé avec succès !");
        }
    }
}