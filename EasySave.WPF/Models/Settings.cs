using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace EasySave.Core.Models
{
    public class Settings
    {
        public string ExtensionsToEncrypt { get; set; } = "txt,docx,xlsx";
        public string BusinessSoftware { get; set; } = "calc";
        public string LogFormat { get; set; } = "json";
        public string CryptoSoftPath { get; set; } = "";
        private static string settingsFile = "settings.json";

        public static Settings Load()
        {
            if (File.Exists(settingsFile))
            {
                try
                {
                    string json = File.ReadAllText(settingsFile);
                    return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
                }
                catch { return new Settings(); }
            }
            return new Settings();
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(settingsFile, json);
        }
    }
}