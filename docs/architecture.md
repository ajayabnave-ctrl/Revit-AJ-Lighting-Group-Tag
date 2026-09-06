# AJ Lighting Group Tag — Architecture & Design

## 1. Clean Separation of Concerns

```text
┌───────────────────────────┐
│          UI Layer         │
│  (WPF Modeless + MVVM)    │
└─────────────┬─────────────┘
              │
              ▼
┌───────────────────────────┐
│     Selection Service     │
│ (FixtureSelectionService) │
└─────────────┬─────────────┘
              │
              ▼
┌───────────────────────────┐
│    Validation Service     │
│ (FixtureValidationService)│
└─────────────┬─────────────┘
              │
              ▼
┌───────────────────────────┐
│    LightingGroup Model    │
│  (State carrier object)   │
└─────────────┬─────────────┘
              │
              ▼
┌───────────────────────────┐
│        Tag Service        │
│   (LightingTagService)    │
└─────────────┬─────────────┘
              │
              ▼
┌───────────────────────────┐
│       Revit 2026 API      │
│  (Transactions / Models)  │
└───────────────────────────┘
```

---

## 2. Modeless Window Threading (`ExternalEvent`)

Because the UI is modeless (`LightingGroupTagWindow.xaml`), it runs asynchronously from Revit's UI thread. Direct calls to Revit API methods from WPF button click handlers would trigger:
`Autodesk.Revit.Exceptions.InvalidOperationException: Cannot execute Revit API outside of Revit API context!`

### Solution:
- [RevitExternalEventHandler.cs](file:///C:/Users/SHREE/Revit_Addins/AJ%20Lighting%20Group%20Tag/src/AJ.LightingGroupTag/Services/LightingGroupTag/RevitExternalEventHandler.cs) implements Revit's `IExternalEventHandler`.
- When an operation is initiated (Pick Fixtures, Pick Representative, Create Tag), the ViewModel queues an `Action<UIApplication>` and calls `ExternalEvent.Raise()`.
- Revit invokes `Execute(UIApplication)` when safe on the Revit main thread.
- Upon completion, the result is dispatched back to the WPF UI thread via `Application.Current.Dispatcher.Invoke(...)`.

---

## 3. Data Model: `LightingGroup`

The `LightingGroup` class acts as the core state object passed between services:
- `FamilySymbol`: Target fixture symbol.
- `FamilyName`: Family name string.
- `TypeName`: Type name string.
- `TypeMark`: Extracted Type Mark parameter.
- `Fixtures`: List of all `FamilyInstance` members.
- `Count`: Integer count of fixtures (`Fixtures.Count`).
- `Representative`: The single `FamilyInstance` chosen to anchor the leader.
- `View`: Active Revit `View`.
- `Tag`: Created `IndependentTag` reference.

---

## 4. Multi-Reference Tag & Single Free Leader Control

Revit 2022+ introduced multi-reference tags on `IndependentTag`.
`LightingTagService` configures this via:
1. `IndependentTag.Create(doc, tagSymbol.Id, view.Id, repRef, true, TagOrientation.Horizontal, headPoint)`
2. `tag.AddReferences(remainingRefs)`
3. `tag.LeaderEndCondition = LeaderEndCondition.Free`
4. For each reference `r` in `tag.GetTaggedReferences()`:
   - `tag.SetIsLeaderVisible(r, isRepresentative)`
   - Only the representative fixture has a visible leader; all others are set to `false`.
