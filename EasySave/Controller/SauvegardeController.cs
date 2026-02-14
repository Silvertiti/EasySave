using System;
using EasySave.Core.Models;
using EasySave.View;

namespace EasySave.Controller
{
    public class SauvegardeController
    {
        SauvegardeModel model = new SauvegardeModel();
        ConsoleView view = new ConsoleView();

        public void Start(string[] args)
        {
            model.LoadData();
            if (args.Length > 0)
            {
                model.ExecuterSauvegarde(Console.WriteLine);
                return;
            }
            view.ChoixLangue();
            var k = Console.ReadKey();
            Console.WriteLine();
            if (Lang.Msg == null || Lang.Msg.Count == 0) Lang.Init(k.KeyChar == '1' ? "en" : "fr");
            else Lang.Init(k.KeyChar == '1' ? "en" : "fr");

            Console.Clear();
            view.AfficherLogo();

            bool continuer = true;
            while (continuer)
            {
                view.AfficherMenu();
                string choix = view.LireSaisie();

                switch (choix)
                {
                    case "1": Lister(); break;
                    case "2": Ajouter(); break;
                    case "3":
                        model.ExecuterSauvegarde(view.AfficherMessage);

                        view.AfficherMessage("\nAppuyez sur une touche...");
                        Console.ReadKey();
                        Console.Clear();
                        view.AfficherLogo();
                        break;
                    case "4": continuer = false; break;
                }
            }
        }

        void Lister()
        {
            view.AfficherListe(model.myJobs);
            view.AfficherMessage("\n(Entrez un numéro pour supprimer, ou 0 pour retour)");
            string choix = view.LireSaisie();

            if (int.TryParse(choix, out int index) && index > 0 && index <= model.myJobs.Count)
            {
                model.DeleteJob(index - 1);
                view.AfficherMessage("Travail supprimé !");
            }
        }

        void Ajouter()
        {

            view.AfficherMessage("Nom du travail :");
            string nom = view.LireSaisie();

            view.AfficherMessage("Source :");
            string src = view.LireSaisie().Replace("\"", "").Trim();

            view.AfficherMessage("Cible :");
            string dest = view.LireSaisie().Replace("\"", "").Trim();

            view.AfficherMessage("Type (1=Complet, 2=Différentiel) :");
            string type = view.LireSaisie();
            var newJob = new ModelJob
            {
                Name = nom,
                Source = src,
                Target = dest,
                IsFull = (type == "1")
            };

            model.AddJob(newJob);

            view.AfficherMessage("Sauvegardé avec succès !");
        }
    }
}