using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace EasySave.Core.Models
{
    public static class LangConsole
    {
        public static Dictionary<string, string> Msg = new Dictionary<string, string>();

        public static void Init(string culture)
        {
            Msg.Clear();

            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lang_console.json");
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Error: {filePath} not found so default keys are missing.");
                    return;
                }

                string json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);

                if (data != null && data.ContainsKey(culture))
                {
                    Msg = data[culture];
                }
                else
                {
                    if (data != null && data.ContainsKey("en"))
                        Msg = data["en"];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading lang_console.json: " + ex.Message);
            }
        }
    }
}