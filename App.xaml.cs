using System;
using System.IO;
using System.Windows;
using RimWorldModManager.Config;
using RimWorldModManager.Utils;

namespace RimWorldModManager
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeDirectories();

            if (!ValidateSteamCmd())
            {
                System.Windows.MessageBox.Show("未找到 steamcmd.exe，Mod 下载功能将不可用。\n\n请从 https://developer.valvesoftware.com/wiki/SteamCMD 下载 steamcmd.zip，\n解压到解决方案根目录的 steamcmd 文件夹中。", 
                    "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }

            SettingsManager.Load();
        }

        private void InitializeDirectories()
        {
            var modsDir = PathHelper.GetDefaultModsPath();
            if (!Directory.Exists(modsDir))
                Directory.CreateDirectory(modsDir);

            var configDir = PathHelper.GetConfigPath();
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);
        }

        private bool ValidateSteamCmd()
        {
            var settings = SettingsManager.GetCurrent();
            var steamCmdPath = settings.SteamCmdPath ?? PathHelper.GetSteamCmdExePath();
            return File.Exists(steamCmdPath);
        }
    }
}