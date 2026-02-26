using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using EasySave.Core.Models;
using EasySave.Core.Services;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave.Core.Controller
{
    public class SauvegardeController
    {
        private static readonly object _stateLock    = new object();
        private static readonly object _largeFileLock = new object();
        private static readonly Mutex  _cryptoMutex  = new Mutex(false, "CryptoSoftGlobalMutex");
        private static int             _globalPriorityFilesCount = 0;
        private static readonly object _priorityLock = new object();

        public List<ModelJob> myJobs;

        // Gardé pour compatibilité avec le reste du code, mais plus utilisé en interne
        public bool IsPausedRequested { get; set; } = false;
        public bool IsStopRequested   { get; set; } = false;

        SettingsManager         settingsManager         = new SettingsManager();
        BusinessSoftwareService businessSoftwareService = new BusinessSoftwareService();
        JobManager              jobManager              = new JobManager();
        private string          stateFile               = "state.json";

        public SauvegardeController() { myJobs = jobManager.LoadData(); }

        public void AddJob(ModelJob newJob)  { myJobs.Add(newJob); jobManager.SaveData(myJobs); }
        public void DeleteJob(int index)
        {
            if (index >= 0 && index < myJobs.Count) { myJobs.RemoveAt(index); jobManager.SaveData(myJobs); }
        }
        public void DeleteAllJobs() { myJobs.Clear(); jobManager.SaveData(myJobs); }

        // ── Lancement de tous les jobs en parallèle ───────────────────────
        public async Task ExecuterSauvegarde(Action<string> uiCallback)
        {
            var tasks = myJobs.Select(job => Task.Run(() => ExecuterUnSeulJob(job, uiCallback)));
            await Task.WhenAll(tasks);
        }

        // ── Lancement d'un seul job ───────────────────────────────────────
        public void ExecuterUnSeulJob(ModelJob job, Action<string> uiCallback)
        {
            // Le controller ne touche PLUS à job.State : c'est le ViewModel qui gère l'UI.
            var settings = settingsManager.GetSettings();

            try
            {
                uiCallback("Lancement : " + job.Name);

                if (!Directory.Exists(job.Source))
                {
                    uiCallback("ERREUR : Source introuvable.");
                    return;
                }

                string[] allFiles = Directory.GetFiles(job.Source, "*.*", SearchOption.AllDirectories);
                int totalFiles = allFiles.Length;

                string prioExtStr = string.IsNullOrEmpty(settings.PrioritizedExtensions)
                    ? "pdf,txt" : settings.PrioritizedExtensions;
                List<string> priorityExtensions = prioExtStr.Split(',')
                    .Select(e => e.Trim().ToLower()).ToList();

                var priorityFiles = new List<string>();
                var normalFiles   = new List<string>();
                foreach (var file in allFiles)
                {
                    string ext = Path.GetExtension(file).Replace(".", "").ToLower();
                    if (priorityExtensions.Contains(ext)) priorityFiles.Add(file);
                    else normalFiles.Add(file);
                }

                lock (_priorityLock) { _globalPriorityFilesCount += priorityFiles.Count; }

                long totalSize = allFiles.Sum(f => new FileInfo(f).Length);
                int  filesLeft = totalFiles;
                long sizeLeft  = totalSize;

                UpdateEtat(job.Name, job.Source, job.Target, "ACTIF", totalFiles, totalSize, filesLeft, sizeLeft);

                List<string> extensionsToEncrypt = settings.ExtensionsToEncrypt
                    .Split(',').Select(e => e.Trim().ToLower()).ToList();

                void ProcessFile(string file, bool isPriority)
                {
                    // ── Arrêt immédiat (flag du job) ─────────────────────
                    if (job.IsStopRequested) return;

                    // ── Pause : on attend que le flag repasse à false ─────
                    while (job.IsPauseRequested)
                    {
                        if (job.IsStopRequested) return;
                        UpdateEtat(job.Name, file, "", "PAUSE", totalFiles, totalSize, filesLeft, sizeLeft);
                        Thread.Sleep(300);
                    }

                    // ── Attente logiciel métier ───────────────────────────
                    while (businessSoftwareService.IsBusinessSoftRunning())
                    {
                        if (job.IsStopRequested) return;
                        UpdateEtat(job.Name, file, "", "ATTENTE METIER", totalFiles, totalSize, filesLeft, sizeLeft);
                        Thread.Sleep(1000);
                    }

                    // ── Attente fichiers prioritaires ─────────────────────
                    if (!isPriority)
                    {
                        while (true)
                        {
                            lock (_priorityLock) { if (_globalPriorityFilesCount == 0) break; }
                            if (job.IsStopRequested) return;
                            UpdateEtat(job.Name, file, "", "ATTENTE PRIORITE", totalFiles, totalSize, filesLeft, sizeLeft);
                            Thread.Sleep(500);
                        }
                    }

                    string relatif = file.Replace(job.Source, "").TrimStart('\\');
                    string dest    = Path.Combine(job.Target, relatif);
                    long fileSize  = new FileInfo(file).Length;

                    // Sauvegarde différentielle : skip si destination plus récente
                    if (!job.IsFull && File.Exists(dest)
                        && File.GetLastWriteTime(file) <= File.GetLastWriteTime(dest))
                    {
                        filesLeft--; sizeLeft -= fileSize;
                        UpdateEtat(job.Name, file, dest, "ACTIF", totalFiles, totalSize, filesLeft, sizeLeft);
                        if (isPriority) lock (_priorityLock) { _globalPriorityFilesCount--; }
                        return;
                    }

                    string? dirDest = Path.GetDirectoryName(dest);
                    if (!string.IsNullOrEmpty(dirDest) && !Directory.Exists(dirDest))
                        Directory.CreateDirectory(dirDest);

                    UpdateEtat(job.Name, file, dest, "ACTIF", totalFiles, totalSize, filesLeft, sizeLeft);

                    Stopwatch sw             = Stopwatch.StartNew();
                    double    encryptionTime = 0;
                    bool      isLargeFile    = (settings.MaxParallelFileSizeKb > 0)
                                               && (fileSize > settings.MaxParallelFileSizeKb * 1024);
                    try
                    {
                        Action copyAndEncrypt = () =>
                        {
                            File.Copy(file, dest, true);
                            Thread.Sleep(2000);
                            string ext = Path.GetExtension(dest).ToLower().Replace(".", "");
                            if (extensionsToEncrypt.Contains(ext))
                                encryptionTime = ExecuteCryptoSoft(dest);
                        };

                        if (isLargeFile) lock (_largeFileLock) { copyAndEncrypt(); }
                        else copyAndEncrypt();

                        sw.Stop();
                        LogManager.SaveLog(job.Name, file, dest, fileSize,
                            sw.Elapsed.TotalMilliseconds, encryptionTime);
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        LogManager.SaveLog(job.Name, file, dest, fileSize, -1, -1);
                        uiCallback("Erreur : " + ex.Message);
                    }
                    finally
                    {
                        if (isPriority) lock (_priorityLock) { _globalPriorityFilesCount--; }
                    }

                    filesLeft--; sizeLeft -= fileSize;
                    uiCallback("Copié : " + relatif);
                }

                foreach (string file in priorityFiles) ProcessFile(file, true);
                foreach (string file in normalFiles)   ProcessFile(file, false);

                if (!job.IsStopRequested)
                {
                    UpdateEtat(job.Name, "", "", "INACTIF", totalFiles, totalSize, 0, 0);
                    uiCallback("Succès : " + job.Name);
                }
            }
            catch (Exception ex)
            {
                uiCallback("ERREUR CRITIQUE : " + ex.Message);
            }
        }

        private double ExecuteCryptoSoft(string sourceFilePath)
        {
            try
            {
                var settings = settingsManager.GetSettings();
                if (string.IsNullOrEmpty(settings.CryptoSoftPath) || !File.Exists(settings.CryptoSoftPath))
                    return -1;

                Stopwatch sw = Stopwatch.StartNew();
                var startInfo = new ProcessStartInfo
                {
                    FileName        = settings.CryptoSoftPath,
                    Arguments       = $"\"{sourceFilePath}\" \"EasyKey\"",
                    CreateNoWindow  = true,
                    UseShellExecute = false
                };

                _cryptoMutex.WaitOne();
                try   { using var p = Process.Start(startInfo); p.WaitForExit(); }
                finally { _cryptoMutex.ReleaseMutex(); }

                sw.Stop();
                return sw.Elapsed.TotalMilliseconds;
            }
            catch { return -1; }
        }

        private void UpdateEtat(string jobName, string src, string dest, string state,
            int totalF, long totalS, int leftF, long leftS)
        {
            try
            {
                int prog = (totalF > 0) ? 100 - (leftF * 100 / totalF) : 100;
                var etat = new ModelEtat
                {
                    Name = jobName, SourceFile = src, TargetFile = dest, State = state,
                    TotalFiles = totalF, TotalSize = totalS, FilesLeft = leftF, SizeLeft = leftS,
                    Progression = prog, Timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                };
                lock (_stateLock)
                    File.WriteAllText(stateFile, JsonConvert.SerializeObject(etat, Formatting.Indented));
            }
            catch { }
        }
    }
}