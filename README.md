# Revit-AJ-Lighting-Group-Tag — Revit 2026 Add-in

Revit 2026 add-in for creating multi-reference lighting fixture group tags with one free representative leader. Built with C# and .NET 8 using the Revit API.

## Overview
Standard Revit tagging tools often create multiple messy leaders when tagging a group of fixtures. **AJ Lighting Group Tag** provides a streamlined modeless workflow that:
1. Groups identical lighting fixture types.
2. Lets you pick all fixtures in the group.
3. Lets you designate one **representative fixture**.
4. Automatically generates a native multi-reference `IndependentTag` where:
   - **Only the representative fixture has a visible leader**.
   - All other fixture leaders are hidden.
   - Leader End Condition is set to **Free End** so the endpoint can be repositioned independently.
5. Strictly enforces the `AJ_Lighting_Group_Tag` tag family without silent substitution.

## Features
- **Modeless WPF UI**: Clean BIM dark theme that remains open while allowing full navigation and interaction inside Revit.
- **Thread-Safe Architecture**: Uses `IExternalEventHandler` to safely dispatch selection and transaction operations to the Revit main thread.
- **Interactive Picking**:
  - `[ PICK FIXTURES ]`: Select fixtures in the Revit viewport.
  - `[ PICK REPRESENTATIVE ]`: Select or choose which fixture hosts the leader.
- **Comprehensive Validation**:
  - Validates `OST_LightingFixtures` category.
  - Ensures all fixtures belong to the same Family Type (`FamilySymbol`).
  - Ensures representative fixture is a member of the group.
- **One-Click Deploy**: Includes `deploy.bat` to compile and copy directly to `%APPDATA%\Autodesk\Revit\Addins\2026\`.

## Project Structure
```text
├── AJ.LightingGroupTag.csproj        # .NET 8.0 WPF project
├── AJ.LightingGroupTag.addin         # Revit manifest file
├── App.cs                           # External Application (Ribbon Tab & Button)
├── Commands/
│   └── LaunchLightingGroupTagCommand.cs
├── UI/
│   ├── LightingGroupTagWindow.xaml  # Modeless WPF Dialog
│   ├── LightingGroupTagViewModel.cs # MVVM ViewModel
│   └── RelayCommand.cs
├── Models/
│   ├── LightingGroup.cs
│   └── FixtureItemViewModel.cs
├── Selection/
│   ├── LightingFixtureSelectionFilter.cs
│   ├── FixtureSelectionService.cs
│   └── RepresentativeSelectionService.cs
├── Validation/
│   ├── FixtureGroupValidator.cs
│   └── ValidationResult.cs
├── Tags/
│   └── LightingGroupTagService.cs
├── Revit/
│   ├── RevitElementService.cs
│   └── RevitExternalEventHandler.cs
└── Resources/
    └── README_TAG_FAMILY.md
```

## Requirements
- Autodesk Revit 2026
- .NET 8.0 SDK (Windows Desktop)
- Revit tag family named `AJ_Lighting_Group_Tag` loaded in your project.

## How to Build & Deploy
Run `deploy.bat` or run:
```cmd
dotnet build AJ.LightingGroupTag.csproj -c Release
```
Copy `bin\Release\AJ.LightingGroupTag.dll` and `AJ.LightingGroupTag.addin` into:
`%APPDATA%\Autodesk\Revit\Addins\2026\`
