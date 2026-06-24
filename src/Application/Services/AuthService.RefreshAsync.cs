using System.Data.SqlClient;
using TicketManagementSystem.Shared;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace TicketManagementSystem.Application.Services
{
    public partial class AuthService : IAuthService
    {
        public async Task<ResponseModel<AuthResultDto>> RefreshAsync(string refreshToken)
        {
            // reuse existing synchronous RefreshToken method
            var result = RefreshToken(refreshToken);
            return await Task.FromResult(result);
        }
    }
}
