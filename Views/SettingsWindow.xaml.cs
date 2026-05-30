using RimWorldModManager.Config;
using RimWorldModManager.ViewModels;
using System.IO;
using System.Windows;
using Wpf.Ui.Controls;

namespace RimWorldModManager.Views
{
    public partial class SettingsWindow : FluentWindow
    {
        private readonly SettingsViewModel _viewModel;
        private readonly string _originalSteamCmdPath;
        private readonly string _originalModPath;
        private bool _pathsChanged;
        private bool _isInitialized;

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

            _viewModel.SteamAccount = "anonymous";
            _viewModel.SteamPassword = "anonymous";
            SteamPasswordBox.Password = "anonymous";

            _isInitialized = true;
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

        private void FluentWindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void SteamAccountTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!_isInitialized)
                return;

            if (!string.IsNullOrEmpty(_viewModel.SteamAccount) && _viewModel.SteamAccount != "anonymous")
            {
                System.Windows.MessageBox.Show("Steam账号设置未完善，暂无法使用。", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void SteamPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized)
                return;

            var passwordBox = sender as Wpf.Ui.Controls.PasswordBox;
            if (passwordBox != null && !string.IsNullOrEmpty(passwordBox.Password))
            {
                System.Windows.MessageBox.Show("Steam密码设置未完善，暂无法使用。", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
    }
}
