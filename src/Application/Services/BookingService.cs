using TicketManagementSystem.Shared;
using TicketManagementSystem.Application.DTOs;
using System.Data.SqlClient;

namespace TicketManagementSystem.Application.Services
{
    public interface IBookingService
    {
        Task<ResponseModel<object>> CreateBookingAsync(CreateBookingDto model);
    }

    public class BookingService : IBookingService
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public BookingService(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<ResponseModel<object>> CreateBookingAsync(CreateBookingDto model)
        {
            using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
            // Check availability
            using(var checkCmd = conn.CreateCommand())
            {
                checkCmd.CommandType = System.Data.CommandType.StoredProcedure;
                checkCmd.CommandText = "sp_CheckSeatAvailability";
                checkCmd.Parameters.AddWithValue("@ScheduleId", model.ScheduleId);
                checkCmd.Parameters.AddWithValue("@SeatId", model.SeatId);
                checkCmd.Parameters.AddWithValue("@FromStopId", model.FromStopId);
                checkCmd.Parameters.AddWithValue("@ToStopId", model.ToStopId);
                using var reader = checkCmd.ExecuteReader();
                if(reader.Read())
                {
                    var available = reader.GetInt32(reader.GetOrdinal("IsAvailable"));
                    if(available == 0) return new ResponseModel<object>{ Success = false, Message = "Seat not available for selected segment" };
                }
            }

            // Create booking transactionally
            using(var cmd = conn.CreateCommand())
            {
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "sp_CreateBooking";
                cmd.Parameters.AddWithValue("@ScheduleId", model.ScheduleId);
                cmd.Parameters.AddWithValue("@UserId", model.UserId);
                cmd.Parameters.AddWithValue("@TotalAmount", model.TotalAmount);
                cmd.Parameters.AddWithValue("@Status", "Pending");
                cmd.Parameters.AddWithValue("@SeatId", model.SeatId);
                cmd.Parameters.AddWithValue("@FromStopId", model.FromStopId);
                cmd.Parameters.AddWithValue("@ToStopId", model.ToStopId);
                var outParam = new SqlParameter
                {
                    ParameterName = "@NewBookingId",
                    SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
                    Direction = System.Data.ParameterDirection.Output
                };
                cmd.Parameters.Add(outParam);
                cmd.ExecuteNonQuery();
                var newBookingId = (Guid)outParam.Value;
                return new ResponseModel<object>{ Success = true, Message = "Booking created", Data = new { BookingId = newBookingId } };
            }
        }
    }
}
