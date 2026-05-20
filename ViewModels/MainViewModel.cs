using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RimWorldModManager.Models;
using RimWorldModManager.Services;

namespace RimWorldModManager.ViewModels
{
    public enum ModViewMode
    {
        LocalMods,
        WorkshopMods
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ModScannerService _scannerService;
        private ModViewMode _currentViewMode;
        private string _statusMessage = string.Empty;
        private string _steamCmdOutput = string.Empty;
        private bool _isLoading;
        private string _searchText = string.Empty;
        private ObservableCollection<WorkshopModItem> _allWorkshopMods = new();
        private ObservableCollection<LocalModItem> _allLocalMods = new();
        private ModDetailViewModel _selectedModDetail = new();

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
                ApplySearchFilter();
            }
        }

        public bool IsLocalModsView => CurrentViewMode == ModViewMode.LocalMods;
        public bool IsWorkshopModsView => CurrentViewMode == ModViewMode.WorkshopMods;

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
            IsLoading = true;
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
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplySearchFilter()
        {
            LocalMods.Clear();
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                foreach (var mod in _allLocalMods)
                    LocalMods.Add(mod);
            }
            else
            {
                var searchLower = _searchText.ToLowerInvariant();
                foreach (var mod in _allLocalMods)
                {
                    if ((mod.DisplayName != null && mod.DisplayName.ToLowerInvariant().Contains(searchLower)) ||
                        (mod.Author != null && mod.Author.ToLowerInvariant().Contains(searchLower)) ||
                        (mod.WorkshopId.HasValue && mod.WorkshopId.Value.ToString().Contains(searchLower)))
                    {
                        LocalMods.Add(mod);
                    }
                }
            }

            WorkshopMods.Clear();
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                foreach (var mod in _allWorkshopMods)
                    WorkshopMods.Add(mod);
            }
            else
            {
                var searchLower = _searchText.ToLowerInvariant();
                foreach (var mod in _allWorkshopMods)
                {
                    if ((mod.DisplayName != null && mod.DisplayName.ToLowerInvariant().Contains(searchLower)) ||
                        (mod.WorkshopId.ToString().Contains(searchLower)))
                    {
                        WorkshopMods.Add(mod);
                    }
                }
            }
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