using RimWorldModManager.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RimWorldModManager.ViewModels
{
    public enum ModViewMode
    {
        LocalMods,
        WorkshopMods
    }

    public enum ModSortOption
    {
        Name,
        LastModified,
        WorkshopId,
        Size
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ModScannerService _scannerService;
        private ModViewMode _currentViewMode;
        private string _statusMessage = string.Empty;
        private string _steamCmdOutput = string.Empty;
        private bool _isLoading;
        private string _searchText = string.Empty;
        private int _currentProgress;
        private int _totalProgress;
        private string _currentModName = string.Empty;
        private string _progressText = string.Empty;
        private ObservableCollection<WorkshopModItem> _allWorkshopMods = new();
        private ObservableCollection<LocalModItem> _allLocalMods = new();
        private ModDetailViewModel _selectedModDetail = new();
        private ModSortOption _currentSortOption = ModSortOption.Name;
        private bool _isSortAscending = true;

        public ObservableCollection<WorkshopModItem> WorkshopMods { get; } = new();
        public ObservableCollection<LocalModItem> LocalMods { get; } = new();

        public ModDetailViewModel SelectedModDetail
        {
            get => _selectedModDetail;
            set
            {
                _selectedModDetail = value;
                OnPropertyChanged();
            }
        }

        public ModViewMode CurrentViewMode
        {
            get => _currentViewMode;
            set
            {
                _currentViewMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLocalModsView));
                OnPropertyChanged(nameof(IsWorkshopModsView));
                OnPropertyChanged(nameof(CurrentViewModeIndex));
                ApplySearchFilter();
            }
        }

        public int CurrentViewModeIndex
        {
            get => (int)CurrentViewMode;
            set => CurrentViewMode = (ModViewMode)value;
        }

        public bool IsLocalModsView => CurrentViewMode == ModViewMode.LocalMods;
        public bool IsWorkshopModsView => CurrentViewMode == ModViewMode.WorkshopMods;

        public ModSortOption CurrentSortOption
        {
            get => _currentSortOption;
            set
            {
                _currentSortOption = value;
                OnPropertyChanged();
                ApplySearchFilter();
            }
        }

        public bool IsSortAscending
        {
            get => _isSortAscending;
            set
            {
                _isSortAscending = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SortDirectionText));
                ApplySearchFilter();
            }
        }

        public string SortDirectionText => IsSortAscending ? "↑" : "↓";

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public string SteamCmdOutput
        {
            get => _steamCmdOutput;
            set
            {
                _steamCmdOutput = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public int CurrentProgress
        {
            get => _currentProgress;
            set
            {
                _currentProgress = value;
                OnPropertyChanged();
            }
        }

        public int TotalProgress
        {
            get => _totalProgress;
            set
            {
                _totalProgress = value;
                OnPropertyChanged();
            }
        }

        public string CurrentModName
        {
            get => _currentModName;
            set
            {
                _currentModName = value;
                OnPropertyChanged();
            }
        }

        public string ProgressText
        {
            get => _progressText;
            set
            {
                _progressText = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplySearchFilter();
            }
        }

        public MainViewModel(ModScannerService scannerService)
        {
            _scannerService = scannerService;
        }

        public async Task RefreshModsAsync()
        {
            StatusMessage = "正在扫描 Mod...";

            try
            {
                await Task.Run(() =>
                {
                    var workshopItems = _scannerService.ScanWorkshopMods();
                    var localItems = _scannerService.ScanLocalMods();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _allWorkshopMods.Clear();
                        foreach (var item in workshopItems)
                            _allWorkshopMods.Add(item);

                        _allLocalMods.Clear();
                        foreach (var item in localItems)
                            _allLocalMods.Add(item);
                    });
                });

                ApplySearchFilter();
                StatusMessage = $"扫描完成 - {WorkshopMods.Count} 个 Workshop Mod，{LocalMods.Count} 个本地 Mod";
            }
            catch (System.Exception ex)
            {
                StatusMessage = $"扫描失败: {ex.Message}";
            }
        }

        private void ApplySearchFilter()
        {
            var localFiltered = string.IsNullOrWhiteSpace(_searchText)
                ? _allLocalMods.ToList()
                : _allLocalMods.Where(mod =>
                    (mod.DisplayName != null && mod.DisplayName.ToLowerInvariant().Contains(_searchText.ToLowerInvariant())) ||
                    (mod.Author != null && mod.Author.ToLowerInvariant().Contains(_searchText.ToLowerInvariant())) ||
                    (mod.WorkshopId.HasValue && mod.WorkshopId.Value.ToString().Contains(_searchText.ToLowerInvariant()))).ToList();

            var sortedLocal = ApplySort(localFiltered, true);

            LocalMods.Clear();
            foreach (var mod in sortedLocal)
                LocalMods.Add(mod);

            var workshopFiltered = string.IsNullOrWhiteSpace(_searchText)
                ? _allWorkshopMods.ToList()
                : _allWorkshopMods.Where(mod =>
                    (mod.DisplayName != null && mod.DisplayName.ToLowerInvariant().Contains(_searchText.ToLowerInvariant())) ||
                    (mod.WorkshopId.ToString().Contains(_searchText.ToLowerInvariant()))).ToList();

            var sortedWorkshop = ApplySort(workshopFiltered, false);

            WorkshopMods.Clear();
            foreach (var mod in sortedWorkshop)
                WorkshopMods.Add(mod);
        }

        private IEnumerable<LocalModItem> ApplySort(IEnumerable<LocalModItem> items, bool isLocal)
        {
            return _currentSortOption switch
            {
                ModSortOption.Name => _isSortAscending
                    ? items.OrderBy(m => m.DisplayName ?? "")
                    : items.OrderByDescending(m => m.DisplayName ?? ""),
                ModSortOption.LastModified => _isSortAscending
                    ? items.OrderBy(m => m.LastModified)
                    : items.OrderByDescending(m => m.LastModified),
                ModSortOption.WorkshopId when isLocal => _isSortAscending
                    ? items.OrderBy(m => m.WorkshopId ?? uint.MaxValue)
                    : items.OrderByDescending(m => m.WorkshopId ?? 0),
                ModSortOption.WorkshopId => _isSortAscending
                    ? items.OrderBy(m => m.DisplayName ?? "")
                    : items.OrderByDescending(m => m.DisplayName ?? ""),
                _ => _isSortAscending
                    ? items.OrderBy(m => m.DisplayName ?? "")
                    : items.OrderByDescending(m => m.DisplayName ?? "")
            };
        }

        private IEnumerable<WorkshopModItem> ApplySort(IEnumerable<WorkshopModItem> items, bool isLocal)
        {
            return _currentSortOption switch
            {
                ModSortOption.Name => _isSortAscending
                    ? items.OrderBy(m => m.DisplayName ?? "")
                    : items.OrderByDescending(m => m.DisplayName ?? ""),
                ModSortOption.LastModified => _isSortAscending
                    ? items.OrderBy(m => m.LastModified)
                    : items.OrderByDescending(m => m.LastModified),
                ModSortOption.WorkshopId => _isSortAscending
                    ? items.OrderBy(m => m.WorkshopId)
                    : items.OrderByDescending(m => m.WorkshopId),
                _ => _isSortAscending
                    ? items.OrderBy(m => m.DisplayName ?? "")
                    : items.OrderByDescending(m => m.DisplayName ?? "")
            };
        }

        public void ToggleSortDirection()
        {
            IsSortAscending = !IsSortAscending;
        }

        public void SelectAllWorkshopMods()
        {
            foreach (var mod in WorkshopMods)
                mod.Selected = true;
        }

        public void DeselectAllWorkshopMods()
        {
            foreach (var mod in WorkshopMods)
                mod.Selected = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}