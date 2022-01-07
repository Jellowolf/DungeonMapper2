using DungeonMapper2.Utilities;
using DungeonMapper2.Views;
using DungeonMapperStandard.DataAccess;
using DungeonMapperStandard.Models;
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
        private readonly Action<Map> _updateMapOffsets;
        private readonly Action _closeWindow;
        private IPathItem _selectedTreeItem;
        private Point _dragPositionStart;
        private Window _settingsWindow;

        #region Dependency Properties

        public static readonly DependencyProperty AutoSaveEnabledProperty = DependencyProperty.Register("AutoSaveEnabled", typeof(bool), typeof(DungeonMapperViewModel));
        public static readonly DependencyProperty AddEnabledProperty = DependencyProperty.Register("AddEnabled", typeof(bool), typeof(DungeonMapperViewModel));
        public static readonly DependencyProperty MapCanvasProperty = DependencyProperty.Register("MapCanvas", typeof(Canvas), typeof(DungeonMapperViewModel));
        public static readonly DependencyProperty TreeDataProperty = DependencyProperty.Register("TreeData", typeof(ObservableCollection<IPathItem>), typeof(DungeonMapperViewModel));
        public static readonly DependencyProperty CurrentMapProperty = DependencyProperty.Register("CurrentMap", typeof(Map), typeof(DungeonMapperViewModel));

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

        public Map CurrentMap
        {
            get => (Map)GetValue(CurrentMapProperty);
            set => SetValue(CurrentMapProperty, value);
        }

        #endregion

        #region Commands

        private RelayCommand _mapKeyDownCommand;
        private RelayCommand _windowKeyDownCommand;
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
        private RelayCommand _openSettingsCommand;
        private RelayCommand _closeWindowCommand;

        public RelayCommand MapKeyDownCommand => _mapKeyDownCommand ??= new RelayCommand(eventArgs => HandleMapKeyDown((KeyEventArgs)eventArgs), o => true);

        public RelayCommand WindowKeyDownCommand => _windowKeyDownCommand ??= new RelayCommand(eventArgs => HandleWindowKeyDown((KeyEventArgs)eventArgs), o => true);

        public RelayCommand SaveCurrentMapCommand => _saveCurrentMapCommand ??= new RelayCommand(o => SaveCurrentMap(), o => CurrentMap != null);

        public RelayCommand DeleteCurrentMapCommand => _deleteCurrentMapCommand ??= new RelayCommand(o => DeleteCurrentMap(), o => CurrentMap != null && CurrentMap.Id.HasValue);

        public RelayCommand ClearCurrentMapCommand => _clearCurrentMapCommand ??= new RelayCommand(o => ClearCurrentMap(), o => CurrentMap != null && CurrentMap.Id.HasValue);

        public RelayCommand HandleTreeSelectionChangedCommand => _handleTreeSelectionChangedCommand ??= new RelayCommand(eventArgs => HandleTreeSelectionChanged((RoutedPropertyChangedEventArgs<object>)eventArgs), o => true);

        public RelayCommand HandleTreeLeftMouseDownCommand => _handleTreeLeftMouseDownCommand ??= new RelayCommand(eventArgs => HandleTreeLeftMouseDown((MouseButtonEventArgs)eventArgs), o => true);

        public RelayCommand HandleTreeRightMouseDownCommand => _handleTreeRightMouseDownCommand ??= new RelayCommand(eventArgs => HandleTreeRightMouseDown((MouseButtonEventArgs)eventArgs), o => true);

        public RelayCommand HandleTreeMouseMoveCommand => _handleTreeMouseMoveCommand ??= new RelayCommand(eventArgs => HandleTreeMouseMove((MouseEventArgs)eventArgs), o => true);

        public RelayCommand HandleTreeDropCommand => _handleTreeDropCommand ??= new RelayCommand(eventArgs => HandleTreeDrop((DragEventArgs)eventArgs), o => true);

        public RelayCommand HandleWidowClosingCommand => _handleWidowClosingCommand ??= new RelayCommand(o => HandleWindowClosing(), o => true);

        public RelayCommand StartAddPathItemCommand => _startAddPathItemCommand ??= new RelayCommand(itemType => StartAddPathItem((PathItemType)itemType), o => AddEnabled);

        public RelayCommand StartRenamePathItemCommand => _startRenamePathItemCommand ??= new RelayCommand(o => StartRenamePathItem(), o => _selectedTreeItem != null);

        public RelayCommand CompleteEditPathItemCommand => _completeEditPathItemCommand ??= new RelayCommand(o => CompleteEditPathItem(), editEnabled => (bool)editEnabled);

        public RelayCommand DeletePathItemCommand => _deletePathItemCommand ??= new RelayCommand(o => DeletePathItem(), o => _selectedTreeItem != null);

        public RelayCommand OpenSettingsCommand => _openSettingsCommand ??= new RelayCommand(o => OpenSettings(), o => true);

        public RelayCommand CloseWindowCommand => _closeWindowCommand ??= new RelayCommand(o => _closeWindow(), o => true);

        #endregion

        public DungeonMapperViewModel(Action<Map> printAction, Action<Map> updateMapOffsets, Action closeWindow)
        {
            _printMap = printAction;
            _updateMapOffsets = updateMapOffsets;
            _closeWindow = closeWindow;
            DatabaseManager.InitializeDatabase();
            TreeData = new ObservableCollection<IPathItem>(BuildTreeData());

            // I might just want to save the IsExpanded and IsSelected states in general, but for now flipping back to the user's last map seems reasonable
            var currentMapId = SettingDataAccess.GetSetting<int?>(Setting.CurrentMapId);
            SetPathItemInTreeData<Map>(currentMapId, true);
            LoadSettings();
        }

        private List<IPathItem> BuildTreeData()
        {
            var maps = MapDataAccess.GetMaps();
            var data = new List<IPathItem>();
            data.AddRange(FolderDataAccess.GetFolders(maps));
            data.AddRange(maps.Where(map => !map.FolderId.HasValue));
            return data;
        }

        private void HandleMapKeyDown(KeyEventArgs args)
        {
            var shiftDown = args.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift);
            var ctrlDown = args.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);

            if (args.Key == Key.H)
            {
                CurrentMap.HallMode = !CurrentMap.HallMode;
                return;
            }

            if (args.Key == Key.D)
            {
                CurrentMap.ClearCurrentTile();
                _printMap(CurrentMap);
                return;
            }

            if (args.Key == Key.M)
            {
                CurrentMap.MarkTravel();
                _printMap(CurrentMap);
                return;
            }

            if (args.Key == Key.T)
            {
                CurrentMap.MarkTransport();
                _printMap(CurrentMap);
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

            _printMap(CurrentMap);
            _updateMapOffsets(CurrentMap);
        }

        private void HandleWindowKeyDown(KeyEventArgs args)
        {
            var ctrlDown = args.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control);

            if (args.Key == Key.S && ctrlDown && CurrentMap != null)
                SaveCurrentMap();
        }

        private void SaveCurrentMap()
        {
            CurrentMap.Id = MapDataAccess.SaveMap(CurrentMap);
        }

        private void DeleteCurrentMap()
        {
            MapDataAccess.DeleteMap(CurrentMap);
            RemovePathItemFromParent(CurrentMap);
            CurrentMap = null;
            _printMap(null);
        }

        private void ClearCurrentMap()
        {
            foreach (var tile in CurrentMap.MapData.SelectMany(tileArray => tileArray).Where(tile => tile != null && tile.Traveled))
                tile.Clear();
            _printMap(CurrentMap);
        }

        private void HandleTreeSelectionChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedTreeItem = e.NewValue as IPathItem;
            if (AddEnabled = e.NewValue is not Map)
                return;
            if (e.NewValue != null && e.NewValue != e.OldValue)
                ChangeMaps((Map)e.NewValue);
        }

        private void ChangeMaps(Map map)
        {
            if (map == null)
                return;
            if (AutoSaveEnabled)
                CurrentMap.Id = MapDataAccess.SaveMap(CurrentMap);
            CurrentMap = map;
            CurrentMap.LoadData();
            _printMap(CurrentMap);
        }

        private void HandleTreeLeftMouseDown(MouseButtonEventArgs e)
        {
            var selectedPathItem = (((e.OriginalSource as TextBlock)?.TemplatedParent as ContentPresenter)?.TemplatedParent as TreeViewItem)?.DataContext as IPathItem;
            if (selectedPathItem == null && _selectedTreeItem != null)
                _selectedTreeItem.IsSelected = false;
            _dragPositionStart = e.GetPosition(null);
        }

        private void HandleTreeRightMouseDown(MouseButtonEventArgs e)
        {
            var selectedPathItem = (((e.OriginalSource as TextBlock)?.TemplatedParent as ContentPresenter)?.TemplatedParent as TreeViewItem)?.DataContext as IPathItem;
            if (selectedPathItem == null && _selectedTreeItem != null)
                _selectedTreeItem.IsSelected = false;
            if (selectedPathItem != null)
                selectedPathItem.IsSelected = true;
            AddEnabled = selectedPathItem is not Map;
        }

        private void HandleTreeMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _selectedTreeItem != null)
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

            // This is to filter out Maps and considers everything that's not a folder to be the base path of the tree
            var destinationItem = (((e.OriginalSource as TextBlock)?.TemplatedParent as ContentPresenter)?.TemplatedParent as TreeViewItem)?.DataContext as IPathItem;
            if (sourceItem == destinationItem || destinationItem?.GetType() == typeof(Map))
                return;
            var destinationFolder = destinationItem as Folder;

            if (sourceItem is Folder)
            {
                var sourceFolder = sourceItem as Folder;

                RemovePathItemFromParent(sourceFolder);

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

                RemovePathItemFromParent(sourceMap);

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
            SettingDataAccess.SaveSetting(Setting.CurrentMapId, CurrentMap?.Id);
        }

        private void StartRenamePathItem()
        {
            _selectedTreeItem.EditModeEnabled = true;
        }

        private void StartAddPathItem(PathItemType type)
        {
            IPathItem newItem = null;

            if (type == PathItemType.Folder)
                newItem = new Folder { Parent = _selectedTreeItem as Folder };
            else if (type == PathItemType.Map)
            {
                newItem = new Map { FolderId = (_selectedTreeItem as Folder)?.Id };
                ((Map)newItem).Initialize();
            }

            newItem.IsSelected = newItem.EditModeEnabled = true;

            if (_selectedTreeItem != null)
            {
                _selectedTreeItem.ChildItems ??= new ObservableCollection<IPathItem>();
                _selectedTreeItem.ChildItems.Add(newItem);

                // Setting IsExpanded on _selectedTreeItem doesn't always work, there might be a binding or reference issue I'm missing on that
                var treeDataSelectedItem = FindPathItemParent(newItem);
                if (treeDataSelectedItem != null)
                    treeDataSelectedItem.IsExpanded = true;
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
            if (_selectedTreeItem is Map)
            {
                MapDataAccess.DeleteMap(_selectedTreeItem as Map);
                _printMap(null);
            }
            else if (_selectedTreeItem is Folder)
            {
                FolderDataAccess.DeleteFolderAndChildren(_selectedTreeItem as Folder);
            }
            RemovePathItemFromParent(_selectedTreeItem);
            _selectedTreeItem = null;
        }

        private void RemovePathItemFromParent(IPathItem item)
        {
            if (((item as Folder)?.Parent?.Id ?? (item as Map)?.FolderId) == null)
                TreeData.Remove(item);
            else
                ((item as Folder)?.Parent ?? FindPathItemParent(item)).ChildItems.Remove(item);
        }

        private void OpenSettings()
        {
            _settingsWindow = new SettingsWindow();
            _settingsWindow.ShowDialog();
            LoadSettings();
        }

        private void LoadSettings()
        {
            AutoSaveEnabled = SettingDataAccess.GetSetting<bool?>(Setting.AutoSaveEnabled) ?? false;
        }
    }
}
