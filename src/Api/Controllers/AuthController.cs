using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Application.DTOs;
using TicketManagementSystem.Application.Services;

namespace TicketManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var result = await _authService.RegisterAsync(model);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var result = await _authService.LoginAsync(model);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto model)
        {
            if (model == null || string.IsNullOrEmpty(model.RefreshToken))
                return BadRequest(new { success = false, message = "Refresh token is required." });

            var result = await _authService.RefreshAsync(model.RefreshToken);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
