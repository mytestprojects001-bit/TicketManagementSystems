using TicketManagementSystem.Shared;
using TicketManagementSystem.Application.DTOs;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace TicketManagementSystem.Application.Services
{
    public interface IAuthService
    {
        Task<ResponseModel<object>> RegisterAsync(RegisterDto model);
        Task<ResponseModel<AuthResultDto>> LoginAsync(LoginDto model);
    }

    public class AuthService : IAuthService
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IConfiguration _configuration;
        public AuthService(IDbConnectionFactory dbConnectionFactory, IConfiguration configuration)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _configuration = configuration;
        }

        public async Task<ResponseModel<object>> RegisterAsync(RegisterDto model)
        {
            // Password hashing
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
            return new ResponseModel<object> { Success = true, Message = "User registered", Data = new { UserId = newUserId } };
        }

        public async Task<ResponseModel<AuthResultDto>> LoginAsync(LoginDto model)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "sp_ValidateLogin";
            cmd.Parameters.AddWithValue("@UserNameOrEmail", model.UserNameOrEmail);
            using var reader = cmd.ExecuteReader();
            if(!reader.Read()) return new ResponseModel<AuthResultDto>{ Success = false, Message = "User not found" };
            var id = reader.GetGuid(reader.GetOrdinal("Id"));
            var passwordHash = reader.GetString(reader.GetOrdinal("PasswordHash"));
            var isActive = reader.GetBoolean(reader.GetOrdinal("IsActive"));
            if(!isActive) return new ResponseModel<AuthResultDto>{ Success = false, Message = "User is inactive" };
            if(!VerifyPassword(model.Password, passwordHash)) return new ResponseModel<AuthResultDto>{ Success = false, Message = "Invalid credentials" };

            // Generate JWT
            var token = GenerateJwtToken(id.ToString());
            var authResult = new AuthResultDto { Token = token, RefreshToken = "TODO-RefreshToken", ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetSection("Jwt").GetValue<int>("ExpireMinutes")) };
            return new ResponseModel<AuthResultDto>{ Success = true, Message = "Login successful", Data = authResult };
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        private string GenerateJwtToken(string userId)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection.GetValue<string>("Key")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            var token = new JwtSecurityToken(
                issuer: jwtSection.GetValue<string>("Issuer"),
                audience: jwtSection.GetValue<string>("Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSection.GetValue<int>("ExpireMinutes")),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
