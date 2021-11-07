using DungeonMapper2.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DungeonMapper2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Map> TreeData;
        private List<Map> Maps;
        private Map CurrentMap;
        private double PreviousSplitterWidth;
        private Point DragPositionStart;

        public MainWindow()
        {
            InitializeComponent();
            DatabaseManager.InitializeDatabase();
            Maps = MapDataAccess.GetMaps();
            if (Maps.Any())
            {
                CurrentMap = Maps.First();
                CurrentMap.LoadData();
                MapName.Text = CurrentMap.Name;
            }
            else
            {
                CurrentMap = new Map();
                CurrentMap.Initialize();
            }
            CurrentMap.PrintToCanvas(canvas);

            /*var folder1Id = FolderDataAccess.SaveFolder(new Folder { Name = "Folder 1" });
            FolderDataAccess.SaveFolder(new Folder { Name = "Folder 1-1" }, folder1Id);
            FolderDataAccess.SaveFolder(new Folder { Name = "Folder 1-2" }, folder1Id);
            var folder1Id2 = FolderDataAccess.SaveFolder(new Folder { Name = "Folder 2" });
            FolderDataAccess.SaveFolder(new Folder { Name = "Folder 2-1" }, folder1Id2);
            FolderDataAccess.SaveFolder(new Folder { Name = "Folder 2-2" }, folder1Id2);*/
            treeView.ItemsSource = BuildTreeData();
        }

        private List<IPathItem> BuildTreeData()
        {
            var maps = MapDataAccess.GetMaps();
            var data = new List<IPathItem>();
            data.AddRange(FolderDataAccess.GetFolders(Maps));
            data.AddRange(maps.Where(map => !map.FolderId.HasValue));
            return data;
        }

        private void Canvas_KeyDown(object sender, KeyEventArgs e)
        {
            var shiftDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            var ctrlDown = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

            if (e.Key == Key.H)
            {
                CurrentMap.HallMode = !(CurrentMap.HallMode);
                return;
            }

            if (e.Key == Key.D)
            {
                CurrentMap.ClearCurrentTile();
                CurrentMap.PrintToCanvas(canvas);
                return;
            }

            if (e.Key == Key.T)
            {
                CurrentMap.MarkTravel();
                CurrentMap.PrintToCanvas(canvas);
                return;
            }

            if (e.Key == Key.Up && !(shiftDown || ctrlDown))
                CurrentMap.MoveUp();
            if (e.Key == Key.Down && !(shiftDown || ctrlDown))
                CurrentMap.MoveDown();
            if (e.Key == Key.Left && !(shiftDown || ctrlDown))
                CurrentMap.MoveLeft();
            if (e.Key == Key.Right && !(shiftDown || ctrlDown))
                CurrentMap.MoveRight();

            if (e.Key == Key.Up && ctrlDown)
                CurrentMap.SetTileWall(Wall.Up);
            if (e.Key == Key.Down && ctrlDown)
                CurrentMap.SetTileWall(Wall.Down);
            if (e.Key == Key.Left && ctrlDown)
                CurrentMap.SetTileWall(Wall.Left);
            if (e.Key == Key.Right && ctrlDown)
                CurrentMap.SetTileWall(Wall.Right);

            if (e.Key == Key.Up && shiftDown)
                CurrentMap.SetTileDoor(Wall.Up);
            if (e.Key == Key.Down && shiftDown)
                CurrentMap.SetTileDoor(Wall.Down);
            if (e.Key == Key.Left && shiftDown)
                CurrentMap.SetTileDoor(Wall.Left);
            if (e.Key == Key.Right && shiftDown)
                CurrentMap.SetTileDoor(Wall.Right);

            CurrentMap.PrintToCanvas(canvas);
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMap.Name = MapName.Text;
            CurrentMap.Id = MapDataAccess.SaveMap(CurrentMap);
            if (!Maps.Any()) Maps.Add(CurrentMap);
        }

        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentMap == Maps.First())
                return;
            if (autoSavecheckBox.IsChecked.HasValue && autoSavecheckBox.IsChecked.Value)
            {
                CurrentMap.Name = MapName.Text;
                CurrentMap.Id = MapDataAccess.SaveMap(CurrentMap);
            }
            CurrentMap = Maps.ElementAt(Maps.IndexOf(CurrentMap) - 1);
            MapName.Text = CurrentMap.Name;
            CurrentMap.PrintToCanvas(canvas);
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (autoSavecheckBox.IsChecked.HasValue && autoSavecheckBox.IsChecked.Value)
            {
                CurrentMap.Name = MapName.Text;
                CurrentMap.Id = MapDataAccess.SaveMap(CurrentMap);
                if (!Maps.Any()) Maps.Add(CurrentMap);
            }
            if (CurrentMap == Maps.Last())
            {
                Maps.Add(new Map());
                CurrentMap = Maps.Last();
            }
            else
                CurrentMap = Maps.ElementAt(Maps.IndexOf(CurrentMap) + 1);
            if (CurrentMap.mapData == null)
            {
                if (CurrentMap.Id.HasValue)
                    CurrentMap.LoadData();
                else
                    CurrentMap.Initialize();
            }
            MapName.Text = CurrentMap.Name;
            CurrentMap.PrintToCanvas(canvas);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            MapDataAccess.DeleteMap(CurrentMap);
            Maps.Remove(CurrentMap);
            CurrentMap = Maps.First();
            CurrentMap.PrintToCanvas(canvas);
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var tile in CurrentMap.mapData.SelectMany(tileArray => tileArray).Where(tile => tile != null && tile.Traveled))
                tile.Clear();
            CurrentMap.PrintToCanvas(canvas);
        }

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

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!(e.NewValue.GetType() == typeof(Map)))
                return;
            if (e.NewValue != null && e.NewValue != e.OldValue)
            {
                if (autoSavecheckBox.IsChecked.HasValue && autoSavecheckBox.IsChecked.Value)
                {
                    var mapExisted = CurrentMap.Id.HasValue;
                    CurrentMap.Name = MapName.Text;
                    CurrentMap.Id = MapDataAccess.SaveMap(CurrentMap);
                    if (!mapExisted) Maps.Add(CurrentMap);
                }
                CurrentMap = (Map)e.NewValue;
                CurrentMap.LoadData();
                MapName.Text = CurrentMap.Name;
                CurrentMap.PrintToCanvas(canvas);
            }
        }

        private void treeView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragPositionStart = e.GetPosition(null);
        }

        private void treeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(null);
                var positionDifference = DragPositionStart - currentPosition;
                if (Math.Abs(positionDifference.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(positionDifference.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // Struggling to find an elegant way to snag the tree view item, might come back to this
                    var treeViewItem = ((e.OriginalSource as TextBlock)?.TemplatedParent as ContentPresenter)?.TemplatedParent as TreeViewItem;
                    if (treeViewItem == null || (sender as TreeView)?.SelectedItem == null)
                        return;
                    DragDrop.DoDragDrop(treeViewItem, ((TreeView)sender).SelectedItem, DragDropEffects.Move);
                }

            }
        }

        private void treeView_Drop(object sender, DragEventArgs e)
        {
            IPathItem fromItem = e.Data.GetData(typeof(Folder)) as IPathItem ?? e.Data.GetData(typeof(Map)) as IPathItem;
            if (fromItem == null)
                return;

            var toItem = (((e.OriginalSource as TextBlock)?.TemplatedParent as ContentPresenter)?.TemplatedParent as TreeViewItem)?.DataContext as Folder;
            if (toItem == null)
                return;

            if (fromItem.GetType() == typeof(Folder))
            {
                var fromFolder = fromItem as Folder;
                var toFolder = toItem as Folder;
                fromFolder.Parent.ChildItems.Remove(fromFolder);
                fromFolder.Parent = toFolder;
                toFolder.ChildItems.Add(fromFolder);
            }
            else if (fromItem.GetType() == typeof(Map))
            {

            }
        }
    }
}
