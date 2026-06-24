using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Application.Services;

namespace TicketManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll() => Ok(new { Message = "List schedules (implement repository)" });
    }
}
