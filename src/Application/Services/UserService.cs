using TicketManagementSystem.Shared;
using System.Data.SqlClient;
using System.Data;

namespace TicketManagementSystem.Application.Services
{
    public interface IUserService
    {
        Task<ResponseModel<object>> AssignRoleToUserAsync(Guid userId, Guid roleId);
        Task<ResponseModel<object>> RemoveRoleFromUserAsync(Guid userId, Guid roleId);
    }

    public class UserService : IUserService
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public UserService(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<ResponseModel<object>> AssignRoleToUserAsync(Guid userId, Guid roleId)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "INSERT INTO AspNetUserRoles(UserId, RoleId) VALUES(@UserId, @RoleId)";
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@RoleId", roleId);
            await cmd.ExecuteNonQueryAsync();
            return new ResponseModel<object>{ Success = true, Message = "Role assigned to user" };
        }

        public async Task<ResponseModel<object>> RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "DELETE FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId";
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@RoleId", roleId);
            await cmd.ExecuteNonQueryAsync();
            return new ResponseModel<object>{ Success = true, Message = "Role removed from user" };
        }
    }
}
