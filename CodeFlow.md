# Mod 管理器 Code 流工作文档（WPF-UI 版）

> 本文件用于指导大模型按顺序生成代码，每页对应一个独立模块。  
> **技术栈**：.NET 6+ WPF + [WPF-UI](https://github.com/lepoco/wpfui) 组件库（现代化 Fluent Design 风格）  
> **生成顺序**：从基础工具 → 核心服务 → 数据模型 → UI 绑定 → 配置管理

---

## 📄 第 1 页：路径与日志工具（Utils/PathHelper.cs + Utils/Logger.cs）

### 依赖
- 无

### 任务
1. 实现 `PathHelper.GetSolutionRoot()`：返回解决方案根目录（`AppContext.BaseDirectory` 的上一级）
2. 实现 `PathHelper.GetSteamCmdExePath()`：拼接 `steamcmd/steamcmd.exe`
3. 实现 `PathHelper.GetDefaultModsPath()`：返回 `mods/` 目录
4. 创建简单日志接口 `ILogger` 和控制台实现 `ConsoleLogger`

### 输出文件
- `src/RimWorldModManager/Utils/PathHelper.cs`
- `src/RimWorldModManager/Utils/Logger.cs`

---

## 📄 第 2 页：SteamCMD 服务封装（Services/SteamCmdService.cs）

### 依赖
- 第 1 页的 `PathHelper` 和 `ILogger`

### 任务
1. 创建 `ProcessResult` 类（含 `ExitCode`, `StandardOutput`, `StandardError`）
2. 实现 `SteamCmdService.ExecuteAsync(string[] args)`：
   - 使用 `ProcessStartInfo` 启动 `steamcmd.exe`
   - 设置 `WorkingDirectory` 为 steamcmd 所在目录
   - 异步读取 stdout/stderr
   - 超时处理（默认 5 分钟）
3. 实现 `DownloadModAsync(int workshopId, string installDir)`：
   - 构造参数：`+login anonymous +force_install_dir [dir] +workshop_download_item 294100 [id] +quit`

### 输出文件
- `src/RimWorldModManager/Services/SteamCmdService.cs`

---

## 📄 第 3 页：Mod 元数据模型（Models/ModInfo.cs + Models/ModParser.cs）

### 依赖
- 无

### 任务
1. 创建 `ModInfo` 类（属性：`WorkshopId`, `Name`, `Description`, `LocalPath`, `LastUpdated`, `IsEnabled`）
2. 创建 `ModParser` 类：
   - 方法 `ParseFromDirectory(string modDir)`：解析 `mod_info.xml` 或 `About.xml` 获取名称/描述
   - 处理 RimWorld/ARK 等不同游戏的元数据格式

### 输出文件
- `src/RimWorldModManager/Models/ModInfo.cs`
- `src/RimWorldModManager/Models/ModParser.cs`

---

## 📄 第 4 页：配置管理（Config/Settings.cs + Config/SettingsManager.cs）

### 依赖
- 第 1 页的 `PathHelper`

### 任务
1. 创建 `Settings` 类（属性：`GamePaths`, `ModDirectories`, `SteamCmdPath`）
2. 实现 `SettingsManager`：
   - `Load()`：从 `config/settings.json` 反序列化（若不存在则创建默认配置）
   - `Save()`：持久化到 JSON
   - 默认配置路径：`Path.Combine(PathHelper.GetSolutionRoot(), "config", "settings.json")`

### 输出文件
- `src/RimWorldModManager/Config/Settings.cs`
- `src/RimWorldModManager/Config/SettingsManager.cs`

---

## 📄 第 5 页：主 ViewModel（ViewModels/MainViewModel.cs）

### 依赖
- 第 2 页 `SteamCmdService`
- 第 3 页 `ModInfo` 和 `ModParser`
- 第 4 页 `SettingsManager`

### 任务
1. 实现 `INotifyPropertyChanged`
2. 属性 `ObservableCollection<ModInfo> Mods`
3. 方法 `AddModAsync(int workshopId)`：
   - 从 `SettingsManager` 获取 Mod 目录
   - 调用 `SteamCmdService.DownloadModAsync()`
   - 用 `ModParser` 解析下载后的 Mod 信息
   - 添加到 `Mods` 集合
4. 方法 `LoadMods()`：扫描 Mod 目录并加载现有 Mod

### 输出文件
- `src/RimWorldModManager/ViewModels/MainViewModel.cs`

---

## 📄 第 6 页：WPF-UI 主界面（MainWindow.xaml + MainWindow.xaml.cs）

### 依赖
- 第 5 页 `MainViewModel`
- **WPF-UI NuGet 包**：`Wpf.Ui`（需提前安装）

### 任务
#### XAML 布局要求（使用 WPF-UI 组件）：
1. **窗口框架**：`<ui:FluentWindow>` 替代原生 `Window`
2. **标题栏**：集成 `ui:TitleBar`（支持最小化/最大化/关闭）
3. **输入区域**：
   - `ui:TextBox`（绑定 Workshop ID）
   - `ui:Button`（“下载 Mod”，图标：`Download24`)
4. **Mod 列表**：
   - `ui:DataGrid` 或 `ItemsControl` + `ui:Card` 模板
   - 每项显示：Mod 名称、ID、启用状态（`ui:ToggleSwitch`）
5. **状态栏**：
   - `ui:Snackbar` 用于操作反馈（如“下载完成”）
   - `ui:ProgressRing` 显示下载进度（可选）

#### 后台代码：
1. 初始化 WPF-UI：
   ```csharp
   public partial class MainWindow : FluentWindow
   {
       public MainWindow()
       {
           InitializeComponent();
           DataContext = new MainViewModel();
           Loaded += (s, e) => SnackBar.SetForeground(this, Brushes.White);
       }
   }
   ```
2. 绑定按钮点击 → `MainViewModel.AddModAsync()`
3. 错误处理：通过 `SnackBar.Show("错误信息", "关闭")` 提示

### 输出文件
- `src/RimWorldModManager/MainWindow.xaml`
- `src/RimWorldModManager/MainWindow.xaml.cs`

---

## 📄 第 7 页：应用入口与初始化（App.xaml + App.xaml.cs）

### 依赖
- 第 4 页 `SettingsManager`
- 第 1 页 `PathHelper`
- **WPF-UI 资源**

### 任务
#### App.xaml：
1. 合并 WPF-UI 资源字典：
   ```xml
   <Application.Resources>
       <ResourceDictionary>
           <ResourceDictionary.MergedDictionaries>
               <ui:ThemesDictionary Theme="Dark" />
               <ui:ControlsDictionary />
           </ResourceDictionary.MergedDictionaries>
       </ResourceDictionary>
   </Application.Resources>
   ```

#### App.xaml.cs：
1. 在 `OnStartup` 中：
   - 验证 `steamcmd.exe` 是否存在（否则通过 `ui:MessageBox` 提示）
   - 初始化 `SettingsManager`
   - 创建 `mods/` 和 `config/` 目录（若不存在）
2. 启用 WPF-UI 主题：
   ```csharp
   protected override void OnStartup(StartupEventArgs e)
   {
       base.OnStartup(e);
       // ... 初始化逻辑
       Wpf.Ui.Appearance.Watcher.Watch(this); // 启用主题监听
   }
   ```

### 输出文件
- `src/RimWorldModManager/App.xaml`
- `src/RimWorldModManager/App.xaml.cs`

---

## ✅ 生成完成验证
1. 所有 `.cs` 文件应无编译错误
2. 运行后显示 **Fluent Design 风格界面**（深色主题、圆角控件）
3. 输入 Workshop ID → 点击下载 → `mods/` 生成文件夹
4. UI 通过 `ui:Snackbar` 显示成功/失败消息
5. 支持 Windows 系统主题自动切换（亮/暗）

> **WPF-UI 安装命令**（需在项目中执行）：  
> `Install-Package Wpf.Ui`  
> 
> **下一步**：请按页码顺序逐页生成代码。每完成一页，确认依赖项已就绪。