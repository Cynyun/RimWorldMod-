using RimWorldModManager.Config;
using RimWorldModManager.Utils;
using System.IO;
using System.Windows;

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
                System.Windows.MessageBox.Show("未找到 steamcmd.exe，Mod 下载功能将不可用。\n\n请从 https://developer.valvesoftware.com/wiki/SteamCMD 下载 steamcmd.zip，\n然后修改设置中 steamcmd.exe 的路径为您下载的 steamcmd.exe 所在位置。",
                    "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }

            SettingsManager.Load();
        }

        private void InitializeDirectories()
        {
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