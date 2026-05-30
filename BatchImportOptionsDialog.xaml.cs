using RimWorldModManager.Services;
using System.Windows;
using Wpf.Ui.Controls;

namespace RimWorldModManager
{
    public partial class BatchImportOptionsDialog : FluentWindow
    {
        public FileConflictAction SelectedAction { get; private set; }
        public bool AskForEachConflict { get; private set; } = true;

        public BatchImportOptionsDialog(int totalCount)
        {
            InitializeComponent();
            ModCountText.Text = $"您选择了 {totalCount} 个 Mod 进行导入。";
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (AskEachRadio.IsChecked == true)
            {
                SelectedAction = FileConflictAction.Replace;
                AskForEachConflict = true;
            }
            else if (ReplaceAllRadio.IsChecked == true)
            {
                SelectedAction = FileConflictAction.ReplaceAll;
                AskForEachConflict = false;
            }
            else if (KeepBothAllRadio.IsChecked == true)
            {
                SelectedAction = FileConflictAction.KeepBothAll;
                AskForEachConflict = false;
            }
            else if (SkipAllRadio.IsChecked == true)
            {
                SelectedAction = FileConflictAction.SkipAll;
                AskForEachConflict = false;
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
