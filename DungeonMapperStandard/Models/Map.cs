using DungeonMapperStandard.DataAccess;
using System;

namespace DungeonMapperStandard.Models
{
    public class Map : BasePathItem
    {
        private Tile[][] _mapData;
        public Tile[][] MapData { get => _mapData; set => _mapData = value; }

        private (int x, int y) _position;
        public (int x, int y) Position { get => _position; set => _position = value; }

        public bool HallMode { get; set; }

        public int WallWidth { get; set; }

        public int DoorWidth { get; set; }

        public int TileSize { get; set; }

        public int MaxIndexX { get; set; }

        public int MaxIndexY { get; set; }

        public int? FolderId { get; set; }

        public override SegoeIcon Icon => SegoeIcon.Document;

        public Map()
        {
            TileSize = 25;
            WallWidth = 6;
            DoorWidth = 6;
        }

        public void Initialize()
        {
            _position = (0, 0);
            _mapData = new Tile[][] { new Tile[] { new Tile(true) } };
            MaxIndexX = 0;
            MaxIndexY = 0;
            HallMode = false;
        }

        public (int x, int y) MoveUp()
        {
            _position.y += 1;
            if (MaxIndexY < _position.y)
            {
                for (var index = 0; index < _mapData.Length; index++)
                {
                    Array.Resize(ref _mapData[index], _mapData[index].Length + 1);
                }
                MaxIndexY++;
            }
            UpdateTile(Movement.Up);
            return _position;
        }

        public (int x, int y) MoveDown()
        {
            _position.y -= 1;
            if (_position.y < 0)
            {
                for (var index = 0; index < _mapData.Length; index++)
                {
                    Array.Resize(ref _mapData[index], _mapData[index].Length + 1);
                    Array.Copy(_mapData[index], 0, _mapData[index], 1, _mapData[index].Length - 1);
                    _mapData[index][0] = new Tile(false);
                }
                MaxIndexY++;
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
                Array.Resize(ref _mapData, _mapData.Length + 1);
                Array.Copy(_mapData, 0, _mapData, 1, _mapData.Length - 1);
                _mapData[0] = new Tile[MaxIndexY + 1];
                MaxIndexX++;
                _position.x = 0;
            }
            UpdateTile(Movement.Left);
            return _position;
        }

        public (int x, int y) MoveRight()
        {
            _position.x += 1;
            if (MaxIndexX < _position.x)
            {
                Array.Resize(ref _mapData, _mapData.Length + 1);
                _mapData[_mapData.Length - 1] = new Tile[MaxIndexY + 1];
                MaxIndexX++;
            }
            UpdateTile(Movement.Right);
            return _position;
        }

        public void ClearCurrentTile()
        {
            _mapData[_position.x][_position.y].Clear();
        }

        private void UpdateTile(Movement movement)
        {
            var tileTraveled = _mapData[_position.x][_position.y] != null && _mapData[_position.x][_position.y].Traveled;
            if (!tileTraveled)
            {
                MarkTravel();
                if (HallMode && !tileTraveled) SetHalls(movement);
            }
        }

        public void MarkTravel()
        {
            if (_mapData[_position.x][_position.y] != null)
                _mapData[_position.x][_position.y].Traveled = true;
            else
                _mapData[_position.x][_position.y] = new Tile(true);
        }

        public void MarkTransport()
        {
            _mapData[_position.x][_position.y].Transport = !_mapData[_position.x][_position.y].Transport.HasValue ? TransportType.Unknown : (TransportType?)null;
        }

        private void SetHalls(Movement movement)
        {
            // set current tile walls
            if (movement == Movement.Up || movement == Movement.Down)
                _mapData[_position.x][_position.y].Walls |= Wall.Left | Wall.Right;
            if (movement == Movement.Left || movement == Movement.Right)
                _mapData[_position.x][_position.y].Walls |= Wall.Up | Wall.Down;

            // set previous tile walls
            if (movement == Movement.Up)
                _mapData[_position.x][_position.y - 1].Walls &= ~Wall.Up;
            if (movement == Movement.Down)
                _mapData[_position.x][_position.y + 1].Walls &= ~Wall.Down;
            if (movement == Movement.Left)
                _mapData[_position.x + 1][_position.y].Walls &= ~Wall.Left;
            if (movement == Movement.Right)
                _mapData[_position.x - 1][_position.y].Walls &= ~Wall.Right;
        }

        public void SetTileWall(Wall wall)
        {
            if (_mapData[_position.x][_position.y].Walls.HasFlag(wall))
                _mapData[_position.x][_position.y].Walls &= ~wall;
            else
                _mapData[_position.x][_position.y].Walls |= wall;
        }

        public void SetTileDoor(Wall wall)
        {
            if (_mapData[_position.x][_position.y].Doors.HasFlag(wall))
                _mapData[_position.x][_position.y].Doors &= ~wall;
            else
                _mapData[_position.x][_position.y].Doors |= wall;
        }

        public string PrintToString()
        {
            var mapString = string.Empty;
            for (int indexY = 0; indexY < MaxIndexY + 1; indexY++)
            {
                for (int indexX = 0; indexX < MaxIndexX + 1; indexX++)
                {
                    var tile = _mapData[indexX][MaxIndexY - indexY];
                    mapString += (tile != null && tile.Traveled) ? "1" : "0";
                }
                mapString += "\n";
            }
            return mapString;
        }

        public void LoadData()
        {
            if (!Id.HasValue) return;
            _mapData = TileDataAccess.GetTiles(Id.Value);
            MaxIndexX = _mapData.Length - 1;
            MaxIndexY = _mapData[0].Length - 1;
        }
    }
}
