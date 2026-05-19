using System.IO;
using System.Windows;
using RimWorldModManager.Config;
using RimWorldModManager.ViewModels;
using Wpf.Ui.Controls;

namespace RimWorldModManager
{
    public partial class SettingsWindow : FluentWindow
    {
        private readonly SettingsViewModel _viewModel;
        private readonly string _originalSteamCmdPath;
        private readonly string _originalModPath;
        private bool _pathsChanged;

        public bool PathsChanged => _pathsChanged;

        public SettingsWindow()
        {
            InitializeComponent();
            _viewModel = new SettingsViewModel();
            DataContext = _viewModel;

            var settings = SettingsManager.GetCurrent();
            _originalSteamCmdPath = settings.SteamCmdPath;
            _originalModPath = settings.ModDirectories.Count > 0 ? settings.ModDirectories[0] : string.Empty;
            _pathsChanged = false;
        }

        private void BrowseSteamCmd_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "SteamCMD Executable|steamcmd.exe",
                Title = "选择 steamcmd.exe"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _viewModel.SteamCmdPath = openFileDialog.FileName;
            }
        }

        private void BrowseModPath_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择 Mod 存储目录"
            };

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _viewModel.ModDownloadPath = folderDialog.SelectedPath;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(_viewModel.ModDownloadPath))
            {
                Directory.CreateDirectory(_viewModel.ModDownloadPath);
            }

            _pathsChanged = 
                _viewModel.SteamCmdPath != _originalSteamCmdPath ||
                _viewModel.ModDownloadPath != _originalModPath;

            _viewModel.SaveSettings();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
