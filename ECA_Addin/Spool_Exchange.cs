#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Microsoft.Win32;
using System.Windows.Forms;

#endregion

namespace ECA_Addin
{
    [Transaction(TransactionMode.Manual)]
    public class Spool_Exchange : IExternalCommand
    {
        // HashSet to store unique eV_PackageId values
        public static HashSet<string> uniqueSpoolIDs = new HashSet<string>();

        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;



            // Collect elements in specific categories
            FilteredElementCollector SpoolCollector = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WherePasses(new LogicalOrFilter(new ElementFilter[]
                {
                new ElementCategoryFilter(BuiltInCategory.OST_Conduit),
                new ElementCategoryFilter(BuiltInCategory.OST_ConduitFitting),
                new ElementCategoryFilter(BuiltInCategory.OST_GenericModel),
                new ElementCategoryFilter(BuiltInCategory.OST_Assemblies)
                }));

            foreach (Element elem in SpoolCollector)
            {
                // Access the eV_PackageId parameter
                Parameter Param = elem.LookupParameter("eV_SpoolId");

                if (Param != null && Param.StorageType == StorageType.String)
                {
                    string paramValue = Param.AsString();

                    if (!string.IsNullOrEmpty(paramValue))
                    {

                        // Add unique values to HashSet
                        uniqueSpoolIDs.Add(paramValue);

                    }
                }
            }




            return Result.Succeeded;
        }


        public class CsvGenerator
        {
            // Property to store the HashSet
            public HashSet<string> DataSet { get; private set; }
            public String projectName{ get; private set; }

            // Constructor to initialize the HashSet
            public CsvGenerator(HashSet<string> dataSet)
            {
                DataSet = dataSet ?? throw new ArgumentNullException(nameof(dataSet), "DataSet cannot be null.");
            }

            // Method to generate the CSV file
            public void GenerateCsv(string filePath, string delimiter = ",")
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
                }

                // Use StringBuilder for efficiency
                StringBuilder csvContent = new StringBuilder();
                csvContent.AppendLine("eV_SpoolID");

                foreach (var item in DataSet)
                {
                    csvContent.AppendLine(item.Replace(delimiter, " ")); // Ensure delimiter safety
                }

                // Write to file
                File.WriteAllText(filePath, csvContent.ToString());
            }
            public void ExportCsv(string delimiter = ",")
            {
                using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog())
                {
                    string currentDate = DateTime.Now.ToString("yyyyMMdd");

                    saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
                    saveFileDialog.Title = "Save CSV File";
                    saveFileDialog.FileName = $"{currentDate}_output.csv";



                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        GenerateCsv(saveFileDialog.FileName, delimiter);
                        System.Windows.Forms.MessageBox.Show("CSV file exported successfully!", "Success", MessageBoxButtons.OK);
                    }
                }
            }
        }
    }
}