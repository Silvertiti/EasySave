using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace EasySave.WPF
{
    public partial class FenetreAjouterJob : Window
    {
        public string JobName { get; private set; } = string.Empty;
        public string SourcePath { get; private set; } = string.Empty;
        public string TargetPath { get; private set; } = string.Empty;
        public bool IsFull { get; private set; } = true;

        public FenetreAjouterJob()
        {
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text) ||
                string.IsNullOrWhiteSpace(TxtSource.Text) ||
                string.IsNullOrWhiteSpace(TxtTarget.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            JobName = TxtName.Text;
            SourcePath = TxtSource.Text;
            TargetPath = TxtTarget.Text;
            IsFull = RadioFull.IsChecked == true;
            this.DialogResult = true;
            this.Close();
        }

        private void BtnBrowseSource_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Sélectionner le dossier source";
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == true)
            {
                TxtSource.Text = dialog.FolderName;
            }
        }
        private void BtnBrowseTarget_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Sélectionner le dossier cible";
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == true)
            {
                TxtTarget.Text = dialog.FolderName;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnHeaderClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}