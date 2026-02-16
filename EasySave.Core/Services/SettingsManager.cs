using EasySave.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Services
{
    public class SettingsManager
    {
        Settings settings;

        private static string settingsFile = "settings.json";

        public Settings GetSettings() => settings ??= Load();

        private Settings Load()
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

        public void Save(Settings s)
        {
            string json = JsonConvert.SerializeObject(s, Formatting.Indented);
            File.WriteAllText(settingsFile, json);
        }
    }
}
