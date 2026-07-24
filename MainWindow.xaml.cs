using RimWorldModManager.Models;
using RimWorldModManager.Services;
using RimWorldModManager.Utils;
using RimWorldModManager.ViewModels;
using RimWorldModManager.Views;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Wpf.Ui.Controls;

namespace RimWorldModManager
{
    public partial class MainWindow : FluentWindow
    {
        private readonly MainViewModel _viewModel;
        private readonly ModCache _cache;
        private readonly ModScannerService _scannerService;
        private readonly SteamCmdService _steamService;

        public MainWindow()
        {
            InitializeComponent();
            _cache = ModCacheManager.LoadCache();
            _scannerService = new ModScannerService(_cache);
            _viewModel = new MainViewModel(_scannerService);
            DataContext = _viewModel;

            var settings = Config.SettingsManager.Load();
            _steamService = new SteamCmdService(settings.SteamCmdPath ?? PathHelper.GetSteamCmdExePath(), new ConsoleLogger());
            _steamService.SetOutputCallback(OnSteamCmdOutput);
            _steamService.LoginStatusChanged += OnLoginStatusChanged;

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
            LocalModsToggle.IsChecked = true;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.StatusMessage = "正在初始化 SteamCMD...";
            var success = await _steamService.StartAsync();
            if (success && _steamService.IsLoggedIn)
            {
                _viewModel.StatusMessage = "SteamCMD 已连接";
            }
            else
            {
                _viewModel.StatusMessage = "SteamCMD 连接失败，请检查设置";
            }

            await _viewModel.RefreshModsAsync();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _steamService.Dispose();
        }

        private void OnSteamCmdOutput(string output)
        {
            Dispatcher.Invoke(() =>
            {
                _viewModel.SteamCmdOutput = output;
            });
        }

        private void OnLoginStatusChanged(bool isLoggedIn)
        {
            Dispatcher.Invoke(() =>
            {
                _viewModel.StatusMessage = isLoggedIn ? "SteamCMD 已连接" : "SteamCMD 已断开";
            });
        }

        private void LocalModsButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CurrentViewMode = ModViewMode.LocalMods;
            LocalModsToggle.IsChecked = true;
            WorkshopModsToggle.IsChecked = false;
            _viewModel.SelectedModDetail = new ModDetailViewModel();
        }

        private void WorkshopModsButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CurrentViewMode = ModViewMode.WorkshopMods;
            WorkshopModsToggle.IsChecked = true;
            LocalModsToggle.IsChecked = false;
            _viewModel.SelectedModDetail = new ModDetailViewModel();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshModsAsync();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectAllWorkshopMods();
        }

        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.DeselectAllWorkshopMods();
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = _viewModel.WorkshopMods.Where(m => m.Selected).ToList();

            if (selectedItems.Count == 0)
            {
                _viewModel.StatusMessage = "请先选中要导入的 Mod";
                return;
            }

            _viewModel.IsLoading = true;
            _viewModel.StatusMessage = "正在检测冲突...";

            try
            {
                var importService = new ModImportService(_cache);
                var conflicts = importService.DetectConflicts(selectedItems);

                FileConflictAction selectedAction = FileConflictAction.Replace;
                bool askForEach = true;

                if (conflicts.Count == 0)
                {
                    askForEach = false;
                    selectedAction = FileConflictAction.Replace;
                }
                else if (conflicts.Count == 1)
                {
                    var dialog = new FileConflictDialog(conflicts[0].ModItem.DisplayName);
                    dialog.Owner = this;
                    if (dialog.ShowDialog() == true)
                    {
                        selectedAction = dialog.SelectedAction;
                        if (dialog.ApplyToAll)
                        {
                            askForEach = false;
                        }
                    }
                    else
                    {
                        _viewModel.IsLoading = false;
                        _viewModel.StatusMessage = "导入已取消";
                        return;
                    }
                }
                else
                {
                    var batchDialog = new BatchImportOptionsDialog(selectedItems.Count, conflicts.Count);
                    batchDialog.Owner = this;
                    if (batchDialog.ShowDialog() != true)
                    {
                        _viewModel.IsLoading = false;
                        _viewModel.StatusMessage = "导入已取消";
                        return;
                    }

                    selectedAction = batchDialog.SelectedAction;
                    askForEach = batchDialog.AskForEachConflict;
                }

                _viewModel.StatusMessage = "正在导入 Mod...";

                importService.SetBatchImportOptions(selectedAction, askForEach);

                Func<string, FileConflictAction> conflictHandler = null;
                if (askForEach)
                {
                    conflictHandler = (modName) =>
                    {
                        FileConflictAction action = FileConflictAction.Replace;
                        Dispatcher.Invoke(() =>
                        {
                            var dialog = new FileConflictDialog(modName);
                            dialog.Owner = this;
                            if (dialog.ShowDialog() == true)
                            {
                                action = dialog.SelectedAction;
                            }
                            else
                            {
                                action = FileConflictAction.Cancel;
                            }
                        });
                        return action;
                    };
                }

                var result = await System.Threading.Tasks.Task.Run(() => 
                {
                    return importService.ImportMods(selectedItems, conflictHandler);
                });

                if (result.Cancelled)
                {
                    _viewModel.StatusMessage = "导入已取消";
                }
                else
                {
                    var messages = new List<string>();
                    if (result.SuccessCount > 0)
                        messages.Add($"{result.SuccessCount} 个成功");
                    if (result.SkippedCount > 0)
                        messages.Add($"{result.SkippedCount} 个跳过");
                    if (result.FailureCount > 0)
                        messages.Add($"{result.FailureCount} 个失败");

                    if (messages.Count > 0)
                    {
                        _viewModel.StatusMessage = string.Join("，", messages);
                    }
                    else
                    {
                        _viewModel.StatusMessage = "导入完成";
                    }
                }

                await _viewModel.RefreshModsAsync();
            }
            catch (System.Exception ex)
            {
                _viewModel.StatusMessage = $"导入失败: {ex.Message}";
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }

        private async void UpdateSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = _viewModel.WorkshopMods.Where(m => m.Selected).ToList();

            if (selectedItems.Count == 0)
            {
                _viewModel.StatusMessage = "请先选中要更新的 Mod";
                return;
            }

            _viewModel.IsLoading = true;
            _viewModel.CurrentProgress = 0;
            _viewModel.TotalProgress = selectedItems.Count;

            int successCount = 0;
            int failureCount = 0;

            try
            {
                for (int i = 0; i < selectedItems.Count; i++)
                {
                    var item = selectedItems[i];
                    int currentIndex = i + 1;
                    
                    _viewModel.CurrentModName = $"正在更新 Mod {item.WorkshopId}";
                    _viewModel.ProgressText = $"第 {currentIndex} 个/共 {selectedItems.Count} 个";
                    _viewModel.CurrentProgress = currentIndex;

                    var result = await _steamService.DownloadModAsync(item.WorkshopId);

                    if (result.ExitCode == 0 && !result.TimedOut)
                    {
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                    }
                }

                string updateResultMessage;
                if (successCount > 0 && failureCount == 0)
                {
                    updateResultMessage = $"{successCount} 个 Mod 更新成功";
                }
                else if (successCount > 0 && failureCount > 0)
                {
                    updateResultMessage = $"{successCount} 个成功，{failureCount} 个失败";
                }
                else
                {
                    updateResultMessage = $"{failureCount} 个 Mod 更新失败";
                }

                _viewModel.StatusMessage = updateResultMessage;
                await Task.Delay(1500);

                _viewModel.StatusMessage = "更新完成，正在刷新 Mod 列表...";
                await _viewModel.RefreshModsAsync();

                _viewModel.StatusMessage = $"{updateResultMessage} - 已刷新 Mod 列表";
            }
            catch (System.Exception ex)
            {
                _viewModel.StatusMessage = $"更新失败: {ex.Message}";
            }
            finally
            {
                _viewModel.IsLoading = false;
                _viewModel.CurrentProgress = 0;
                _viewModel.TotalProgress = 0;
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!uint.TryParse(WorkshopIdTextBox.Text, out uint workshopId))
            {
                _viewModel.StatusMessage = "请输入有效的 Workshop ID";
                return;
            }

            _viewModel.IsLoading = true;
            _viewModel.StatusMessage = $"正在下载 Mod {workshopId}...";

            try
            {
                var result = await _steamService.DownloadModAsync(workshopId);

                if (result.ExitCode == 0 && !result.TimedOut)
                {
                    _viewModel.StatusMessage = $"Mod {workshopId} 下载完成";
                    await _viewModel.RefreshModsAsync();
                }
                else
                {
                    var errorMsg = result.TimedOut ? "下载超时" :
                        (string.IsNullOrEmpty(result.StandardError) ? "下载失败" : result.StandardError);
                    _viewModel.StatusMessage = $"下载失败: {errorMsg}";
                }
            }
            catch (Exception ex)
            {
                _viewModel.StatusMessage = $"下载异常: {ex.Message}";
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }

        private void LocalModsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LocalModsList.SelectedItem is LocalModItem item)
            {
                var modInfo = ModParser.ParseFromDirectory(item.LocalPath);
                _viewModel.SelectedModDetail = new ModDetailViewModel
                {
                    Name = item.DisplayName,
                    Author = item.Author,
                    Version = item.Version,
                    WorkshopId = item.WorkshopId?.ToString() ?? string.Empty,
                    Description = modInfo?.Description ?? string.Empty,
                    TagsString = modInfo?.Tags != null ? string.Join(", ", modInfo.Tags) : string.Empty,
                    LocalPath = item.LocalPath,
                    PreviewImagePath = modInfo?.PreviewImagePath ?? string.Empty
                };
            }
        }

        private void WorkshopModsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (WorkshopModsList.SelectedItem is WorkshopModItem item)
            {
                var modInfo = ModParser.ParseFromDirectory(item.SourcePath);
                _viewModel.SelectedModDetail = new ModDetailViewModel
                {
                    Name = item.DisplayName,
                    Author = modInfo?.Author ?? string.Empty,
                    Version = modInfo?.Version ?? string.Empty,
                    WorkshopId = item.WorkshopId.ToString(),
                    Description = modInfo?.Description ?? string.Empty,
                    TagsString = modInfo?.Tags != null ? string.Join(", ", modInfo.Tags) : string.Empty,
                    LocalPath = item.SourcePath,
                    PreviewImagePath = modInfo?.PreviewImagePath ?? string.Empty
                };
            }
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string pathToOpen = null;

            if (!string.IsNullOrEmpty(_viewModel.SelectedModDetail.LocalPath) &&
                System.IO.Directory.Exists(_viewModel.SelectedModDetail.LocalPath))
            {
                pathToOpen = _viewModel.SelectedModDetail.LocalPath;
            }
            else
            {
                var settings = Config.SettingsManager.Load();
                var localModsPath = settings.ModDirectories.FirstOrDefault();
                if (string.IsNullOrEmpty(localModsPath))
                {
                    localModsPath = PathHelper.GetDefaultModsPath();
                }

                if (System.IO.Directory.Exists(localModsPath))
                {
                    pathToOpen = localModsPath;
                }
            }

            if (!string.IsNullOrEmpty(pathToOpen))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = pathToOpen,
                    UseShellExecute = true
                });
            }
        }

        private void OpenWorkshopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_viewModel.SelectedModDetail.WorkshopId) &&
                uint.TryParse(_viewModel.SelectedModDetail.WorkshopId, out uint workshopId))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"https://steamcommunity.com/workshop/filedetails/?id={workshopId}",
                    UseShellExecute = true
                });
            }
        }

        private void SortComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_viewModel == null)
                return;

            if (SortComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item)
            {
                var sortText = item.Content?.ToString();
                _viewModel.CurrentSortOption = sortText switch
                {
                    "名称" => ModSortOption.Name,
                    "修改时间" => ModSortOption.LastModified,
                    "Workshop ID" => ModSortOption.WorkshopId,
                    _ => ModSortOption.Name
                };
            }
        }

        private void SortDirectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null)
                return;

            _viewModel.ToggleSortDirection();
        }

        private async void BatchDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BatchDownloadDialog();
            dialog.Owner = this;
            
            if (dialog.ShowDialog() != true)
                return;

            var workshopIds = dialog.SelectedWorkshopIds;
            if (workshopIds.Count == 0)
                return;

            _viewModel.IsLoading = true;
            _viewModel.CurrentProgress = 0;
            _viewModel.TotalProgress = workshopIds.Count;

            int successCount = 0;
            int failureCount = 0;

            try
            {
                for (int i = 0; i < workshopIds.Count; i++)
                {
                    uint workshopId = workshopIds[i];
                    int currentIndex = i + 1;
                    
                    _viewModel.CurrentModName = $"正在下载 Mod {workshopId}";
                    _viewModel.ProgressText = $"第 {currentIndex} 个/共 {workshopIds.Count} 个";
                    _viewModel.CurrentProgress = currentIndex;

                    var result = await _steamService.DownloadModAsync(workshopId);

                    if (result.ExitCode == 0 && !result.TimedOut)
                    {
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                    }
                }

                string downloadResultMessage;
                if (successCount > 0 && failureCount == 0)
                {
                    downloadResultMessage = $"{successCount} 个 Mod 下载成功";
                }
                else if (successCount > 0 && failureCount > 0)
                {
                    downloadResultMessage = $"{successCount} 个成功，{failureCount} 个失败";
                }
                else
                {
                    downloadResultMessage = $"{failureCount} 个 Mod 下载失败";
                }

                _viewModel.StatusMessage = downloadResultMessage;
                await Task.Delay(1500);

                _viewModel.StatusMessage = "下载完成，正在刷新 Mod 列表...";
                await _viewModel.RefreshModsAsync();

                _viewModel.StatusMessage = $"{downloadResultMessage} - 已刷新 Mod 列表";
            }
            catch (Exception ex)
            {
                _viewModel.StatusMessage = $"下载失败: {ex.Message}";
            }
            finally
            {
                _viewModel.IsLoading = false;
                _viewModel.CurrentProgress = 0;
                _viewModel.TotalProgress = 0;
            }
        }
    }
}