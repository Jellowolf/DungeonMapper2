using DungeonMapper2.Models;
using Microsoft.Data.Sqlite;
using System;

namespace DungeonMapper2.DataAccess
{
    public class SettingDataAccess
    {
        public static void SaveSetting(Setting setting, object value)
        {
            using var database = DatabaseManager.GetDatabaseConnection();
            database.Open();
            var sql = @$"INSERT INTO Setting (Id, Value) VALUES ({(int)setting}, '{(value == null ? "NULL" : value)}')
                ON CONFLICT(Id) DO UPDATE SET Value = excluded.Value";
            var command = new SqliteCommand(sql, database);
            command.ExecuteNonQuery();
        }

        // The typing here is a bit lazy, but it doesn't need to be that robust
        public static T GetSetting<T>(Setting setting)
        {
            using var database = DatabaseManager.GetDatabaseConnection();
            database.Open();
            var sql = $"SELECT Value FROM Setting WHERE Id = {(int)setting}";
            var command = new SqliteCommand(sql, database);
            using var reader = command.ExecuteReader();
            string dbValue = null;
            while (reader.Read()) { dbValue = reader.GetString(0); }
            return dbValue != null ? (T)Convert.ChangeType(dbValue, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T)) : default;
        }
    }
}
