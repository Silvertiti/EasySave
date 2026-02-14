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
        public string LblMenu { get; set; }
        public string BtnAddText { get; set; }
        public string BtnRunText { get; set; }
        public string BtnDeleteAllText { get; set; }
        public string BtnSettingsText { get; set; }

        public MainViewModel()
        {
            _model = new SauvegardeModel();
            _model.LoadData();
            JobsList = new ObservableCollection<ModelJob>(_model.myJobs);

            // Initialisation des textes
            Title = GetTxt("MenuTitle", "EasySave Dashboard").Replace("\n", "").Replace("-", "").Trim();
            LblMenu = GetTxt("MenuLabel", "MENU");

            BtnAddText = CleanTranslation(GetTxt("Add", "Ajouter"));
            BtnRunText = CleanTranslation(GetTxt("Run", "Tout Lancer"));
            if (BtnRunText.ToUpper().Contains("LANCER")) BtnRunText = "Tout Lancer";

            BtnDeleteAllText = CleanTranslation(GetTxt("DeleteAll", "Tout Supprimer"));
            BtnSettingsText = "⚙  " + GetTxt("Settings", "Paramètres");
        }

        private string GetTxt(string key, string def)
            => Lang.Msg.ContainsKey(key) ? Lang.Msg[key] : def;

        private string CleanTranslation(string raw)
        {
            if (raw.Contains("."))
                return raw.Substring(raw.IndexOf('.') + 1).Trim();
            return raw.Trim();
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
                if (msg.Contains("STOP") || msg.Contains("INTERRUPTION"))
                {
                    MessageBox.Show(msg, "Arrêt Logiciel Métier", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (msg.Contains("Error") || msg.Contains("introuvable") || msg.Contains("ERREUR"))
                {
                    MessageBox.Show(msg, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }
        public void RunJob(ModelJob job)
        {
            if (job == null) return;

            _model.ExecuterUnSeulJob(job, (msg) =>
            {
                if (msg.Contains("STOP") || msg.Contains("INTERRUPTION"))
                {
                    MessageBox.Show(msg, "Arrêt Logiciel Métier", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (msg.Contains("Error") || msg.Contains("introuvable") || msg.Contains("ERREUR"))
                {
                    MessageBox.Show(msg, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (msg.Contains("Succès") || msg.Contains("Success"))
                {
                    MessageBox.Show($"Le travail '{job.Name}' a été effectué avec succès.", "Terminé", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });
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
            var newJob = new ModelJob { Name = name, Source = src, Target = dest, IsFull = isFull };
            _model.AddJob(newJob);
            JobsList.Add(newJob);
        }

        public void DeleteAllJobs()
        {
            if (JobsList.Count == 0) return;

            var result = MessageBox.Show(
                GetTxt("ConfirmDeleteAll", "Supprimer tous les travaux ?"),
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _model.myJobs.Clear();
                _model.SaveData();
                JobsList.Clear();
            }
        }
    }
}