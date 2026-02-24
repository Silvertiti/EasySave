using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EasySave.Core.Controller;

namespace EasySave.Core.Services
{
    public class BackupServer
    {
        public const int Port = 11000;
        public bool IsRunning { get; private set; }
        public event Action<string>? OnLog;

        private TcpListener? _listener;
        private SauvegardeController? _controller;

        public void Start(SauvegardeController controller)
        {
            if (IsRunning) return;
            _controller = controller;
            IsRunning = true;
            Task.Run(Listen);
            Log($"Serveur démarré sur le port {Port}");
        }

        public void Stop()
        {
            IsRunning = false;
            _listener?.Stop();
            Log("Serveur arrêté.");
        }

        private void Listen()
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            while (IsRunning)
            {
                try { Task.Run(() => Handle(_listener.AcceptTcpClient())); }
                catch { break; }
            }
        }

        private void Handle(TcpClient tcp)
        {
            try
            {
                var enc = new UTF8Encoding(false);
                var reader = new StreamReader(tcp.GetStream(), enc);
                var writer = new StreamWriter(tcp.GetStream(), enc) { AutoFlush = true };

                string cmd = reader.ReadLine()?.Trim() ?? "";
                Log($"Commande reçue : {cmd}");

                if (cmd == "RUN_ALL")
                {
                    // Stream chaque message de progression directement au client
                    _controller?.ExecuterSauvegarde(msg =>
                    {
                        writer.WriteLine(msg);
                        Log(msg);
                    });
                    writer.WriteLine("=== TERMINÉ ===");
                }
                else if (cmd == "STATUS")
                {
                    writer.WriteLine(File.Exists("state.json")
                        ? File.ReadAllText("state.json")
                        : "Aucune sauvegarde en cours.");
                }
                else if (cmd == "LIST")
                {
                    if (_controller == null || _controller.myJobs.Count == 0)
                        writer.WriteLine("Aucun job configuré.");
                    else
                        foreach (var j in _controller.myJobs)
                            writer.WriteLine($"{j.Name} | {j.Source} -> {j.Target}");
                }
                else
                {
                    writer.WriteLine($"Commande inconnue : {cmd}");
                }
            }
            catch (Exception ex) { Log($"Erreur: {ex.Message}"); }
            finally { tcp.Close(); }
        }

        private void Log(string msg) => OnLog?.Invoke(msg);
    }
}
