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
using ECA_Addin.UI.Popup_Windows;
using System.Threading.Tasks;
using System.Linq;

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

            if (uiapp == null)
            {
                Debug.WriteLine("UIApplication is null in Execute.");
            }
            else
            {
                Debug.WriteLine("UIApplication is valid in Execute.");
            }

            ECA_Addin.UI.Popup_Windows.Spool_Exchange window = new ECA_Addin.UI.Popup_Windows.Spool_Exchange(uiapp);
            window.Show();


            return Result.Succeeded;
        }
        public class CSVImporter
        {
            private string[,] importData;

            public string[,] ImportCSV()
            {
                string filePath = PromptForFile();
                
                try
                {
                    var lines = File.ReadAllLines(filePath);

                    if (lines.Length == 0)
                    {
                        Debug.WriteLine("The file is empty or invalid.");
                    }

                    // Parse the CSV
                    int rowCount = lines.Length; // Exclude the header row
                    int columnCount = lines[0].Split(',').Length;

                    // Initialize the 2D array
                    importData = new string[rowCount, columnCount];

                    for (int row = 0; row < rowCount; row++)
                    {
                        string[] values = lines[row].Split(',');
                        for (int col = 0; col < columnCount; col++)
                        {
                            importData[row, col] = col < values.Length ? values[col].Trim() : "";
                        }
                    }

                    return importData;


                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error reading file: {ex.Message}");
                    return null;

                }
            }

            private string PromptForFile()
            {
                var openFileDialog = new System.Windows.Forms.OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    Title = "Select a CSV File"
                };

                var result = openFileDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }

                return null;
            }
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

    public class UpdateElementsHandler : IExternalEventHandler
    {
        private string[,] _csvData;

        public void SetData(string[,] csvData)
        {
            _csvData = csvData;
        }

        [Obsolete]
        public void Execute(UIApplication uiapp)
        {
            if (_csvData == null)
            {
                TaskDialog.Show("Error", "No CSV data provided.");
                return;
            }

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (Transaction trans = new Transaction(doc, "Update Parameters from CSV"))
            {
                trans.Start();

                //Get row and column counts
                int rowCount = _csvData.GetLength(0);
                int columnCount = _csvData.GetLength(1);

                //Ensure header row and data
                if (rowCount < 2 || columnCount < 2)
                {
                    throw new InvalidOperationException("CSV File must contain a header row and at least one data row");
                }

                //Read Headers from the first row
                string[] headers = new string[columnCount];
                for (int col = 0; col < columnCount; col++)
                {
                    headers[col] = _csvData[0, col].Trim();
                }

                //Iterate through eV_SpoolIds
                for (int row = 1; row < rowCount; row++)
                {
                    String spoolId = _csvData[row, 0].Trim();
                    if (string.IsNullOrEmpty(spoolId))
                    {
                        Debug.WriteLine($"Skipping row {row}: eV_SpoolId is empty.");
                        continue;
                    }

                    Guid eV_SpoolIDGuid = new Guid("ff51f21e-c8b7-4c05-9207-10a393e499ac");

                    // Get the SharedParameterElement using the GUID
                    SharedParameterElement sharedParameter = SharedParameterElement.Lookup(doc, eV_SpoolIDGuid);
                    if (sharedParameter == null)
                    {
                        throw new Exception($"Shared parameter with GUID {eV_SpoolIDGuid} not found in the document.");
                    }

                    // Use the ElementId of the shared parameter
                    ElementId paramId = sharedParameter.Id;

                    // Create the FilteredElementCollector
                    FilteredElementCollector collector = new FilteredElementCollector(doc)
                        .WhereElementIsNotElementType()
                        .WherePasses(new LogicalOrFilter(new ElementFilter[]
                        {
                            new ElementCategoryFilter(BuiltInCategory.OST_Conduit),
                            new ElementCategoryFilter(BuiltInCategory.OST_ConduitFitting),
                            new ElementCategoryFilter(BuiltInCategory.OST_GenericModel),
                            new ElementCategoryFilter(BuiltInCategory.OST_Assemblies),
                            new ElementCategoryFilter(BuiltInCategory.OST_CableTray),
                            new ElementCategoryFilter(BuiltInCategory.OST_CableTrayFitting)
                        }))
                        .WherePasses(new ElementParameterFilter(ParameterFilterRuleFactory.CreateEqualsRule(
                            paramId,
                            spoolId,
                            false))); // false = case-insensitive comparison

                    foreach (Element element in collector)
                    {
                        for (int col = 1; col < columnCount; col++)
                        {
                            string parameterName = headers[col];
                            string parameterValue = _csvData[row, col]?.Trim();

                            if (string.IsNullOrEmpty(parameterName)) continue;

                            try
                            {
                                Parameter parameter = element.LookupParameter(parameterName);
                                if (parameter == null)
                                {
                                    Debug.WriteLine($"Parameter '{parameterName}' not found on element ID {element.Id}.");
                                    continue;
                                }

                                //Handle Storage Type
                                if (parameter.StorageType == StorageType.String)
                                {
                                    parameter.Set(parameterValue);
                                }
                                else if (parameter.StorageType == StorageType.Double)
                                {
                                    if (double.TryParse(parameterValue, out double doubleValue))
                                    {
                                        parameter.Set(doubleValue);
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"Invalid double value for parameter '{parameterName}' on element ID {element.Id}.");
                                    }
                                }
                                else if (parameter.StorageType == StorageType.Integer)
                                {
                                    if (int.TryParse(parameterValue, out int intValue))
                                    {
                                        parameter.Set(intValue);
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"Invalid integer value for parameter '{parameterName}' on element ID {element.Id}.");
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine($"Unsupported parameter type for '{parameterName}' on element ID {element.Id}.");
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error updating parameter '{parameterName}' on element ID {element.Id}");
                            }
                        }
                    }
                }
                trans.Commit();
            }

        }

        public string GetName()
        {
            return "Update Elements from CSV";
        }
    }

}