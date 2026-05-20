using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RimWorldModManager.Config;
using RimWorldModManager.Utils;

namespace RimWorldModManager.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _steamCmdPath;
        private string _modDownloadPath;
        private bool _isCheckingSteamCmd;
        private string _steamCmdStatus;
        private string _steamAccount;
        private string _steamPassword;

        public string SteamAccount
        {
            get => _steamAccount;
            set
            {
                _steamAccount = value;
                OnPropertyChanged();
            }
        }

        public string SteamPassword
        {
            get => _steamPassword;
            set
            {
                _steamPassword = value;
                OnPropertyChanged();
            }
        }

        public string SteamCmdPath
        {
            get => _steamCmdPath;
            set
            {
                _steamCmdPath = value;
                OnPropertyChanged();
            }
        }

        public string ModDownloadPath
        {
            get => _modDownloadPath;
            set
            {
                _modDownloadPath = value;
                OnPropertyChanged();
            }
        }

        public bool IsCheckingSteamCmd
        {
            get => _isCheckingSteamCmd;
            set
            {
                _isCheckingSteamCmd = value;
                OnPropertyChanged();
            }
        }

        public string SteamCmdStatus
        {
            get => _steamCmdStatus;
            set
            {
                _steamCmdStatus = value;
                OnPropertyChanged();
            }
        }

        public SettingsViewModel()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            var settings = SettingsManager.GetCurrent();
            SteamCmdPath = settings.SteamCmdPath ?? PathHelper.GetSteamCmdExePath();
            
            if (settings.ModDirectories != null && settings.ModDirectories.Count > 0)
            {
                ModDownloadPath = settings.ModDirectories[0];
            }
            else
            {
                ModDownloadPath = PathHelper.GetDefaultModsPath();
            }

            SteamAccount = "anonymous";
            SteamPassword = "anonymous";
        }

        public void SaveSettings()
        {
            var settings = SettingsManager.GetCurrent();
            settings.SteamCmdPath = SteamCmdPath;
            
            if (settings.ModDirectories == null)
                settings.ModDirectories = new List<string>();
            
            if (settings.ModDirectories.Count == 0)
            {
                settings.ModDirectories.Add(ModDownloadPath);
            }
            else
            {
                settings.ModDirectories[0] = ModDownloadPath;
            }
            
            SettingsManager.Save();
        }

        public async Task<bool> TestSteamCmdAsync()
        {
            IsCheckingSteamCmd = true;
            SteamCmdStatus = "正在检测 SteamCMD...";

            try
            {
                if (string.IsNullOrWhiteSpace(SteamCmdPath) || !File.Exists(SteamCmdPath))
                {
                    SteamCmdStatus = "SteamCMD 路径不存在";
                    IsCheckingSteamCmd = false;
                    return false;
                }

                using var process = new Process();
                process.StartInfo.FileName = SteamCmdPath;
                process.StartInfo.Arguments = "+quit";
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(SteamCmdPath);
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                var tcs = new TaskCompletionSource<bool>();
                process.Exited += (sender, e) =>
                {
                    tcs.SetResult(process.ExitCode == 0);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var timeoutTask = Task.Delay(30000);
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    try { process.Kill(); } catch { }
                    SteamCmdStatus = "检测超时";
                    IsCheckingSteamCmd = false;
                    return false;
                }

                var result = await tcs.Task;
                SteamCmdStatus = result ? "SteamCMD 检测成功" : "SteamCMD 执行失败";
                IsCheckingSteamCmd = false;
                return result;
            }
            catch (Exception ex)
            {
                SteamCmdStatus = $"检测异常: {ex.Message}";
                IsCheckingSteamCmd = false;
                return false;
            }
        }

        public bool ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(SteamCmdPath) || !File.Exists(SteamCmdPath))
                return false;
            
            if (string.IsNullOrWhiteSpace(ModDownloadPath))
                return false;
            
            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
