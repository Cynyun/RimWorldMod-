using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorldModManager.Models;
using RimWorldModManager.Utils;

namespace RimWorldModManager.Services
{
    public class ModImportService
    {
        private readonly ModCache _cache;

        public ModImportService(ModCache cache)
        {
            _cache = cache;
        }

        public ImportResult ImportMods(IEnumerable<WorkshopModItem> items)
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

                    var destPath = GetDestinationPath(item.DisplayName);
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

            var cleanName = SanitizeFolderName(baseName);
            var destPath = Path.Combine(modsPath, cleanName);

            if (!Directory.Exists(destPath))
                return destPath;

            for (int i = 2; i <= 100; i++)
            {
                var numberedName = $"{cleanName} ({i})";
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
        public List<string> SuccessMessages { get; } = new();
        public List<string> FailureMessages { get; } = new();
    }
}