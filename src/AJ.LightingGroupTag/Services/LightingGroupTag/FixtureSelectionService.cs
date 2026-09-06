using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace AJ.LightingGroupTag.Services.LightingGroupTag
{
    /// <summary>
    /// Service responsible for sequential and interactive fixture selection in Revit.
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
                        if (!results.Any(x => x.Id.Value == fi.Id.Value))
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

        public static FamilyInstance? PickRepresentative(UIDocument uidoc, List<FamilyInstance>? allowedGroup = null)
        {
            if (uidoc == null)
                throw new ArgumentNullException(nameof(uidoc));

            ISelectionFilter filter;
            if (allowedGroup != null && allowedGroup.Count > 0)
            {
                var allowedIds = new HashSet<long>(allowedGroup.Select(f => f.Id.Value));
                filter = new SpecificFixturesSelectionFilter(allowedIds);
            }
            else
            {
                filter = new LightingFixtureSelectionFilter();
            }

            try
            {
                Reference pickedRef = uidoc.Selection.PickObject(
                    ObjectType.Element,
                    filter,
                    "Click on one representative lighting fixture from the group to attach the leader."
                );

                if (pickedRef != null)
                {
                    Element elem = uidoc.Document.GetElement(pickedRef);
                    if (elem is FamilyInstance fi)
                    {
                        return fi;
                    }
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // User pressed Esc or cancelled
            }

            return null;
        }

        private class LightingFixtureSelectionFilter : ISelectionFilter
        {
            private readonly ElementId? _requiredSymbolId;

            public LightingFixtureSelectionFilter(ElementId? requiredSymbolId = null)
            {
                _requiredSymbolId = requiredSymbolId;
            }

            public bool AllowElement(Element elem)
            {
                if (elem is not FamilyInstance fi)
                    return false;

                if (fi.Category == null || fi.Category.Id.Value != (long)BuiltInCategory.OST_LightingFixtures)
                    return false;

                if (_requiredSymbolId != null && _requiredSymbolId != ElementId.InvalidElementId)
                {
                    if (fi.Symbol == null || fi.Symbol.Id.Value != _requiredSymbolId.Value)
                        return false;
                }

                return true;
            }

            public bool AllowReference(Reference reference, XYZ position) => false;
        }

        private class SpecificFixturesSelectionFilter : ISelectionFilter
        {
            private readonly HashSet<long> _allowedIds;

            public SpecificFixturesSelectionFilter(HashSet<long> allowedIds)
            {
                _allowedIds = allowedIds;
            }

            public bool AllowElement(Element elem)
            {
                return elem is FamilyInstance fi && _allowedIds.Contains(fi.Id.Value);
            }

            public bool AllowReference(Reference reference, XYZ position) => false;
        }
    }
}
