using EasySave.WPF.ViewModels;
using System.Windows;
using System.Windows.Input;

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

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            //TEMPORAIRE TODO
            MessageBox.Show("Work in progress: Fenêtre d'ajout bientôt disponible !", "EasySave");
        }

        private void OnRunAllClick(object sender, RoutedEventArgs e)
        {
            _vm.RunAllSave();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            var job = button.DataContext as EasySave.Core.Models.ModelJob;

            if (job != null)
            {
                _vm.DeleteJob(job);
            }
        }
    }
}