using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace TicketManagementSystem.Infrastructure
{
    public abstract class BaseRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        protected BaseRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        protected SqlConnection GetOpenSqlConnection()
        {
            return (SqlConnection)_dbConnectionFactory.CreateConnection();
        }

        protected void ExecuteNonQuery(string spName, params SqlParameter[] parameters)
        {
            using var conn = GetOpenSqlConnection();
            using var cmd = SqlHelper.CreateCommand(conn, spName, parameters);
            cmd.ExecuteNonQuery();
        }

        protected object ExecuteScalar(string spName, params SqlParameter[] parameters)
        {
            using var conn = GetOpenSqlConnection();
            using var cmd = SqlHelper.CreateCommand(conn, spName, parameters);
            return cmd.ExecuteScalar();
        }

        protected IEnumerable<T> ExecuteReader<T>(string spName, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        {
            var list = new List<T>();
            using var conn = GetOpenSqlConnection();
            using var cmd = SqlHelper.CreateCommand(conn, spName, parameters);
            using var reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                list.Add(map(reader));
            }
            return list;
        }
    }
}
