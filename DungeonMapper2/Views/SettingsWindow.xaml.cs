using DungeonMapper2.ViewModels;
using System.Windows;

namespace DungeonMapper2.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel(Close);
        }
    }
}
