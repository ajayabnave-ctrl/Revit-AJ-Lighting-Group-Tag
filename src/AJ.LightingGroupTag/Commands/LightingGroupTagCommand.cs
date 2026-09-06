using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AJ.LightingGroupTag.Services.LightingGroupTag;
using AJ.LightingGroupTag.UI.LightingGroupTag;

namespace AJ.LightingGroupTag.Commands
{
    /// <summary>
    /// Revit external command to launch or focus the modeless AJ Lighting Group Tag tool window.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LightingGroupTagCommand : IExternalCommand
    {
        private static LightingGroupTagWindow? _window;
        private static RevitExternalEventHandler? _eventHandler;
        private static ExternalEvent? _externalEvent;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiapp = commandData.Application;
                UIDocument? uidoc = uiapp.ActiveUIDocument;

                if (uidoc == null)
                {
                    TaskDialog.Show("AJ Lighting Group Tag", "Please open a project with lighting fixtures first.");
                    return Result.Cancelled;
                }

                // If window already open, restore and focus
                if (_window != null && _window.IsLoaded)
                {
                    if (_window.WindowState == System.Windows.WindowState.Minimized)
                    {
                        _window.WindowState = System.Windows.WindowState.Normal;
                    }
                    _window.Activate();
                    _window.Focus();
                    return Result.Succeeded;
                }

                // Initialize external event handler if not already done
                if (_eventHandler == null)
                {
                    _eventHandler = new RevitExternalEventHandler();
                    _externalEvent = ExternalEvent.Create(_eventHandler);
                    _eventHandler.ExternalEventInstance = _externalEvent;
                }

                // Create ViewModel and populate initial fixture types
                var viewModel = new LightingGroupTagViewModel(_eventHandler);
                viewModel.LoadInitialData(uidoc.Document);

                // Create and show modeless window
                _window = new LightingGroupTagWindow(viewModel);
                _window.Closed += (s, e) => _window = null;

                _window.Show();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
