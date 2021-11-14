using DungeonMapper2.Models;
using DungeonMapper2.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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
            if (map == null)
                return;
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

        // this isn't awesome, but I'm a little lazy on making a full custom toolbar style
        private void SetToolBarTemplateBackground(object sender, RoutedEventArgs e)
        {
            var toolBar = e.OriginalSource as ToolBar;
            var toggleButton = toolBar?.Template.FindName("OverflowButton", toolBar) as ToggleButton;
            if (toggleButton != null)
                toggleButton.Background = toolBar.Background;
            var overflowPanel = toolBar?.Template.FindName("ToolBarSubMenuBorder", toolBar) as Border;
            if (overflowPanel != null)
                overflowPanel.Background = toolBar.Background;

            // Sets the Icon's shadow
            var buttonIconBorder = VisualTreeHelper.GetChild(toggleButton, 0) as Border;
            var buttonIconCanvas = buttonIconBorder.Child as Canvas;
            foreach (Path path in buttonIconCanvas.Children)
            {
                if (path?.Fill == Brushes.White)
                    path.Fill = Brushes.LightGray;
                if (path?.Stroke == Brushes.White)
                    path.Stroke = Brushes.LightGray;
            }

        }
    }
}
