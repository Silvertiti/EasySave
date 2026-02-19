using System.Windows;
using System.Windows.Input;
using EasySave.Core.Models;

namespace EasySave.WPF.Views
{ 
    public partial class LanguageWindow : Window
    {
        public LanguageWindow()
        {
            InitializeComponent();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnFR_Click(object sender, RoutedEventArgs e)
        {
            StartApplication("fr");
        }

        private void BtnEN_Click(object sender, RoutedEventArgs e)
        {
            StartApplication("en");
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void StartApplication(string culture)
        {
            LangGUI.Init(culture);
            MainWindow main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}