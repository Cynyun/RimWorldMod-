using RimWorldModManager.Models;
using RimWorldModManager.Utils;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace RimWorldModManager.Services
{
    public class ModScannerService
    {
        private readonly ModCache _cache;

        public ModScannerService(ModCache cache)
        {
            _cache = cache;
        }

        public List<WorkshopModItem> ScanWorkshopMods()
        {
            var settings = Config.SettingsManager.GetCurrent();
            string steamCmdDir = null;

            if (!string.IsNullOrEmpty(settings.SteamCmdPath) && File.Exists(settings.SteamCmdPath))
            {
                steamCmdDir = Path.GetDirectoryName(settings.SteamCmdPath);
            }

            if (string.IsNullOrEmpty(steamCmdDir))
            {
                steamCmdDir = Path.GetDirectoryName(PathHelper.GetSteamCmdExePath());
            }

            if (string.IsNullOrEmpty(steamCmdDir))
            {
                steamCmdDir = Path.Combine(PathHelper.GetAppRoot(), "steamcmd");
            }

            var workshopPath = Path.Combine(steamCmdDir, "steamapps", "workshop", "content", "294100");
            var result = new List<WorkshopModItem>();

            if (!Directory.Exists(workshopPath))
                return result;

            foreach (var dir in Directory.GetDirectories(workshopPath))
            {
                var dirName = Path.GetFileName(dir);
                if (uint.TryParse(dirName, out uint workshopId))
                {
                    var displayName = ModParser.GetModDisplayName(dir);
                    ModCacheManager.UpdateWorkshopEntry(_cache, workshopId, displayName);

                    var cachedName = ModCacheManager.GetDisplayName(_cache, workshopId);
                    var isImported = _cache.WorkshopCache.TryGetValue(workshopId, out var entry) && entry.IsImported;
                    var dirInfo = new DirectoryInfo(dir);

                    result.Add(new WorkshopModItem
                    {
                        WorkshopId = workshopId,
                        DisplayName = cachedName,
                        SourcePath = dir,
                        IsImported = isImported,
                        Selected = false,
                        LastModified = dirInfo.LastWriteTime
                    });
                }
            }

            ModCacheManager.SaveCache(_cache);
            return result.OrderBy(m => m.DisplayName).ToList();
        }

        public List<LocalModItem> ScanLocalMods()
        {
            var settings = Config.SettingsManager.GetCurrent();
            var modDirectories = settings.ModDirectories ?? new List<string>();

            if (!modDirectories.Any())
            {
                modDirectories.Add(PathHelper.GetDefaultModsPath());
            }

            var result = new List<LocalModItem>();

            foreach (var modsPath in modDirectories)
            {
                if (!Directory.Exists(modsPath))
                    continue;

                foreach (var dir in Directory.GetDirectories(modsPath))
                {
                    if (!ModParser.IsValidModDirectory(dir))
                        continue;

                    var dirName = Path.GetFileName(dir);
                    var modInfo = ModParser.ParseFromDirectory(dir);

                    uint? workshopId = null;
                    string workshopSourceInfo = null;

                    if (ModCacheManager.TryGetWorkshopIdByLocalName(_cache, dirName, out uint foundId))
                    {
                        workshopId = foundId;
                        workshopSourceInfo = $"来自 Workshop #{foundId}";
                    }

                    var dirInfo = new DirectoryInfo(dir);

                    result.Add(new LocalModItem
                    {
                        LocalFolderName = dirName,
                        DisplayName = modInfo.Name,
                        Author = modInfo.Author,
                        Version = modInfo.Version,
                        LocalPath = dir,
                        WorkshopId = workshopId,
                        WorkshopSourceInfo = workshopSourceInfo,
                        Selected = false,
                        LastModified = dirInfo.LastWriteTime
                    });
                }
            }

            return result.OrderBy(m => m.DisplayName).ToList();
        }
    }

    public class WorkshopModItem : INotifyPropertyChanged
    {
        private bool _selected;

        public uint WorkshopId { get; set; }
        public string DisplayName { get; set; }
        public string SourcePath { get; set; }
        public bool IsImported { get; set; }
        public DateTime LastModified { get; set; }

        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class LocalModItem
    {
        public string LocalFolderName { get; set; }
        public string DisplayName { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string LocalPath { get; set; }
        public uint? WorkshopId { get; set; }
        public string WorkshopSourceInfo { get; set; }
        public bool Selected { get; set; }
        public DateTime LastModified { get; set; }
    }
}