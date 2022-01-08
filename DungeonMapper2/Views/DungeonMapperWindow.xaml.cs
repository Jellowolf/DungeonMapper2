using DungeonMapperStandard.Models;
using DungeonMapper2.Utilities;
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
        private const int _canvasMargin = 50;
        private double _previousSplitterWidth;

        public DungeonMapperWindow()
        {
            InitializeComponent();
            DataContext = new DungeonMapperViewModel(PrintMap, UpdateMapOffsets, Close);
        }

        // It would be ideal to move this to something binding based, but the the solutions I've seen aren't super clean, should come back to this.
        private void PrintMap(Map map)
        {
            canvas.Children.Clear();
            if (map == null)
                return;
            canvas.Height = (map.MaxIndexY + 1) * map.TileSize;
            canvas.Width = (map.MaxIndexX + 1) * map.TileSize;
            canvas.Children.Add(map.PrintTileHost());
        }

        // Should probably do this as a trigger or something xaml-side
        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var splitterColumn = mainGrid.ColumnDefinitions.Where(def => !(def.Width.IsAuto || def.Width.IsStar)).First();
            if (gridSplitter.IsEnabled)
            {
                _previousSplitterWidth = splitterColumn.Width.Value;
                splitterColumn.Width = new GridLength(0, GridUnitType.Pixel);
                gridSplitter.IsEnabled = false;
            }
            else
            {
                splitterColumn.Width = new GridLength(_previousSplitterWidth, GridUnitType.Pixel);
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

        // None of the necessary values here are available for binding, so resorting to a view function for this. Also I'm seeing some weird scroll behavior.
        // Without this code the viewer doesn't move at all, but with it the scroll bars are having their offset automatically updated even when the code is hit.
        // I think there's something with observing the offset causing the default behavior? need to look into that further
        private void UpdateMapOffsets(Map map)
        {
            var positionY = map.MaxIndexY - map.Position.y;
            if (((map.Position.x * map.TileSize) + _canvasMargin) <= scrollViewer.HorizontalOffset)
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - map.TileSize);
            if ((((map.Position.x + 1) * map.TileSize) + _canvasMargin) >= (scrollViewer.HorizontalOffset + scrollViewer.ViewportWidth))
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + map.TileSize);

            if (((positionY * map.TileSize) + _canvasMargin) <= scrollViewer.VerticalOffset)
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - map.TileSize);
            if ((((positionY + 1) * map.TileSize) + _canvasMargin) >= (scrollViewer.VerticalOffset + scrollViewer.ViewportHeight))
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + map.TileSize);
        }
    }
}
