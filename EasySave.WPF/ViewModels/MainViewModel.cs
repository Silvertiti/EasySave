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
        public string Title          { get; set; }
        public string LblMenu        { get; set; }
        public string BtnAddText     { get; set; }
        public string BtnRunText     { get; set; }
        public string BtnDeleteAllText { get; set; }
        public string BtnSettingsText  { get; set; }

        private BackupServer _server = new BackupServer();
        [ObservableProperty] private bool   _isServerRunning  = false;
        [ObservableProperty] private string _serverLog        = "";
        [ObservableProperty] private bool   _isBackupRunning  = false;
        [ObservableProperty] private int    _backupProgress   = 0;
        [ObservableProperty] private string _backupStatusText = "";
        [ObservableProperty] private string _activeJobName    = "";

        public MainViewModel()
        {
            _model    = new SauvegardeController();
            JobsList  = new ObservableCollection<ModelJob>(_model.myJobs);
            Title     = GetTxt("MenuTitle", "EasySave Dashboard").Replace("\n","").Replace("-","").Trim();
            LblMenu   = GetTxt("MenuLabel", "MENU");
            BtnAddText     = CleanTranslation(GetTxt("Add",       "Ajouter"));
            BtnRunText     = CleanTranslation(GetTxt("Run",       "Tout Lancer"));
            if (BtnRunText.ToUpper().Contains("LANCER")) BtnRunText = "Tout Lancer";
            BtnDeleteAllText = CleanTranslation(GetTxt("DeleteAll", "Tout Supprimer"));
            BtnSettingsText  = "⚙  " + GetTxt("Settings", "Paramètres");

            _server.OnLog += msg => Application.Current.Dispatcher.Invoke(() => AppendLog(msg));

            // Polling state.json uniquement pour la barre de progression globale
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
                                    BackupProgress = etat.Progression / 10;
                                    ActiveJobName  = etat.Name;
                                }
                            });
                        }
                    }
                    catch { }
                }
            });
        }

        // ── Serveur ───────────────────────────────────────────────────────
        public void ToggleServer()
        {
            if (_server.IsRunning)
            {
                _server.Stop();
                _server = new BackupServer();
                _server.OnLog += msg => Application.Current.Dispatcher.Invoke(() => AppendLog(msg));
                IsServerRunning = false;
            }
            else { _server.Start(_model); IsServerRunning = true; }
        }

        private void AppendLog(string msg)
        {
            var lines = (ServerLog + "\n" + msg).Split('\n');
            ServerLog = string.Join("\n", lines.Length > 20 ? lines[^20..] : lines);
        }

        public string SendClientCommand(string host, int port, string command)
            => new BackupClient(host, port).SendCommand(command);

        // ── Tout lancer ───────────────────────────────────────────────────
        public async void RunAllSave()
        {
            if (JobsList.Count == 0) { MessageBox.Show("Liste vide", "Info"); return; }

            // Réinitialiser les flags de chaque job et les marquer RUNNING
            foreach (var j in JobsList)
            {
                j.IsPauseRequested = false;
                j.IsStopRequested  = false;
                j.State            = "RUNNING";
            }

            IsBackupRunning  = true;
            BackupProgress   = 0;
            BackupStatusText = "Lancement de tous les jobs...";

            // On lance chaque job dans sa propre tâche pour pouvoir les contrôler indépendamment
            var tasks = JobsList.Select(job => Task.Run(() =>
                _model.ExecuterUnSeulJob(job, msg =>
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        BackupStatusText = msg;
                        if (msg.Contains("ERREUR"))
                            MessageBox.Show(msg, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    })
                )
            )).ToList();

            await Task.WhenAll(tasks);

            // Fin : mettre à jour l'état visuel de chaque job
            foreach (var j in JobsList)
            {
                if (j.State == "RUNNING") // pas encore modifié par Stop/Pause
                    j.State = "STOPPED";
            }

            ActiveJobName    = "";
            BackupProgress   = 10;
            BackupStatusText = "Tous les jobs terminés !";
            await Task.Delay(2000);
            IsBackupRunning  = false;
            BackupStatusText = "";
        }

        // ── Lancer / Pause / Reprendre un job individuel ──────────────────
        public async void RunJob(ModelJob job)
        {
            if (job == null) return;

            // RUNNING → Pause
            if (job.State == "RUNNING")
            {
                job.IsPauseRequested = true;
                job.State            = "PAUSED";
                BackupStatusText     = $"'{job.Name}' en pause.";
                return;
            }

            // PAUSED → Reprise
            if (job.State == "PAUSED")
            {
                job.IsPauseRequested = false;
                job.IsStopRequested  = false;
                job.State            = "RUNNING";
                BackupStatusText     = $"Reprise de '{job.Name}'...";
                // Le thread du controller est encore en vie et attend IsPauseRequested == false
                // → il reprend automatiquement, pas besoin de relancer ExecuterUnSeulJob
                return;
            }

            // STOPPED → Démarrage normal
            // Si un job solo tourne déjà, on bloque
            if (IsBackupRunning) return;

            IsBackupRunning      = true;
            job.IsPauseRequested = false;
            job.IsStopRequested  = false;
            job.State            = "RUNNING";
            BackupProgress       = 0;
            BackupStatusText     = $"Lancement de '{job.Name}'...";
            ActiveJobName        = job.Name;

            await Task.Run(() =>
                _model.ExecuterUnSeulJob(job, msg =>
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        BackupStatusText = msg;
                        if (msg.Contains("ERREUR"))
                            MessageBox.Show(msg, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    })
                )
            );

            // État final
            if (job.IsStopRequested)
            {
                job.State        = "STOPPED";
                job.IsStopRequested = false;
                BackupStatusText = $"'{job.Name}' arrêté.";
            }
            else
            {
                job.State        = "STOPPED";
                BackupProgress   = 10;
                BackupStatusText = $"'{job.Name}' terminé avec succès.";
                await Task.Delay(2000);
                BackupStatusText = "";
            }

            ActiveJobName   = "";
            IsBackupRunning = false;
        }

        // ── Stop immédiat d'un job (bouton Stop rouge) ────────────────────
        public void StopJob(ModelJob job)
        {
            if (job == null) return;
            if (job.State != "RUNNING" && job.State != "PAUSED") return;

            job.IsStopRequested  = true;
            job.IsPauseRequested = false;
            job.State            = "STOPPED";
            BackupStatusText     = $"'{job.Name}' arrêté.";

            // Si c'était le seul job solo en cours, libérer le verrou global
            bool anyStillRunning = JobsList.Any(j => j.State == "RUNNING" || j.State == "PAUSED");
            if (!anyStillRunning)
            {
                IsBackupRunning = false;
                ActiveJobName   = "";
            }
        }

        // ── CRUD jobs ─────────────────────────────────────────────────────
        public void DeleteJob(ModelJob job)
        {
            if (MessageBox.Show($"Supprimer {job.Name} ?", "Confirmation", MessageBoxButton.YesNo)
                == MessageBoxResult.Yes)
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
            if (MessageBox.Show("Supprimer tous les travaux ?", "Confirmation", MessageBoxButton.YesNo)
                == MessageBoxResult.Yes)
            { _model.DeleteAllJobs(); JobsList.Clear(); }
        }

        [RelayCommand]
        public void OpenSettings() => new FenetreParametres().ShowDialog();

        private string GetTxt(string key, string def)
            => LangGUI.Msg.ContainsKey(key) ? LangGUI.Msg[key] : def;
        private string CleanTranslation(string raw)
            => raw.Contains(".") ? raw.Substring(raw.IndexOf('.') + 1).Trim() : raw.Trim();
    }
}