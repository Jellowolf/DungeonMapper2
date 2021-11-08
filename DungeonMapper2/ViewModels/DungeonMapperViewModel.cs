using DungeonMapper2.DataAccess;
using DungeonMapper2.Models;
using DungeonMapper2.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DungeonMapper2.ViewModels
{
    public class DungeonMapperViewModel : DependencyObject
    {
        private Action<Map> PrintMap;
        private List<Map> Maps;
        private Map CurrentMap;
        private Point DragPositionStart;

        #region Dependency Properties

        public static readonly DependencyProperty AutoSaveEnabledProperty = DependencyProperty.Register("AutoSaveEnabled", typeof(bool), typeof(DungeonMapperViewModel));
        public static readonly DependencyProperty MapNameProperty = DependencyProperty.Register("MapName", typeof(string), typeof(DungeonMapperViewModel));
        public static readonly DependencyProperty MapCanvasProperty = DependencyProperty.Register("MapCanvas", typeof(Canvas), typeof(DungeonMapperViewModel));
        public static readonly DependencyProperty TreeDataProperty = DependencyProperty.Register("TreeData", typeof(ObservableCollection<IPathItem>), typeof(DungeonMapperViewModel));

        public bool AutoSaveEnabled
        {
            get => (bool)GetValue(AutoSaveEnabledProperty);
            set => SetValue(AutoSaveEnabledProperty, value);
        }

        public string MapName
        {
            get => (string)GetValue(MapNameProperty);
            set => SetValue(MapNameProperty, value);
        }

        public Canvas MapCanvas
        {
            get => (Canvas)GetValue(MapCanvasProperty);
            set => SetValue(MapCanvasProperty, value);
        }

        public ObservableCollection<IPathItem> TreeData
        {
            get => (ObservableCollection<IPathItem>)GetValue(TreeDataProperty);
            set => SetValue(TreeDataProperty, value);
        }

        #endregion

        #region Commands

        private RelayCommand _mapKeyDownCommand;
        private RelayCommand _saveCurrentMapCommand;
        private RelayCommand _deleteCurrentMapCommand;
        private RelayCommand _clearCurrentMapCommand;
        private RelayCommand _moveToPreviousMapCommand;
        private RelayCommand _moveToNextMapCommand;
        private RelayCommand _handleTreeSelectionChangedCommand;
        private RelayCommand _handleTreeMouseDownCommand;
        private RelayCommand _handleTreeMouseMoveCommand;
        private RelayCommand _handleTreeDropCommand;

        public RelayCommand MapKeyDownCommand
        {
            get => _mapKeyDownCommand ?? (_mapKeyDownCommand = new RelayCommand(o => HandleMapKeyDown((KeyEventArgs)o), o => true));
        }

        public RelayCommand SaveCurrentMapCommand
        {
            get => _saveCurrentMapCommand ?? (_saveCurrentMapCommand = new RelayCommand(o => SaveCurrentMap(), o => true));
        }

        public RelayCommand DeleteCurrentMapCommand
        {
            get => _deleteCurrentMapCommand ?? (_deleteCurrentMapCommand = new RelayCommand(o => DeleteCurrentMap(), o => true));
        }

        public RelayCommand ClearCurrentMapCommand
        {
            get => _clearCurrentMapCommand ?? (_clearCurrentMapCommand = new RelayCommand(o => ClearCurrentMap(), o => true));
        }

        public RelayCommand MoveToPreviousMapCommand
        {
            get => _moveToPreviousMapCommand ?? (_moveToPreviousMapCommand = new RelayCommand(o => MoveToPreviousMap(), o => true));
        }

        public RelayCommand MoveToNextMapCommand
        {
            get => _moveToNextMapCommand ?? (_moveToNextMapCommand = new RelayCommand(o => MoveToNextMap(), o => true));
        }

        public RelayCommand HandleTreeSelectionChangedCommand
        {
            get => _handleTreeSelectionChangedCommand ?? (_handleTreeSelectionChangedCommand = new RelayCommand(o => HandleTreeSelectionChanged((RoutedPropertyChangedEventArgs<object>)o), o => true));
        }

        public RelayCommand HandleTreeMouseDownCommand
        {
            get => _handleTreeMouseDownCommand ?? (_handleTreeMouseDownCommand = new RelayCommand(o => HandleTreeMouseDown((MouseButtonEventArgs)o), o => true));
        }

        public RelayCommand HandleTreeMouseMoveCommand
        {
            get => _handleTreeMouseMoveCommand ?? (_handleTreeMouseMoveCommand = new RelayCommand(o => HandleTreeMouseMove((MouseEventArgs)o), o => true));
        }

        public RelayCommand HandleTreeDropCommand
        {
            get => _handleTreeDropCommand ?? (_handleTreeDropCommand = new RelayCommand(o => HandleTreeDrop((DragEventArgs)o), o => true));
        }

        #endregion

        public DungeonMapperViewModel(Action<Map> printAction)
        {
            PrintMap = printAction;
            DatabaseManager.InitializeDatabase();
            Maps = MapDataAccess.GetMaps();
            if (Maps.Any())
            {
                CurrentMap = Maps.First();
                CurrentMap.LoadData();
                MapName = CurrentMap.Name;
            }
            else
            {
                CurrentMap = new Map();
                CurrentMap.Initialize();
            }
            TreeData = new ObservableCollection<IPathItem>(BuildTreeData());
            PrintMap(CurrentMap);
        }

        private List<IPathItem> BuildTreeData()
        {
            var maps = MapDataAccess.GetMaps();
            var data = new List<IPathItem>();
            data.AddRange(FolderDataAccess.GetFolders(Maps));
            data.AddRange(maps.Where(map => !map.FolderId.HasValue));
            return data;
        }

        private void HandleMapKeyDown(KeyEventArgs args)
        {
            var shiftDown = args.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift);
            var ctrlDown = args.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);

            if (args.Key == Key.H)
            {
                CurrentMap.HallMode = !(CurrentMap.HallMode);
                return;
            }

            if (args.Key == Key.D)
            {
                CurrentMap.ClearCurrentTile();
                PrintMap(CurrentMap);
                return;
            }

            if (args.Key == Key.T)
            {
                CurrentMap.MarkTravel();
                PrintMap(CurrentMap);
                return;
            }

            if (args.Key == Key.Up && !(shiftDown || ctrlDown))
                CurrentMap.MoveUp();
            if (args.Key == Key.Down && !(shiftDown || ctrlDown))
                CurrentMap.MoveDown();
            if (args.Key == Key.Left && !(shiftDown || ctrlDown))
                CurrentMap.MoveLeft();
            if (args.Key == Key.Right && !(shiftDown || ctrlDown))
                CurrentMap.MoveRight();

            if (args.Key == Key.Up && ctrlDown)
                CurrentMap.SetTileWall(Wall.Up);
            if (args.Key == Key.Down && ctrlDown)
                CurrentMap.SetTileWall(Wall.Down);
            if (args.Key == Key.Left && ctrlDown)
                CurrentMap.SetTileWall(Wall.Left);
            if (args.Key == Key.Right && ctrlDown)
                CurrentMap.SetTileWall(Wall.Right);

            if (args.Key == Key.Up && shiftDown)
                CurrentMap.SetTileDoor(Wall.Up);
            if (args.Key == Key.Down && shiftDown)
                CurrentMap.SetTileDoor(Wall.Down);
            if (args.Key == Key.Left && shiftDown)
                CurrentMap.SetTileDoor(Wall.Left);
            if (args.Key == Key.Right && shiftDown)
                CurrentMap.SetTileDoor(Wall.Right);

            PrintMap(CurrentMap);
        }

        private void SaveCurrentMap()
        {
            CurrentMap.Name = MapName;
            CurrentMap.Id = MapDataAccess.SaveMap(CurrentMap);
            if (!Maps.Any()) Maps.Add(CurrentMap);
        }

        private void DeleteCurrentMap()
        {
            MapDataAccess.DeleteMap(CurrentMap);
            Maps.Remove(CurrentMap);
            CurrentMap = Maps.First();
            PrintMap(CurrentMap);
        }

        private void ClearCurrentMap()
        {
            foreach (var tile in CurrentMap.mapData.SelectMany(tileArray => tileArray).Where(tile => tile != null && tile.Traveled))
                tile.Clear();
            PrintMap(CurrentMap);
        }

        private void MoveToPreviousMap()
        {
            if (CurrentMap == Maps.First())
                return;
            if (AutoSaveEnabled)
            {
                CurrentMap.Name = MapName;
                CurrentMap.Id = MapDataAccess.SaveMap(CurrentMap);
            }
            CurrentMap = Maps.ElementAt(Maps.IndexOf(CurrentMap) - 1);
            MapName = CurrentMap.Name;
            PrintMap(CurrentMap);
        }

        private void MoveToNextMap()
        {
            if (AutoSaveEnabled)
            {
                CurrentMap.Name = MapName;
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
            MapName = CurrentMap.Name;
            PrintMap(CurrentMap);
        }

        private void HandleTreeSelectionChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            if (!(e.NewValue.GetType() == typeof(Map)))
                return;
            if (e.NewValue != null && e.NewValue != e.OldValue)
            {
                if (AutoSaveEnabled)
                {
                    var mapExisted = CurrentMap.Id.HasValue;
                    CurrentMap.Name = MapName;
                    CurrentMap.Id = MapDataAccess.SaveMap(CurrentMap);
                    if (!mapExisted) Maps.Add(CurrentMap);
                }
                CurrentMap = (Map)e.NewValue;
                CurrentMap.LoadData();
                MapName = CurrentMap.Name;
                PrintMap(CurrentMap);
            }
        }

        private void HandleTreeMouseDown(MouseButtonEventArgs e)
        {
            DragPositionStart = e.GetPosition(null);
        }

        private void HandleTreeMouseMove(MouseEventArgs e)
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
                    if (treeViewItem == null)
                        return;
                    DragDrop.DoDragDrop(treeViewItem, CurrentMap, DragDropEffects.Move);
                }

            }
        }

        private void HandleTreeDrop(DragEventArgs e)
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
