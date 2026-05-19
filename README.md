# RimWorldModManager

RimWorld Mod 管理器 - 基于 SteamCMD 的 WPF 桌面应用程序，用于管理 RimWorld 创意工坊 Mod。

## 功能特性

- 通过 Steam Workshop ID 下载 Mod
- 批量更新已安装的 Mod
- 本地 Mod 列表管理
- Mod 详情查看（名称、作者、版本、描述）
- 暗色主题界面
- 配置文件持久化

## 系统要求

- Windows 10/11
- .NET 10.0 Runtime
- SteamCMD（首次运行时会提示下载）

## 快速开始

### 1. 准备 SteamCMD

从 [SteamCMD 官网](https://developer.valvesoftware.com/wiki/SteamCMD) 下载 steamcmd.zip，解压到项目根目录的 `steamcmd` 文件夹中。

目录结构应为：
```
WpfApp1/
├── steamcmd/
│   ├── steamcmd.exe
│   └── ...
├── mods/
└── ...
```

### 2. 运行程序

使用 Visual Studio 2022+ 打开 `WpfApp1.slnx`，或直接运行编译后的可执行文件。

### 3. 下载 Mod

在主界面输入 Workshop ID，点击"下载 Mod"即可。

## 项目结构

```
WpfApp1/
├── App.xaml/cs              # 应用程序入口
├── MainWindow.xaml/cs      # 主窗口
├── SettingsWindow.xaml/cs  # 设置窗口
├── Converters/              # 值转换器
├── Models/                  # 数据模型
├── Services/                # SteamCMD 服务
├── Utils/                   # 工具类
├── ViewModels/              # MVVM ViewModel
├── Views/                   # 用户控件
├── config/                  # 配置管理
└── Properties/              # 发布配置
```

## 技术栈

| 组件 | 技术 |
|------|------|
| UI 框架 | WPF + WPF-UI 4.3.0 |
| 语言 | C# 12 |
| 目标框架 | .NET 10.0 Windows |
| 架构模式 | MVVM |

## 配置说明

配置文件位于 `config/settings.json`，包含以下设置：

- `SteamCmdPath` - SteamCMD 可执行文件路径
- `ModDirectories` - Mod 下载目录列表
- `GamePaths` - 游戏安装路径（预留）

## 界面预览

程序采用 Fluent Design 暗色主题，包含以下主要功能：

- **Mod 列表**：显示已安装的 Mod，支持搜索
- **下载面板**：输入 Workshop ID 下载新 Mod
- **详情面板**：查看选中 Mod 的详细信息
- **设置窗口**：配置 SteamCMD 路径和 Mod 目录

## License

MIT
