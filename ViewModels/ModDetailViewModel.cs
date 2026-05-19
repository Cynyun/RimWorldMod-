using System.ComponentModel;
using System.Runtime.CompilerServices;
using RimWorldModManager.Models;

namespace RimWorldModManager.ViewModels
{
    public class ModDetailViewModel : INotifyPropertyChanged
    {
        private ModInfo _mod;

        public ModInfo Mod
        {
            get => _mod;
            set
            {
                _mod = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMod));
                OnPropertyChanged(nameof(WorkshopUrl));
                OnPropertyChanged(nameof(AuthorDisplay));
                OnPropertyChanged(nameof(VersionDisplay));
                OnPropertyChanged(nameof(DescriptionDisplay));
                OnPropertyChanged(nameof(TagsDisplay));
                OnPropertyChanged(nameof(LastUpdatedDisplay));
            }
        }

        public bool HasMod => Mod != null;

        public string WorkshopUrl => Mod != null 
            ? $"https://steamcommunity.com/workshop/filedetails/?id={Mod.WorkshopId}" 
            : string.Empty;

        public string AuthorDisplay => Mod?.Author ?? "未知";

        public string VersionDisplay => Mod?.Version ?? "未知";

        public string DescriptionDisplay => Mod?.Description ?? "暂无描述";

        public string TagsDisplay => Mod?.Tags != null && Mod.Tags.Length > 0 
            ? string.Join(", ", Mod.Tags) 
            : "无";

        public string LastUpdatedDisplay => Mod?.LastUpdated != DateTime.MinValue 
            ? Mod.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss") 
            : "未知";

        public void LoadMod(ModInfo mod)
        {
            Mod = mod;
        }

        public void Clear()
        {
            Mod = null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
