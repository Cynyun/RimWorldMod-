using RimWorldModManager.Services;
using System.Windows;
using Wpf.Ui.Controls;

namespace RimWorldModManager.Views
{
    public partial class FileConflictDialog : FluentWindow
    {
        private FileConflictAction _selectedAction;
        private bool _applyToAll;

        public FileConflictAction SelectedAction => _selectedAction;
        public bool ApplyToAll => _applyToAll;

        public FileConflictDialog(string modName)
        {
            InitializeComponent();
            ModNameText.Text = $"Mod \"{modName}\" 已存在于目标目录中。";
            _selectedAction = FileConflictAction.Replace;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReplaceRadio.IsChecked == true)
            {
                _selectedAction = ApplyToAllCheck.IsChecked == true 
                    ? FileConflictAction.ReplaceAll 
                    : FileConflictAction.Replace;
            }
            else if (KeepBothRadio.IsChecked == true)
            {
                _selectedAction = ApplyToAllCheck.IsChecked == true 
                    ? FileConflictAction.KeepBothAll 
                    : FileConflictAction.KeepBoth;
            }
            else if (SkipRadio.IsChecked == true)
            {
                _selectedAction = FileConflictAction.Skip;
            }

            _applyToAll = ApplyToAllCheck.IsChecked == true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _selectedAction = FileConflictAction.Cancel;
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
