using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Data.SqlClient;
using System.Data;
using TicketManagementSystem.Infrastructure;

namespace TicketManagementSystem.Infrastructure.Identity
{
    public class SqlRoleStore : IRoleStore<IdentityRole>
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public SqlRoleStore(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public void Dispose() { }

        public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "INSERT INTO AspNetRoles(Id, Name, NormalizedName, ConcurrencyStamp) VALUES(@Id, @Name, @NormalizedName, @ConcurrencyStamp)";
            cmd.Parameters.AddWithValue("@Id", Guid.Parse(role.Id));
            cmd.Parameters.AddWithValue("@Name", role.Name ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@NormalizedName", role.NormalizedName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ConcurrencyStamp", role.ConcurrencyStamp ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "DELETE FROM AspNetRoles WHERE Id = @Id";
            cmd.Parameters.AddWithValue("@Id", Guid.Parse(role.Id));
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            if(!Guid.TryParse(roleId, out var id)) return null;
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT Id, Name, NormalizedName, ConcurrencyStamp FROM AspNetRoles WHERE Id = @Id";
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if(await reader.ReadAsync(cancellationToken))
            {
                var r = new IdentityRole
                {
                    Id = reader.GetGuid(0).ToString(),
                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                    NormalizedName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ConcurrencyStamp = reader.IsDBNull(3) ? null : reader.GetString(3)
                };
                return r;
            }
            return null;
        }

        public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT Id, Name, NormalizedName, ConcurrencyStamp FROM AspNetRoles WHERE NormalizedName = @NormalizedName";
            cmd.Parameters.AddWithValue("@NormalizedName", normalizedRoleName);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if(await reader.ReadAsync(cancellationToken))
            {
                return new IdentityRole
                {
                    Id = reader.GetGuid(0).ToString(),
                    Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                    NormalizedName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ConcurrencyStamp = reader.IsDBNull(3) ? null : reader.GetString(3)
                };
            }
            return null;
        }

        public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken) => Task.FromResult(role.NormalizedName);
        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken) => Task.FromResult(role.Id);
        public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken) => Task.FromResult(role.Name);
        public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken) { role.NormalizedName = normalizedName; return Task.CompletedTask; }
        public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken) { role.Name = roleName; return Task.CompletedTask; }
        public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "UPDATE AspNetRoles SET Name=@Name, NormalizedName=@NormalizedName WHERE Id=@Id";
            cmd.Parameters.AddWithValue("@Name", role.Name ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@NormalizedName", role.NormalizedName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Id", Guid.Parse(role.Id));
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return IdentityResult.Success;
        }
    }
}
