using System;
using System.Collections.Generic;
using EasySave.Core.Models;

namespace EasySave.View
{
    public class ConsoleView
    {
        public void AfficherLogo()
        {
            string logo = @"
 /$$$$$$$$                                /$$$$$$                               
| $$_____/                               /$$__  $$                              
| $$        /$$$$$$   /$$$$$$$ /$$   /$$| $$  \__/  /$$$$$$  /$$    /$$ /$$$$$$ 
| $$$$$    |____  $$ /$$_____/| $$  | $$|  $$$$$$  |____  $$|  $$  /$$//$$__  $$
| $$__/     /$$$$$$$|  $$$$$$ | $$  | $$ \____  $$  /$$$$$$$ \  $$/$$/| $$$$$$$$
| $$       /$$__  $$ \____  $$| $$  | $$ /$$  \ $$ /$$__  $$  \  $$$/ | $$_____/
| $$$$$$$$|  $$$$$$$ /$$$$$$$/|  $$$$$$$|  $$$$$$/|  $$$$$$$   \  $/  |  $$$$$$$
|________/ \_______/|_______/  \____  $$ \______/  \_______/    \_/    \_______/
                               /$$  | $$                                        
                              |  $$$$$$/                                        
                               \______/                                                                                                                                              
";

            ConsoleColor[] colors = {
                ConsoleColor.Red,
                ConsoleColor.DarkYellow,
                ConsoleColor.Yellow,
                ConsoleColor.Green,
                ConsoleColor.Cyan,
                ConsoleColor.Blue,
                ConsoleColor.Magenta
            };

            int colorIndex = 0;

            foreach (char c in logo)
            {
                if (c == '\n' || c == '\r')
                {
                    Console.Write(c);
                    continue;
                }

                Console.ForegroundColor = colors[colorIndex % colors.Length];

                Console.Write(c);

                colorIndex++;
            }

            Console.ResetColor();
            Console.WriteLine("\nBienvenue dans EasySave CLI");
            Console.WriteLine("--------------------------------------------------\n");
        }

        public void AfficherMenu()
        {
            Console.WriteLine(Lang.Msg["MenuTitle"]);
            Console.WriteLine(Lang.Msg["List"]);
            Console.WriteLine(Lang.Msg["Add"]);
            Console.WriteLine(Lang.Msg["Run"]);
            Console.WriteLine(Lang.Msg["Quit"]);
            Console.WriteLine(Lang.Msg["Settings"]);
            Console.Write(Lang.Msg["Choice"]);
        }

        public void AfficherSettings(bool useXml)
        {
            Console.WriteLine(Lang.Msg["MenuTitle"]);
            Console.WriteLine(Lang.Msg["CurrentLog"] + (useXml ? "XML" : "JSON"));
            Console.WriteLine(Lang.Msg["ChangeLog"]);
            Console.WriteLine(Lang.Msg["Back"]);
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