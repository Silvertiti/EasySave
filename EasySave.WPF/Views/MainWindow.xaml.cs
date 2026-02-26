using EasySave.Core.Models;
using EasySave.WPF.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace EasySave.WPF
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

        private void BtnClose_Click(object sender, RoutedEventArgs e)
            => Close();

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            var fenetre = new FenetreAjouterJob();
            fenetre.ShowDialog();

            foreach (var job in ViewModel._model.myJobs)
            {
                if (!ViewModel.JobsList.Contains(job))
                    ViewModel.JobsList.Add(job);
            }
        }
        private void OnRunAllClick(object sender, RoutedEventArgs e)
            => ViewModel.RunAllSave();

        private void OnDeleteAllClick(object sender, RoutedEventArgs e)
            => ViewModel.DeleteAllJobs();

        private void OnToggleServerClick(object sender, RoutedEventArgs e)
            => ViewModel.ToggleServer();

        private void OnRunJobClick(object sender, RoutedEventArgs e)
        {
            if (GetJobFromSender(sender) is ModelJob job)
                ViewModel.RunJob(job);
        }

        private void OnStopJobClick(object sender, RoutedEventArgs e)
        {
            if (GetJobFromSender(sender) is ModelJob job)
                ViewModel.StopJob(job);
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (GetJobFromSender(sender) is ModelJob job)
                ViewModel.DeleteJob(job);
        }
        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ((System.Windows.Controls.ListBox)sender).SelectedItem = null;
        }
        private static ModelJob? GetJobFromSender(object sender)
            => (sender as FrameworkElement)?.DataContext as ModelJob;
    }
}