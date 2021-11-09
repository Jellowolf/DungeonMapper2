using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace DungeonMapper2.DataAccess
{
    public static class DatabaseManager
    {
        public static void InitializeDatabase()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DungeonMapper2");
            var dbFilePath = Path.Combine(appDataPath, "storage.db");

            if (File.Exists(dbFilePath))
                return;

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            var dbFile = File.Create(dbFilePath);
            dbFile.Close();

            using var database = new SqliteConnection($"Filename={dbFilePath}");
            database.Open();

            string sql =
                @"CREATE TABLE Folder (
                        Id INTEGER PRIMARY KEY,
                        Name VARCHAR(256) NOT NULL,
                        ParentFolderId INTEGER NULL
                )";

            var command = new SqliteCommand(sql, database);
            command.ExecuteNonQuery();

            sql =
                @"CREATE TABLE Map (
                        Id INTEGER PRIMARY KEY,
                        Name VARCHAR(256) NOT NULL,
                        PositionX INTEGER NOT NULL,
                        PositionY INTEGER NOT NULL,
                        FolderId INTEGER NULL
                )";

            command = new SqliteCommand(sql, database);
            command.ExecuteNonQuery();

            sql =
                @"CREATE TABLE Tile (
                        Id INTEGER PRIMARY KEY,
                        MapId INTEGER NOT NULL,
                        PositionX INTEGER NOT NULL,
                        PositionY INTEGER NOT NULL,
                        Traveled INTEGER NOT NULL,
                        Walls INTEGER NOT NULL,
                        Doors INTEGER NOT NULL
                )";

            command = new SqliteCommand(sql, database);
            command.ExecuteNonQuery();

            sql =
                @"CREATE TABLE Setting (
                        Id INTEGER PRIMARY KEY,
                        Value VARCHAR(256) NULL
                )";

            command = new SqliteCommand(sql, database);
            command.ExecuteNonQuery();
        }

        public static SqliteConnection GetDatabaseConnection()
        {
            var dbFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DungeonMapper2\\storage.db");
            return new SqliteConnection($"Filename={dbFilePath}");
        }
    }
}
