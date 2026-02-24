using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            string ip = args.Length > 0 ? args[0] : "127.0.0.1";
            Console.WriteLine("=== EasySave - Client distant ===");
            Console.WriteLine($"Connexion au serveur {ip}:11000...\n");

            try
            {
                var tcp = new TcpClient(ip, 11000);
                var writer = new StreamWriter(tcp.GetStream(), new UTF8Encoding(false)) { AutoFlush = true };
                var reader = new StreamReader(tcp.GetStream(), new UTF8Encoding(false));

                writer.WriteLine("RUN_ALL");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Sauvegardes en cours...\n");
                Console.ResetColor();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains("TERMIN") || line.Contains("ucces"))
                        Console.ForegroundColor = ConsoleColor.Green;
                    else if (line.Contains("ERREUR") || line.Contains("rreur"))
                        Console.ForegroundColor = ConsoleColor.Red;
                    else
                        Console.ForegroundColor = ConsoleColor.Gray;

                    Console.WriteLine(line);
                    Console.ResetColor();
                }

                tcp.Close();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Erreur : {ex.Message}");
                Console.WriteLine("Verifiez que le serveur (WPF) est demarre.");
                Console.ResetColor();
            }

            Console.WriteLine("\nAppuyez sur une touche...");
            Console.ReadKey();
        }
    }
}