using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TicketManagementSystem.Application.Services;

namespace TicketManagementSystem.Api.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;
        public PermissionHandler(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                context.Fail();
                return;
            }

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                context.Fail();
                return;
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                context.Fail();
                return;
            }

            var has = await _permissionService.UserHasPermissionAsync(userId, requirement.Permission);
            if (has) context.Succeed(requirement); else context.Fail();
        }
    }
}
