using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Application.Services;
using TicketManagementSystem.Shared;

namespace TicketManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost("{roleId}/permissions")]
        public async Task<IActionResult> AssignPermission(Guid roleId, [FromBody] string permission)
        {
            var res = await _roleService.AssignPermissionToRoleAsync(roleId, permission);
            if(!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpDelete("{roleId}/permissions")]
        public async Task<IActionResult> RemovePermission(Guid roleId, [FromBody] string permission)
        {
            var res = await _roleService.RemovePermissionFromRoleAsync(roleId, permission);
            if(!res.Success) return BadRequest(res);
            return Ok(res);
        }
    }
}
