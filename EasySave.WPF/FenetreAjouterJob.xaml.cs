using System.Windows;

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

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}