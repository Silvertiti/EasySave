using EasySave.Core.Services;
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

        // ── Fenêtre ──────────────────────────────────────────────────────────────────
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }
        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void BtnMaximize_Click(object sender, RoutedEventArgs e) =>
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        // ── Jobs ─────────────────────────────────────────────────────────────────────
        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            var f = new FenetreAjouterJob { Owner = this };
            if (f.ShowDialog() == true)
                _vm.CreateJob(f.JobName, f.SourcePath, f.TargetPath, f.IsFull);
        }
        private void OnRunAllClick(object sender, RoutedEventArgs e) => _vm.RunAllSave();
        private void OnRunJobClick(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.DataContext is Core.Models.ModelJob job)
                _vm.RunJob(job);
        }
        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.DataContext is Core.Models.ModelJob job)
                _vm.DeleteJob(job);
        }
        private void OnDeleteAllClick(object sender, RoutedEventArgs e) => _vm.DeleteAllJobs();
        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) { }

        // ── Serveur ──────────────────────────────────────────────────────────────────
        private void OnToggleServerClick(object sender, RoutedEventArgs e) => _vm.ToggleServer();

        // ── Client distant ────────────────────────────────────────────────────────────
        private string GetHost() => TxtRemoteHost.Text.Trim();
        private int GetPort() => int.TryParse(TxtRemotePort.Text, out int p) ? p : BackupServer.Port;

        private void ShowResponse(string response) => TxtClientResponse.Text = response;

        private void OnClientStatus(object sender, RoutedEventArgs e)  => ShowResponse(_vm.SendClientCommand(GetHost(), GetPort(), "STATUS"));
        private void OnClientList(object sender, RoutedEventArgs e)    => ShowResponse(_vm.SendClientCommand(GetHost(), GetPort(), "LIST"));
        private void OnClientRunAll(object sender, RoutedEventArgs e)  => ShowResponse(_vm.SendClientCommand(GetHost(), GetPort(), "RUN_ALL"));
        private void OnClientRunJob(object sender, RoutedEventArgs e)
        {
            string name = TxtJobName.Text.Trim();
            if (!string.IsNullOrEmpty(name))
                ShowResponse(_vm.SendClientCommand(GetHost(), GetPort(), $"RUN:{name}"));
        }
    }
}