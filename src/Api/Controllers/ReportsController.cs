using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Application.Services;
using System;

namespace TicketManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("bookings/daily")]
        public async Task<IActionResult> GetDaily([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var res = await _reportService.GetBookingsDailyAsync(start, end);
            if(!res.Success) return BadRequest(res);
            return Ok(res);
        }

        [HttpGet("bookings/daily/export")]
        public async Task<IActionResult> ExportDaily([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] string format = "excel")
        {
            if(format.ToLower() == "excel")
            {
                var ms = await _reportService.ExportBookingsDailyExcelAsync(start, end);
                return File(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "daily_bookings.xlsx");
            }
            else
            {
                var ms = await _reportService.ExportBookingsDailyPdfAsync(start, end);
                return File(ms, "application/pdf", "daily_bookings.pdf");
            }
        }
    }
}
