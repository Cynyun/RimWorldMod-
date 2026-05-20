# RimWorldModManager

RimWorld Mod 管理器 - 基于 SteamCMD 的 WPF 桌面应用程序，用于管理 RimWorld 创意工坊 Mod。

## 功能特性

- **双视图管理**：同时管理本地游戏 Mod 和 Workshop 下载的 Mod
- **Workshop Mod 下载**：通过 Steam Workshop ID 下载 Mod
- **批量更新**：一键更新所有已下载的 Workshop Mod
- **Mod 详情预览**：查看 Mod 的名称、作者、版本、描述及预览图片
- **搜索过滤**：快速搜索和过滤 Mod 列表
- **打开文件夹**：直接打开选中 Mod 的文件夹或本地 Mod 目录
- **打开 Workshop**：跳转到 Mod 的 Steam Workshop 页面
- **持久化连接**：SteamCMD 保持登录状态，无需每次重新登录
- **心跳检测**：自动检测并重新登录 SteamCMD
- **便携部署**：支持相对路径配置，可移动到任意位置运行
- **暗色主题**：采用 Fluent Design 暗色主题界面
- **配置持久化**：自动保存用户设置

## 系统要求

- Windows 10/11
- .NET 10.0 Runtime
- SteamCMD（首次运行时会提示下载）

## 快速开始

### 1. 准备 SteamCMD

从 [SteamCMD 官网](https://developer.valvesoftware.com/wiki/SteamCMD) 下载 steamcmd.zip，解压到程序目录的 `steamcmd` 文件夹中。

目录结构应为：
```
RimWorldModManager/
├── RimWorldModManager.exe
├── steamcmd/
│   ├── steamcmd.exe
│   └── ...
├── mods/
└── config/
    └── settings.json
```

### 2. 运行程序

直接运行编译后的可执行文件 `RimWorldModManager.exe`，或使用 Visual Studio 2022+ 打开 `WpfApp1.slnx` 进行开发调试。

### 3. 配置设置

首次运行时，程序会自动检测或提示配置以下路径：

- **SteamCMD 路径**：指向 `steamcmd.exe` 文件
- **游戏 Mod 路径**：指向 RimWorld 的 Mods 目录

### 4. 使用说明

- **游戏 Mods 视图**：显示本地游戏目录中的 Mod
- **Workshop Mods 视图**：显示通过 SteamCMD 下载的 Mod
- **搜索框**：输入关键词过滤 Mod 列表
- **下载 Mod**：输入 Workshop ID 点击下载
- **更新 Mod**：选中 Workshop Mod 后点击更新按钮
- **导入到游戏**：将 Workshop Mod 复制到游戏目录

## 项目结构

```
WpfApp1/
├── App.xaml/cs              # 应用程序入口
├── MainWindow.xaml/cs      # 主窗口（双视图布局）
├── SettingsWindow.xaml/cs  # 设置窗口
├── Converters/              # WPF 值转换器
│   ├── VisibilityConverters.cs
│   ├── InvertBooleanToVisibilityConverter.cs
│   └── NullImageConverter.cs
├── Models/                  # 数据模型
│   ├── ModInfo.cs          # Mod 信息模型
│   ├── ModParser.cs        # Mod 元数据解析器
│   └── ModCache.cs         # Mod 缓存模型
├── Services/                # 业务服务
│   ├── SteamCmdService.cs  # SteamCMD 进程管理（持久连接）
│   ├── ModScannerService.cs # Mod 目录扫描服务
│   ├── ModImportService.cs # Mod 导入服务
│   └── ModCacheManager.cs  # Mod 缓存管理
├── Utils/                   # 工具类
│   ├── Logger.cs           # 日志工具
│   └── PathHelper.cs       # 路径辅助（支持相对路径）
├── ViewModels/              # MVVM ViewModel
│   ├── MainViewModel.cs    # 主窗口 ViewModel
│   ├── ModDetailViewModel.cs # Mod 详情 ViewModel
│   └── SettingsViewModel.cs # 设置窗口 ViewModel
├── Views/                   # 用户控件
│   └── ModDetailPage.xaml/cs # Mod 详情页面
├── config/                  # 配置管理
│   ├── Settings.cs         # 设置数据模型
│   └── SettingsManager.cs  # 设置持久化管理器
└── Properties/              # 发布配置
```

## 技术栈

| 组件 | 技术 |
|------|------|
| UI 框架 | WPF + WPF-UI 4.3.0 (Fluent Design) |
| 语言 | C# 12 |
| 目标框架 | .NET 10.0 Windows |
| 架构模式 | MVVM (Model-View-ViewModel) |
| 配置格式 | JSON |

## 配置说明

配置文件位于 `config/settings.json`，包含以下设置：

```json
{
  "GamePaths": {
    "RimWorld": "C:\\Program Files (x86)\\Steam\\steamapps\\common\\RimWorld"
  },
  "ModDirectories": [
    "./mods"
  ],
  "SteamCmdPath": "./steamcmd/steamcmd.exe"
}
```

- `GamePaths` - 游戏安装路径（预留）
- `ModDirectories` - Mod 下载目录列表（支持相对路径）
- `SteamCmdPath` - SteamCMD 可执行文件路径（支持相对路径）

## 界面预览

程序采用 Fluent Design 暗色主题，包含以下主要功能区域：

- **顶部标签页**：切换"游戏 Mods"和"Workshop Mods"视图
- **搜索框**：实时过滤 Mod 列表
- **Mod 列表**：显示 Mod 名称、作者、版本、上次修改时间
- **详情面板**：显示 Mod 描述和预览图片
- **操作按钮**：打开文件夹、打开 Workshop、导入到游戏、更新 Mod
- **状态栏**：显示操作提示和 SteamCMD 状态信息

## 使用提示

- **匿名登录**：程序默认使用 `anonymous` 账号登录 Steam，无需 Steam 账号即可下载公开 Mod
- **首次下载**：第一次下载 Mod 时，SteamCMD 需要下载必要的文件，可能需要较长时间
- **Workshop 路径**：Workshop Mod 下载后默认存放在 `steamcmd/steamapps/workshop/content/294100` 目录

## License

MIT