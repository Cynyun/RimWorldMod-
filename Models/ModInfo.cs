namespace RimWorldModManager.Models
{
    public class ModInfo
    {
        public uint WorkshopId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LocalPath { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsEnabled { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string[] Tags { get; set; }
        public string PreviewImagePath { get; set; }
    }
}
