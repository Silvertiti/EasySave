using EasySave.Core.Models;
using EasySave.Core.Services;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace EasySave.WPF
{
    public partial class FenetreParametres : Window
    {
        private SettingsManager settingsManager = new SettingsManager();

        public FenetreParametres()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var _settings = settingsManager.GetSettings();

            TxtExtensions.Text = _settings.ExtensionsToEncrypt;
            TxtBusinessSoft.Text = _settings.BusinessSoftware;
            TxtCryptoPath.Text = _settings.CryptoSoftPath;

            if (_settings.LogFormat == "xml") RadioXml.IsChecked = true;
            else RadioJson.IsChecked = true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var _settings = settingsManager.GetSettings();

            _settings.ExtensionsToEncrypt = TxtExtensions.Text.Trim();
            _settings.BusinessSoftware = TxtBusinessSoft.Text.Trim();
            _settings.CryptoSoftPath = TxtCryptoPath.Text.Trim();
            _settings.LogFormat = (RadioXml.IsChecked == true) ? "xml" : "json";

            settingsManager.Save(_settings);

            MessageBox.Show("Paramètres sauvegardés avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Exécutables (*.exe)|*.exe";
            if (openFileDialog.ShowDialog() == true)
            {
                TxtCryptoPath.Text = openFileDialog.FileName;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnHeaderClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}