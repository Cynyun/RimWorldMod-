using System.Collections.Generic;

namespace RimWorldModManager.Config
{
    public class Settings
    {
        public Dictionary<string, string> GamePaths { get; set; } = new();
        public List<string> ModDirectories { get; set; } = new();
        public string SteamCmdPath { get; set; }
    }
}
