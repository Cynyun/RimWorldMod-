using System.IO;
using System.Text.Json;
using RimWorldModManager.Models;

namespace RimWorldModManager.Services
{
    public static class ModCacheManager
    {
        private static readonly string CacheDirectory = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "RimWorldModManager"
        );

        private static readonly string CacheFilePath = Path.Combine(CacheDirectory, "mod_cache.json");

        public static ModCache LoadCache()
        {
            if (File.Exists(CacheFilePath))
            {
                try
                {
                    var json = File.ReadAllText(CacheFilePath);
                    return JsonSerializer.Deserialize<ModCache>(json) ?? new ModCache();
                }
                catch
                {
                    return new ModCache();
                }
            }
            return new ModCache();
        }

        public static void SaveCache(ModCache cache)
        {
            if (!Directory.Exists(CacheDirectory))
            {
                Directory.CreateDirectory(CacheDirectory);
            }

            var json = JsonSerializer.Serialize(cache, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(CacheFilePath, json);
        }

        public static void UpdateWorkshopEntry(ModCache cache, uint workshopId, string displayName)
        {
            if (!cache.WorkshopCache.TryGetValue(workshopId, out var entry))
            {
                entry = new ModCacheEntry { WorkshopId = workshopId };
                cache.WorkshopCache[workshopId] = entry;
            }

            if (!string.IsNullOrEmpty(displayName))
            {
                entry.DisplayName = displayName;
            }

            SaveCache(cache);
        }

        public static void MarkAsImported(ModCache cache, uint workshopId, string localFolderName)
        {
            if (cache.WorkshopCache.TryGetValue(workshopId, out var entry))
            {
                entry.IsImported = true;
                entry.LocalFolderName = localFolderName;
                cache.LocalToWorkshopMap[localFolderName] = workshopId;
                SaveCache(cache);
            }
        }

        public static bool TryGetWorkshopIdByLocalName(ModCache cache, string localName, out uint workshopId)
        {
            return cache.LocalToWorkshopMap.TryGetValue(localName, out workshopId);
        }

        public static string GetDisplayName(ModCache cache, uint workshopId)
        {
            if (cache.WorkshopCache.TryGetValue(workshopId, out var entry) && 
                !string.IsNullOrEmpty(entry.DisplayName))
            {
                return entry.DisplayName;
            }
            return $"未知 Mod (ID: {workshopId})";
        }
    }
}