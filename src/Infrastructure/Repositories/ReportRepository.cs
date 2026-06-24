using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace TicketManagementSystem.Infrastructure.Repositories
{
    public interface IReportRepository
    {
        Task<IEnumerable<DailyBookingDto>> GetBookingsDailyAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<MonthlyRevenueDto>> GetRevenueMonthlyAsync(int year);
        Task<IEnumerable<RouteBookingDto>> GetBookingsByRouteAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<BusBookingDto>> GetBookingsByBusAsync(DateTime startDate, DateTime endDate);
        Task<OccupancyDto> GetOccupancyRateAsync(int scheduleId);
    }

    public class ReportRepository : IReportRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public ReportRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<DailyBookingDto>> GetBookingsDailyAsync(DateTime startDate, DateTime endDate)
        {
            var list = new List<DailyBookingDto>();
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "sp_GetBookingsDaily";
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new DailyBookingDto
                {
                    DayDate = rdr.GetDateTime(0),
                    TotalBookings = rdr.GetInt32(1),
                    TotalAmount = rdr.IsDBNull(2) ? 0 : rdr.GetDecimal(2)
                });
            }
            return list;
        }

        public async Task<IEnumerable<MonthlyRevenueDto>> GetRevenueMonthlyAsync(int year)
        {
            var list = new List<MonthlyRevenueDto>();
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "sp_GetRevenueMonthly";
            cmd.Parameters.AddWithValue("@Year", year);
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new MonthlyRevenueDto { Month = rdr.GetInt32(0), Revenue = rdr.IsDBNull(1) ? 0 : rdr.GetDecimal(1) });
            }
            return list;
        }

        public async Task<IEnumerable<RouteBookingDto>> GetBookingsByRouteAsync(DateTime startDate, DateTime endDate)
        {
            var list = new List<RouteBookingDto>();
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "sp_GetBookingsByRoute";
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new RouteBookingDto { RouteId = rdr.GetInt32(0), RouteName = rdr.GetString(1), TotalBookings = rdr.GetInt32(2), TotalAmount = rdr.IsDBNull(3) ? 0 : rdr.GetDecimal(3) });
            }
            return list;
        }

        public async Task<IEnumerable<BusBookingDto>> GetBookingsByBusAsync(DateTime startDate, DateTime endDate)
        {
            var list = new List<BusBookingDto>();
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "sp_GetBookingsByBus";
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);
            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new BusBookingDto { BusId = rdr.GetInt32(0), BusNo = rdr.GetString(1), TotalBookings = rdr.GetInt32(2), TotalAmount = rdr.IsDBNull(3) ? 0 : rdr.GetDecimal(3) });
            }
            return list;
        }

        public async Task<OccupancyDto> GetOccupancyRateAsync(int scheduleId)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "sp_GetOccupancyRate";
            cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
            using var rdr = await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
            {
                return new OccupancyDto { TotalSeats = rdr.GetInt32(0), BookedSeats = rdr.GetInt32(1), OccupancyPercent = rdr.GetDecimal(2) };
            }
            return null;
        }
    }

    // DTOs local to repository for reporting
    public class DailyBookingDto { public DateTime DayDate { get; set; } public int TotalBookings { get; set; } public decimal TotalAmount { get; set; } }
    public class MonthlyRevenueDto { public int Month { get; set; } public decimal Revenue { get; set; } }
    public class RouteBookingDto { public int RouteId { get; set; } public string RouteName { get; set; } public int TotalBookings { get; set; } public decimal TotalAmount { get; set; } }
    public class BusBookingDto { public int BusId { get; set; } public string BusNo { get; set; } public int TotalBookings { get; set; } public decimal TotalAmount { get; set; } }
    public class OccupancyDto { public int TotalSeats { get; set; } public int BookedSeats { get; set; } public decimal OccupancyPercent { get; set; } }
}
