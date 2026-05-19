using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RimWorldModManager.Config;
using RimWorldModManager.Models;
using RimWorldModManager.Services;
using RimWorldModManager.Utils;

namespace RimWorldModManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly SteamCmdService _steamService;
        private readonly ILogger _logger;
        private bool _isLoading;
        private string _statusMessage = string.Empty;
        private ModInfo _selectedMod;
        private bool _isRefreshing;

        public ObservableCollection<ModInfo> Mods { get; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ModInfo SelectedMod
        {
            get => _selectedMod;
            set
            {
                _selectedMod = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedMod));
            }
        }

        public bool HasSelectedMod => SelectedMod != null;

        public MainViewModel()
        {
            _logger = new ConsoleLogger();
            var settings = SettingsManager.Load();
            _steamService = new SteamCmdService(settings.SteamCmdPath ?? PathHelper.GetSteamCmdExePath(), _logger);
        }

        public async Task AddModAsync(uint workshopId)
        {
            IsLoading = true;
            StatusMessage = "正在下载 Mod...";

            try
            {
                var settings = SettingsManager.GetCurrent();
                var installDir = settings.ModDirectories.Count > 0 
                    ? settings.ModDirectories[0] 
                    : PathHelper.GetDefaultModsPath();

                if (!Directory.Exists(installDir))
                    Directory.CreateDirectory(installDir);

                var result = await _steamService.DownloadModAsync(workshopId, installDir);

                if (result.ExitCode == 0 && !result.TimedOut)
                {
                    var mod = ModParser.ParseFromWorkshopId(workshopId, installDir);
                    if (mod != null)
                    {
                        var existingMod = Mods.FirstOrDefault(m => m.WorkshopId == workshopId);
                        if (existingMod != null)
                        {
                            existingMod.Name = mod.Name;
                            existingMod.Description = mod.Description;
                            existingMod.LocalPath = mod.LocalPath;
                            existingMod.LastUpdated = mod.LastUpdated;
                            existingMod.Author = mod.Author;
                            existingMod.Version = mod.Version;
                            existingMod.Tags = mod.Tags;
                            existingMod.PreviewImagePath = mod.PreviewImagePath;
                        }
                        else
                        {
                            Mods.Add(mod);
                        }
                        StatusMessage = $"Mod 下载完成: {mod.Name}";
                    }
                    else
                    {
                        StatusMessage = "Mod 下载成功，但无法解析 Mod 信息";
                    }
                }
                else
                {
                    var errorMsg = result.TimedOut ? "下载超时" : 
                        (string.IsNullOrEmpty(result.StandardError) ? "下载失败" : result.StandardError);
                    StatusMessage = $"下载失败: {errorMsg}";
                    _logger.Error(errorMsg);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"下载异常: {ex.Message}";
                _logger.Error("下载 Mod 异常", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task UpdateModAsync(uint workshopId)
        {
            IsLoading = true;
            StatusMessage = "正在更新 Mod...";

            try
            {
                var settings = SettingsManager.GetCurrent();
                var installDir = settings.ModDirectories.Count > 0 
                    ? settings.ModDirectories[0] 
                    : PathHelper.GetDefaultModsPath();

                var result = await _steamService.DownloadModAsync(workshopId, installDir);

                if (result.ExitCode == 0 && !result.TimedOut)
                {
                    var mod = ModParser.ParseFromWorkshopId(workshopId, installDir);
                    if (mod != null)
                    {
                        var existingMod = Mods.FirstOrDefault(m => m.WorkshopId == workshopId);
                        if (existingMod != null)
                        {
                            existingMod.Name = mod.Name;
                            existingMod.Description = mod.Description;
                            existingMod.LocalPath = mod.LocalPath;
                            existingMod.LastUpdated = mod.LastUpdated;
                            existingMod.Author = mod.Author;
                            existingMod.Version = mod.Version;
                            existingMod.Tags = mod.Tags;
                            existingMod.PreviewImagePath = mod.PreviewImagePath;
                        }
                        StatusMessage = $"Mod 更新完成: {mod.Name}";
                    }
                    else
                    {
                        StatusMessage = "Mod 更新成功，但无法解析 Mod 信息";
                    }
                }
                else
                {
                    var errorMsg = result.TimedOut ? "更新超时" : 
                        (string.IsNullOrEmpty(result.StandardError) ? "更新失败" : result.StandardError);
                    StatusMessage = $"更新失败: {errorMsg}";
                    _logger.Error(errorMsg);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"更新异常: {ex.Message}";
                _logger.Error("更新 Mod 异常", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void LoadMods()
        {
            Mods.Clear();

            try
            {
                var settings = SettingsManager.GetCurrent();
                foreach (var modDir in settings.ModDirectories)
                {
                    if (Directory.Exists(modDir))
                    {
                        foreach (var dir in Directory.GetDirectories(modDir))
                        {
                            var dirName = Path.GetFileName(dir);
                            if (uint.TryParse(dirName, out uint workshopId))
                            {
                                var mod = ModParser.ParseFromDirectory(dir);
                                mod.WorkshopId = workshopId;
                                Mods.Add(mod);
                            }
                            else
                            {
                                var mod = ModParser.ParseFromDirectory(dir);
                                Mods.Add(mod);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("加载 Mod 列表失败", ex);
            }
        }

        public async Task RefreshModsAsync()
        {
            IsRefreshing = true;
            StatusMessage = "正在刷新 Mod 列表...";

            try
            {
                await Task.Delay(100);
                LoadMods();
                StatusMessage = $"已加载 {Mods.Count} 个 Mod";
            }
            catch (Exception ex)
            {
                StatusMessage = $"刷新失败: {ex.Message}";
                _logger.Error("刷新 Mod 列表失败", ex);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public void SelectMod(ModInfo mod)
        {
            SelectedMod = mod;
        }

        public void ClearSelection()
        {
            SelectedMod = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
