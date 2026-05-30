using RimWorldModManager.ViewModels;
using System.Windows;

namespace RimWorldModManager.Views
{
    public partial class ModDetailPage : System.Windows.Controls.UserControl
    {
        public ModDetailViewModel ViewModel { get; } = new ModDetailViewModel();

        public event RoutedEventHandler BackRequested;

        public ModDetailPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }

        public void LoadMod(Models.ModInfo mod)
        {
            ViewModel.LoadMod(mod);
        }

        public void Clear()
        {
            ViewModel.Clear();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, e);
        }
    }
}
