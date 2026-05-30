using System.IO;

namespace RimWorldModManager.Utils
{
    public static class PathHelper
    {
        public static string GetAppRoot()
        {
            return AppContext.BaseDirectory;
        }

        public static string GetSteamCmdExePath()
        {
            return Path.Combine(GetAppRoot(), "steamcmd", "steamcmd.exe");
        }

        public static string GetDefaultModsPath()
        {
            return Path.Combine(GetAppRoot(), "mods");
        }

        public static string GetConfigPath()
        {
            return Path.Combine(GetAppRoot(), "config");
        }

        public static string GetWorkshopContentPath()
        {
            var steamCmdPath = GetSteamCmdExePath();
            var steamCmdDir = Path.GetDirectoryName(steamCmdPath);

            if (string.IsNullOrEmpty(steamCmdDir))
            {
                steamCmdDir = Path.Combine(GetAppRoot(), "steamcmd");
            }

            var workshopPath = Path.Combine(steamCmdDir, "steamapps", "workshop", "content", "294100");

            return workshopPath;
        }

        public static string GetSettingsFilePath()
        {
            return Path.Combine(GetConfigPath(), "settings.json");
        }
    }
}