using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Application.Services;
using System;

namespace TicketManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("{userId}/roles/{roleId}")]
        public async Task<IActionResult> AssignRole(Guid userId, Guid roleId)
        {
            var res = await _userService.AssignRoleToUserAsync(userId, roleId);
            if(!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpDelete("{userId}/roles/{roleId}")]
        public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId)
        {
            var res = await _userService.RemoveRoleFromUserAsync(userId, roleId);
            if(!res.Success) return BadRequest(res);
            return Ok(res);
        }
    }
}
