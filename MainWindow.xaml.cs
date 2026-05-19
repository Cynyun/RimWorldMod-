using System;
using System.Windows;
using RimWorldModManager.ViewModels;
using Wpf.Ui.Controls;

namespace RimWorldModManager
{
    public partial class MainWindow : FluentWindow
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadMods();
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(WorkshopIdTextBox.Text.Trim(), out int workshopId))
            {
                try
                {
                    await _viewModel.AddModAsync(workshopId);
                    System.Windows.MessageBox.Show(_viewModel.StatusMessage, "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"下载失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("请输入有效的 Workshop ID", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }
    }
}