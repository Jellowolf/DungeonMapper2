using DungeonMapperStandard.Models;
using System.Windows;
using System.Windows.Media;

namespace DungeonMapper2.Utilities
{
    public static class MapUtility
    {
        public static TileHost PrintToCanvas(this Map map)
        {
            var mapDrawing = new DrawingVisual();
            var drawingContext = mapDrawing.RenderOpen();

            int left, top;

            for (int indexY = 0; indexY < map.MaxIndexY + 1; indexY++)
            {
                for (int indexX = 0; indexX < map.MaxIndexX + 1; indexX++)
                {
                    var tile = map.MapData[indexX][map.MaxIndexY - indexY];
                    left = (indexX * map.TileSize) + 1;
                    top = (indexY * map.TileSize) + 1;

                    // draw the base for the tile if it's null or hasn't been marked for travel
                    var rectangle = new Rect(left, top, map.TileSize, map.TileSize);
                    if (tile == null || !tile.Traveled)
                    {
                        drawingContext.DrawRectangle(Brushes.Black, null, rectangle);
                        continue;
                    }
                    drawingContext.DrawRectangle(Brushes.DarkGray, new Pen { Thickness = 1, Brush = Brushes.Black }, rectangle);

                    // draw walls
                    if (tile.Walls.HasFlag(Wall.Up))
                        drawingContext.DrawRectangle(Brushes.DimGray, null, new Rect(left, top, map.TileSize, map.WallWidth));
                    if (tile.Walls.HasFlag(Wall.Down))
                        drawingContext.DrawRectangle(Brushes.DimGray, null, new Rect(left, top + map.TileSize - map.WallWidth, map.TileSize, map.WallWidth));
                    if (tile.Walls.HasFlag(Wall.Left))
                        drawingContext.DrawRectangle(Brushes.DimGray, null, new Rect(left, top, map.WallWidth, map.TileSize));
                    if (tile.Walls.HasFlag(Wall.Right))
                        drawingContext.DrawRectangle(Brushes.DimGray, null, new Rect(left + map.TileSize - map.WallWidth, top, map.WallWidth, map.TileSize));

                    // draw doors
                    if (tile.Doors.HasFlag(Wall.Up))
                        drawingContext.DrawRectangle(Brushes.Brown, null, new Rect(left, top, map.TileSize, map.DoorWidth));
                    if (tile.Doors.HasFlag(Wall.Down))
                        drawingContext.DrawRectangle(Brushes.Brown, null, new Rect(left, top + map.TileSize - map.DoorWidth, map.TileSize, map.DoorWidth));
                    if (tile.Doors.HasFlag(Wall.Left))
                        drawingContext.DrawRectangle(Brushes.Brown, null, new Rect(left, top, map.DoorWidth, map.TileSize));
                    if (tile.Doors.HasFlag(Wall.Right))
                        drawingContext.DrawRectangle(Brushes.Brown, null, new Rect(left + map.TileSize - map.DoorWidth, top, map.DoorWidth, map.TileSize));

                    // draw transport if available
                    if (tile.Transport != null)
                    {
                        var halfTileSize = (double)map.TileSize / 2;
                        var quarterTileSize = (double)map.TileSize / 4;

                        if (tile.Transport == TransportType.Unknown)
                            drawingContext.DrawEllipse(Brushes.Black, null, new Point(left + halfTileSize, top + halfTileSize), quarterTileSize, quarterTileSize);
                        else if (tile.Transport == TransportType.Pit)
                            drawingContext.DrawEllipse(Brushes.Black, null, new Point(left + halfTileSize, top + halfTileSize), quarterTileSize, quarterTileSize);
                        else if (tile.Transport == TransportType.Portal)
                            drawingContext.DrawEllipse(Brushes.Black, null, new Point(left + halfTileSize, top + halfTileSize), quarterTileSize, quarterTileSize);
                    }
                }
            }

            left = map.Position.x * map.TileSize;
            top = (map.MaxIndexY - map.Position.y) * map.TileSize;

            // draw the _position marker
            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + 1, top + 1, 3, 7));
            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + 1, top + 1, 7, 3));

            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + 1, top + map.TileSize - 6, 3, 7));
            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + 1, top + map.TileSize - 2, 7, 3));

            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + map.TileSize - 6, top + 1, 7, 3));
            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + map.TileSize - 2, top + 1, 3, 7));

            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + map.TileSize - 6, top + map.TileSize - 2, 7, 3));
            drawingContext.DrawRectangle(Brushes.Red, null, new Rect(left + map.TileSize - 2, top + map.TileSize - 6, 3, 7));

            drawingContext.Close();
            return new TileHost { VisualElement = mapDrawing };
        }
    }
}
