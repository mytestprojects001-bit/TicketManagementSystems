using System;
using System.Data.SqlClient;
using System.Data;
using TicketManagementSystem.Shared;
using TicketManagementSystem.Application.DTOs;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace TicketManagementSystem.Application.Services
{
    public partial class AuthService : IAuthService
    {
        // Add refresh token persistence
        private void SaveRefreshToken(string token, Guid userId, DateTime expiresAt)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "INSERT INTO RefreshTokens(Token, UserId, ExpiresAt) VALUES(@Token, @UserId, @ExpiresAt)";
            cmd.Parameters.AddWithValue("@Token", token);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);
            cmd.ExecuteNonQuery();
        }

        public ResponseModel<AuthResultDto> RefreshToken(string refreshToken)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_GetRefreshToken";
            cmd.Parameters.AddWithValue("@Token", refreshToken);
            using var reader = cmd.ExecuteReader();
            if(!reader.Read()) return new ResponseModel<AuthResultDto>{ Success = false, Message = "Invalid refresh token" };
            var revoked = reader.GetBoolean(reader.GetOrdinal("Revoked"));
            if(revoked) return new ResponseModel<AuthResultDto>{ Success = false, Message = "Refresh token revoked" };
            var expiresAt = reader.GetDateTime(reader.GetOrdinal("ExpiresAt"));
            if(expiresAt < DateTime.UtcNow) return new ResponseModel<AuthResultDto>{ Success = false, Message = "Refresh token expired" };
            var userId = reader.GetGuid(reader.GetOrdinal("UserId")).ToString();

            var newJwt = GenerateJwtToken(userId);
            var newRefresh = TokenGenerator.GenerateRefreshToken();
            // persist new refresh token
            SaveRefreshToken(newRefresh, Guid.Parse(userId), DateTime.UtcNow.AddDays(_configuration.GetSection("Jwt").GetValue<int>("RefreshTokenExpireDays")));

            var auth = new AuthResultDto { Token = newJwt, RefreshToken = newRefresh, ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetSection("Jwt").GetValue<int>("ExpireMinutes")) };
            return new ResponseModel<AuthResultDto>{ Success = true, Message = "Token refreshed", Data = auth };
        }
    }
}
