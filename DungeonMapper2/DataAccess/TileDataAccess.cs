using DungeonMapper2.Models;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;

namespace DungeonMapper2.DataAccess
{
    public class TileDataAccess
    {
        public static void SaveTile(int mapId, Tile tile)
        {
            using var database = DatabaseManager.GetDatabaseConnection();
            database.Open();
            var sql = $"INSERT INTO Tile (MapId, Traveled, Walls, Door) VALUES ({mapId}, {tile.Traveled}, {tile.Walls}, {tile.Doors})";
            var command = new SqliteCommand(sql, database);
            command.ExecuteNonQuery();
        }

        public static void SaveTiles(int mapId, Tile[][] tiles)
        {
            var tileinput = new List<string>();
            for (int x = 0; x < tiles.Length; x++)
                for (int y = 0; y < tiles[x].Length; y++)
                    if (tiles[x][y] != null && (tiles[x][y].Traveled || tiles[x][y].Id.HasValue)) tileinput.Add($"({(tiles[x][y].Id.HasValue ? tiles[x][y].Id.ToString() : "NULL")}, {mapId}, {x}, {y}, {(tiles[x][y].Traveled ? 1 : 0)}, {(int)tiles[x][y].Walls}, {(int)tiles[x][y].Doors})");
            using var database = DatabaseManager.GetDatabaseConnection();
            database.Open();
            var sql = @$"INSERT INTO Tile (Id, MapId, PositionX, PositionY, Traveled, Walls, Doors) VALUES {string.Join(",", tileinput)} 
                ON CONFLICT(Id) DO UPDATE SET MapId = excluded.MapId, PositionX = excluded.PositionX, PositionY = excluded.PositionY, Traveled = excluded.Traveled, Walls = excluded.Walls, Doors = excluded.Doors";
            var command = new SqliteCommand(sql, database);
            command.ExecuteNonQuery();
        }

        public static Tile[][] GetTiles(int mapId)
        {
            using var database = DatabaseManager.GetDatabaseConnection();
            database.Open();
            var sql = $"SELECT Id, PositionX, PositionY, Traveled, Walls, Doors FROM Tile WHERE MapId = {mapId}";
            var command = new SqliteCommand(sql, database);
            using var reader = command.ExecuteReader();
            var tileDictionary = new Dictionary<(int x, int y), Tile>();
            while (reader.Read()) {
                tileDictionary.Add(
                    (reader.GetInt32(reader.GetOrdinal("PositionX")), reader.GetInt32(reader.GetOrdinal("PositionY"))),
                    new Tile
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Traveled = reader.GetBoolean(reader.GetOrdinal("Traveled")),
                        Walls = (Wall)reader.GetInt32(reader.GetOrdinal("Walls")),
                        Doors = (Wall)reader.GetInt32(reader.GetOrdinal("Doors")),
                    });
            }
            var maxX = tileDictionary.Select(tile => tile.Key.x).Max();
            var maxY = tileDictionary.Select(tile => tile.Key.y).Max();
            var tileArray = new Tile[maxX + 1][];
            for (int x = 0; x < maxX + 1; x++)
            {
                tileArray[x] = new Tile[maxY + 1];
                for (int y = 0; y < maxY + 1; y++)
                    tileDictionary.TryGetValue((x, y), out tileArray[x][y]);
            }
            return tileArray;
        }

        public static void DeleteTiles(int mapId)
        {
            using var database = DatabaseManager.GetDatabaseConnection();
            database.Open();
            var sql = $"DELETE FROM Tile WHERE MapId = {mapId}";
            var command = new SqliteCommand(sql, database);
            command.ExecuteNonQuery();
        }
    }
}
