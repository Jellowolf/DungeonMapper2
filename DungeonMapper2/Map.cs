using DungeonMapper2.DataAccess;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DungeonMapper2
{
    public class Map : BasePathItem
    {
        private (int x, int y) _position;
        public bool HallMode;
        public Tile[][] mapData;
        private int TileSize;
        private int WallWidth;
        private int DoorWidth;
        private TileHost MapHost;

        public int? Id { get; set; }

        public (int x, int y) Position { get => _position; set => _position = value; }

        public int maxIndexX { get; set; }

        public int maxIndexY { get; set; }

        public int? FolderId { get; set; }

        public Map()
        {
            TileSize = 25;
            WallWidth = 6;
            DoorWidth = 6;
        }

        public void Initialize()
        {
            _position = (0, 0);
            mapData = new Tile[][] { new Tile[] { new Tile(true) } };
            maxIndexX = 0;
            maxIndexY = 0;
            HallMode = false;
        }

        public (int x, int y) MoveUp()
        {
            _position.y += 1;
            if (maxIndexY < _position.y)
            {
                for (var index = 0; index < mapData.Length; index++)
                {
                    Array.Resize(ref mapData[index], mapData[index].Length + 1);
                }
                maxIndexY++;
            }
            UpdateTile(Movement.Up);
            return _position;
        }

        public (int x, int y) MoveDown()
        {
            _position.y -= 1;
            if (_position.y < 0)
            {
                for (var index = 0; index < mapData.Length; index++)
                {
                    Array.Resize(ref mapData[index], mapData[index].Length + 1);
                    Array.Copy(mapData[index], 0, mapData[index], 1, mapData[index].Length - 1);
                    mapData[index][0] = new Tile(false);
                }
                maxIndexY++;
                _position.y = 0;
            }
            UpdateTile(Movement.Down);
            return _position;
        }

        public (int x, int y) MoveLeft()
        {
            _position.x -= 1;
            if (_position.x < 0)
            {
                Array.Resize(ref mapData, mapData.Length + 1);
                Array.Copy(mapData, 0, mapData, 1, mapData.Length - 1);
                mapData[0] = new Tile[maxIndexY + 1];
                maxIndexX++;
                _position.x = 0;
            }
            UpdateTile(Movement.Left);
            return _position;
        }

        public (int x, int y) MoveRight()
        {
            _position.x += 1;
            if (maxIndexX < _position.x)
            {
                Array.Resize(ref mapData, mapData.Length + 1);
                mapData[mapData.Length - 1] = new Tile[maxIndexY + 1];
                maxIndexX++;
            }
            UpdateTile(Movement.Right);
            return _position;
        }

        public void ClearCurrentTile()
        {
            mapData[_position.x][_position.y].Clear();
        }

        private void UpdateTile(Movement movement)
        {
            var tileTraveled = mapData[_position.x][_position.y] != null && mapData[_position.x][_position.y].Traveled;
            if (!tileTraveled)
            {
                MarkTravel();
                if (HallMode && !tileTraveled) SetHalls(movement);
            }
        }

        public void MarkTravel()
        {
            if (mapData[_position.x][_position.y] != null)
                mapData[_position.x][_position.y].Traveled = true;
            else
                mapData[_position.x][_position.y] = new Tile(true);
        }

        private void SetHalls(Movement movement)
        {
            // set current tile walls
            if (movement == Movement.Up || movement == Movement.Down)
                mapData[_position.x][_position.y].Walls |= Wall.Left | Wall.Right;
            if (movement == Movement.Left || movement == Movement.Right)
                mapData[_position.x][_position.y].Walls |= Wall.Up | Wall.Down;

            // set previous tile walls
            if (movement == Movement.Up)
                mapData[_position.x][_position.y - 1].Walls &= ~Wall.Up;
            if (movement == Movement.Down)
                mapData[_position.x][_position.y + 1].Walls &= ~Wall.Down;
            if (movement == Movement.Left)
                mapData[_position.x + 1][_position.y].Walls &= ~Wall.Left;
            if (movement == Movement.Right)
                mapData[_position.x - 1][_position.y].Walls &= ~Wall.Right;
        }

        public void SetTileWall(Wall wall)
        {
            if (mapData[_position.x][_position.y].Walls.HasFlag(wall))
                mapData[_position.x][_position.y].Walls &= ~wall;
            else
                mapData[_position.x][_position.y].Walls |= wall;
        }

        public void SetTileDoor(Wall wall)
        {
            if (mapData[_position.x][_position.y].Doors.HasFlag(wall))
                mapData[_position.x][_position.y].Doors &= ~wall;
            else
                mapData[_position.x][_position.y].Doors |= wall;
        }

        public string PrintToString()
        {
            var mapString = string.Empty;
            for (int indexY = 0; indexY < maxIndexY + 1; indexY++)
            {
                for (int indexX = 0; indexX < maxIndexX + 1; indexX++)
                {
                    var tile = mapData[indexX][maxIndexY - indexY];
                    mapString += (tile != null && tile.Traveled) ? "1" : "0";
                }
                mapString += "\n";
            }
            return mapString;
        }

        public void PrintToCanvas(Canvas canvas)
        {
            canvas.Children.Clear();
            canvas.Height = maxIndexY * TileSize;
            canvas.Width = maxIndexX * TileSize;

            var mapDrawing = new DrawingVisual();
            var drawingContext = mapDrawing.RenderOpen();

            int left, top;

            for (int indexY = 0; indexY < maxIndexY + 1; indexY++)
            {
                for (int indexX = 0; indexX < maxIndexX + 1; indexX++)
                {
                    var tile = mapData[indexX][maxIndexY - indexY];
                    left = (indexX * TileSize) + 1;
                    top = (indexY * TileSize) + 1;

                    // draw the base for the tile if it's null or hasn't been marked for travel
                    var rectangle = new Rect(left, top, TileSize, TileSize);
                    if (tile == null || !tile.Traveled)
                    {
                        drawingContext.DrawRectangle(Brushes.Black, null, rectangle);
                        continue;
                    }
                    drawingContext.DrawRectangle(Brushes.DarkGray, new Pen { Thickness = 1, Brush = Brushes.Black }, rectangle);

                    // draw walls
                    if (tile.Walls.HasFlag(Wall.Up))
                        drawingContext.DrawRectangle(Brushes.Gray, null, new Rect(left, top, TileSize, WallWidth));
                    if (tile.Walls.HasFlag(Wall.Down))
                        drawingContext.DrawRectangle(Brushes.Gray, null, new Rect(left, top + TileSize - WallWidth, TileSize, WallWidth));
                    if (tile.Walls.HasFlag(Wall.Left))
                        drawingContext.DrawRectangle(Brushes.Gray, null, new Rect(left, top, WallWidth, TileSize));
                    if (tile.Walls.HasFlag(Wall.Right))
                        drawingContext.DrawRectangle(Brushes.Gray, null, new Rect(left + TileSize - WallWidth, top, WallWidth, TileSize));

                    // draw doors
                    if (tile.Doors.HasFlag(Wall.Up))
                        drawingContext.DrawRectangle(Brushes.Brown, null, new Rect(left, top, TileSize, DoorWidth));
                    if (tile.Doors.HasFlag(Wall.Down))
                        drawingContext.DrawRectangle(Brushes.Brown, null, new Rect(left, top + TileSize - DoorWidth, TileSize, DoorWidth));
                    if (tile.Doors.HasFlag(Wall.Left))
                        drawingContext.DrawRectangle(Brushes.Brown, null, new Rect(left, top, DoorWidth, TileSize));
                    if (tile.Doors.HasFlag(Wall.Right))
                        drawingContext.DrawRectangle(Brushes.Brown, null, new Rect(left + TileSize - DoorWidth, top, DoorWidth, TileSize));
                }
            }

            left = _position.x * TileSize;
            top = (maxIndexY - _position.y) * TileSize;

            // draw the _position marker
            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + 1, top + 1, 3, 7));
            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + 1, top + 1, 7, 3));

            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + 1, top + TileSize - 6, 3, 7));
            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + 1, top + TileSize - 2, 7, 3));

            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + TileSize - 6, top + 1, 7, 3));
            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + TileSize - 2, top + 1, 3, 7));

            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + TileSize - 6, top + TileSize - 2, 7, 3));
            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + TileSize - 2, top + TileSize - 6, 3, 7));

            drawingContext.Close();
            MapHost = new TileHost { VisualElement = mapDrawing };
            canvas.Children.Add(MapHost);
        }

        public void LoadData()
        {
            if (!Id.HasValue) return;
            mapData = TileDataAccess.GetTiles(Id.Value);
            maxIndexX = mapData.Length - 1;
            maxIndexY = mapData[0].Length - 1;
        }
    }
}
