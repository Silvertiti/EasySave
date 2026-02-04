using System;
using System.Collections.Generic;
using EasySave.Models;

namespace EasySave.View
{
    public class VueConsole
    {
        public void AfficherMenu()
        {
            Console.WriteLine(Lang.Msg["MenuTitle"]);
            Console.WriteLine(Lang.Msg["List"]);
            Console.WriteLine(Lang.Msg["Add"]);
            Console.WriteLine(Lang.Msg["Run"]);
            Console.WriteLine(Lang.Msg["Quit"]);
            Console.Write(Lang.Msg["Choice"]);
        }

        public string LireSaisie()
        {
            return Console.ReadLine();
        }

        public void AfficherMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void AfficherListe(List<ModelJob> jobs)
        {
            Console.WriteLine(Lang.Msg["MenuTitle"]);
            if (jobs.Count == 0)
            {
                Console.WriteLine(Lang.Msg["NoJob"]);
                return;
            }
            for (int i = 0; i < jobs.Count; i++)
            {
                Console.WriteLine((i + 1) + ". " + jobs[i].Name + " | " + jobs[i].Source);
            }
        }

        public void ChoixLangue()
        {
            Console.WriteLine("\nSelect Language / Choisissez la langue :");
            Console.WriteLine("1. English");
            Console.WriteLine("2. Français");
            Console.Write("Choice / Choix : ");
        }
    }
}