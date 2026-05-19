using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RimWorldModManager.Models;
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

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshModsAsync();
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(WorkshopIdTextBox.Text.Trim(), out uint workshopId))
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

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = ModsDataGrid.SelectedItems.Cast<ModInfo>().ToList();
            if (selectedItems.Count == 0)
            {
                System.Windows.MessageBox.Show("请先选中要更新的 Mod", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            foreach (var mod in selectedItems)
            {
                try
                {
                    await _viewModel.UpdateModAsync(mod.WorkshopId);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"更新 Mod {mod.Name} 失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            System.Windows.MessageBox.Show(_viewModel.StatusMessage, "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private async void UpdateAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Mods.Count == 0)
            {
                System.Windows.MessageBox.Show("没有已安装的 Mod", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            foreach (var mod in _viewModel.Mods)
            {
                try
                {
                    await _viewModel.UpdateModAsync(mod.WorkshopId);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"更新 Mod {mod.Name} 失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
            System.Windows.MessageBox.Show("所有 Mod 更新完成", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.RefreshModsAsync();
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();

            if (settingsWindow.PathsChanged)
            {
                await _viewModel.RefreshModsAsync();
            }
        }

        private void ModsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel.SelectedMod != null)
            {
                ModDetailPage.LoadMod(_viewModel.SelectedMod);
            }
        }

        private void ModDetailPage_BackRequested(object sender, RoutedEventArgs e)
        {
            _viewModel.ClearSelection();
            ModsDataGrid.SelectedItem = null;
        }
    }
}
