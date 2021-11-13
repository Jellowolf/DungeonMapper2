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
        private bool _isrightClickPathItem;

        #region Dependency Properties

        public static readonly DependencyProperty AutoSaveEnabledProperty = DependencyProperty.Register("AutoSaveEnabled", typeof(bool), typeof(DungeonMapperViewModel));
        public static readonly DependencyProperty AddEnabledProperty = DependencyProperty.Register("AddEnabled", typeof(bool), typeof(DungeonMapperViewModel));
        public static readonly DependencyProperty MapNameProperty = DependencyProperty.Register("MapName", typeof(string), typeof(DungeonMapperViewModel));
        public static readonly DependencyProperty MapCanvasProperty = DependencyProperty.Register("MapCanvas", typeof(Canvas), typeof(DungeonMapperViewModel));
        public static readonly DependencyProperty TreeDataProperty = DependencyProperty.Register("TreeData", typeof(ObservableCollection<IPathItem>), typeof(DungeonMapperViewModel));

        public bool AutoSaveEnabled
        {
            get => (bool)GetValue(AutoSaveEnabledProperty);
            set => SetValue(AutoSaveEnabledProperty, value);
        }

        public bool AddEnabled
        {
            get => (bool)GetValue(AddEnabledProperty);
            set => SetValue(AddEnabledProperty, value);
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
        private RelayCommand _handleTreeLeftMouseDownCommand;
        private RelayCommand _handleTreeRightMouseDownCommand;
        private RelayCommand _handleTreeMouseMoveCommand;
        private RelayCommand _handleTreeDropCommand;
        private RelayCommand _handleWidowClosingCommand;
        private RelayCommand _startAddPathItemCommand;
        private RelayCommand _startRenamePathItemCommand;
        private RelayCommand _completeEditPathItemCommand;
        private RelayCommand _deletePathItemCommand;

        public RelayCommand MapKeyDownCommand => _mapKeyDownCommand ??= new RelayCommand(o => HandleMapKeyDown((KeyEventArgs)o), o => true);

        public RelayCommand SaveCurrentMapCommand => _saveCurrentMapCommand ??= new RelayCommand(o => SaveCurrentMap(), o => true);

        public RelayCommand DeleteCurrentMapCommand => _deleteCurrentMapCommand ??= new RelayCommand(o => DeleteCurrentMap(), o => true);

        public RelayCommand ClearCurrentMapCommand => _clearCurrentMapCommand ??= new RelayCommand(o => ClearCurrentMap(), o => true);

        public RelayCommand HandleTreeSelectionChangedCommand => _handleTreeSelectionChangedCommand ??= new RelayCommand(o => HandleTreeSelectionChanged((RoutedPropertyChangedEventArgs<object>)o), o => true);

        public RelayCommand HandleTreeLeftMouseDownCommand => _handleTreeLeftMouseDownCommand ??= new RelayCommand(o => HandleTreeLeftMouseDown((MouseButtonEventArgs)o), o => true);

        public RelayCommand HandleTreeRightMouseDownCommand => _handleTreeRightMouseDownCommand ??= new RelayCommand(o => HandleTreeRightMouseDown((MouseButtonEventArgs)o), o => true);

        public RelayCommand HandleTreeMouseMoveCommand => _handleTreeMouseMoveCommand ??= new RelayCommand(o => HandleTreeMouseMove((MouseEventArgs)o), o => true);

        public RelayCommand HandleTreeDropCommand => _handleTreeDropCommand ??= new RelayCommand(o => HandleTreeDrop((DragEventArgs)o), o => true);

        public RelayCommand HandleWidowClosingCommand => _handleWidowClosingCommand ??= new RelayCommand(o => HandleWindowClosing(), o => true);

        public RelayCommand StartAddPathItemCommand => _startAddPathItemCommand ??= new RelayCommand(o => StartAddPathItem((PathItemType)o), o => true);

        public RelayCommand StartRenamePathItemCommand => _startRenamePathItemCommand ??= new RelayCommand(o => StartRenamePathItem(), o => true);

        public RelayCommand CompleteEditPathItemCommand => _completeEditPathItemCommand ??= new RelayCommand(o => CompleteEditPathItem(), o => true);

        public RelayCommand DeletePathItemCommand => _deletePathItemCommand ??= new RelayCommand(o => DeletePathItem(), o => true);

        #endregion

        public DungeonMapperViewModel(Action<Map> printAction)
        {
            _printMap = printAction;
            DatabaseManager.InitializeDatabase();
            _maps = MapDataAccess.GetMaps();
            TreeData = new ObservableCollection<IPathItem>(BuildTreeData());

            // I might just want to save the IsExpanded and IsSelected states in general, but for now flipping back to the user's last map seems reasonable
            var currentMapId = SettingDataAccess.GetSetting<int?>(Setting.CurrentMapId);
            SetPathItemInTreeData<Map>(currentMapId, true);
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
            if (!(e.NewValue is Map))
                return;
            if (e.NewValue != null && e.NewValue != e.OldValue)
                ChangeMaps((Map)e.NewValue);
        }

        private void ChangeMaps(Map map)
        {
            if (map == null)
                return;
            if (AutoSaveEnabled)
            {
                var mapExisted = _currentMap.Id.HasValue;
                _currentMap.Name = MapName;
                _currentMap.Id = MapDataAccess.SaveMap(_currentMap);
                if (!mapExisted) _maps.Add(_currentMap);
            }
            _currentMap = map;
            _currentMap.LoadData();
            MapName = _currentMap.Name;
            _printMap(_currentMap);
        }

        private void HandleTreeLeftMouseDown(MouseButtonEventArgs e)
        {
            _dragPositionStart = e.GetPosition(null);
        }

        private void HandleTreeRightMouseDown(MouseButtonEventArgs e)
        {
            var selectedPathItem = (((e.OriginalSource as TextBlock)?.TemplatedParent as ContentPresenter)?.TemplatedParent as TreeViewItem)?.DataContext as IPathItem;
            if (_isrightClickPathItem = selectedPathItem != null)
                selectedPathItem.IsSelected = true;
            AddEnabled = selectedPathItem is not Map;
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

            if (sourceItem is Folder)
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
            else if (sourceItem is Map)
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
                if (item is Folder && item.Id == parentId)
                    return item;
                if (item.ChildItems == null || !item.ChildItems.Any())
                    return null;
                IPathItem childMatch = null;
                foreach (var dataItem in item.ChildItems)
                    childMatch ??= FindParentInItemOrChildren(dataItem);
                return childMatch;
            }
        }

        private void SetPathItemInTreeData<T>(int? itemId, bool OpenPath)
        {
            if (itemId == null)
                return;
            IPathItem match = null;
            foreach (var dataItem in TreeData)
                match ??= FindInItemOrChildren(dataItem);

            if (match != null)
            {
                match.IsSelected = true;
                _selectedTreeItem = match;
                ChangeMaps(match as Map);
            }

            IPathItem FindInItemOrChildren(IPathItem childItem)
            {
                if (childItem is T && itemId == childItem.Id)
                    return childItem;
                if (childItem.ChildItems == null || !childItem.ChildItems.Any())
                    return null;
                IPathItem childMatch = null;
                foreach (var dataItem in childItem.ChildItems)
                    childMatch ??= FindInItemOrChildren(dataItem);
                if (OpenPath && childMatch != null)
                    childItem.IsExpanded = true;
                return childMatch;
            }
        }

        private void HandleWindowClosing()
        {
            SettingDataAccess.SaveSetting(Setting.CurrentMapId, _currentMap?.Id);
        }

        private void StartRenamePathItem()
        {
            _selectedTreeItem.EditModeEnabled = true;
        }

        private void StartAddPathItem(PathItemType type)
        {
            IPathItem newItem = null;

            if (type == PathItemType.Folder)
                newItem = new Folder { Parent = _isrightClickPathItem ? (Folder)_selectedTreeItem : null };
            else if (type == PathItemType.Map)
            {
                newItem = new Map { FolderId = _isrightClickPathItem ? _selectedTreeItem.Id : null };
                ((Map)newItem).Initialize();
            }

            newItem.IsSelected = newItem.EditModeEnabled = true;

            if (_isrightClickPathItem)
            {
                _selectedTreeItem.ChildItems ??= new ObservableCollection<IPathItem>();
                _selectedTreeItem.ChildItems.Add(newItem);
                _selectedTreeItem.IsExpanded = true;
            }
            else
            {
                TreeData.Add(newItem);
            }

            _selectedTreeItem = newItem;
        }

        private void CompleteEditPathItem()
        {
            if (_selectedTreeItem == null || !_selectedTreeItem.EditModeEnabled)
                return;
            if (_selectedTreeItem is Folder)
                _selectedTreeItem.Id = FolderDataAccess.SaveFolder(_selectedTreeItem as Folder);
            else if (_selectedTreeItem is Map)
                _selectedTreeItem.Id = MapDataAccess.SaveMap(_selectedTreeItem as Map);
            _selectedTreeItem.EditModeEnabled = false;
        }

        private void DeletePathItem()
        {
            //TODO
        }
    }
}
