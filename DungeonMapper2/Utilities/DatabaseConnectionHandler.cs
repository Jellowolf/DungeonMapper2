using DungeonMapperStandard.DataAccess;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.IO;

namespace DungeonMapper2.Utilities
{
    public class DatabaseConnectionHandler : IDatabaseConnectionHandler
    {
        public IDbConnection CreateDatabaseConnection(string appDataPath)
        {
            return new SqliteConnection($"Filename={Path.Combine(appDataPath, "storage.db")}");
        }

        public IDbCommand CreateSqlCommand(string sql, IDbConnection databaseConnection)
        {
            if (!(databaseConnection is SqliteConnection))
                throw new ArgumentException($"Database Connection of type {databaseConnection.GetType()} is not support.");
            return new SqliteCommand(sql, databaseConnection as SqliteConnection);
        }
    }
}
