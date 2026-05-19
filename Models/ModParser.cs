using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace RimWorldModManager.Models
{
    public static class ModParser
    {
        public static ModInfo ParseFromDirectory(string modDir)
        {
            var modInfo = new ModInfo
            {
                LocalPath = modDir,
                IsEnabled = true
            };

            var di = new DirectoryInfo(modDir);
            if (di.Exists)
            {
                modInfo.LastUpdated = di.LastWriteTime;

                var modInfoXmlPath = Path.Combine(modDir, "mod_info.xml");
                var aboutXmlPath = Path.Combine(modDir, "About", "About.xml");

                if (File.Exists(modInfoXmlPath))
                {
                    ParseModInfoXml(modInfoXmlPath, modInfo);
                }
                else if (File.Exists(aboutXmlPath))
                {
                    ParseAboutXml(aboutXmlPath, modInfo);
                }
                else
                {
                    modInfo.Name = di.Name;
                    modInfo.Description = "无法解析 Mod 信息";
                }

                FindPreviewImage(modDir, modInfo);
            }

            return modInfo;
        }

        private static void ParseModInfoXml(string xmlPath, ModInfo modInfo)
        {
            try
            {
                using var reader = XmlReader.Create(xmlPath);
                var tags = new List<string>();
                
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.LocalName)
                        {
                            case "name":
                                modInfo.Name = reader.ReadElementContentAsString();
                                break;
                            case "description":
                                modInfo.Description = reader.ReadElementContentAsString();
                                break;
                            case "author":
                                modInfo.Author = reader.ReadElementContentAsString();
                                break;
                            case "version":
                                modInfo.Version = reader.ReadElementContentAsString();
                                break;
                            case "tag":
                                tags.Add(reader.ReadElementContentAsString());
                                break;
                        }
                    }
                }
                
                modInfo.Tags = tags.ToArray();
            }
            catch
            {
                var dirName = Path.GetDirectoryName(xmlPath);
                if (dirName != null)
                {
                    var lastSeparator = dirName.LastIndexOf(Path.DirectorySeparatorChar);
                    modInfo.Name = lastSeparator >= 0 ? dirName.Substring(lastSeparator + 1) : dirName;
                }
                else
                {
                    modInfo.Name = "未知";
                }
                modInfo.Description = "解析 mod_info.xml 失败";
            }
        }

        private static void ParseAboutXml(string xmlPath, ModInfo modInfo)
        {
            try
            {
                using var reader = XmlReader.Create(xmlPath);
                var tags = new List<string>();
                
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.LocalName)
                        {
                            case "name":
                                modInfo.Name = reader.ReadElementContentAsString();
                                break;
                            case "description":
                                modInfo.Description = reader.ReadElementContentAsString();
                                break;
                            case "author":
                                modInfo.Author = reader.ReadElementContentAsString();
                                break;
                            case "version":
                                modInfo.Version = reader.ReadElementContentAsString();
                                break;
                            case "tag":
                                tags.Add(reader.ReadElementContentAsString());
                                break;
                        }
                    }
                }
                
                modInfo.Tags = tags.ToArray();
            }
            catch
            {
                var dirName = Path.GetDirectoryName(Path.GetDirectoryName(xmlPath));
                if (dirName != null)
                {
                    var lastSeparator = dirName.LastIndexOf(Path.DirectorySeparatorChar);
                    modInfo.Name = lastSeparator >= 0 ? dirName.Substring(lastSeparator + 1) : dirName;
                }
                else
                {
                    modInfo.Name = "未知";
                }
                modInfo.Description = "解析 About.xml 失败";
            }
        }

        private static void FindPreviewImage(string modDir, ModInfo modInfo)
        {
            var aboutDir = Path.Combine(modDir, "About");
            if (Directory.Exists(aboutDir))
            {
                var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
                var imageFiles = Directory.GetFiles(aboutDir)
                    .Where(f => imageExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(f => 
                    {
                        var name = Path.GetFileNameWithoutExtension(f).ToLower();
                        if (name.Contains("preview")) return 0;
                        if (name.Contains("icon")) return 1;
                        return 2;
                    })
                    .ToList();

                if (imageFiles.Any())
                {
                    modInfo.PreviewImagePath = imageFiles.First();
                }
            }
        }

        public static ModInfo ParseFromWorkshopId(uint workshopId, string baseModsPath)
        {
            var modDir = Path.Combine(baseModsPath, "steamapps", "workshop", "content", "294100", workshopId.ToString());
            if (Directory.Exists(modDir))
            {
                var modInfo = ParseFromDirectory(modDir);
                modInfo.WorkshopId = workshopId;
                return modInfo;
            }
            return null;
        }
    }
}
