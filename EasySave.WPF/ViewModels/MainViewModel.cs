using EasySave.Core.Models;
using EasySave.Core.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using System.Threading.Tasks;
using EasySave.Core.Controller;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EasySave.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<ModelJob> JobsList { get; set; }
        public SauvegardeController _model;
        public string Title { get; set; }
        public string LblMenu { get; set; }
        public string BtnAddText { get; set; }
        public string BtnRunText { get; set; }
        public string BtnDeleteAllText { get; set; }
        public string BtnSettingsText { get; set; }

        // Serveur
        private BackupServer _server = new BackupServer();
        [ObservableProperty] private bool _isServerRunning = false;
        [ObservableProperty] private string _serverLog = "";

        // Progression Sauvegarde
        [ObservableProperty] private bool _isBackupRunning = false;
        [ObservableProperty] private int _backupProgress = 0; // 0 à 10
        [ObservableProperty] private string _backupStatusText = "";
        [ObservableProperty] private string _activeJobName = "";

        public MainViewModel()
        {
            _model = new SauvegardeController();
            JobsList = new ObservableCollection<ModelJob>(_model.myJobs);
            Title = GetTxt("MenuTitle", "EasySave Dashboard").Replace("\n", "").Replace("-", "").Trim();
            LblMenu = GetTxt("MenuLabel", "MENU");
            BtnAddText = CleanTranslation(GetTxt("Add", "Ajouter"));
            BtnRunText = CleanTranslation(GetTxt("Run", "Tout Lancer"));
            if (BtnRunText.ToUpper().Contains("LANCER")) BtnRunText = "Tout Lancer";
            BtnDeleteAllText = CleanTranslation(GetTxt("DeleteAll", "Tout Supprimer"));
            BtnSettingsText = "⚙  " + GetTxt("Settings", "Paramètres");

            _server.OnLog += msg => Application.Current.Dispatcher.Invoke(() => AppendLog(msg));

            // Synchronisation basique universelle en lisant state.json
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    try
                    {
                        if (File.Exists("state.json"))
                        {
                            var json = File.ReadAllText("state.json");
                            var etat = Newtonsoft.Json.JsonConvert.DeserializeObject<ModelEtat>(json);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (etat != null && etat.State != "INACTIF")
                                {
                                    IsBackupRunning = true;
                                    BackupProgress = etat.Progression / 10;
                                    ActiveJobName = etat.Name;
                                    
                                    var job = JobsList.FirstOrDefault(j => j.Name == etat.Name);
                                    if (job != null)
                                    {
                                        job.State = etat.State == "PAUSE" ? "PAUSED" : "RUNNING";
                                    }
                                }
                                else
                                {
                                    // Etat inactif -> s'assurer que les jobs sont arrêtés visuellement
                                    bool wasRunningExternally = false;
                                    foreach (var j in JobsList)
                                    {
                                        if (j.State != "STOPPED") 
                                        { 
                                            j.State = "STOPPED"; 
                                            wasRunningExternally = true; 
                                        }
                                    }

                                    // Si la tâche tournait mais sans passer par le bloc local "RunJob" final
                                    if (IsBackupRunning && wasRunningExternally && !BackupStatusText.Contains("terminé") && !BackupStatusText.Contains("Arrêt"))
                                    {
                                        BackupProgress = 10;
                                        BackupStatusText = "Sauvegarde terminée !";
                                        Task.Run(async () => { 
                                            await Task.Delay(2000); 
                                            Application.Current.Dispatcher.Invoke(() => { IsBackupRunning = false; ActiveJobName = ""; }); 
                                        });
                                    }
                                }
                            });
                        }
                    }
                    catch { }
                }
            });
        }

        public void ToggleServer()
        {
            if (_server.IsRunning)
            {
                _server.Stop();
                _server = new BackupServer();
                _server.OnLog += msg => Application.Current.Dispatcher.Invoke(() => AppendLog(msg));
                IsServerRunning = false;
            }
            else
            {
                _server.Start(_model);
                IsServerRunning = true;
            }
        }

        private void AppendLog(string msg)
        {
            var lines = (ServerLog + "\n" + msg).Split('\n');
            if (lines.Length > 20)
                ServerLog = string.Join("\n", lines[^20..]);
            else
                ServerLog = string.Join("\n", lines);
        }

        public string SendClientCommand(string host, int port, string command)
        {
            var client = new BackupClient(host, port);
            return client.SendCommand(command);
        }

        public async void RunAllSave()
        {
            if (JobsList.Count == 0) { MessageBox.Show("Liste vide", "Info"); return; }
            if (IsBackupRunning && !_model.IsPausedRequested) return;

            IsBackupRunning = true;
            _model.IsPausedRequested = false;
            BackupProgress = 0;
            BackupStatusText = "Initialisation...";

            await Task.Run(() =>
            {
                _model.ExecuterSauvegarde(msg =>
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        BackupStatusText = msg;

                        // Extraire le nom du job si le message commence par "Lancement : "
                        if (msg.StartsWith("Lancement : "))
                            ActiveJobName = msg.Replace("Lancement : ", "").Trim();

                        if (msg.Contains("STOP") || msg.Contains("INTERRUPTION"))
                            MessageBox.Show(msg, "Arrêt", MessageBoxButton.OK, MessageBoxImage.Warning);
                        else if (msg.Contains("ERREUR"))
                            MessageBox.Show(msg, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                });
            });

            ActiveJobName = "";
            BackupProgress = 10;
            BackupStatusText = "Sauvegarde terminée !";
            await Task.Delay(2000);
            IsBackupRunning = false;
        }

        public async void RunJob(ModelJob job)
        {
            if (job == null) return;

            if (job.State == "RUNNING")
            {
                _model.IsPausedRequested = true;
                return;
            }

            // Si une backup tourne, on interdit d'en lancer une nouvelle SAUF si on clique sur un job en pause pour le relancer
            if (IsBackupRunning && job.State != "PAUSED") return;

            IsBackupRunning = true;
            _model.IsPausedRequested = false;
            job.State = "RUNNING";
            BackupProgress = 0;
            BackupStatusText = $"Lancement de {job.Name}...";

            await Task.Run(() =>
            {
                _model.ExecuterUnSeulJob(job, msg =>
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        BackupStatusText = msg;
                        ActiveJobName = job.Name;

                        if (msg.Contains("STOP") || msg.Contains("INTERRUPTION"))
                            MessageBox.Show(msg, "Arrêt", MessageBoxButton.OK, MessageBoxImage.Warning);
                        else if (msg.Contains("ERREUR"))
                            MessageBox.Show(msg, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                });
            });

            ActiveJobName = "";
            bool wasPaused = _model.IsPausedRequested;
            job.State = wasPaused ? "PAUSED" : "STOPPED";

            if (wasPaused) 
            {
                 BackupStatusText = $"'{job.Name}' en pause.";
            } 
            else 
            {
                 BackupProgress = 10;
                 BackupStatusText = $"'{job.Name}' terminé avec succès.";
            }

            await Task.Delay(2000);
            IsBackupRunning = false;
        }

        public void DeleteJob(ModelJob job)
        {
            if (MessageBox.Show($"Supprimer {job.Name} ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                int i = _model.myJobs.IndexOf(job);
                if (i >= 0) { _model.DeleteJob(i); JobsList.Remove(job); }
            }
        }

        public void CreateJob(string name, string src, string dest, bool isFull)
        {
            var job = new ModelJob { Name = name, Source = src, Target = dest, IsFull = isFull };
            _model.AddJob(job);
            JobsList.Add(job);
        }

        public void DeleteAllJobs()
        {
            if (JobsList.Count == 0) return;
            if (MessageBox.Show("Supprimer tous les travaux ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            { _model.DeleteAllJobs(); JobsList.Clear(); }
        }

        [RelayCommand]
        public void OpenSettings()
        {
            new FenetreParametres().ShowDialog();
        }

        private string GetTxt(string key, string def) => LangGUI.Msg.ContainsKey(key) ? LangGUI.Msg[key] : def;
        private string CleanTranslation(string raw) => raw.Contains(".") ? raw.Substring(raw.IndexOf('.') + 1).Trim() : raw.Trim();
    }
}