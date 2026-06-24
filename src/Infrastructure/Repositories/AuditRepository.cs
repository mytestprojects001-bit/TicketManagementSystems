using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace TicketManagementSystem.Infrastructure.Repositories
{
    public interface IAuditRepository
    {
        Task LogAsync(string entity, string action, string performedBy, string details);
    }

    public class AuditRepository : IAuditRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public AuditRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task LogAsync(string entity, string action, string performedBy, string details)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "INSERT INTO AuditLogs(Entity, Action, PerformedBy, PerformedAt, Details) VALUES(@Entity, @Action, @PerformedBy, SYSUTCDATETIME(), @Details)";
            cmd.Parameters.AddWithValue("@Entity", entity);
            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@PerformedBy", performedBy ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Details", details ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
