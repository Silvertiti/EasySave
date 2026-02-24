using System;
using System.Collections.Generic;
using EasySave.Core.Models;
using EasySave.Core.Services;

namespace EasySave.View
{
    public class ConsoleTools
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

        public void AfficherMenu(bool isServerRunning = false)
        {
            Console.WriteLine(LangConsole.Msg["MenuTitle"]);
            Console.WriteLine(LangConsole.Msg["List"]);
            Console.WriteLine(LangConsole.Msg["Add"]);
            Console.WriteLine(LangConsole.Msg["Run"]);
            Console.WriteLine(LangConsole.Msg["Quit"]);

            // Option serveur avec couleur
            if (isServerRunning)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"5. Serveur HTTP [EN LIGNE - port {BackupServer.Port}]");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"5. Serveur HTTP [ARRETE]");
            }
            Console.ResetColor();

            Console.WriteLine("6. Paramètres");
            Console.Write(LangConsole.Msg["Choice"]);
        }

        public void AfficherSettings(bool useXml)
        {
            Console.WriteLine(LangConsole.Msg["MenuTitle"]);
            Console.WriteLine(LangConsole.Msg["CurrentLog"] + (useXml ? "XML" : "JSON"));
            Console.WriteLine(LangConsole.Msg["ChangeLog"]);
            Console.WriteLine(LangConsole.Msg["Back"]);
            Console.Write(LangConsole.Msg["Choice"]);
        }

        public string? LireSaisie()
        {
            return Console.ReadLine();
        }

        public void AfficherMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void AfficherListe(List<ModelJob> jobs)
        {
            Console.WriteLine(LangConsole.Msg["MenuTitle"]);
            if (jobs.Count == 0)
            {
                Console.WriteLine(LangConsole.Msg["NoJob"]);
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