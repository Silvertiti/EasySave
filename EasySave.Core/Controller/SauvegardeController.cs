using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasySave.Core.Models;
using EasySave.Core.Services;

namespace EasySave.Core.Controller
{
    public class SauvegardeController
    {
        public List<ModelJob> myJobs;

        public bool IsPausedRequested { get; set; } = false;

        private readonly JobManager  _jobManager  = new JobManager();
        private readonly CopyService _copyService = new CopyService();

        public SauvegardeController() { myJobs = _jobManager.LoadData(); }

        public void AddJob(ModelJob newJob)
        {
            myJobs.Add(newJob);
            _jobManager.SaveData(myJobs);
        }

        public void DeleteJob(int index)
        {
            if (index >= 0 && index < myJobs.Count)
            {
                myJobs.RemoveAt(index);
                _jobManager.SaveData(myJobs);
            }
        }

        public void DeleteAllJobs()
        {
            myJobs.Clear();
            _jobManager.SaveData(myJobs);
        }

        public async Task ExecuterSauvegarde(Action<string> uiCallback)
        {
            var tasks = new List<Task>();
            foreach (var job in myJobs)
                tasks.Add(Task.Run(() => ExecuterUnSeulJob(job, uiCallback)));
            await Task.WhenAll(tasks);
        }
        public void ExecuterUnSeulJob(ModelJob job, Action<string> uiCallback)
        {
            _copyService.ExecuteJob(job, uiCallback);
        }
    }
}