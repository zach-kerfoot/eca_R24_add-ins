#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

#endregion

namespace ECA_Addin
{
    [Transaction(TransactionMode.Manual)]
    public class Spool_Exchange : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // HashSet to store unique eV_PackageId values
            HashSet<string> uniquePackageIDs = new HashSet<string>();
            HashSet<string> uniqueTemplateIDs = new HashSet<string>();

            // Collect elements in specific categories
            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .WherePasses(new LogicalOrFilter(new ElementFilter[]
                {
                new ElementCategoryFilter(BuiltInCategory.OST_Conduit),
                new ElementCategoryFilter(BuiltInCategory.OST_ConduitFitting),
                new ElementCategoryFilter(BuiltInCategory.OST_GenericModel),
                new ElementCategoryFilter(BuiltInCategory.OST_Assemblies)
                }));

            foreach (Element elem in collector)
            {
                // Access the eV_PackageId parameter
                Parameter packageIdParam = elem.LookupParameter("eV_PackageId");

                if (packageIdParam != null && packageIdParam.StorageType == StorageType.String)
                {
                    string paramValue = packageIdParam.AsString();

                    if (!string.IsNullOrEmpty(paramValue))
                    {

                        // Add unique values to HashSet
                        uniquePackageIDs.Add(paramValue);

                    }
                }
            }

            TaskDialog.Show("Welcome", "Spool Exchange");

            return Result.Succeeded;
        }
    }
}