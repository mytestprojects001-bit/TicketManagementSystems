using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Application.DTOs;
using TicketManagementSystem.Application.Services;

namespace TicketManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto model)
        {
            var res = await _bookingService.CreateBookingAsync(model);
            if(!res.Success) return BadRequest(res);
            return Ok(res);
        }
    }
}
