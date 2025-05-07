using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using ECA_Addin.UI.Popup_Windows;

namespace ECA_Addin
{
    [Transaction(TransactionMode.Manual)]
    public class Model_Clone : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;

            try
            {
                Reference linkRef = sel.PickObject(ObjectType.Element, "Select a link to copy from");
                Element linkElem = doc.GetElement(linkRef.ElementId);

                if (linkElem is RevitLinkInstance linkInst)
                {
                    string linkName = linkInst.Name;
                    //TaskDialog.Show("Selected Link", $"Select elements to copy from linked model: {linkName}");

                    IList<Reference> linkedRefs = sel.PickObjects(ObjectType.LinkedElement, $"Select elements from {linkName} to copy into active project");

                    Document linkDoc = linkInst.GetLinkDocument();
                    Transform tf = linkInst.GetTotalTransform();

                    List<ElementId> linkedElemIds = new List<ElementId>();

                    foreach (Reference r in linkedRefs)
                    {
                        Element linkedElem = linkDoc.GetElement(r.LinkedElementId);
                        if (linkedElem != null)
                        {
                            linkedElemIds.Add(r.LinkedElementId);
                        }
                    }

                    var groupedByType = linkedElemIds
                        .Select(id => linkDoc.GetElement(id))
                        .Where(e => e != null)
                        .GroupBy(e => e.GetTypeId());

                    FilteredElementCollector hostSymbols = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol));

                    List<TypeMappingViewModel> mappingViewModels = new List<TypeMappingViewModel>();

                    foreach(var group in groupedByType)
                    {
                        Element LinkedType = linkDoc.GetElement(group.Key);
                        if (LinkedType == null || LinkedType.Category == null)
                            continue;

                        string sourceName = LinkedType.Name;
                        BuiltInCategory category = (BuiltInCategory)LinkedType.Category.Id.Value;

                        List<FamilySymbol> hostOptions = hostSymbols
                            .Where(s => s.Category != null && s.Category.Id.Value == (int)category)
                            .Cast<FamilySymbol>()
                            .ToList();
                        mappingViewModels.Add(new TypeMappingViewModel
                        {
                            SourceTypeName = sourceName,
                            LinkedTypeId = group.Key,
                            HostTypeOptions = hostOptions,
                            SelectedHostType = hostOptions.FirstOrDefault()
                        });
                    }

                    var typeWindow = new TypeMappingWindow(mappingViewModels);
                    typeWindow.ShowDialog();

                    if (!typeWindow.Confirmed)
                        return Result.Cancelled;

                    Dictionary<ElementId, FamilySymbol> typeMap = mappingViewModels
                        .Where(m => m.SelectedHostType != null)
                        .ToDictionary(m => m.LinkedTypeId, m => m.SelectedHostType);

                    using (Transaction tx = new Transaction(doc, $"Copy Elements from {linkName}"))
                    {
                        tx.Start();

                        foreach (ElementId id in linkedElemIds)
                        {
                            Element linkedElem = linkDoc.GetElement(id);
                            if(linkedElem == null)
                                continue;

                            ElementId linkedTypeId = linkedElem.GetTypeId();
                            if (!typeMap.TryGetValue(linkedTypeId, out FamilySymbol replacement))
                                continue;

                            if(linkedElem == null) continue;

                            LocationPoint loc = linkedElem.Location as LocationPoint;
                            if (loc == null) continue;

                            XYZ linkPoint = loc.Point;
                            XYZ worldPoint = tf.OfPoint(linkPoint);

                            if (!replacement.IsActive)
                                replacement.Activate();

                            XYZ finalPoint = new XYZ(worldPoint.X, worldPoint.Y, worldPoint.Z);

                            doc.Create.NewFamilyInstance(finalPoint, replacement, StructuralType.NonStructural);

                        }

                        tx.Commit();
                    }

                    TaskDialog.Show($"Copy from {linkName}", $"{linkedElemIds.Count} elements copied from {linkName}.");
                }
                else
                {
                    TaskDialog.Show("Error", "The selected element is not a valid link in the active project.");
                    return Result.Failed;
                }

                return Result.Succeeded;
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { return Result.Failed; }

            catch (Exception ex) 
            {
                TaskDialog.Show("Error", ex.Message);
                return Result.Failed;
            }
        }
    }

    public class TypeMappingViewModel
    {
        public string SourceTypeName { get; set; }
        public ElementId LinkedTypeId { get; set; }
        public List<FamilySymbol> HostTypeOptions { get; set; }
        public FamilySymbol SelectedHostType { get; set; }
    }
}

