using System;
using System.Reflection;
using Autodesk.Revit.UI;

namespace AJ.LightingGroupTag
{
    /// <summary>
    /// External application entry point that registers the ribbon panel and button in Revit.
    /// </summary>
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                string tabName = "AJ Tools";
                string panelName = "Lighting";

                // Create ribbon tab if it doesn't already exist
                try
                {
                    application.CreateRibbonTab(tabName);
                }
                catch
                {
                    // Tab might already exist
                }

                // Create ribbon panel
                RibbonPanel? panel = null;
                var panels = application.GetRibbonPanels(tabName);
                foreach (var p in panels)
                {
                    if (p.Name.Equals(panelName, StringComparison.OrdinalIgnoreCase))
                    {
                        panel = p;
                        break;
                    }
                }

                if (panel == null)
                {
                    panel = application.CreateRibbonPanel(tabName, panelName);
                }

                string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

                // Create PushButton pointing to LightingGroupTagCommand
                var buttonData = new PushButtonData(
                    "cmdAJLightingGroupTag",
                    "Lighting\nGroup Tag",
                    thisAssemblyPath,
                    "AJ.LightingGroupTag.Commands.LightingGroupTagCommand"
                )
                {
                    ToolTip = "Creates multi-reference lighting group tags with single representative leader in Revit 2026.",
                    LongDescription = "Launches the AJ Lighting Group Tag modeless tool to select fixtures, pick a representative fixture, and automatically create multi-reference tags."
                };

                panel.AddItem(buttonData);

                return Result.Succeeded;
            }
            catch (Exception)
            {
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
