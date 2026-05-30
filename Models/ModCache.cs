namespace RimWorldModManager.Models
{
    public class ModCacheEntry
    {
        public uint WorkshopId { get; set; }
        public string DisplayName { get; set; }
        public string LocalFolderName { get; set; }
        public bool IsImported { get; set; }
        public string LastUpdated { get; set; }
    }

    public class ModCache
    {
        public Dictionary<uint, ModCacheEntry> WorkshopCache { get; set; } = new();
        public Dictionary<string, uint> LocalToWorkshopMap { get; set; } = new();
    }
}