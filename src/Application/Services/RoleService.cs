using TicketManagementSystem.Shared;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace TicketManagementSystem.Application.Services
{
    public interface IRoleService
    {
        Task<ResponseModel<object>> AssignPermissionToRoleAsync(Guid roleId, string permission);
        Task<ResponseModel<object>> RemovePermissionFromRoleAsync(Guid roleId, string permission);
    }

    public class RoleService : IRoleService
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public RoleService(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<ResponseModel<object>> AssignPermissionToRoleAsync(Guid roleId, string permission)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "INSERT INTO RolePermissions(RoleId, Permission) VALUES(@RoleId, @Permission)";
            cmd.Parameters.AddWithValue("@RoleId", roleId);
            cmd.Parameters.AddWithValue("@Permission", permission);
            await cmd.ExecuteNonQueryAsync();
            return new ResponseModel<object>{ Success = true, Message = "Permission assigned to role" };
        }

        public async Task<ResponseModel<object>> RemovePermissionFromRoleAsync(Guid roleId, string permission)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "DELETE FROM RolePermissions WHERE RoleId = @RoleId AND Permission = @Permission";
            cmd.Parameters.AddWithValue("@RoleId", roleId);
            cmd.Parameters.AddWithValue("@Permission", permission);
            await cmd.ExecuteNonQueryAsync();
            return new ResponseModel<object>{ Success = true, Message = "Permission removed from role" };
        }
    }
}
