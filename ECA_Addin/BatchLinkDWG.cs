using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;

namespace ECA_Addin
{
    [Transaction(TransactionMode.Manual)]
    public class BatchLinkDWGs : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select Folder Containing DWG Files";
                if (folderDialog.ShowDialog() != DialogResult.OK) return Result.Cancelled;

                string[] dwgFiles = Directory.GetFiles(folderDialog.SelectedPath, "*.dwg");

                if (dwgFiles.Length == 0)
                {
                    TaskDialog.Show("DWG Import", "No DWG files found in selected folder.");
                    return Result.Cancelled;
                }

                using (Transaction tx = new Transaction(doc, "Batch Link DWGs"))
                {
                    tx.Start();

                    foreach (var dwgPath in dwgFiles)
                    {
                        DWGImportOptions options = new DWGImportOptions
                        {
                            Placement = ImportPlacement.Origin,
                            ColorMode = ImportColorMode.BlackAndWhite,
                            Unit = ImportUnit.Default,
                            VisibleLayersOnly = true,
                            OrientToView = false
                        };

                        ElementId linkedId;
                        bool success = doc.Link(dwgPath, options, uidoc.ActiveView, out linkedId);

                        if (!success)
                        {
                            TaskDialog.Show("Link Failed", $"Could not link: {Path.GetFileName(dwgPath)}");
                        }
                    }

                    tx.Commit();
                }

                TaskDialog.Show("Success", "All DWG files linked.");
            }

            return Result.Succeeded;
        }
    }
}
