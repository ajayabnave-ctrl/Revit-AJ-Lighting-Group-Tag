using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace AJ.LightingGroupTag.Selection
{
    /// <summary>
    /// Service responsible for handling user selection of multiple lighting fixtures in Revit.
    /// </summary>
    public class FixtureSelectionService
    {
        public static List<FamilyInstance> PickFixtures(UIDocument uidoc, ElementId? requiredSymbolId = null)
        {
            if (uidoc == null)
                throw new ArgumentNullException(nameof(uidoc));

            var results = new List<FamilyInstance>();
            var filter = new LightingFixtureSelectionFilter(requiredSymbolId);

            try
            {
                IList<Reference> pickedRefs = uidoc.Selection.PickObjects(
                    ObjectType.Element,
                    filter,
                    "Select lighting fixtures for group tagging, then click Finish."
                );

                Document doc = uidoc.Document;
                foreach (Reference r in pickedRefs)
                {
                    Element elem = doc.GetElement(r);
                    if (elem is FamilyInstance fi && fi.Category?.Id.Value == (long)BuiltInCategory.OST_LightingFixtures)
                    {
                        // Avoid duplicates if user clicked same element multiple times
                        if (!results.Any(x => x.Id == fi.Id))
                        {
                            results.Add(fi);
                        }
                    }
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // User pressed Esc or clicked Cancel in Revit selection mode
            }

            return results;
        }
    }
}
