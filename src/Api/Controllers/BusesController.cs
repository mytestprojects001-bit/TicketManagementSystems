using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Application.Services;
using TicketManagementSystem.Shared;

namespace TicketManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusesController : ControllerBase
    {
        // For brevity using UnitOfWork + raw queries via repo could be used; here we provide example endpoints
        [HttpGet]
        public IActionResult GetAll() => Ok(new ResponseModel<object>{ Success = true, Data = "Endpoint to list buses (implement repository)" });

        [HttpPost]
        public IActionResult Create([FromBody] object model) => Ok(new ResponseModel<object>{ Success = true, Message = "Create bus endpoint (implement)" });
    }
}
