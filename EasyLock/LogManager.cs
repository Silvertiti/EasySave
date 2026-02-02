using System;
using System.IO;
using System.Xml;
using Newtonsoft.Json;

namespace EasyLog
{
    public class LogManager
    {
        private static string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        public static void SaveLog(string jobName, string source, string target, long size, double timeMs)
        {
            Directory.CreateDirectory(logFolder);

            string filePath = Path.Combine(logFolder, DateTime.Now.ToString("yyyy-MM-dd") + ".json");

            var logEntry = new
            {
                Timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                BackupName = jobName,
                SourcePath = source,
                TargetPath = target,
                FileSize = size,
                TransferTime = timeMs 
            };

            string json = JsonConvert.SerializeObject(logEntry, Newtonsoft.Json.Formatting.Indented);
            File.AppendAllText(filePath, json);
        }
    }
}