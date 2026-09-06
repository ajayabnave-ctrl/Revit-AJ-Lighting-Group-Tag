using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using AJ.LightingGroupTag.Models;

namespace AJ.LightingGroupTag.Services.LightingGroupTag
{
    public class ValidationResult
    {
        public bool IsValid { get; }
        public string ErrorMessage { get; }

        private ValidationResult(bool isValid, string errorMessage)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Success() => new ValidationResult(true, string.Empty);
        public static ValidationResult Failure(string message) => new ValidationResult(false, message);
    }

    /// <summary>
    /// Service responsible for validating fixture selection and grouping rules.
    /// </summary>
    public static class FixtureValidationService
    {
        public static ValidationResult Validate(LightingGroup group)
        {
            if (group == null)
                return ValidationResult.Failure("Lighting group cannot be null.");

            return Validate(group.Fixtures, group.Representative);
        }

        public static ValidationResult Validate(IReadOnlyList<FamilyInstance>? fixtures, FamilyInstance? representative)
        {
            // Check 4: At least one fixture must be selected
            if (fixtures == null || fixtures.Count == 0)
            {
                return ValidationResult.Failure("At least one lighting fixture must be selected.");
            }

            ElementId? expectedSymbolId = null;

            for (int i = 0; i < fixtures.Count; i++)
            {
                var f = fixtures[i];

                // Check 1: All selected elements must be FamilyInstance
                if (f == null)
                {
                    return ValidationResult.Failure($"Element at index {i} is not a valid FamilyInstance.");
                }

                // Check 2: All must be OST_LightingFixtures
                if (f.Category == null || f.Category.Id.Value != (long)BuiltInCategory.OST_LightingFixtures)
                {
                    return ValidationResult.Failure($"Element {f.Id.Value} is not a Lighting Fixture (Category: {f.Category?.Name ?? "None"}).");
                }

                // Check 3: All must have the same Family Type (FamilySymbol.Id)
                if (f.Symbol == null)
                {
                    return ValidationResult.Failure($"Element {f.Id.Value} has no associated FamilySymbol.");
                }

                if (expectedSymbolId == null)
                {
                    expectedSymbolId = f.Symbol.Id;
                }
                else if (f.Symbol.Id.Value != expectedSymbolId.Value)
                {
                    return ValidationResult.Failure($"Mixed fixture types detected: Element {f.Id.Value} ({f.Symbol.Name}) does not match the group type ({fixtures[0].Symbol?.Name}). All fixtures in a group must have the same Family Type.");
                }
            }

            // Check 5 (Representative Fixture)
            if (representative == null)
            {
                return ValidationResult.Failure("A representative fixture must be selected to anchor the leader.");
            }

            bool repInList = fixtures.Any(f => f.Id.Value == representative.Id.Value);
            if (!repInList)
            {
                return ValidationResult.Failure($"Representative fixture (Id: {representative.Id.Value}) is not among the selected fixtures in this group.");
            }

            return ValidationResult.Success();
        }
    }
}
