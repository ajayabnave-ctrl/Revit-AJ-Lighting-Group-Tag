using System;
using System.Collections.Concurrent;
using Autodesk.Revit.UI;

namespace AJ.LightingGroupTag.Revit
{
    /// <summary>
    /// Thread-safe external event handler to marshal calls from modeless WPF UI onto the Revit API main thread.
    /// </summary>
    public class RevitExternalEventHandler : IExternalEventHandler
    {
        private readonly ConcurrentQueue<Action<UIApplication>> _actionQueue = new ConcurrentQueue<Action<UIApplication>>();

        public ExternalEvent? ExternalEventInstance { get; set; }

        public void Run(Action<UIApplication> action)
        {
            if (action == null) return;
            _actionQueue.Enqueue(action);
            ExternalEventInstance?.Raise();
        }

        public void Execute(UIApplication app)
        {
            while (_actionQueue.TryDequeue(out var action))
            {
                try
                {
                    action(app);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("AJ Lighting Group Tag", $"An error occurred during execution:\n\n{ex.Message}");
                }
            }
        }

        public string GetName()
        {
            return "AJ Lighting Group Tag Event Handler";
        }
    }
}
