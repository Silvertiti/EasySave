using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Services
{
    class BusinessSoftwareService
    {
        SettingsManager settingsManager = new SettingsManager();

        public bool IsBusinessSoftRunning()
        {
            try
            {
                var settings = settingsManager.GetSettings();
                string targetName = settings.BusinessSoftware;
                if (string.IsNullOrEmpty(targetName)) return false;
                if (targetName.ToLower().EndsWith(".exe")) targetName = targetName.Substring(0, targetName.Length - 4);

                Process[] processes = Process.GetProcesses();
                foreach (var p in processes)
                {
                    if (string.Equals(p.ProcessName, targetName, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }
            catch { return false; }
            return false;
        }
    }
}
