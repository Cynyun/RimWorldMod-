using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RimWorldModManager.Models;

namespace RimWorldModManager.ViewModels
{
    public class ModDetailViewModel : INotifyPropertyChanged
    {
        private ModInfo _mod;
        private string _name = string.Empty;
        private string _author = string.Empty;
        private string _version = string.Empty;
        private string _workshopId = string.Empty;
        private string _description = string.Empty;
        private string _tagsString = string.Empty;
        private string _localPath = string.Empty;
        private string _previewImagePath = string.Empty;

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

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Author
        {
            get => _author;
            set
            {
                _author = value;
                OnPropertyChanged();
            }
        }

        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged();
            }
        }

        public string WorkshopId
        {
            get => _workshopId;
            set
            {
                _workshopId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasWorkshopId));
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public string TagsString
        {
            get => _tagsString;
            set
            {
                _tagsString = value;
                OnPropertyChanged();
            }
        }

        public string LocalPath
        {
            get => _localPath;
            set
            {
                _localPath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasLocalPath));
            }
        }

        public string PreviewImagePath
        {
            get => _previewImagePath;
            set
            {
                _previewImagePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPreviewImage));
            }
        }

        public bool HasLocalPath => !string.IsNullOrEmpty(LocalPath) && System.IO.Directory.Exists(LocalPath);
        public bool HasPreviewImage => !string.IsNullOrEmpty(PreviewImagePath) && System.IO.File.Exists(PreviewImagePath);
        public bool HasWorkshopId => !string.IsNullOrEmpty(WorkshopId) && uint.TryParse(WorkshopId, out _);

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
            Name = string.Empty;
            Author = string.Empty;
            Version = string.Empty;
            WorkshopId = string.Empty;
            Description = string.Empty;
            TagsString = string.Empty;
            LocalPath = string.Empty;
            PreviewImagePath = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}