# AJ Lighting Group Tag — User & Technical Workflow

## 1. Overall Workflow

```text
┌──────────────────────────────┐
│          REVIT 2026          │
│                              │
│    AJ Lighting Group Tag     │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│         TOOL WINDOW          │
│                              │
│  1. Select Fixture Type      │
│  2. Pick Fixtures            │
│  3. Pick Representative      │
│  4. Create Group Tag         │
└──────────────┬───────────────┘
               │
┌──────────────┼──────────────┐
│              │              │
▼              ▼              ▼
Fixture Type   Fixture List   Representative
Selection      Selection      Selection
│              │              │
└──────────────┼──────────────┘
               ▼
       VALIDATE SELECTION
               │
        ┌──────┴──────┐
        │             │
      VALID        INVALID
        │             │
        ▼             ▼
  Count fixtures   Show message
  Get Type Mark
        │
        ▼
  CREATE MULTI-REFERENCE
        REVIT TAG
        │
        ▼
  ONE REPRESENTATIVE
        LEADER
        │
        ▼
    FREE END
```

---

## 2. Step-by-Step Operation

### Step 1: Fixture Type Selection
- Displays all loaded `OST_LightingFixtures` types in the active document.
- Selecting a type pre-filters sequential selection or indicates the target type mark.

### Step 2: Sequential Fixture Selection
- Click `[ PICK FIXTURES ]`.
- The tool window minimizes to reveal the Revit viewport.
- Pick lighting fixtures individually or with cross-window selection.
- Click **Finish** on the Revit Options bar (or press Esc to cancel).
- Window restores showing the total count (`Selected Fixtures: X`) and fixture entries.

### Step 3: Representative Selection
- Click `[ PICK REPRESENTATIVE ]` or pick from the fixture list.
- Click on the designated fixture in Revit that will receive the leader.
- The representative fixture is assigned as the leader target.

### Step 4: Validation
- **Check 1**: All elements are `FamilyInstance`.
- **Check 2**: Category is `OST_LightingFixtures`.
- **Check 3**: All fixtures share the same `FamilySymbol` (Family Type).
- **Check 4**: Selection count > 0.
- **Check 5**: Representative fixture is a member of the selected group.

### Step 5: Tag Creation
- The add-in searches for `AJ_Lighting_Group_Tag`.
  - If missing: Displays `AJ_Lighting_Group_Tag was not found in this project. Please load the tag family.` (strictly no silent substitution).
- Creates `IndependentTag` attached to representative fixture.
- Adds remaining fixture references via `tag.AddReferences(otherRefs)`.
- Assigns `tag.LeaderEndCondition = LeaderEndCondition.Free`.
- Shows only the representative fixture's leader (`SetIsLeaderVisible(repRef, true)`), hiding all other leaders (`SetIsLeaderVisible(otherRef, false)`).

---

## 3. Supported View Types
The add-in functions across all standard Revit tagging views:
- **Floor Plans**
- **Reflected Ceiling Plans (RCP)**
- **Elevations**
- **Sections**
- **3D Views** (with saved orientation/locking)
