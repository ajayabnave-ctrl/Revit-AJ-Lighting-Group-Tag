using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace AJ.LightingGroupTag.Services.LightingGroupTag
{
    /// <summary>
    /// Service responsible for querying lighting fixture types, type parameters, and view geometry.
    /// </summary>
    public static class FixtureTypeService
    {
        public static List<FamilySymbol> GetLightingFixtureTypes(Document doc)
        {
            if (doc == null)
                return new List<FamilySymbol>();

            return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_LightingFixtures)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .OrderBy(s => s.FamilyName)
                .ThenBy(s => s.Name)
                .ToList();
        }

        public static string GetTypeMark(Element elem)
        {
            if (elem == null)
                return string.Empty;

            // First try BuiltInParameter ALL_MODEL_TYPE_MARK
            Parameter p = elem.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK);
            if (p != null && !string.IsNullOrWhiteSpace(p.AsString()))
            {
                return p.AsString().Trim();
            }

            // If it is a FamilyInstance, check its FamilySymbol
            if (elem is FamilyInstance fi && fi.Symbol != null)
            {
                p = fi.Symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK);
                if (p != null && !string.IsNullOrWhiteSpace(p.AsString()))
                {
                    return p.AsString().Trim();
                }

                return fi.Symbol.Name;
            }

            if (elem is FamilySymbol fs)
            {
                return fs.Name;
            }

            return elem.Name ?? string.Empty;
        }

        public static XYZ GetElementLocationPoint(Element elem)
        {
            if (elem == null)
                return XYZ.Zero;

            if (elem.Location is LocationPoint lp)
            {
                return lp.Point;
            }

            BoundingBoxXYZ bbox = elem.get_BoundingBox(null);
            if (bbox != null)
            {
                return (bbox.Min + bbox.Max) * 0.5;
            }

            return XYZ.Zero;
        }

        public static bool IsValidViewForTagging(View view)
        {
            if (view == null || view.IsTemplate)
                return false;

            return view.ViewType == ViewType.FloorPlan ||
                   view.ViewType == ViewType.CeilingPlan ||
                   view.ViewType == ViewType.Elevation ||
                   view.ViewType == ViewType.Section ||
                   view.ViewType == ViewType.ThreeD ||
                   view.ViewType == ViewType.AreaPlan ||
                   view.ViewType == ViewType.EngineeringPlan;
        }
    }
}
