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
            BtnAddText = "➕  " + rawAdd;

            string rawRun = Lang.Msg.ContainsKey("Run") ? Lang.Msg["Run"] : "Run";
            BtnRunText = rawRun.Contains("LANCER") ? "Tout Lancer" : "🚀  " + rawRun;
        }
        public void RunAllSave()
        {
            if (JobsList.Count == 0)
            {
                MessageBox.Show("Aucun travail dans la liste.", "Info");
                return;
            }
            _model.ExecuterSauvegarde((msg) =>
            {
                if (msg.Contains("Error") || msg.Contains("Missing") || msg.Contains("introuvable"))
                {
                    MessageBox.Show(msg, "Erreur Critique", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            MessageBox.Show("Tous les travaux ont été traités.", "Terminé");
        }
        public void RunJob(ModelJob job)
        {
            if (job == null) return;
            _model.ExecuterUnSeulJob(job, (msg) =>
            {
                if (msg.Contains("Error") || msg.Contains("Missing") || msg.Contains("introuvable"))
                {
                    MessageBox.Show(msg, "Erreur Critique", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            MessageBox.Show($"Travail '{job.Name}' terminé.", "Succès");
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
            _model.AddJob(name, src, dest, isFull);
            JobsList.Add(_model.myJobs.Last());
        }
    }
}