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
            Log($"Serveur HTTP démarré sur le port {Port}");
        }

        public void Stop()
        {
            IsRunning = false;
            _listener?.Stop();
            Log("Serveur arrêté.");
        }

        private void Listen()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, Port);
                _listener.Start();
            }
            catch (Exception ex)
            {
                Log($"Erreur : impossible de démarrer ({ex.Message}). Port déjà utilisé ?");
                IsRunning = false;
                return;
            }
            while (IsRunning)
            {
                try { Handle(_listener.AcceptTcpClient()); }
                catch { break; }
            }
        }

        private void Handle(TcpClient tcp)
        {
            try
            {
                var stream = tcp.GetStream();
                var reader = new StreamReader(stream, Encoding.UTF8);

                // Lire la requête HTTP (GET /path HTTP/1.1)
                string? requestLine = reader.ReadLine();
                if (requestLine == null) return;

                // Lire les headers (on les ignore)
                string? header;
                while (!string.IsNullOrEmpty(header = reader.ReadLine())) { }

                // Extraire le chemin
                string path = "/";
                var parts = requestLine.Split(' ');
                if (parts.Length >= 2) path = parts[1].ToLower();

                var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

                // Headers HTTP
                writer.WriteLine("HTTP/1.1 200 OK");
                writer.WriteLine("Access-Control-Allow-Origin: *");
                writer.WriteLine("Connection: close");

                if (path == "/run")
                {
                    writer.WriteLine("Content-Type: text/plain; charset=utf-8");
                    writer.WriteLine();
                    Log("Commande RUN_ALL reçue");
                    _controller?.ExecuterSauvegarde(msg =>
                    {
                        writer.WriteLine(msg);
                        Log(msg);
                    });
                    writer.WriteLine("=== TERMINÉ ===");
                }
                else if (path == "/list")
                {
                    writer.WriteLine("Content-Type: text/plain; charset=utf-8");
                    writer.WriteLine();
                    if (_controller == null || _controller.myJobs.Count == 0)
                        writer.WriteLine("Aucun job configuré.");
                    else
                        foreach (var j in _controller.myJobs)
                            writer.WriteLine($"{j.Name} | {j.Source} -> {j.Target}");
                }
                else if (path == "/status")
                {
                    writer.WriteLine("Content-Type: text/plain; charset=utf-8");
                    writer.WriteLine();
                    writer.WriteLine(File.Exists("state.json")
                        ? File.ReadAllText("state.json")
                        : "Aucune sauvegarde en cours.");
                }
                else
                {
                    // Page d'accueil HTML
                    string html = $@"<!DOCTYPE html>
<html><head><meta charset=""utf-8""><title>EasySave</title>
<style>body{{font-family:Arial;background:#111;color:#eee;text-align:center;padding-top:60px}}a{{display:block;color:#4af;font-size:18px;margin:10px auto;width:300px;padding:10px;border:1px solid #4af;text-decoration:none}}a:hover{{background:#223}}</style>
</head><body>
<h2>EasySave Server</h2>
<p>{_controller?.myJobs.Count ?? 0} job(s)</p>
<a href=""/run"">Lancer toutes les sauvegardes</a>
<a href=""/list"">Lister les jobs</a>
<a href=""/status"">Statut</a>
</body></html>";
                    writer.WriteLine("Content-Type: text/html; charset=utf-8");
                    writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(html)}");
                    writer.WriteLine();
                    writer.Write(html);
                }
            }
            catch (Exception ex) { Log($"Erreur: {ex.Message}"); }
            finally { tcp.Close(); }
        }

        private void Log(string msg) => OnLog?.Invoke(msg);
    }
}
