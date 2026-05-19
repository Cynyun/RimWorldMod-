using System.IO;
using System.Text.Json;
using RimWorldModManager.Utils;

namespace RimWorldModManager.Config
{
    public static class SettingsManager
    {
        private static Settings _settings;

        public static Settings Load()
        {
            var settingsPath = PathHelper.GetSettingsFilePath();

            if (File.Exists(settingsPath))
            {
                try
                {
                    var json = File.ReadAllText(settingsPath);
                    _settings = JsonSerializer.Deserialize<Settings>(json);
                }
                catch
                {
                    _settings = CreateDefaultSettings();
                }
            }
            else
            {
                _settings = CreateDefaultSettings();
                Save();
            }

            return _settings;
        }

        public static void Save()
        {
            if (_settings == null)
                _settings = CreateDefaultSettings();

            var configDir = PathHelper.GetConfigPath();
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);

            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(PathHelper.GetSettingsFilePath(), json);
        }

        private static Settings CreateDefaultSettings()
        {
            return new Settings
            {
                GamePaths = new()
                {
                    { "RimWorld", @"C:\Program Files (x86)\Steam\steamapps\common\RimWorld" }
                },
                ModDirectories = new()
                {
                    PathHelper.GetDefaultModsPath()
                },
                SteamCmdPath = PathHelper.GetSteamCmdExePath()
            };
        }

        public static Settings GetCurrent()
        {
            return _settings ?? Load();
        }
    }
}