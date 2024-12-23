using System.Windows;

namespace MainWindow
{
    public partial class MainWindow : Window
    {
        // Class to represent DataGrid rows
        public class PackageIdData
        {
            public string ID { get; set; }
            public string PackageId { get; set; }
        }

        public MainWindow(List<PackageIdData> results)
        {
            InitializeComponent();
            ResultsDataGrid.ItemsSource = results;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Changes applied!", "Apply", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
