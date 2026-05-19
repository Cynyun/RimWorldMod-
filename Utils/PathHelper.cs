using System.IO;

namespace RimWorldModManager.Utils
{
    public static class PathHelper
    {
        public static string GetSolutionRoot()
        {
            var baseDir = AppContext.BaseDirectory;
            return Path.GetFullPath(Path.Combine(baseDir, "..", ".."));
        }

        public static string GetSteamCmdExePath()
        {
            return Path.Combine(GetSolutionRoot(), "steamcmd", "steamcmd.exe");
        }

        public static string GetDefaultModsPath()
        {
            return Path.Combine(GetSolutionRoot(), "mods");
        }

        public static string GetConfigPath()
        {
            return Path.Combine(GetSolutionRoot(), "config");
        }

        public static string GetSettingsFilePath()
        {
            return Path.Combine(GetConfigPath(), "settings.json");
        }
    }
}