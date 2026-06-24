using System;
using System.Security.Cryptography;

namespace TicketManagementSystem.Application.Services
{
    public static class TokenGenerator
    {
        public static string GenerateRefreshToken(int size = 64)
        {
            var randomNumber = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
