using EasySave.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;

namespace EasySave.WPF.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<ModelJob> JobsList { get; set; }
        public SauvegardeModel _model;

        public string Title { get; set; }
        public string BtnAddText { get; set; }
        public string BtnRunText { get; set; }

        public MainViewModel()
        {
            _model = new SauvegardeModel();
            _model.LoadData();
            JobsList = new ObservableCollection<ModelJob>(_model.myJobs);
            if (Lang.Msg.ContainsKey("MenuTitle"))
                Title = Lang.Msg["MenuTitle"].Replace("\n", "").Replace("-", "").Trim();
            else
                Title = "EasySave Dashboard";
            string rawAdd = Lang.Msg.ContainsKey("Add") ? Lang.Msg["Add"] : "Add";
            if (rawAdd.Contains(".")) rawAdd = rawAdd.Substring(rawAdd.IndexOf('.') + 1).Trim();
            BtnAddText = rawAdd;
            string rawRun = Lang.Msg.ContainsKey("Run") ? Lang.Msg["Run"] : "Run";
            if (rawRun.Contains(".")) rawRun = rawRun.Substring(rawRun.IndexOf('.') + 1).Trim();
            if (rawRun.ToUpper().Contains("LANCER")) rawRun = "Tout Lancer";

            BtnRunText = rawRun;
        }

        public void RunAllSave()
        {
            if (JobsList.Count == 0)
            {
                MessageBox.Show("Liste vide / Empty list", "Info");
                return;
            }

            _model.ExecuterSauvegarde((msg) =>
            {
                if (msg.Contains("Error") || msg.Contains("introuvable"))
                {
                    MessageBox.Show(msg, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
            MessageBox.Show("Terminé / Done", "Info");
        }

        public void RunJob(ModelJob job)
        {
            if (job == null) return;

            _model.ExecuterUnSeulJob(job, (msg) =>
            {
                if (msg.Contains("Error") || msg.Contains("introuvable"))
                {
                    MessageBox.Show(msg, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
            MessageBox.Show($"Job '{job.Name}' fini.", "Succès");
        }

        public void DeleteJob(ModelJob job)
        {
            var result = MessageBox.Show($"Supprimer {job.Name} ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                int index = _model.myJobs.IndexOf(job);
                if (index >= 0)
                {
                    _model.DeleteJob(index);
                    JobsList.Remove(job);
                }
            }
        }
        public void CreateJob(string name, string src, string dest, bool isFull)
        {
            var newJob = new ModelJob
            {
                Name = name,
                Source = src,
                Target = dest,
                IsFull = isFull
            };
            _model.AddJob(newJob);
            JobsList.Add(newJob);
        }
    }
}