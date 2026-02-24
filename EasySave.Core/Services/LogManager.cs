using System;
using System.IO;
using Newtonsoft.Json;
using EasySave.Core.Models;

namespace EasySave.Core.Services
{
    public class LogManager
    {
        private static string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static SettingsManager _settingsManager = new SettingsManager();
        public static void SaveLog(string jobName, string source, string target, long size, double transferTime, double encryptionTime)
        {
            var settings = _settingsManager.GetSettings();
            string format = settings.LogFormat.ToLower();

            Directory.CreateDirectory(logFolder);
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + "." + format;
            string filePath = Path.Combine(logFolder, fileName);

            var logEntry = new
            {
                Timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                BackupName = jobName,
                SourcePath = source,
                TargetPath = target,
                FileSize = size,
                TransferTime = transferTime + " ms",
                EncryptionTime = encryptionTime
            };

            if (format == "xml")
            {
                AppendXml(filePath, logEntry);
            }
            else
            {
                AppendJson(filePath, logEntry);
            }
        }

        private static void AppendJson(string path, object entry)
        {
            string json = JsonConvert.SerializeObject(entry, Newtonsoft.Json.Formatting.None);
            File.AppendAllText(path, json + Environment.NewLine);
        }

        private static void AppendXml(string path, object entry)
        {
            dynamic log = entry;
            string xmlEntry = $"  <LogEntry>\n" +
                              $"    <Timestamp>{log.Timestamp}</Timestamp>\n" +
                              $"    <BackupName>{log.BackupName}</BackupName>\n" +
                              $"    <SourcePath>{log.SourcePath}</SourcePath>\n" +
                              $"    <TargetPath>{log.TargetPath}</TargetPath>\n" +
                              $"    <FileSize>{log.FileSize}</FileSize>\n" +
                              $"    <TransferTime>{log.TransferTime}</TransferTime>\n" +
                              $"    <EncryptionTime>{log.EncryptionTime}</EncryptionTime>\n" +
                              $"  </LogEntry>\n";

            if (!File.Exists(path))
            {
                File.WriteAllText(path, "<Logs>\n" + xmlEntry + "</Logs>");
            }
            else
            {
                string content = File.ReadAllText(path);
                content = content.Replace("</Logs>", xmlEntry + "</Logs>");
                File.WriteAllText(path, content);
            }
        }
    }
}