using Microsoft.Data.Sqlite;
using System.IO;

namespace DungeonMapperStandard.DataAccess
{
    public static class DatabaseManager
    {
        public static string AppDataPath { get; set; }

        public static void InitializeDatabase()
        {
            var dbFilePath = Path.Combine(AppDataPath, "storage.db");

            if (File.Exists(dbFilePath))
                return;

            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);

            var dbFile = File.Create(dbFilePath);
            dbFile.Close();

            using (var database = new SqliteConnection($"Filename={dbFilePath}"))
            {
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
                        Doors INTEGER NOT NULL,
                        Transport INTEGER NULL
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
        }

        public static SqliteConnection GetDatabaseConnection()
        {
            return new SqliteConnection($"Filename={Path.Combine(AppDataPath, "storage.db")}");
        }
    }
}
