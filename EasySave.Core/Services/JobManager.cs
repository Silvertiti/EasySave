using EasySave.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Services
{
    class JobManager
    {
        private string configFile = "jobs.json";

        public List<ModelJob> LoadData()
        {
            if (File.Exists(configFile))
            {
                string json = File.ReadAllText(configFile);
                return JsonConvert.DeserializeObject<List<ModelJob>>(json) ?? new List<ModelJob>();
            }
            return new List<ModelJob>();
        }

        public void SaveData(List<ModelJob> jobs)
        {
            string json = JsonConvert.SerializeObject(jobs, Formatting.Indented);
            File.WriteAllText(configFile, json);
        }

    }
}
