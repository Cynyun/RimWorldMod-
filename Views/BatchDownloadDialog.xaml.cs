using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RimWorldModManager.Views
{
    public partial class BatchDownloadDialog : Wpf.Ui.Controls.FluentWindow
    {
        public List<uint> SelectedWorkshopIds { get; private set; } = new();

        public BatchDownloadDialog()
        {
            InitializeComponent();
        }

        private void AddToListButton_Click(object sender, RoutedEventArgs e)
        {
            var input = WorkshopIdsTextBox.Text;
            if (string.IsNullOrWhiteSpace(input))
                return;

            var lines = input.Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrEmpty(l));

            foreach (var line in lines)
            {
                if (uint.TryParse(line, out uint workshopId))
                {
                    if (!SelectedWorkshopIds.Contains(workshopId))
                    {
                        SelectedWorkshopIds.Add(workshopId);
                    }
                }
            }

            WorkshopIdsTextBox.Clear();
            UpdateListBox();
        }

        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is uint workshopId)
            {
                SelectedWorkshopIds.Remove(workshopId);
                UpdateListBox();
            }
        }

        private void ClearListButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedWorkshopIds.Clear();
            UpdateListBox();
        }

        private void UpdateListBox()
        {
            WorkshopIdsList.Items.Clear();
            foreach (var id in SelectedWorkshopIds)
            {
                WorkshopIdsList.Items.Add(id);
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedWorkshopIds.Count == 0)
            {
                System.Windows.MessageBox.Show("请先添加要下载的 Workshop ID", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

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
    }
}