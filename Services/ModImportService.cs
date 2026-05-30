using RimWorldModManager.Models;
using RimWorldModManager.Utils;
using System.IO;

namespace RimWorldModManager.Services
{
    public class ModImportService
    {
        private readonly ModCache _cache;
        private FileConflictAction _defaultConflictAction;
        private bool _applyAllAction;
        private bool _askForEachConflict;

        public ModImportService(ModCache cache)
        {
            _cache = cache;
            _defaultConflictAction = FileConflictAction.Replace;
            _applyAllAction = false;
            _askForEachConflict = true;
        }

        public void SetBatchImportOptions(FileConflictAction action, bool askForEach)
        {
            _askForEachConflict = askForEach;
            _defaultConflictAction = action;
            _applyAllAction = !askForEach;
        }

        public ImportResult ImportMods(IEnumerable<WorkshopModItem> items, 
            Func<string, FileConflictAction> conflictHandler = null)
        {
            var result = new ImportResult();

            foreach (var item in items)
            {
                try
                {
                    if (string.IsNullOrEmpty(item.SourcePath) || !Directory.Exists(item.SourcePath))
                    {
                        result.FailureCount++;
                        result.FailureMessages.Add($"{item.DisplayName} 源路径不存在");
                        continue;
                    }

                    if (string.IsNullOrEmpty(item.DisplayName))
                    {
                        result.FailureCount++;
                        result.FailureMessages.Add($"Workshop ID {item.WorkshopId} 显示名称为空");
                        continue;
                    }

                    var cleanName = SanitizeFolderName(item.DisplayName);
                    var destPath = GetDestinationPath(cleanName);
                    bool conflictExists = Directory.Exists(destPath);

                    FileConflictAction action = _defaultConflictAction;
                    if (conflictExists && !_applyAllAction)
                    {
                        if (conflictHandler != null && _askForEachConflict)
                        {
                            action = conflictHandler(item.DisplayName);
                            if (action == FileConflictAction.Cancel)
                            {
                                result.Cancelled = true;
                                break;
                            }
                            if (action == FileConflictAction.ReplaceAll || 
                                action == FileConflictAction.KeepBothAll || 
                                action == FileConflictAction.SkipAll)
                            {
                                _applyAllAction = true;
                                _defaultConflictAction = action switch
                                {
                                    FileConflictAction.ReplaceAll => FileConflictAction.Replace,
                                    FileConflictAction.KeepBothAll => FileConflictAction.KeepBoth,
                                    FileConflictAction.SkipAll => FileConflictAction.Skip,
                                    _ => _defaultConflictAction
                                };
                            }
                        }
                        else
                        {
                            action = FileConflictAction.Replace;
                        }
                    }

                    if (conflictExists)
                    {
                        switch (action)
                        {
                            case FileConflictAction.Skip:
                            case FileConflictAction.SkipAll:
                                result.SkippedCount++;
                                result.SkippedMessages.Add($"{item.DisplayName} 已跳过（冲突）");
                                continue;
                            case FileConflictAction.Replace:
                            case FileConflictAction.ReplaceAll:
                                if (Directory.Exists(destPath))
                                {
                                    Directory.Delete(destPath, recursive: true);
                                }
                                break;
                            case FileConflictAction.KeepBoth:
                            case FileConflictAction.KeepBothAll:
                                destPath = GetUniquePath(cleanName);
                                break;
                        }
                    }

                    CopyModDirectory(item.SourcePath, destPath);
                    ModCacheManager.MarkAsImported(_cache, item.WorkshopId, Path.GetFileName(destPath));
                    result.SuccessCount++;
                    result.SuccessMessages.Add($"{item.DisplayName} 导入成功");
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.FailureMessages.Add($"{item.DisplayName} 导入失败: {ex.Message}");
                }
            }

            return result;
        }

        private string GetDestinationPath(string baseName)
        {
            var settings = Config.SettingsManager.GetCurrent();
            var modsPath = settings.ModDirectories?.FirstOrDefault() ?? PathHelper.GetDefaultModsPath();

            if (!Directory.Exists(modsPath))
                Directory.CreateDirectory(modsPath);

            var destPath = Path.Combine(modsPath, baseName);
            return destPath;
        }

        private string GetUniquePath(string baseName)
        {
            var settings = Config.SettingsManager.GetCurrent();
            var modsPath = settings.ModDirectories?.FirstOrDefault() ?? PathHelper.GetDefaultModsPath();

            var destPath = Path.Combine(modsPath, baseName);

            if (!Directory.Exists(destPath))
                return destPath;

            for (int i = 2; i <= 100; i++)
            {
                var numberedName = $"{baseName} ({i})";
                destPath = Path.Combine(modsPath, numberedName);
                if (!Directory.Exists(destPath))
                    return destPath;
            }

            return destPath;
        }

        private string SanitizeFolderName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleanName = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());

            if (string.IsNullOrWhiteSpace(cleanName))
                cleanName = "UnknownMod";

            return cleanName.Trim();
        }

        private void CopyModDirectory(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyModDirectory(subDir, destSubDir);
            }
        }
    }

    public class ImportResult
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int SkippedCount { get; set; }
        public bool Cancelled { get; set; }
        public List<string> SuccessMessages { get; } = new();
        public List<string> FailureMessages { get; } = new();
        public List<string> SkippedMessages { get; } = new();
    }
}