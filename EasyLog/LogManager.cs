using System;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace EasyLog
{
    public class LogManager
    {
        private static string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        public static bool UseXml = false;

        public static void SaveLog(string jobName, string source, string target, long size, double timeMs)
        {
            Directory.CreateDirectory(logFolder);

            var logEntry = new
            {
                Timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                BackupName = jobName,
                SourcePath = source,
                TargetPath = target,
                FileSize = size,
                TransferTime = timeMs
            };

            if (UseXml)
            {
                string filePath = Path.Combine(logFolder, DateTime.Now.ToString("yyyy-MM-dd") + ".xml");
                var xmlEntry = new XElement("LogEntry",
                    new XElement("Timestamp", logEntry.Timestamp),
                    new XElement("BackupName", logEntry.BackupName),
                    new XElement("SourcePath", logEntry.SourcePath),
                    new XElement("TargetPath", logEntry.TargetPath),
                    new XElement("FileSize", logEntry.FileSize),
                    new XElement("TransferTime", logEntry.TransferTime)
                );
                File.AppendAllText(filePath, xmlEntry.ToString() + Environment.NewLine);
            }
            else
            {
                string filePath = Path.Combine(logFolder, DateTime.Now.ToString("yyyy-MM-dd") + ".json");
                string json = JsonConvert.SerializeObject(logEntry, Newtonsoft.Json.Formatting.Indented);
                File.AppendAllText(filePath, json + Environment.NewLine); 
            }
        }
    }
}