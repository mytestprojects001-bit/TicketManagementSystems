using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Application.Services;

namespace TicketManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoutesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAll() => Ok(new { Message = "List routes (implement repository)" });
    }
}
