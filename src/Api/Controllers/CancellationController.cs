using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Application.Services;
using TicketManagementSystem.Shared;
using System;

namespace TicketManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CancellationController : ControllerBase
    {
        private readonly ICancellationService _cancellationService;
        public CancellationController(ICancellationService cancellationService)
        {
            _cancellationService = cancellationService;
        }

        [HttpPost("{bookingId}")]
        public async Task<IActionResult> Cancel(Guid bookingId)
        {
            // In real scenario, get user id from token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if(string.IsNullOrEmpty(userIdClaim)) return Unauthorized(new ResponseModel<object>{ Success = false, Message = "Unauthorized" });
            var userId = Guid.Parse(userIdClaim);
            var res = await _cancellationService.CancelBookingAsync(bookingId, userId);
            if(!res.Success) return BadRequest(res);
            return Ok(res);
        }
    }
}
