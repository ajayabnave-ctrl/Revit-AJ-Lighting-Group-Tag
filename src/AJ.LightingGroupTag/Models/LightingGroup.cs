using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace AJ.LightingGroupTag.Models
{
    /// <summary>
    /// Core data model representing a group of lighting fixtures to be tagged together.
    /// Carries state between UI, Selection, Validation, and Tagging services.
    /// </summary>
    public class LightingGroup
    {
        public FamilySymbol? FamilySymbol { get; set; }
        public string FamilyName { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string TypeMark { get; set; } = string.Empty;

        public List<FamilyInstance> Fixtures { get; set; } = new List<FamilyInstance>();
        public int Count => Fixtures?.Count ?? 0;

        public FamilyInstance? Representative { get; set; }
        public View? View { get; set; }
        public IndependentTag? Tag { get; set; }
    }
}
