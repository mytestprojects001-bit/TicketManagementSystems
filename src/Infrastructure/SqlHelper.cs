using System.Data;
using System.Data.SqlClient;

namespace TicketManagementSystem.Infrastructure
{
    public static class SqlHelper
    {
        public static SqlCommand CreateCommand(SqlConnection connection, string storedProcedureName, params SqlParameter[] parameters)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = storedProcedureName;
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return cmd;
        }
    }
}
