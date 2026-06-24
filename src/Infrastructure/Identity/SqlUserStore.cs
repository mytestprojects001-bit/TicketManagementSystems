using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Data.SqlClient;
using System.Data;
using TicketManagementSystem.Domain;
using TicketManagementSystem.Infrastructure;

namespace TicketManagementSystem.Infrastructure.Identity
{
    // Minimal ApplicationUser class representing AspNetUsers rows
    public class ApplicationUser
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string NormalizedUserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; }
        public string FullName { get; set; }
    }

    public class SqlUserStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserRoleStore<ApplicationUser>
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public SqlUserStore(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public void Dispose() { }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"INSERT INTO AspNetUsers(Id, UserName, NormalizedUserName, Email, NormalizedEmail, PasswordHash, FullName)
                                VALUES(@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @PasswordHash, @FullName)";
            cmd.Parameters.AddWithValue("@Id", user.Id);
            cmd.Parameters.AddWithValue("@UserName", user.UserName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@NormalizedUserName", user.NormalizedUserName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@NormalizedEmail", user.NormalizedUserName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@FullName", user.FullName ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "DELETE FROM AspNetUsers WHERE Id = @Id";
            cmd.Parameters.AddWithValue("@Id", user.Id);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if(!Guid.TryParse(userId, out var id)) return null;
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT Id, UserName, NormalizedUserName, Email, PasswordHash, IsActive, FullName FROM AspNetUsers WHERE Id = @Id";
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if(await reader.ReadAsync(cancellationToken))
            {
                return new ApplicationUser
                {
                    Id = reader.GetGuid(0),
                    UserName = reader.GetString(1),
                    NormalizedUserName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    PasswordHash = reader.IsDBNull(4) ? null : reader.GetString(4),
                    IsActive = !reader.IsDBNull(5) && reader.GetBoolean(5),
                    FullName = reader.IsDBNull(6) ? null : reader.GetString(6)
                };
            }
            return null;
        }

        public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT Id, UserName, NormalizedUserName, Email, PasswordHash, IsActive, FullName FROM AspNetUsers WHERE NormalizedUserName = @NormalizedUserName";
            cmd.Parameters.AddWithValue("@NormalizedUserName", normalizedUserName);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if(await reader.ReadAsync(cancellationToken))
            {
                return new ApplicationUser
                {
                    Id = reader.GetGuid(0),
                    UserName = reader.GetString(1),
                    NormalizedUserName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    PasswordHash = reader.IsDBNull(4) ? null : reader.GetString(4),
                    IsActive = !reader.IsDBNull(5) && reader.GetBoolean(5),
                    FullName = reader.IsDBNull(6) ? null : reader.GetString(6)
                };
            }
            return null;
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "UPDATE AspNetUsers SET UserName=@UserName, NormalizedUserName=@NormalizedUserName, Email=@Email, PasswordHash=@PasswordHash, FullName=@FullName WHERE Id=@Id";
            cmd.Parameters.AddWithValue("@UserName", user.UserName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@NormalizedUserName", user.NormalizedUserName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@FullName", user.FullName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Id", user.Id);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            return IdentityResult.Success;
        }

        // Password store
        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }

        // Role store minimal implementations
        public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            // find role id
            using(var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT Id FROM AspNetRoles WHERE NormalizedName = @NormalizedName";
                cmd.Parameters.AddWithValue("@NormalizedName", roleName.ToUpperInvariant());
                var roleIdObj = await cmd.ExecuteScalarAsync(cancellationToken);
                if(roleIdObj == null) return;
                var roleId = (Guid)roleIdObj;

                using var ins = conn.CreateCommand();
                ins.CommandType = CommandType.Text;
                ins.CommandText = "INSERT INTO AspNetUserRoles(UserId, RoleId) VALUES(@UserId, @RoleId)";
                ins.Parameters.AddWithValue("@UserId", user.Id);
                ins.Parameters.AddWithValue("@RoleId", roleId);
                await ins.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using(var cmd = conn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "DELETE ur FROM AspNetUserRoles ur INNER JOIN AspNetRoles r ON ur.RoleId = r.Id WHERE ur.UserId = @UserId AND r.NormalizedName = @NormalizedName";
                cmd.Parameters.AddWithValue("@UserId", user.Id);
                cmd.Parameters.AddWithValue("@NormalizedName", roleName.ToUpperInvariant());
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var roles = new List<string>();
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT r.Name FROM AspNetRoles r INNER JOIN AspNetUserRoles ur ON r.Id = ur.RoleId WHERE ur.UserId = @UserId";
            cmd.Parameters.AddWithValue("@UserId", user.Id);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while(await reader.ReadAsync(cancellationToken))
            {
                roles.Add(reader.GetString(0));
            }
            return roles;
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT COUNT(1) FROM AspNetUserRoles ur INNER JOIN AspNetRoles r ON ur.RoleId = r.Id WHERE ur.UserId = @UserId AND r.NormalizedName = @NormalizedName";
            cmd.Parameters.AddWithValue("@UserId", user.Id);
            cmd.Parameters.AddWithValue("@NormalizedName", roleName.ToUpperInvariant());
            var cnt = (int)await cmd.ExecuteScalarAsync(cancellationToken);
            return cnt > 0;
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var list = new List<ApplicationUser>();
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"SELECT u.Id, u.UserName, u.NormalizedUserName, u.Email, u.PasswordHash, u.IsActive, u.FullName
                                FROM AspNetUsers u
                                INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                                INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
                                WHERE r.NormalizedName = @NormalizedName";
            cmd.Parameters.AddWithValue("@NormalizedName", roleName.ToUpperInvariant());
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while(await reader.ReadAsync(cancellationToken))
            {
                list.Add(new ApplicationUser
                {
                    Id = reader.GetGuid(0),
                    UserName = reader.GetString(1),
                    NormalizedUserName = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Email = reader.IsDBNull(3) ? null : reader.GetString(3),
                    PasswordHash = reader.IsDBNull(4) ? null : reader.GetString(4),
                    IsActive = !reader.IsDBNull(5) && reader.GetBoolean(5),
                    FullName = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }
            return list;
        }

        // Not implemented helpers
        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);
        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id.ToString());
    }
}
