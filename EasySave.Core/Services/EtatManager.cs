using EasySave.Core.Models;
using Newtonsoft.Json;
using System;
using System.IO;

namespace EasySave.Core.Services
{
    public class EtatManager
    {
        private static readonly object _stateLock = new object();
        private readonly string _stateFile = "state.json";

        public void UpdateEtat(string jobName, string src, string dest, string state, int totalF, long totalS, int leftF, long leftS)
        {
            try
            {
                int prog = (totalF > 0) ? 100 - (leftF * 100 / totalF) : 100;
                
                ModelEtat etat = new ModelEtat()
                {
                    Name = jobName, 
                    SourceFile = src, 
                    TargetFile = dest, 
                    State = state,
                    TotalFiles = totalF, 
                    TotalSize = totalS, 
                    FilesLeft = leftF, 
                    SizeLeft = leftS,
                    Progression = prog, 
                    Timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:sss")
                };

                lock (_stateLock)
                {
                    File.WriteAllText(_stateFile, JsonConvert.SerializeObject(etat, Formatting.Indented));
                }
            }
            catch {}
        }

        public void ClearEtat(string jobName)
        {
            UpdateEtat(jobName, "", "", "INACTIF", 0, 0, 0, 0);
        }
    }
}