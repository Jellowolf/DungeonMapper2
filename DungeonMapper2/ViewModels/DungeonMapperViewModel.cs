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
        private IPathItem _selectedTreeItem;
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
        private RelayCommand _handleTreeSelectionChangedCommand;
        private RelayCommand _handleTreeMouseDownCommand;
        private RelayCommand _handleTreeMouseMoveCommand;
        private RelayCommand _handleTreeDropCommand;

        public RelayCommand MapKeyDownCommand => _mapKeyDownCommand ??= new RelayCommand(o => HandleMapKeyDown((KeyEventArgs)o), o => true);

        public RelayCommand SaveCurrentMapCommand => _saveCurrentMapCommand ??= new RelayCommand(o => SaveCurrentMap(), o => true);

        public RelayCommand DeleteCurrentMapCommand => _deleteCurrentMapCommand ??= new RelayCommand(o => DeleteCurrentMap(), o => true);

        public RelayCommand ClearCurrentMapCommand => _clearCurrentMapCommand ??= new RelayCommand(o => ClearCurrentMap(), o => true);

        public RelayCommand HandleTreeSelectionChangedCommand => _handleTreeSelectionChangedCommand ??= new RelayCommand(o => HandleTreeSelectionChanged((RoutedPropertyChangedEventArgs<object>)o), o => true);

        public RelayCommand HandleTreeMouseDownCommand => _handleTreeMouseDownCommand ??= new RelayCommand(o => HandleTreeMouseDown((MouseButtonEventArgs)o), o => true);

        public RelayCommand HandleTreeMouseMoveCommand => _handleTreeMouseMoveCommand ??= new RelayCommand(o => HandleTreeMouseMove((MouseEventArgs)o), o => true);

        public RelayCommand HandleTreeDropCommand => _handleTreeDropCommand ??= new RelayCommand(o => HandleTreeDrop((DragEventArgs)o), o => true);

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
            var data = new List<IPathItem>();
            data.AddRange(FolderDataAccess.GetFolders(_maps));
            data.AddRange(_maps.Where(map => !map.FolderId.HasValue));
            return data;
        }

        private void HandleMapKeyDown(KeyEventArgs args)
        {
            var shiftDown = args.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift);
            var ctrlDown = args.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);

            if (args.Key == Key.H)
            {
                _currentMap.HallMode = !_currentMap.HallMode;
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

        private void HandleTreeSelectionChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedTreeItem = e.NewValue as IPathItem;
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
                    DragDrop.DoDragDrop(treeViewItem, _selectedTreeItem, DragDropEffects.Move);
                }
            }
        }

        private void HandleTreeDrop(DragEventArgs e)
        {
            IPathItem sourceItem = e.Data.GetData(typeof(Folder)) as IPathItem ?? e.Data.GetData(typeof(Map)) as IPathItem;
            if (sourceItem == null)
                return;

            // IThis is to filter out Maps but to consider everything that's not otherwise a folder to be the base path of the tree
            var destinationItem = (((e.OriginalSource as TextBlock)?.TemplatedParent as ContentPresenter)?.TemplatedParent as TreeViewItem)?.DataContext as IPathItem;
            if (sourceItem == destinationItem || destinationItem?.GetType() == typeof(Map))
                return;
            var destinationFolder = destinationItem as Folder;

            if (sourceItem.GetType() == typeof(Folder))
            {
                var sourceFolder = sourceItem as Folder;

                if (sourceFolder.Parent?.Id == null)
                    TreeData.Remove(sourceFolder);
                else
                    sourceFolder.Parent.ChildItems.Remove(sourceFolder);

                if (destinationFolder == null)
                {
                    sourceFolder.Parent = null;
                    TreeData.Add(sourceFolder);
                }
                else
                {
                    sourceFolder.Parent = destinationFolder;
                    destinationFolder.ChildItems.Add(sourceFolder);
                }

                FolderDataAccess.SaveFolder(sourceFolder);
            }
            else if (sourceItem.GetType() == typeof(Map))
            {
                var sourceMap = sourceItem as Map;

                if (sourceMap.FolderId == null)
                    TreeData.Remove(sourceMap);
                else
                {
                    var sourceMapParent = FindPathItemParent(sourceMap);
                    if (sourceMapParent != null)
                        sourceMapParent.ChildItems.Remove(sourceMap);
                }

                if (destinationFolder == null)
                {
                    sourceMap.FolderId = null;
                    TreeData.Add(sourceMap);
                }
                else
                {
                    sourceMap.FolderId = destinationFolder.Id;
                    destinationFolder.ChildItems.Add(sourceMap);
                }

                MapDataAccess.SaveMap(sourceMap);
            }
        }

        // I should probably just add a full folder parent to the Map class, but I'm still thinking on that, I haven't come up with a great implementation for it yet 
        private IPathItem FindPathItemParent(IPathItem item)
        {
            var parentId = (item as Folder)?.Parent?.Id ?? (item as Map)?.FolderId;
            if (parentId == null)
                return null;
            IPathItem match = null;
            foreach (var dataItem in TreeData)
                match ??= FindParentInItemOrChildren(dataItem);
            return match;

            IPathItem FindParentInItemOrChildren(IPathItem item)
            {
                if (item.Id == parentId)
                    return item;
                if (item.ChildItems == null || !item.ChildItems.Any())
                    return null;
                IPathItem childMatch = null;
                foreach (var dataItem in item.ChildItems)
                    childMatch ??= FindParentInItemOrChildren(dataItem);
                return childMatch;
            }
        }
    }
}
