using DungeonMapper2.Models;
using DungeonMapper2.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DungeonMapper2.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DungeonMapperWindow : Window
    {
        private double PreviousSplitterWidth;

        public DungeonMapperWindow()
        {
            InitializeComponent();
            DataContext = new DungeonMapperViewModel(PrintMap);
        }

        // It would be ideal to move this to something binding based, but the the solutions I've seen aren't super clean, should come back to this.
        private void PrintMap(Map map)
        {
            canvas.Children.Clear();
            canvas.Height = map.MaxIndexY * map.TileSize;
            canvas.Width = map.MaxIndexX * map.TileSize;
            canvas.Children.Add(map.PrintToHost());
        }

        // Should probably do this as a trigger or something xaml-side
        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var splitterColumn = mainGrid.ColumnDefinitions.Where(def => !(def.Width.IsAuto || def.Width.IsStar)).First();
            if (gridSplitter.IsEnabled)
            {
                PreviousSplitterWidth = splitterColumn.Width.Value;
                splitterColumn.Width = new GridLength(0, GridUnitType.Pixel);
                gridSplitter.IsEnabled = false;
            }
            else
            {
                splitterColumn.Width = new GridLength(PreviousSplitterWidth, GridUnitType.Pixel);
                gridSplitter.IsEnabled = true;
            }
        }
    }
}
