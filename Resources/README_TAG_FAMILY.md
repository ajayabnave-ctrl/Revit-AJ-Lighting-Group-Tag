# AJ_Lighting_Group_Tag Family Configuration Guide

## Overview
The **AJ Lighting Group Tag** add-in strictly looks for a loaded tag family named `AJ_Lighting_Group_Tag`.
If this family is not loaded in the active Revit project, the tool halts with the message:
> **AJ_Lighting_Group_Tag was not found in this project. Please load the tag family.**

Per the architecture specification, it will not substitute another tag family.

---

## How to Create the Tag Family in Revit

1. **Start New Family**:
   - In Revit, go to **File > New > Family**.
   - Choose the template: **Annotations/Multi-Category Tag.rft** (or **Lighting Fixture Tag.rft**).

2. **Configure Label**:
   - Select the default note text and delete it.
   - Click **Create > Label** and click near the intersection of the reference planes.
   - In the **Edit Label** dialog, add the following parameters:
     * **Host Count** (or Multi-Reference Annotation count)
     * **Type Mark**
   - Alternatively, format the label as:
     - **Prefix**: `(`
     - **Sample Value**: `4`
     - **Suffix**: `) `
     - Followed by **Type Mark** (Sample Value: `EL_01`).
     - Resulting preview: `(4) EL_01`

3. **Leader / Reference Planes**:
   - Keep the tag centered on the intersection of the reference planes.
   - Set the horizontal alignment to Center.

4. **Save Family**:
   - Save the family file as:
     `AJ_Lighting_Group_Tag.rfa`
   - Place a copy inside this `Resources/` folder.

5. **Load into Project**:
   - In the Family Editor, click **Load into Project and Close**.
   - Now the add-in will automatically detect `AJ_Lighting_Group_Tag` and tag your lighting fixture groups.
