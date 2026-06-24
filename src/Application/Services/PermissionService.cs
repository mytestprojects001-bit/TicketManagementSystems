using System.Data.SqlClient;
using TicketManagementSystem.Shared;

namespace TicketManagementSystem.Application.Services
{
    public interface IPermissionService
    {
        Task<bool> UserHasPermissionAsync(Guid userId, string permission);
    }

    public class PermissionService : IPermissionService
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public PermissionService(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<bool> UserHasPermissionAsync(Guid userId, string permission)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = @"
                SELECT COUNT(1) FROM (
                    SELECT p.Permission FROM RolePermissions p
                    INNER JOIN AspNetUserRoles ur ON p.RoleId = ur.RoleId
                    WHERE ur.UserId = @UserId
                    UNION
                    SELECT Permission FROM UserPermissions up WHERE up.UserId = @UserId
                ) t WHERE Permission = @Permission";
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Permission", permission);
            var cnt = (int)await cmd.ExecuteScalarAsync();
            return cnt > 0;
        }
    }
}
