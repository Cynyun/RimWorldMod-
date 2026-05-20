using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorldModManager.Models;
using RimWorldModManager.Utils;

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
            var workshopPath = PathHelper.GetWorkshopContentPath();
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

                    result.Add(new WorkshopModItem
                    {
                        WorkshopId = workshopId,
                        DisplayName = cachedName,
                        SourcePath = dir,
                        IsImported = isImported,
                        Selected = false
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

                    result.Add(new LocalModItem
                    {
                        LocalFolderName = dirName,
                        DisplayName = modInfo.Name,
                        Author = modInfo.Author,
                        Version = modInfo.Version,
                        LocalPath = dir,
                        WorkshopId = workshopId,
                        WorkshopSourceInfo = workshopSourceInfo,
                        Selected = false
                    });
                }
            }

            return result.OrderBy(m => m.DisplayName).ToList();
        }
    }

    public class WorkshopModItem
    {
        public uint WorkshopId { get; set; }
        public string DisplayName { get; set; }
        public string SourcePath { get; set; }
        public bool IsImported { get; set; }
        public bool Selected { get; set; }
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
    }
}