# RimWorldModManager 项目组件树

> 本文档描述 RimWorldModManager 项目的完整组件结构

---

## 项目概述

| 项目属性 | 值 |
|---------|-----|
| 项目类型 | WPF 桌面应用程序 |
| 目标框架 | .NET 10.0 (net10.0-windows) |
| UI 框架 | WPF-UI 4.3.0 (Fluent Design) |
| 语言 | C# |
| 根命名空间 | RimWorldModManager |
| 程序集名称 | RimWorldModManager |

---

## 核心组件

### 1. 入口文件

| 文件 | 描述 |
|------|------|
| [App.xaml](App.xaml) | 应用程序入口，配置 WPF-UI 主题资源 |
| [App.xaml.cs](App.xaml.cs) | 启动逻辑，验证 SteamCMD 存在性 |
| [AssemblyInfo.cs](AssemblyInfo.cs) | WPF 主题信息属性 |

### 2. 主窗口

| 文件 | 描述 |
|------|------|
| [MainWindow.xaml](MainWindow.xaml) | 主窗口 UI，使用 FluentWindow |
| [MainWindow.xaml.cs](MainWindow.xaml.cs) | 主窗口后台代码，处理 Mod 选择和 SteamCMD 操作 |

### 3. 设置窗口

| 文件 | 描述 |
|------|------|
| [SettingsWindow.xaml](SettingsWindow.xaml) | 设置窗口 UI |
| [SettingsWindow.xaml.cs](SettingsWindow.xaml.cs) | 设置窗口后台代码 |

---

## 视图层 (Views)

| 文件 | 描述 |
|------|------|
| [Views/ModDetailPage.xaml](Views/ModDetailPage.xaml) | Mod 详情页面 XAML |
| [Views/ModDetailPage.xaml.cs](Views/ModDetailPage.xaml.cs) | Mod 详情页面后台代码 |

---

## 视图模型层 (ViewModels)

| 文件 | 描述 |
|------|------|
| [ViewModels/MainViewModel.cs](ViewModels/MainViewModel.cs) | 主窗口 ViewModel，包含 Mod 列表管理、搜索过滤、状态消息 |
| [ViewModels/ModDetailViewModel.cs](ViewModels/ModDetailViewModel.cs) | Mod 详情 ViewModel，包含图片预览 |
| [ViewModels/SettingsViewModel.cs](ViewModels/SettingsViewModel.cs) | 设置窗口 ViewModel，管理 Steam 账号密码默认值 |

---

## 模型层 (Models)

| 文件 | 描述 |
|------|------|
| [Models/ModInfo.cs](Models/ModInfo.cs) | Mod 数据模型，包含预览图片路径 |
| [Models/ModParser.cs](Models/ModParser.cs) | Mod 元数据解析器，从 XML 文件读取信息 |
| [Models/ModCache.cs](Models/ModCache.cs) | Mod 缓存数据模型 |

---

## 服务层 (Services)

| 文件 | 描述 |
|------|------|
| [Services/SteamCmdService.cs](Services/SteamCmdService.cs) | SteamCMD 进程封装服务，支持持久连接和心跳检测 |
| [Services/ModScannerService.cs](Services/ModScannerService.cs) | Mod 扫描服务，扫描本地和 Workshop Mod 目录 |
| [Services/ModImportService.cs](Services/ModImportService.cs) | Mod 导入服务，处理从 Workshop 导入到游戏目录 |
| [Services/ModCacheManager.cs](Services/ModCacheManager.cs) | Mod 缓存管理服务 |

---

## 工具类 (Utils)

| 文件 | 描述 |
|------|------|
| [Utils/Logger.cs](Utils/Logger.cs) | 日志接口与控制台实现 |
| [Utils/PathHelper.cs](Utils/PathHelper.cs) | 路径辅助工具，支持相对路径和默认路径 |

---

## 转换器 (Converters)

| 文件 | 描述 |
|------|------|
| [Converters/InvertBooleanToVisibilityConverter.cs](Converters/InvertBooleanToVisibilityConverter.cs) | 布尔值取反转换器 |
| [Converters/NullImageConverter.cs](Converters/NullImageConverter.cs) | 空图像保护转换器 |
| [Converters/VisibilityConverters.cs](Converters/VisibilityConverters.cs) | 可见性转换器集合 |

---

## 配置 (Config)

| 文件 | 描述 |
|------|------|
| [config/Settings.cs](config/Settings.cs) | 设置数据模型 |
| [config/SettingsManager.cs](config/SettingsManager.cs) | 设置持久化管理器，处理 JSON 配置读写 |

---

## 项目配置

| 文件 | 描述 |
|------|------|
| [RimWorldModsManage.csproj](RimWorldModsManage.csproj) | 主项目文件 |
| [.gitignore](.gitignore) | Git 忽略配置 |
| [README.md](README.md) | 项目说明文档 |
| [add.md](add.md) | 附加文档 |

---

## 依赖项

### NuGet 包

| 包名 | 版本 | 用途 |
|------|------|------|
| WPF-UI | 4.3.0 | Fluent Design UI 组件库 |

### 系统框架

| 框架 | 用途 |
|------|------|
| net10.0-windows | Windows WPF 支持 |
| UseWPF | WPF UI 框架 |
| UseWindowsForms | Windows Forms (FolderBrowserDialog) |

---

## 外部目录 (不纳入项目版本控制)

| 目录 | 描述 |
|------|------|
| steamcmd/ | SteamCMD 客户端 (需从 Valve 官网下载) |
| mods/ | Mod 下载存储目录 |
| config/*.json | 用户配置文件 (运行时生成) |

---

## 项目文件结构

```
WpfApp1/
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs
├── SettingsWindow.xaml / SettingsWindow.xaml.cs
├── AssemblyInfo.cs
├── ProjectComponentTree.md
├── README.md
├── add.md
├── Converters/
│   ├── InvertBooleanToVisibilityConverter.cs
│   ├── NullImageConverter.cs
│   └── VisibilityConverters.cs
├── Models/
│   ├── ModInfo.cs
│   ├── ModParser.cs
│   └── ModCache.cs
├── Services/
│   ├── SteamCmdService.cs
│   ├── ModScannerService.cs
│   ├── ModImportService.cs
│   └── ModCacheManager.cs
├── Utils/
│   ├── Logger.cs
│   └── PathHelper.cs
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── ModDetailViewModel.cs
│   └── SettingsViewModel.cs
├── Views/
│   ├── ModDetailPage.xaml
│   └── ModDetailPage.xaml.cs
├── config/
│   ├── Settings.cs
│   └── SettingsManager.cs
├── Properties/
│   └── PublishProfiles/
│       ├── FolderProfile.pubxml
│       └── RimWorld Mods 管理系统.pubxml
├── RimWorldModsManage.csproj
├── WpfApp1.slnx
└── .gitignore
```

---

## 解决方案结构

| 文件 | 格式 | 说明 |
|------|------|------|
| WpfApp1.slnx | 新版 Solution Format (VS 2022+) | 当前使用的解决方案文件 |

---

## 组件依赖关系

```
MainWindow.xaml.cs
├── MainViewModel.cs
│   ├── ModInfo.cs
│   ├── ModScannerService.cs
│   │   └── ModParser.cs
│   └── SteamCmdService.cs
└── ModDetailPage.xaml.cs
    └── ModDetailViewModel.cs
        └── ModInfo.cs

SettingsWindow.xaml.cs
└── SettingsViewModel.cs
    └── SettingsManager.cs
        └── Settings.cs

App.xaml.cs
├── SettingsManager.cs
└── PathHelper.cs

SteamCmdService.cs
├── SettingsManager.cs
└── Logger.cs

ModImportService.cs
├── ModInfo.cs
└── ModParser.cs

ModCacheManager.cs
└── ModCache.cs
```