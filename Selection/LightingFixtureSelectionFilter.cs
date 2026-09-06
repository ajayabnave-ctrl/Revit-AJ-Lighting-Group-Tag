using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace AJ.LightingGroupTag.Selection
{
    /// <summary>
    /// Selection filter that restricts user selection to lighting fixture family instances.
    /// Optionally restricts selection to a specific FamilySymbol (Family Type).
    /// </summary>
    public class LightingFixtureSelectionFilter : ISelectionFilter
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
                if (fi.Symbol == null || fi.Symbol.Id != _requiredSymbolId)
                    return false;
            }

            return true;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
