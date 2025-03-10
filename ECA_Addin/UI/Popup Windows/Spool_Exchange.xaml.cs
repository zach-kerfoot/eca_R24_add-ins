using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using static ECA_Addin.Spool_Exchange;

namespace ECA_Addin.UI.Popup_Windows
{
    /// <summary>
    /// Interaction logic for Spool_Exchange.xaml
    /// </summary>
    public partial class Spool_Exchange : Window
    {
        private ExternalEvent _externalEvent;
        private UpdateElementsHandler _handler;
        public Spool_Exchange(UIApplication uiapp)
        {
            InitializeComponent();

            if (uiapp == null)
            {
                Debug.WriteLine("UIApplication is null in Spool_Exchange constructor.");
            }
            else
            {
                Debug.WriteLine("UIApplication is valid in Spool_Exchange constructor.");
            }

            _handler = new UpdateElementsHandler();
            _externalEvent = ExternalEvent.Create(_handler);


        }

        private void ExportCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Access the global HashSet from the Spool_Exchange class
                List<string[]> spoolIDs = ECA_Addin.Spool_Exchange.spoolData;

                if (spoolIDs == null || spoolIDs.Count == 0)
                {
                    System.Windows.MessageBox.Show("No spool IDs found to export.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Instantiate CsvGenerator with the collected data
                ECA_Addin.CsvGenerator csvGenerator = new ECA_Addin.CsvGenerator(spoolIDs);

                // Export the CSV
                csvGenerator.ExportCsv();
                this.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportCSV_Click(Object sender, RoutedEventArgs e)
        {
            try
            {
                CSVImporter importer = new CSVImporter();
                string[,] csvData = importer.ImportCSV();

                if (csvData == null || csvData.Length == 0)
                {
                    System.Windows.MessageBox.Show("No valid data found in the CSV file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                _handler.SetData(csvData);
                Debug.WriteLine($"Handler is {_handler}");
                Debug.WriteLine($"ExternalEvent is {_externalEvent}");
                Debug.WriteLine("Setting data for handler...");
                _handler.SetData(csvData);
                Debug.WriteLine("Calling Raise...");
                _externalEvent.Raise();
                Debug.WriteLine("Raise called.");
                this.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }



        }

    }

}

