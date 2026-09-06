using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace AJ.LightingGroupTag.Selection
{
    /// <summary>
    /// Service responsible for selecting the representative fixture that will carry the visible leader.
    /// </summary>
    public class RepresentativeSelectionService
    {
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

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}
