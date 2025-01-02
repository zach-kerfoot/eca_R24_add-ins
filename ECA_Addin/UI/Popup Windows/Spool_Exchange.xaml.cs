using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ECA_Addin.UI.Popup_Windows
{
    /// <summary>
    /// Interaction logic for Spool_Exchange.xaml
    /// </summary>
    public partial class Spool_Exchange : UserControl
    {
        public Spool_Exchange()
        {
            InitializeComponent();
        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Access the global HashSet from the Spool_Exchange class
                HashSet<string> spoolIDs = ECA_Addin.Spool_Exchange.uniqueSpoolIDs;

                if (spoolIDs == null || spoolIDs.Count == 0)
                {
                    System.Windows.MessageBox.Show("No spool IDs found to export.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Instantiate CsvGenerator with the collected data
                ECA_Addin.Spool_Exchange.CsvGenerator csvGenerator = new ECA_Addin.Spool_Exchange.CsvGenerator(spoolIDs);

                // Export the CSV
                csvGenerator.ExportCsv();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    
}

