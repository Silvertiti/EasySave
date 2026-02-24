using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace EasySave.Core.Services
{
    public class BackupClient
    {
        private readonly string _host;
        private readonly int _port;

        public BackupClient(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public string SendCommand(string command)
        {
            try
            {
                using var client = new TcpClient(_host, _port);
                var stream = client.GetStream();
                var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                var reader = new StreamReader(stream, Encoding.UTF8);

                writer.WriteLine(command);
                client.ReceiveTimeout = 3000;

                var sb = new StringBuilder();
                string? line;
                try { while ((line = reader.ReadLine()) != null) sb.AppendLine(line); }
                catch { }

                return sb.ToString().Trim();
            }
            catch (Exception ex)
            {
                return $"Erreur : {ex.Message}";
            }
        }
    }
}
