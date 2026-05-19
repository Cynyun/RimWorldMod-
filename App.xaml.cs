using System;
using System.IO;
using System.Windows;
using RimWorldModManager.Config;
using RimWorldModManager.Utils;

namespace RimWorldModManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeDirectories();

            if (!ValidateSteamCmd())
            {
                MessageBox.Show("未找到 steamcmd.exe，请确保 steamcmd 目录已正确放置在解决方案根目录下。", 
                    "初始化错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
            var steamCmdPath = PathHelper.GetSteamCmdExePath();
            return File.Exists(steamCmdPath);
        }
    }
}