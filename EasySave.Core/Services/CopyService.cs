using EasySave.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace EasySave.Core.Services
{
    public class CopyService
    {
        private static readonly object _largeFileLock = new object();
        private static readonly Mutex _cryptoMutex = new Mutex(false, "CryptoSoftGlobalMutex");
        private static readonly object _priorityLock = new object();
        private static int _globalPriorityFilesCount = 0;

        private readonly SettingsManager _settingsManager = new SettingsManager();
        private readonly BusinessSoftwareService _businessSoftwareService = new BusinessSoftwareService();
        private readonly EtatManager _etatManager = new EtatManager();
        public void ExecuteJob(ModelJob job, Action<string> uiCallback)
        {
            job.IsStopRequested  = false;
            job.IsPauseRequested = false;

            var settings = _settingsManager.GetSettings();

            try
            {
                uiCallback("Lancement : " + job.Name);

                if (!Directory.Exists(job.Source))
                {
                    uiCallback("ERREUR : Source introuvable.");
                    job.State = "ERREUR";
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

                _etatManager.UpdateEtat(job.Name, job.Source, job.Target, "ACTIF",
                    totalFiles, totalSize, filesLeft, sizeLeft);

                List<string> extensionsToEncrypt = settings.ExtensionsToEncrypt
                    .Split(',').Select(e => e.Trim().ToLower()).ToList();

                void ProcessFile(string file, bool isPriority)
                {
                    if (job.IsStopRequested) return;
                    if (job.IsPauseRequested)
                    {
                        job.State = "PAUSED";
                        while (job.IsPauseRequested)
                        {
                            if (job.IsStopRequested) return;
                            _etatManager.UpdateEtat(job.Name, file, "", "PAUSE",
                                totalFiles, totalSize, filesLeft, sizeLeft);
                            Thread.Sleep(300);
                        }
                        if (!job.IsStopRequested) job.State = "RUNNING";
                    }
                    if (_businessSoftwareService.IsBusinessSoftRunning())
                    {
                        job.State = "PAUSED";
                        while (_businessSoftwareService.IsBusinessSoftRunning())
                        {
                            if (job.IsStopRequested) return;
                            _etatManager.UpdateEtat(job.Name, file, "", "ATTENTE METIER",
                                totalFiles, totalSize, filesLeft, sizeLeft);
                            Thread.Sleep(1000);
                        }
                       
                        if (!job.IsPauseRequested && !job.IsStopRequested)
                            job.State = "RUNNING";
                    }

                
                    if (!isPriority)
                    {
                        bool wasWaiting = false;
                        while (true)
                        {
                            lock (_priorityLock) { if (_globalPriorityFilesCount == 0) break; }
                            if (job.IsStopRequested) return;
                            if (!wasWaiting)
                            {
                                job.State  = "PAUSED";
                                wasWaiting = true;
                            }
                            _etatManager.UpdateEtat(job.Name, file, "", "ATTENTE PRIORITE",
                                totalFiles, totalSize, filesLeft, sizeLeft);
                            Thread.Sleep(500);
                        }
                        if (wasWaiting && !job.IsPauseRequested && !job.IsStopRequested)
                            job.State = "RUNNING";
                    }

                    string relatif = file.Replace(job.Source, "").TrimStart('\\');
                    string dest    = Path.Combine(job.Target, relatif);
                    long fileSize  = new FileInfo(file).Length;
                    if (!job.IsFull && File.Exists(dest)
                        && File.GetLastWriteTime(file) <= File.GetLastWriteTime(dest))
                    {
                        filesLeft--; sizeLeft -= fileSize;
                        _etatManager.UpdateEtat(job.Name, file, dest, "ACTIF",
                            totalFiles, totalSize, filesLeft, sizeLeft);
                        if (isPriority) lock (_priorityLock) { _globalPriorityFilesCount--; }
                        return;
                    }

                    string? dirDest = Path.GetDirectoryName(dest);
                    if (!string.IsNullOrEmpty(dirDest) && !Directory.Exists(dirDest))
                        Directory.CreateDirectory(dirDest);

                    _etatManager.UpdateEtat(job.Name, file, dest, "ACTIF",
                        totalFiles, totalSize, filesLeft, sizeLeft);

                    Stopwatch sw             = Stopwatch.StartNew();
                    double    encryptionTime = 0;
                    bool      isLargeFile    = (settings.MaxParallelFileSizeKb > 0)
                                               && (fileSize > settings.MaxParallelFileSizeKb * 1024);
                    try
                    {
                        Action copyAndEncrypt = () =>
                        {
                            File.Copy(file, dest, true);
                            Thread.Sleep(2000); // Prouver copie fichier
                            string ext = Path.GetExtension(dest).ToLower().Replace(".", "");
                            if (extensionsToEncrypt.Contains(ext))
                                encryptionTime = ExecuteCryptoSoft(dest, settings.CryptoSoftPath);
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

                foreach (string file in priorityFiles)
                {
                    if (job.IsStopRequested) break;
                    ProcessFile(file, true);
                }
                foreach (string file in normalFiles)
                {
                    if (job.IsStopRequested) break;
                    ProcessFile(file, false);
                }

                _etatManager.ClearEtat(job.Name);
                uiCallback(job.IsStopRequested ? "STOP : " + job.Name : "Succès : " + job.Name);
            }
            catch (Exception ex)
            {
                uiCallback("ERREUR CRITIQUE : " + ex.Message);
            }
        }

        private double ExecuteCryptoSoft(string sourceFilePath, string cryptoPath)
        {
            try
            {
                if (string.IsNullOrEmpty(cryptoPath) || !File.Exists(cryptoPath)) return -1;

                Stopwatch sw = Stopwatch.StartNew();
                var startInfo = new ProcessStartInfo
                {
                    FileName        = cryptoPath,
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
    }
}