using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace DungeonMapper2.DataAccess
{
    public class MapDataAccess
    {
        public static int SaveMap(Map map)
        {
            int? mapId = map.Id;
            using var database = DatabaseManager.GetDatabaseConnection();
            database.Open();
            var sql = @$"INSERT INTO Map (Id, Name, PositionX, PositionY, FolderId) VALUES ({(map.Id.HasValue ? map.Id.ToString() : "NULL")}, '{map.Name}', {map.Position.x}, {map.Position.y}, {(map.FolderId.HasValue ? map.Id.ToString() : "NULL")})
                ON CONFLICT(Id) DO UPDATE SET Name = excluded.Name, PositionX = excluded.PositionX, PositionY = excluded.PositionY, FolderId = excluded.FolderId;
                SELECT LAST_INSERT_ROWID()";
            var command = new SqliteCommand(sql, database);
            using var reader = command.ExecuteReader();
            if (!mapId.HasValue)
                while (reader.Read()) { mapId = reader.GetInt32(0); }
            if (!mapId.HasValue)
                throw new Exception("Failed to create Map and return an Id.");
            TileDataAccess.SaveTiles(mapId.Value, map.mapData);
            map.mapData = TileDataAccess.GetTiles(mapId.Value);
            return mapId.Value;
        }

        public static List<Map> GetMaps()
        {
            using var database = DatabaseManager.GetDatabaseConnection();
            database.Open();
            var sql = $"SELECT Id, Name, PositionX, PositionY, FolderId FROM Map";
            var command = new SqliteCommand(sql, database);
            using var reader = command.ExecuteReader();
            var maps = new List<Map>();
            while (reader.Read())
            {
                maps.Add(new Map
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Position = (reader.GetInt32(reader.GetOrdinal("PositionX")), reader.GetInt32(reader.GetOrdinal("PositionY"))),
                    FolderId = !reader.IsDBNull(reader.GetOrdinal("FolderId")) ? reader.GetInt32(reader.GetOrdinal("FolderId")) : (int?)null
                });
            }
            return maps;
        }

        public static void DeleteMap(Map map)
        {
            if (!map.Id.HasValue)
                return;
            TileDataAccess.DeleteTiles(map.Id.Value);
            using var database = DatabaseManager.GetDatabaseConnection();
            database.Open();
            var sql = $"DELETE FROM Map WHERE Id = {map.Id}";
            var command = new SqliteCommand(sql, database);
            command.ExecuteNonQuery();
        }
    }
}
