using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TicketManagementSystem.Infrastructure.Repositories;
using TicketManagementSystem.Shared;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TicketManagementSystem.Application.Services
{
    public interface IReportService
    {
        Task<ResponseModel<IEnumerable<object>>> GetBookingsDailyAsync(DateTime start, DateTime end);
        Task<MemoryStream> ExportBookingsDailyExcelAsync(DateTime start, DateTime end);
        Task<MemoryStream> ExportBookingsDailyPdfAsync(DateTime start, DateTime end);
    }

    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<ResponseModel<IEnumerable<object>>> GetBookingsDailyAsync(DateTime start, DateTime end)
        {
            var data = await _reportRepository.GetBookingsDailyAsync(start, end);
            var res = new List<object>();
            foreach(var d in data) res.Add(new { day = d.DayDate, totalBookings = d.TotalBookings, totalAmount = d.TotalAmount });
            return new ResponseModel<IEnumerable<object>>{ Success = true, Data = res };
        }

        public async Task<MemoryStream> ExportBookingsDailyExcelAsync(DateTime start, DateTime end)
        {
            var data = await _reportRepository.GetBookingsDailyAsync(start, end);
            var ms = new MemoryStream();
            using(var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("DailyBookings");
                ws.Cell(1,1).Value = "Date";
                ws.Cell(1,2).Value = "TotalBookings";
                ws.Cell(1,3).Value = "TotalAmount";
                int r = 2;
                foreach(var d in data)
                {
                    ws.Cell(r,1).Value = d.DayDate;
                    ws.Cell(r,2).Value = d.TotalBookings;
                    ws.Cell(r,3).Value = d.TotalAmount;
                    r++;
                }
                wb.SaveAs(ms);
                ms.Position = 0;
            }
            return ms;
        }

        public async Task<MemoryStream> ExportBookingsDailyPdfAsync(DateTime start, DateTime end)
        {
            var data = await _reportRepository.GetBookingsDailyAsync(start, end);
            var ms = new MemoryStream();
            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text("Daily Bookings Report").SemiBold().FontSize(20).AlignCenter();
                    page.Content().Column(col =>
                    {
                        col.Spacing(5);
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            // header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Date");
                                header.Cell().Element(CellStyle).Text("TotalBookings");
                                header.Cell().Element(CellStyle).Text("TotalAmount");
                            });

                            foreach(var d in data)
                            {
                                table.Cell().Element(CellStyle).Text(d.DayDate.ToShortDateString());
                                table.Cell().Element(CellStyle).Text(d.TotalBookings.ToString());
                                table.Cell().Element(CellStyle).Text(d.TotalAmount.ToString("F2"));
                            }

                            IContainer CellStyle(IContainer c) => c.Padding(5).Border(1).BorderColor(Colors.Grey.Lighten2);
                        });
                    });
                    page.Footer().AlignCenter().Text(txt => txt.CurrentPageNumber());
                });
            });
            doc.GeneratePdf(ms);
            ms.Position = 0;
            return ms;
        }
    }
}
