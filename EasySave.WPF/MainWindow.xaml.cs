using EasySave.WPF.ViewModels;
using System.Windows;
using System.Windows.Input; // Nécessaire pour MouseButtonEventArgs

namespace EasySave.WPF
{
    public partial class MainWindow : Window
    {
        private MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            this.DataContext = _vm;
        }

        // --- GESTION DE LA FENÊTRE ---

        // Permet de bouger la fenêtre en cliquant n'importe où
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // Minimiser
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Fermer
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        //Maximiser la fenêtre
        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        // --- ACTIONS MÉTIER ---

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Work in progress: Fenêtre d'ajout bientôt disponible !", "EasySave");
            // Ici tu mettras plus tard : new CreateJobWindow().ShowDialog();
        }

        private void OnRunAllClick(object sender, RoutedEventArgs e)
        {
            _vm.RunAllSave();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            // On récupère l'élément cliqué
            var button = sender as System.Windows.Controls.Button;
            var job = button.DataContext as EasySave.Core.Models.ModelJob;

            if (job != null)
            {
                _vm.DeleteJob(job);
            }
        }
    }
}