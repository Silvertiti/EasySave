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

            // --- TA LOGIQUE DE TEXTE EXACTE ---
            if (Lang.Msg.ContainsKey("MenuTitle"))
                Title = Lang.Msg["MenuTitle"].Replace("\n", "").Replace("-", "").Trim();
            else
                Title = "EasySave Dashboard";

            string rawAdd = Lang.Msg.ContainsKey("Add") ? Lang.Msg["Add"] : "Add";
            if (rawAdd.Contains(".")) rawAdd = rawAdd.Substring(rawAdd.IndexOf('.') + 1).Trim();
            BtnAddText = "➕  " + rawAdd;

            string rawRun = Lang.Msg.ContainsKey("Run") ? Lang.Msg["Run"] : "Run";
            if (rawRun.Contains("LANCER TOUTES"))
            {
                BtnRunText = "Tout Lancer";
            }
            else
            {
                if (rawRun.Contains(".")) rawRun = rawRun.Substring(rawRun.IndexOf('.') + 1).Trim();
                rawRun = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(rawRun.ToLower());
                BtnRunText = rawRun;
            }
        }

        // --- MÉTHODES ---

        public void RunAllSave()
        {
            _model.ExecuterSauvegarde((msg) => { });
            MessageBox.Show(Lang.Msg.ContainsKey("Success") ? Lang.Msg["Success"] : "Done", "EasySave");
        }

        public void DeleteJob(ModelJob jobToDelete)
        {
            var result = MessageBox.Show($"Delete {jobToDelete.Name} ?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                int index = _model.myJobs.IndexOf(jobToDelete);
                if (index >= 0)
                {
                    _model.DeleteJob(index);
                    JobsList.Remove(jobToDelete);
                }
            }
        }

        // --- C'EST ICI QUE J'AI AJOUTÉ LE NÉCESSAIRE ---
        public void CreateJob(string name, string source, string dest, bool isFull)
        {
            // 1. Ajoute au backend (SauvegardeModel + JSON)
            _model.AddJob(name, source, dest, isFull);

            // 2. Récupère le dernier objet créé par le modèle
            var newJob = _model.myJobs.Last();

            // 3. Ajoute à l'interface (ObservableCollection)
            JobsList.Add(newJob);
        }
    }
}