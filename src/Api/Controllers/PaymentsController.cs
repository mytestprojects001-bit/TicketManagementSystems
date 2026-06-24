using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Application.Services;

namespace TicketManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        [HttpPost("mock")]
        public IActionResult MockPayment([FromBody] object model) => Ok(new { Message = "Mock payment processed" });
    }
}
