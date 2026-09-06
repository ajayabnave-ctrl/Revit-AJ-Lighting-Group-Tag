# Revit-AJ-Lighting-Group-Tag

A **Revit 2026 / .NET 8 Revit API add-in** that creates multi-reference lighting fixture tags with a single free representative leader:

```text
Fixture ─────────────────► (4) EL_01
                                  │
                                  │
                                  ▼
                           Representative
```

## Features

* **Family/type-specific fixture selection**
* **Sequential fixture picking**
* **Multiple fixtures represented by one tag**
* **Fixture count & Type Mark display**
* **User-selected representative fixture**
* **One visible leader only** (attached directly to representative fixture)
* **Free leader end** for flexible annotation positioning
* **Plan, Elevation, and Section view support**
* **Modeless WPF UI** with clean BIM dark theme
* Built for **Autodesk Revit 2026 API** on **.NET 8.0**

---

## Tool UI Design

```text
┌────────────────────────────────────────┐
│ AJ LIGHTING GROUP TAG                  │
├────────────────────────────────────────┤
│                                        │
│ Fixture Type                           │
│ [ AJ Lighting : EL_01             ▼ ] │
│                                        │
│ [ PICK FIXTURES ]                      │
│                                        │
│ Selected Fixtures: 4                   │
│                                        │
│  ✓ Fixture 001                         │
│  ✓ Fixture 002                         │
│  ✓ Fixture 003                         │
│  ✓ Fixture 004                         │
│                                        │
│ [ PICK REPRESENTATIVE ]                │
│                                        │
│ Representative: Fixture 002           │
│                                        │
│ ─────────────────────────────────────  │
│ Count:       4                         │
│ Type Mark:   EL_01                     │
│ Leader:      ONE                       │
│ End:         FREE                      │
│                                        │
│ [ RESET ]              [ CREATE TAG ] │
└────────────────────────────────────────┘
```

---

## Repository Structure

```text
Revit-AJ-Lighting-Group-Tag/
│
├── src/
│   └── AJ.LightingGroupTag/
│       │
│       ├── App.cs
│       ├── Commands/
│       │   └── LightingGroupTagCommand.cs
│       │
│       ├── Models/
│       │   ├── LightingGroup.cs
│       │   └── FixtureItemViewModel.cs
│       │
│       ├── Services/
│       │   └── LightingGroupTag/
│       │       ├── FixtureTypeService.cs
│       │       ├── FixtureSelectionService.cs
│       │       ├── FixtureValidationService.cs
│       │       ├── LightingTagService.cs
│       │       └── RevitExternalEventHandler.cs
│       │
│       ├── UI/
│       │   └── LightingGroupTag/
│       │       ├── LightingGroupTagWindow.xaml
│       │       ├── LightingGroupTagWindow.xaml.cs
│       │       ├── LightingGroupTagViewModel.cs
│       │       └── RelayCommand.cs
│       │
│       ├── AJ.LightingGroupTag.csproj
│       └── AJ.LightingGroupTag.addin
│
├── resources/
│   ├── AJ_Lighting_Group_Tag.rfa
│   └── README.md
│
├── docs/
│   ├── workflow.md
│   └── architecture.md
│
├── deploy.bat
├── .gitignore
├── README.md
└── LICENSE
```

---

## Development Sequence

```text
01  Create repository
          ↓
02  Revit 2026 .NET 8 project
          ↓
03  Basic Revit command + ribbon
          ↓
04  WPF tool window
          ↓
05  Fixture Type picker
          ↓
06  Sequential fixture picker
          ↓
07  Fixture validation
          ↓
08  Representative picker
          ↓
09  LightingGroup data model
          ↓
10  Tag family detection
          ↓
11  Multi-reference tag creation
          ↓
12  ONE representative leader
          ↓
13  FREE leader end
          ↓
14  Plan / Elevation / Section testing
          ↓
15  Installer / deployment
          ↓
16  Documentation
```

---

## Requirements & Building

- **Autodesk Revit 2026**
- **.NET 8.0 SDK** (Windows Desktop)
- Revit tag family named `AJ_Lighting_Group_Tag` loaded in your project.

### Build and Deploy
Run the included `deploy.bat` script, or run:
```cmd
dotnet build src\AJ.LightingGroupTag\AJ.LightingGroupTag.csproj -c Release
```
Copy `src\AJ.LightingGroupTag\bin\Release\AJ.LightingGroupTag.dll` and `src\AJ.LightingGroupTag\AJ.LightingGroupTag.addin` into:
```text
%APPDATA%\Autodesk\Revit\Addins\2026\
```
