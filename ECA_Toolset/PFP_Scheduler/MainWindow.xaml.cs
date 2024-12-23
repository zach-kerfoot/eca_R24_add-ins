 using System.Collections.Generic;
using System.Windows;

namespace PFP_Window
{
    public partial class MainWindow : Window
    {
        // Class to represent PFP Names
        public class PrefabPackage
        {
            public string PackageId { get; set; } // Column data for the left grid
        }

        public class ScheduleTemplates
        {

            public string TemplateID { get; set; }
        }

        public MainWindow(HashSet<string> pfpNames, HashSet<string> templateNames)
        {
            InitializeComponent();

            // Convert PFP names into data for the grid
            var pfpData = new List<PrefabPackage>();
            foreach (var name in pfpNames)
            {
                pfpData.Add(new PrefabPackage { PackageId = name });
            }

            var templateData = new List<ScheduleTemplates>();
            foreach (var template in templateNames)
            {
                templateData.Add(new ScheduleTemplates { TemplateID = template });
            }

            // Bind the data to the ResultsDataGrid
            PFPGrid.ItemsSource = pfpData;
            TemplateGrid.ItemsSource = templateData;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Close window with OK result
            this.Close();
        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Close window with OK result
            this.Close();
        }
        private void PFPRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Close window with OK result
            this.Close();
        }
        private void TemplateRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; // Close window with OK result
            this.Close();
        }
    }
}
