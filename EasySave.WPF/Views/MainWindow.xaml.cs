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

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            var fenetreAjout = new FenetreAjouterJob();
            fenetreAjout.Owner = this;
            bool? result = fenetreAjout.ShowDialog();
            if (result == true)
            {
                _vm.CreateJob(
                    fenetreAjout.JobName,
                    fenetreAjout.SourcePath,
                    fenetreAjout.TargetPath,
                    fenetreAjout.IsFull
                );
            }
        }

        private void OnRunAllClick(object sender, RoutedEventArgs e)
        {
            _vm.RunAllSave();
        }
        private void OnRunJobClick(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn &&
                btn.DataContext is Core.Models.ModelJob job)
            {
                _vm.RunJob(job);
            }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button &&
                button.DataContext is Core.Models.ModelJob job)
            {
                _vm.DeleteJob(job);
            }
        }

        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}