using System;
using EasySave.Models;
using EasySave.View;

namespace EasySave.Controller
{
    public class SauvegardeController
    {
        SauvegardeModel model = new SauvegardeModel();
        VueConsole view = new VueConsole();

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
                        break;
                    case "4": continuer = false; break;
                }
            }
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