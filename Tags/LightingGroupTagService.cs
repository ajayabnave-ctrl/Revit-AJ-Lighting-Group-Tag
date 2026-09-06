using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using AJ.LightingGroupTag.Models;
using AJ.LightingGroupTag.Revit;
using AJ.LightingGroupTag.Validation;

namespace AJ.LightingGroupTag.Tags
{
    /// <summary>
    /// Service for creating and configuring multi-reference lighting group tags.
    /// Strictly locates 'AJ_Lighting_Group_Tag' and configures a single visible leader
    /// attached to the representative fixture with Free End condition.
    /// </summary>
    public class LightingGroupTagService
    {
        public const string ExpectedTagFamilyName = "AJ_Lighting_Group_Tag";

        /// <summary>
        /// Attempts to locate the AJ_Lighting_Group_Tag FamilySymbol in the document.
        /// </summary>
        public static FamilySymbol? FindTagSymbol(Document doc)
        {
            if (doc == null)
                return null;

            // Search in Lighting Fixture Tags and Multi-Category Tags
            var tagSymbols = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(s =>
                {
                    string fName = s.FamilyName ?? string.Empty;
                    string sName = s.Name ?? string.Empty;
                    return fName.Equals(ExpectedTagFamilyName, StringComparison.OrdinalIgnoreCase) ||
                           sName.Equals(ExpectedTagFamilyName, StringComparison.OrdinalIgnoreCase);
                })
                .ToList();

            return tagSymbols.FirstOrDefault();
        }

        /// <summary>
        /// Creates the multi-reference group tag in the active view.
        /// </summary>
        public static IndependentTag CreateGroupTag(Document doc, View view, LightingGroup group)
        {
            if (doc == null)
                throw new ArgumentNullException(nameof(doc));
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (group == null)
                throw new ArgumentNullException(nameof(group));

            // Validate the group before doing any Revit modifications
            var validation = FixtureGroupValidator.Validate(group);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"Cannot create tag: {validation.ErrorMessage}");
            }

            // Step 1: Find tag family symbol
            FamilySymbol? tagSymbol = FindTagSymbol(doc);
            if (tagSymbol == null)
            {
                // Strict requirement: Don't silently substitute another tag family!
                throw new InvalidOperationException(
                    $"{ExpectedTagFamilyName} was not found in this project.\n\nPlease load the tag family."
                );
            }

            var representative = group.Representative!;
            var fixtures = group.Fixtures;

            using (var tx = new Transaction(doc, "Create AJ Lighting Group Tag"))
            {
                tx.Start();

                if (!tagSymbol.IsActive)
                {
                    tagSymbol.Activate();
                    doc.Regenerate();
                }

                // Step 2: Compute positions
                XYZ repPoint = RevitElementService.GetElementLocationPoint(representative);
                // Offset tag head for clear visibility and clean drafting layout
                XYZ headPoint = repPoint + new XYZ(2.0, 2.0, 0.0);

                // Step 3: Create tag initially against representative
                Reference repRef = new Reference(representative);
                IndependentTag tag = IndependentTag.Create(
                    doc,
                    tagSymbol.Id,
                    view.Id,
                    repRef,
                    true, // addLeader
                    TagOrientation.Horizontal,
                    headPoint
                );

                doc.Regenerate();

                // Step 4: Multi-reference tag logic - add remaining references
                var remainingRefs = fixtures
                    .Where(f => f.Id.Value != representative.Id.Value)
                    .Select(f => new Reference(f))
                    .ToList();

                if (remainingRefs.Count > 0)
                {
                    tag.AddReferences(remainingRefs);
                    doc.Regenerate();
                }

                // Step 5: Leader Configuration (Free End & One Leader Logic)
                tag.HasLeader = true;

                if (tag.CanLeaderEndConditionBeAssigned(LeaderEndCondition.Free))
                {
                    tag.LeaderEndCondition = LeaderEndCondition.Free;
                }

                // Anchor the leader directly at representative fixture
                try
                {
                    tag.SetLeaderEnd(repRef, repPoint);
                }
                catch
                {
                    // If geometry doesn't permit setting endpoint directly, Free mode still allows user movement
                }

                // Step 6: ONE LEADER Logic (Section 10)
                // Critical: Only the representative fixture has the visible leader; all others are hidden!
                IList<Reference> taggedRefs = tag.GetTaggedReferences();
                foreach (Reference r in taggedRefs)
                {
                    bool isRep = (r.ElementId.Value == representative.Id.Value);
                    try
                    {
                        tag.SetIsLeaderVisible(r, isRep);
                    }
                    catch
                    {
                        // Some Revit versions or tag types may handle individual leader visibility via presentation mode
                    }
                }

                tx.Commit();

                group.Tag = tag;
                return tag;
            }
        }
    }
}
