using System;
using Microsoft.AspNetCore.Identity;
using TicketManagementSystem.Infrastructure.Identity;
using System.Security.Cryptography;
using System.Text;
using System.Data.SqlClient;
using TicketManagementSystem.Infrastructure;

namespace TicketManagementSystem.Application.Services
{
    public partial class AuthService
    {
        private readonly PasswordHasher<object> _passwordHasher = new PasswordHasher<object>();

        private string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var result = _passwordHasher.VerifyHashedPassword(null, hash, password);
            return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
        }

        public async Task<ResponseModel<AuthResultDto>> RegisterAndLoginAsync(RegisterDto model)
        {
            // register and return token
            var hash = HashPassword(model.Password);
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = SqlHelper.CreateCommand(conn, "sp_RegisterUser",
                new SqlParameter("@UserName", model.UserName),
                new SqlParameter("@NormalizedUserName", model.UserName.ToUpperInvariant()),
                new SqlParameter("@Email", model.Email),
                new SqlParameter("@NormalizedEmail", model.Email.ToUpperInvariant()),
                new SqlParameter("@PasswordHash", hash),
                new SqlParameter("@FullName", model.FullName),
                new SqlParameter
                {
                    ParameterName = "@NewUserId",
                    SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
                    Direction = System.Data.ParameterDirection.Output
                }
            );
            cmd.ExecuteNonQuery();
            var newUserId = (Guid)cmd.Parameters["@NewUserId"].Value;

            // generate tokens
            var token = GenerateJwtToken(newUserId.ToString());
            var refresh = TokenGenerator.GenerateRefreshToken();
            SaveRefreshToken(refresh, newUserId, DateTime.UtcNow.AddDays(_configuration.GetSection("Jwt").GetValue<int>("RefreshTokenExpireDays")));
            var authResult = new AuthResultDto { Token = token, RefreshToken = refresh, ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetSection("Jwt").GetValue<int>("ExpireMinutes")) };
            return new ResponseModel<AuthResultDto>{ Success = true, Message = "Registered", Data = authResult };
        }
    }
}
