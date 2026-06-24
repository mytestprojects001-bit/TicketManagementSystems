using TicketManagementSystem.Shared;
using TicketManagementSystem.Application.DTOs;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace TicketManagementSystem.Application.Services
{
    public interface IAuthService
    {
        Task<ResponseModel<object>> RegisterAsync(RegisterDto model);
        Task<ResponseModel<AuthResultDto>> LoginAsync(LoginDto model);
        Task<ResponseModel<AuthResultDto>> RefreshAsync(string refreshToken);
        Task<ResponseModel<AuthResultDto>> RegisterAndLoginAsync(RegisterDto model);
    }

    // Partial AuthService implementations are in other files
}
