using DungeonMapperStandard.Models;
using System;

namespace DungeonMapperStandard.DataAccess
{
    public class SettingDataAccess
    {
        public static void SaveSetting(Setting setting, object value)
        {
            using (var database = DatabaseManager.CreateDatabaseConnection())
            {
                database.Open();
                var sql = $@"INSERT INTO Setting (Id, Value) VALUES ({(int)setting}, {(value == null ? "NULL" : $"'{value}'")})
                ON CONFLICT(Id) DO UPDATE SET Value = excluded.Value";
                var command = DatabaseManager.CreateSqlCommand(sql, database);
                command.ExecuteNonQuery();
            }
        }

        // The typing here is a bit lazy, but it doesn't need to be that robust
        public static T GetSetting<T>(Setting setting)
        {
            using (var database = DatabaseManager.CreateDatabaseConnection())
            {
                database.Open();
                var sql = $"SELECT Value FROM Setting WHERE Id = {(int)setting}";
                var command = DatabaseManager.CreateSqlCommand(sql, database);
                using (var reader = command.ExecuteReader())
                {
                    string dbValue = null;
                    while (reader.Read()) { dbValue = !reader.IsDBNull(0) ? reader.GetString(0) : null; }
                    return dbValue != null ? (T)Convert.ChangeType(dbValue, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T)) : default;
                }
            }
        }
    }
}
