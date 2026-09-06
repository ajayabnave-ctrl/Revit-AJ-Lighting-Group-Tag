using System.ComponentModel;
using System.Runtime.CompilerServices;
using Autodesk.Revit.DB;

namespace AJ.LightingGroupTag.Models
{
    /// <summary>
    /// Item view model for displaying selected fixtures in the UI list.
    /// </summary>
    public class FixtureItemViewModel : INotifyPropertyChanged
    {
        private bool _isRepresentative;

        public FamilyInstance Instance { get; }
        public ElementId Id => Instance.Id;
        public string DisplayName { get; }
        public string TypeName { get; }
        public string TypeMark { get; }

        public bool IsRepresentative
        {
            get => _isRepresentative;
            set
            {
                if (_isRepresentative != value)
                {
                    _isRepresentative = value;
                    OnPropertyChanged();
                }
            }
        }

        public FixtureItemViewModel(FamilyInstance instance, int index, string typeMark)
        {
            Instance = instance;
            TypeName = instance.Symbol?.Name ?? "Unknown Type";
            TypeMark = string.IsNullOrWhiteSpace(typeMark) ? TypeName : typeMark;
            DisplayName = $"Fixture {index:D3}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
