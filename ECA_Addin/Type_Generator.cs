using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;

namespace ECA_Addin
{
    [Transaction(TransactionMode.Manual)]
    internal class Type_Generator : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string messgage, ElementSet elemnts)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            if(!doc.IsFamilyDocument)
            {
                TaskDialog.Show("Type Generator", "This tool is build for only families, open a family in the Family Editor");
                return Result.Cancelled;
            }

            string filepath = PromptForCSV();
            if(filepath == null ) return Result.Cancelled;

            var lines = File.ReadAllLines(filepath);
            if (lines.Length < 2) return Result.Cancelled;

            var headers = lines[0].Split(',');
            var typeNames = new HashSet<string>();

            var famMgr = doc.FamilyManager;

            using(Transaction tx = new Transaction(doc, "Generate Family Types"))
            {
                tx.Start();

                foreach(var line in lines.Skip(1))
                {
                    var values = line.Split(',');
                    string typeName = values[0];

                    if (typeNames.Contains(typeName) || famMgr.Types.Cast<FamilyType>().Any(t => t.Name == typeName))
                        continue;

                    FamilyType newType = famMgr.NewType(typeName);
                    for (int  i = 1; i < headers.Length; i++)
                    {
                        FamilyParameter param = famMgr.get_Parameter(headers[i]);
                        if (param != null)
                        {
                            SetParameterValue(famMgr, param, values[i]);
                        }

                    }
                    typeNames.Add(typeName);
                }

                tx.Commit();
            }

            TaskDialog.Show("Type Generator", $"{typeNames.Count} type(s) created.");
            return Result.Succeeded;

        }

        private void SetParameterValue(FamilyManager famMgr, FamilyParameter param, string value)
        {
            switch (param.StorageType)
            {
                case StorageType.String:
                    famMgr.Set(param, value);
                    break;

                case StorageType.Double:
                    if(double.TryParse(value, out double dVal))
                        famMgr.Set(param, dVal);
                    break;

                case StorageType.Integer:
                    if (int.TryParse(value, out int iVal))
                        famMgr.Set(param, iVal);
                    break;

                case StorageType.ElementId:
                    TaskDialog.Show("ERROR", "This type is not yet supported");
                    break;


            }
        }

        private string PromptForCSV()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select CSV File";
            openFileDialog.Filter = "CSV files (*.csv)|*csv|All files (*.*)|*.*";
            openFileDialog.Multiselect = false;
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (openFileDialog.ShowDialog() == DialogResult.OK && File.Exists(openFileDialog.FileName))
            {
                return openFileDialog.FileName;
            }

            TaskDialog.Show("Type Generator", "No valid file selected.");
            return null;
        }

    }

    

}
