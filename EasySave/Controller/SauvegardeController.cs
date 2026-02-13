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
                Lang.Init("en");
                model.ExecuterSauvegarde(Console.WriteLine);
                return;
            }

            view.ChoixLangue();
            var k = Console.ReadKey();
            Console.WriteLine();
            Lang.Init(k.KeyChar == '1' ? "en" : "fr");

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
                        view.AfficherMessage(Lang.Msg["PressKey"]);
                        Console.ReadKey();
                        Console.Clear();
                        view.AfficherLogo();
                        break;
                    case "4": continuer = false; break;
                    case "5": GererParametres(); break;
                }
            }
        }

        void GererParametres()
        {
            bool retour = false;
            while (!retour)
            {
                Console.Clear();
                view.AfficherSettings(EasyLog.LogManager.UseXml);
                string? choix = view.LireSaisie();

                if (choix == "1")
                {
                    EasyLog.LogManager.UseXml = !EasyLog.LogManager.UseXml;
                }
                else if (choix == "2")
                {
                    retour = true;
                }
            }
            Console.Clear();
            view.AfficherLogo();
        }

        void Lister()
        {
            view.AfficherListe(model.myJobs);

            view.AfficherMessage(Lang.Msg["DeletePrompt"]);
            string choix = view.LireSaisie();
            if (int.TryParse(choix, out int index) && index > 0)
            {
                model.DeleteJob(index - 1);
                view.AfficherMessage(Lang.Msg["Deleted"]);
            }
        }

        void Ajouter()
        {
            if (model.myJobs.Count >= 5)
            {
                view.AfficherMessage(Lang.Msg["MaxJobs"]);
                return;
            }


            view.AfficherMessage(Lang.Msg["EnterName"]); string nom = view.LireSaisie();
            view.AfficherMessage(Lang.Msg["EnterSource"]); string src = view.LireSaisie().Replace("\"", "").Trim();
            view.AfficherMessage(Lang.Msg["EnterTarget"]); string dest = view.LireSaisie().Replace("\"", "").Trim();
            view.AfficherMessage(Lang.Msg["EnterType"]); string type = view.LireSaisie();

            model.AddJob(nom, src, dest, type == "1");
            view.AfficherMessage(Lang.Msg["Saved"]);
        }
    }
}