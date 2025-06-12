using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using ECA_Addin;
using System.Windows.Forms;
using System.IO;

namespace ECA_Addin
{
    [Transaction(TransactionMode.Manual)]
    public class Dependent_View_Gen : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            Reference pickedRef = uidoc.Selection.PickObject(ObjectType.Element, new PlanViewFilter(), "Select a plan view to split.");
            ViewPlan parentView = doc.GetElement(pickedRef) as ViewPlan;
            if (parentView == null)
            {
                TaskDialog.Show("Error", "Selection is not a plan view.");
                return Result.Failed;
            }

            string path = GetExcelFilePath();
            if (string.IsNullOrEmpty(path)) return Result.Cancelled;

            List<ViewGridRegion> entries = CsvReader.ReadViewGridData(path);
            if (entries.Count == 0) return Result.Failed;

            var grids = new FilteredElementCollector(doc, parentView.Id)
                .OfClass(typeof(Grid))
                .Cast<Grid>()
                .ToDictionary(g => g.Name, g => g);

            using (Transaction t = new Transaction(doc, "Create Dependent Views"))
            {
                t.Start();

                foreach (var entry in entries)
                {
                    if (!grids.TryGetValue(entry.GridStartX, out Grid gX1) ||
                        !grids.TryGetValue(entry.GridEndX, out Grid gX2) ||
                        !grids.TryGetValue(entry.GridStartY, out Grid gY1) ||
                        !grids.TryGetValue(entry.GridEndY, out Grid gY2))
                    {
                        TaskDialog.Show("Missing Grid", $"Grids for view '{entry.Name}' not found.");
                        continue;
                    }

                    BoundingBoxXYZ bboxX = GridUtils.GetExtents(gX1, gX2);
                    BoundingBoxXYZ bboxY = GridUtils.GetExtents(gY1, gY2);

                    XYZ min = new XYZ(bboxX.Min.X, bboxY.Min.Y, parentView.CropBox.Min.Z);
                    XYZ max = new XYZ(bboxX.Max.X, bboxY.Max.Y, parentView.CropBox.Max.Z);

                    Autodesk.Revit.DB.View dependent = doc.GetElement(parentView.Duplicate(ViewDuplicateOption.AsDependent)) as Autodesk.Revit.DB.View;
                    dependent.Name = $"{parentView.Name} - {entry.Name}";

                    BoundingBoxXYZ cropBox = new BoundingBoxXYZ
                    {
                        Min = min,
                        Max = max
                    };

                    dependent.CropBox = cropBox;
                    dependent.CropBoxActive = true;
                    dependent.CropBoxVisible = false;
                }

                t.Commit();
            }

            TaskDialog.Show("Success", $"Created {entries.Count} dependent views.");
            return Result.Succeeded;
        }

        private string GetExcelFilePath()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls",
                Title = "Select Excel File with Grid Region Definitions"
            };
            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
        }
    }

    public class ViewGridRegion
    {
        public string Name;
        public string GridStartX;
        public string GridEndX;
        public string GridStartY;
        public string GridEndY;
    }

    public class PlanViewFilter : ISelectionFilter
    {
        public bool AllowElement(Element e) => e is ViewPlan && !(e as ViewPlan).IsTemplate;
        public bool AllowReference(Reference r, XYZ p) => true;
    }
    public static class GridUtils
    {
        public static BoundingBoxXYZ GetExtents(Grid g1, Grid g2)
        {
            Curve c1 = g1.Curve;
            Curve c2 = g2.Curve;

            XYZ p1a = c1.GetEndPoint(0);
            XYZ p1b = c1.GetEndPoint(1);
            XYZ p2a = c2.GetEndPoint(0);
            XYZ p2b = c2.GetEndPoint(1);

            double minX = Math.Min(Math.Min(p1a.X, p1b.X), Math.Min(p2a.X, p2b.X));
            double maxX = Math.Max(Math.Max(p1a.X, p1b.X), Math.Max(p2a.X, p2b.X));
            double minY = Math.Min(Math.Min(p1a.Y, p1b.Y), Math.Min(p2a.Y, p2b.Y));
            double maxY = Math.Max(Math.Max(p1a.Y, p1b.Y), Math.Max(p2a.Y, p2b.Y));

            return new BoundingBoxXYZ
            {
                Min = new XYZ(minX, minY, 0),
                Max = new XYZ(maxX, maxY, 0)
            };
        }
    }
    public static class CsvReader
    {
        public static List<ViewGridRegion> ReadViewGridData(string path)
        {
            var list = new List<ViewGridRegion>();
            var lines = File.ReadAllLines(path);

            for (int i = 1; i < lines.Length; i++) // skip header
            {
                var parts = lines[i].Split(',');

                if (parts.Length < 5)
                    continue;

                list.Add(new ViewGridRegion
                {
                    Name = parts[0].Trim(),
                    GridStartX = parts[1].Trim(),
                    GridEndX = parts[2].Trim(),
                    GridStartY = parts[3].Trim(),
                    GridEndY = parts[4].Trim()
                });
            }

            return list;
        }
    }
}
