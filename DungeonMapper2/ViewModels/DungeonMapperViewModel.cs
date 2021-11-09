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
        private readonly Action<Map> _printMap;
        private readonly List<Map> _maps;
        private Map _currentMap;
        private Point _dragPositionStart;

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
            _printMap = printAction;
            DatabaseManager.InitializeDatabase();
            _maps = MapDataAccess.GetMaps();
            if (_maps.Any())
            {
                _currentMap = _maps.First();
                _currentMap.LoadData();
                MapName = _currentMap.Name;
            }
            else
            {
                _currentMap = new Map();
                _currentMap.Initialize();
            }
            TreeData = new ObservableCollection<IPathItem>(BuildTreeData());
            _printMap(_currentMap);
        }

        private List<IPathItem> BuildTreeData()
        {
            var maps = MapDataAccess.GetMaps();
            var data = new List<IPathItem>();
            data.AddRange(FolderDataAccess.GetFolders(_maps));
            data.AddRange(maps.Where(map => !map.FolderId.HasValue));
            return data;
        }

        private void HandleMapKeyDown(KeyEventArgs args)
        {
            var shiftDown = args.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift);
            var ctrlDown = args.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);

            if (args.Key == Key.H)
            {
                _currentMap.HallMode = !(_currentMap.HallMode);
                return;
            }

            if (args.Key == Key.D)
            {
                _currentMap.ClearCurrentTile();
                _printMap(_currentMap);
                return;
            }

            if (args.Key == Key.T)
            {
                _currentMap.MarkTravel();
                _printMap(_currentMap);
                return;
            }

            if (args.Key == Key.Up && !(shiftDown || ctrlDown))
                _currentMap.MoveUp();
            if (args.Key == Key.Down && !(shiftDown || ctrlDown))
                _currentMap.MoveDown();
            if (args.Key == Key.Left && !(shiftDown || ctrlDown))
                _currentMap.MoveLeft();
            if (args.Key == Key.Right && !(shiftDown || ctrlDown))
                _currentMap.MoveRight();

            if (args.Key == Key.Up && ctrlDown)
                _currentMap.SetTileWall(Wall.Up);
            if (args.Key == Key.Down && ctrlDown)
                _currentMap.SetTileWall(Wall.Down);
            if (args.Key == Key.Left && ctrlDown)
                _currentMap.SetTileWall(Wall.Left);
            if (args.Key == Key.Right && ctrlDown)
                _currentMap.SetTileWall(Wall.Right);

            if (args.Key == Key.Up && shiftDown)
                _currentMap.SetTileDoor(Wall.Up);
            if (args.Key == Key.Down && shiftDown)
                _currentMap.SetTileDoor(Wall.Down);
            if (args.Key == Key.Left && shiftDown)
                _currentMap.SetTileDoor(Wall.Left);
            if (args.Key == Key.Right && shiftDown)
                _currentMap.SetTileDoor(Wall.Right);

            _printMap(_currentMap);
        }

        private void SaveCurrentMap()
        {
            _currentMap.Name = MapName;
            _currentMap.Id = MapDataAccess.SaveMap(_currentMap);
            if (!_maps.Any()) _maps.Add(_currentMap);
        }

        private void DeleteCurrentMap()
        {
            MapDataAccess.DeleteMap(_currentMap);
            _maps.Remove(_currentMap);
            _currentMap = _maps.First();
            _printMap(_currentMap);
        }

        private void ClearCurrentMap()
        {
            foreach (var tile in _currentMap.MapData.SelectMany(tileArray => tileArray).Where(tile => tile != null && tile.Traveled))
                tile.Clear();
            _printMap(_currentMap);
        }

        private void MoveToPreviousMap()
        {
            if (_currentMap == _maps.First())
                return;
            if (AutoSaveEnabled)
            {
                _currentMap.Name = MapName;
                _currentMap.Id = MapDataAccess.SaveMap(_currentMap);
            }
            _currentMap = _maps.ElementAt(_maps.IndexOf(_currentMap) - 1);
            MapName = _currentMap.Name;
            _printMap(_currentMap);
        }

        private void MoveToNextMap()
        {
            if (AutoSaveEnabled)
            {
                _currentMap.Name = MapName;
                _currentMap.Id = MapDataAccess.SaveMap(_currentMap);
                if (!_maps.Any()) _maps.Add(_currentMap);
            }
            if (_currentMap == _maps.Last())
            {
                _maps.Add(new Map());
                _currentMap = _maps.Last();
            }
            else
                _currentMap = _maps.ElementAt(_maps.IndexOf(_currentMap) + 1);
            if (_currentMap.MapData == null)
            {
                if (_currentMap.Id.HasValue)
                    _currentMap.LoadData();
                else
                    _currentMap.Initialize();
            }
            MapName = _currentMap.Name;
            _printMap(_currentMap);
        }

        private void HandleTreeSelectionChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            if (!(e.NewValue.GetType() == typeof(Map)))
                return;
            if (e.NewValue != null && e.NewValue != e.OldValue)
            {
                if (AutoSaveEnabled)
                {
                    var mapExisted = _currentMap.Id.HasValue;
                    _currentMap.Name = MapName;
                    _currentMap.Id = MapDataAccess.SaveMap(_currentMap);
                    if (!mapExisted) _maps.Add(_currentMap);
                }
                _currentMap = (Map)e.NewValue;
                _currentMap.LoadData();
                MapName = _currentMap.Name;
                _printMap(_currentMap);
            }
        }

        private void HandleTreeMouseDown(MouseButtonEventArgs e)
        {
            _dragPositionStart = e.GetPosition(null);
        }

        private void HandleTreeMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(null);
                var positionDifference = _dragPositionStart - currentPosition;
                if (Math.Abs(positionDifference.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(positionDifference.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    // Struggling to find an elegant way to snag the tree view item, might come back to this
                    var treeViewItem = ((e.OriginalSource as TextBlock)?.TemplatedParent as ContentPresenter)?.TemplatedParent as TreeViewItem;
                    if (treeViewItem == null)
                        return;
                    DragDrop.DoDragDrop(treeViewItem, _currentMap, DragDropEffects.Move);
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
