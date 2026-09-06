using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AJ.LightingGroupTag.Models;
using AJ.LightingGroupTag.Revit;
using AJ.LightingGroupTag.Selection;
using AJ.LightingGroupTag.Tags;
using AJ.LightingGroupTag.Validation;

namespace AJ.LightingGroupTag.UI
{
    public class LightingGroupTagViewModel : INotifyPropertyChanged
    {
        private readonly RevitExternalEventHandler _eventHandler;
        private FamilySymbol? _selectedFixtureType;
        private FixtureItemViewModel? _selectedRepresentativeItem;
        private string _statusMessage = "Select a fixture type and pick fixtures to begin.";
        private bool _isError;
        private bool _isSuccess;
        private bool _isBusy;
        private Window? _ownerWindow;

        public ObservableCollection<FamilySymbol> AvailableTypes { get; } = new ObservableCollection<FamilySymbol>();
        public ObservableCollection<FixtureItemViewModel> SelectedFixtures { get; } = new ObservableCollection<FixtureItemViewModel>();

        public FamilySymbol? SelectedFixtureType
        {
            get => _selectedFixtureType;
            set
            {
                if (_selectedFixtureType != value)
                {
                    _selectedFixtureType = value;
                    OnPropertyChanged();
                    UpdateGroupInfo();
                }
            }
        }

        public FixtureItemViewModel? SelectedRepresentativeItem
        {
            get => _selectedRepresentativeItem;
            set
            {
                if (_selectedRepresentativeItem != value)
                {
                    if (_selectedRepresentativeItem != null)
                        _selectedRepresentativeItem.IsRepresentative = false;

                    _selectedRepresentativeItem = value;

                    if (_selectedRepresentativeItem != null)
                        _selectedRepresentativeItem.IsRepresentative = true;

                    OnPropertyChanged();
                    UpdateGroupInfo();
                    ValidateState();
                }
            }
        }

        public int SelectedFixtureCount => SelectedFixtures.Count;

        // Group Information readouts (Section 2 & 3)
        public string TypeMarkText
        {
            get
            {
                if (_selectedRepresentativeItem != null)
                    return _selectedRepresentativeItem.TypeMark;
                if (_selectedFixtureType != null)
                    return RevitElementService.GetTypeMark(_selectedFixtureType);
                return "-";
            }
        }

        public string NumberOfFixturesText => SelectedFixtureCount.ToString();
        public string LeaderText => "ONE";
        public string LeaderTargetText => _selectedRepresentativeItem != null ? _selectedRepresentativeItem.DisplayName : "-";
        public string LeaderEndText => "FREE";

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsError
        {
            get => _isError;
            set
            {
                if (_isError != value)
                {
                    _isError = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSuccess
        {
            get => _isSuccess;
            set
            {
                if (_isSuccess != value)
                {
                    _isSuccess = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CanCreateTag => !IsBusy && SelectedFixtureCount > 0 && SelectedRepresentativeItem != null && !IsError;

        public ICommand PickFixturesCommand { get; }
        public ICommand PickRepresentativeCommand { get; }
        public ICommand CreateTagCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand RefreshTypesCommand { get; }

        public LightingGroupTagViewModel(RevitExternalEventHandler eventHandler, Window? window = null)
        {
            _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
            _ownerWindow = window;

            PickFixturesCommand = new RelayCommand(ExecutePickFixtures, () => !IsBusy);
            PickRepresentativeCommand = new RelayCommand(ExecutePickRepresentative, () => !IsBusy && SelectedFixtureCount > 0);
            CreateTagCommand = new RelayCommand(ExecuteCreateTag, () => CanCreateTag);
            ResetCommand = new RelayCommand(ExecuteReset, () => !IsBusy);
            RefreshTypesCommand = new RelayCommand(ExecuteRefreshTypes, () => !IsBusy);
        }

        public void SetWindow(Window window)
        {
            _ownerWindow = window;
        }

        public void LoadInitialData(Document doc)
        {
            AvailableTypes.Clear();
            var types = RevitElementService.GetLightingFixtureTypes(doc);
            foreach (var t in types)
            {
                AvailableTypes.Add(t);
            }

            if (AvailableTypes.Count > 0)
            {
                SelectedFixtureType = AvailableTypes[0];
            }
        }

        private void ExecuteRefreshTypes()
        {
            _eventHandler.Run(app =>
            {
                var doc = app.ActiveUIDocument?.Document;
                if (doc == null) return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadInitialData(doc);
                });
            });
        }

        private void ExecutePickFixtures()
        {
            IsBusy = true;
            IsError = false;
            IsSuccess = false;
            StatusMessage = "Picking fixtures in Revit... Select fixtures and click Finish.";

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_ownerWindow != null) _ownerWindow.WindowState = WindowState.Minimized;
            });

            _eventHandler.Run(app =>
            {
                var uidoc = app.ActiveUIDocument;
                if (uidoc == null)
                {
                    RestoreWindowWithMessage("No active Revit document found.", true);
                    return;
                }

                ElementId? filterSymbolId = null;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    filterSymbolId = SelectedFixtureType?.Id;
                });

                var picked = FixtureSelectionService.PickFixtures(uidoc, filterSymbolId);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_ownerWindow != null) _ownerWindow.WindowState = WindowState.Normal;

                    if (picked.Count > 0)
                    {
                        SelectedFixtures.Clear();
                        for (int i = 0; i < picked.Count; i++)
                        {
                            var fi = picked[i];
                            string tm = RevitElementService.GetTypeMark(fi);
                            var item = new FixtureItemViewModel(fi, i + 1, tm);
                            SelectedFixtures.Add(item);
                        }

                        // Synchronize fixture type if needed
                        var firstSymbol = picked[0].Symbol;
                        if (firstSymbol != null)
                        {
                            var match = AvailableTypes.FirstOrDefault(t => t.Id.Value == firstSymbol.Id.Value);
                            if (match != null)
                            {
                                _selectedFixtureType = match;
                                OnPropertyChanged(nameof(SelectedFixtureType));
                            }
                        }

                        // Default representative to first picked fixture
                        SelectedRepresentativeItem = SelectedFixtures[0];

                        StatusMessage = $"Selected {picked.Count} fixture(s). Representative automatically set to Fixture 01.";
                        IsError = false;
                    }
                    else
                    {
                        StatusMessage = "Selection cancelled or no fixtures selected.";
                    }

                    IsBusy = false;
                    UpdateGroupInfo();
                    ValidateState();
                });
            });
        }

        private void ExecutePickRepresentative()
        {
            if (SelectedFixtures.Count == 0)
            {
                StatusMessage = "Please select fixtures first.";
                IsError = true;
                return;
            }

            IsBusy = true;
            IsError = false;
            IsSuccess = false;
            StatusMessage = "Click on one fixture in Revit from your selected group to be the representative.";

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_ownerWindow != null) _ownerWindow.WindowState = WindowState.Minimized;
            });

            _eventHandler.Run(app =>
            {
                var uidoc = app.ActiveUIDocument;
                if (uidoc == null)
                {
                    RestoreWindowWithMessage("No active Revit document found.", true);
                    return;
                }

                var allowed = SelectedFixtures.Select(vm => vm.Instance).ToList();
                var rep = RepresentativeSelectionService.PickRepresentative(uidoc, allowed);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_ownerWindow != null) _ownerWindow.WindowState = WindowState.Normal;

                    if (rep != null)
                    {
                        var match = SelectedFixtures.FirstOrDefault(vm => vm.Id.Value == rep.Id.Value);
                        if (match != null)
                        {
                            SelectedRepresentativeItem = match;
                            StatusMessage = $"Representative fixture set to {match.DisplayName}.";
                            IsError = false;
                        }
                        else
                        {
                            StatusMessage = "Picked fixture is not part of the selected group!";
                            IsError = true;
                        }
                    }
                    else
                    {
                        StatusMessage = "Representative picking cancelled.";
                    }

                    IsBusy = false;
                    UpdateGroupInfo();
                    ValidateState();
                });
            });
        }

        private void ExecuteCreateTag()
        {
            if (!CanCreateTag)
            {
                ValidateState();
                return;
            }

            IsBusy = true;
            IsError = false;
            IsSuccess = false;
            StatusMessage = "Creating multi-reference group tag in Revit...";

            _eventHandler.Run(app =>
            {
                var uidoc = app.ActiveUIDocument;
                if (uidoc == null)
                {
                    ShowError("No active document.");
                    return;
                }

                var doc = uidoc.Document;
                var view = doc.ActiveView;

                if (!RevitElementService.IsValidViewForTagging(view))
                {
                    ShowError($"Current view '{view.Name}' is not a valid 2D/3D plan or section view for tagging.");
                    return;
                }

                try
                {
                    var group = new LightingGroup
                    {
                        FamilySymbol = SelectedFixtureType,
                        FamilyName = SelectedFixtureType?.FamilyName ?? string.Empty,
                        TypeName = SelectedFixtureType?.Name ?? string.Empty,
                        TypeMark = TypeMarkText,
                        Fixtures = SelectedFixtures.Select(f => f.Instance).ToList(),
                        Representative = SelectedRepresentativeItem?.Instance,
                        View = view
                    };

                    var tag = LightingGroupTagService.CreateGroupTag(doc, view, group);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsBusy = false;
                        IsSuccess = true;
                        IsError = false;
                        StatusMessage = $"Successfully created group tag for {group.Count} fixture(s) on {SelectedRepresentativeItem?.DisplayName}!";
                    });
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                }
            });
        }

        private void ExecuteReset()
        {
            SelectedFixtures.Clear();
            SelectedRepresentativeItem = null;
            IsError = false;
            IsSuccess = false;
            IsBusy = false;
            StatusMessage = "Reset complete. Select fixtures to begin.";
            UpdateGroupInfo();
            OnPropertyChanged(nameof(CanCreateTag));
        }

        private void ValidateState()
        {
            var fixtures = SelectedFixtures.Select(f => f.Instance).ToList();
            var rep = SelectedRepresentativeItem?.Instance;

            if (fixtures.Count == 0)
            {
                IsError = false;
                IsSuccess = false;
                OnPropertyChanged(nameof(CanCreateTag));
                return;
            }

            var result = FixtureGroupValidator.Validate(fixtures, rep);
            if (!result.IsValid)
            {
                IsError = true;
                IsSuccess = false;
                StatusMessage = result.ErrorMessage;
            }
            else
            {
                IsError = false;
            }

            OnPropertyChanged(nameof(CanCreateTag));
        }

        private void UpdateGroupInfo()
        {
            OnPropertyChanged(nameof(SelectedFixtureCount));
            OnPropertyChanged(nameof(TypeMarkText));
            OnPropertyChanged(nameof(NumberOfFixturesText));
            OnPropertyChanged(nameof(LeaderText));
            OnPropertyChanged(nameof(LeaderTargetText));
            OnPropertyChanged(nameof(LeaderEndText));
            OnPropertyChanged(nameof(CanCreateTag));
        }

        private void RestoreWindowWithMessage(string message, bool isError)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_ownerWindow != null) _ownerWindow.WindowState = WindowState.Normal;
                IsBusy = false;
                IsError = isError;
                StatusMessage = message;
            });
        }

        private void ShowError(string errorMsg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsBusy = false;
                IsError = true;
                IsSuccess = false;
                StatusMessage = errorMsg;

                // Also display a prompt to the user if the tag family is missing
                if (errorMsg.Contains(LightingGroupTagService.ExpectedTagFamilyName))
                {
                    TaskDialog.Show("AJ Lighting Group Tag", errorMsg);
                }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
