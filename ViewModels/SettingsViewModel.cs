using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using RimWorldModManager.Config;
using RimWorldModManager.Utils;

namespace RimWorldModManager.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _steamCmdPath;
        private string _modDownloadPath;

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
