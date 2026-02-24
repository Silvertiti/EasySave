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

        public SettingsManager()
        {
            settings = Load();
        }

        public Settings GetSettings() => settings;

        private Settings Load()
        {
            if (System.IO.File.Exists(settingsFile))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(settingsFile);
                    return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
                }
                catch { return new Settings(); }
            }
            return new Settings();
        }

        public void SaveSettings(Settings s)
        {
            settings = s;
            string json = JsonConvert.SerializeObject(s, Formatting.Indented);
            System.IO.File.WriteAllText(settingsFile, json);
        }

    }
}
