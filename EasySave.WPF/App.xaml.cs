using System.Threading;
using System.Windows;

namespace EasySave.WPF
{
    public partial class App : Application
    {
        private static Mutex? _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            _mutex = new Mutex(true, "EasySaveGlobalApplicationMutex", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("Une instance d'EasySave est déjà en cours d'exécution.", 
                                "Démarrage impossible", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Warning);
                
                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}