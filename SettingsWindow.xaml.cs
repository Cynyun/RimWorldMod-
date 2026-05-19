using System.IO;
using System.Windows;
using RimWorldModManager.ViewModels;

namespace RimWorldModManager
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsWindow()
        {
            _viewModel = new SettingsViewModel();
            DataContext = _viewModel;
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
