using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace EasySave.Core.Models
{
    public static class LangGUI
    {
        public static Dictionary<string, string> Msg = new Dictionary<string, string>();

        public static void Init(string culture)
        {
            Msg.Clear();

            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang_gui.json");
                if (!File.Exists(filePath))
                    filePath = "lang_gui.json";

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

                    if (data != null && data.ContainsKey(culture))
                    {
                        Msg = data[culture];
                    }
                    else if (data != null && data.ContainsKey("en"))
                    {
                        Msg = data["en"];
                    }
                }
            }
            catch (Exception ex)
            {
                Msg = new Dictionary<string, string>();
            }
        }
    }
}
