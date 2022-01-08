using System.Data;

namespace DungeonMapperStandard.DataAccess
{
    public interface IDatabaseConnectionHandler
    {
        IDbConnection CreateDatabaseConnection(string appDataPath);

        IDbCommand CreateSqlCommand(string sql, IDbConnection databaseConnection);
    }
}
