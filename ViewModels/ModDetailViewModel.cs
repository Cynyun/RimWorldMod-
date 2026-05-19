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

        public string AuthorDisplay
        {
            get
            {
                if (Mod != null && !string.IsNullOrEmpty(Mod.Author))
                    return Mod.Author;
                return "未知";
            }
        }

        public string VersionDisplay
        {
            get
            {
                if (Mod != null && !string.IsNullOrEmpty(Mod.Version))
                    return Mod.Version;
                return "未知";
            }
        }

        public string DescriptionDisplay
        {
            get
            {
                if (Mod != null && !string.IsNullOrEmpty(Mod.Description))
                    return Mod.Description;
                return "暂无描述";
            }
        }

        public string TagsDisplay
        {
            get
            {
                if (Mod != null && Mod.Tags != null && Mod.Tags.Length > 0)
                    return string.Join(", ", Mod.Tags);
                return "无";
            }
        }

        public string LastUpdatedDisplay
        {
            get
            {
                if (Mod != null && Mod.LastUpdated != DateTime.MinValue)
                    return Mod.LastUpdated.ToString("yyyy-MM-dd HH:mm:ss");
                return "未知";
            }
        }

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
