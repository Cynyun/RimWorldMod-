using System;
using System.IO;
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
            }

            return modInfo;
        }

        private static void ParseModInfoXml(string xmlPath, ModInfo modInfo)
        {
            try
            {
                using var reader = XmlReader.Create(xmlPath);
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
                        }
                    }
                }
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
                        }
                    }
                }
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

        public static ModInfo ParseFromWorkshopId(int workshopId, string baseModsPath)
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