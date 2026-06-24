using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TicketManagementSystem.Shared;
using System.Data;

namespace TicketManagementSystem.Infrastructure.Repositories
{
    public interface IBookingRepository
    {
        Task<Guid> CreateBookingTransactionalAsync(int scheduleId, Guid userId, decimal totalAmount, int seatId, int fromStopId, int toStopId);
        Task CancelBookingAsync(Guid bookingId);
    }

    public class BookingRepository : IBookingRepository
    {
        private readonly UnitOfWork _unitOfWork;
        public BookingRepository(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> CreateBookingTransactionalAsync(int scheduleId, Guid userId, decimal totalAmount, int seatId, int fromStopId, int toStopId)
        {
            using var cmd = _unitOfWork.Connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_CreateBooking_V2";
            if (_unitOfWork.Transaction != null) cmd.Transaction = _unitOfWork.Transaction;

            cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
            cmd.Parameters.AddWithValue("@Status", "Pending");
            cmd.Parameters.AddWithValue("@SeatId", seatId);
            cmd.Parameters.AddWithValue("@FromStopId", fromStopId);
            cmd.Parameters.AddWithValue("@ToStopId", toStopId);
            var outParam = new SqlParameter
            {
                ParameterName = "@NewBookingId",
                SqlDbType = SqlDbType.UniqueIdentifier,
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outParam);
            await cmd.ExecuteNonQueryAsync();
            var newId = (Guid)outParam.Value;
            return newId;
        }

        public async Task CancelBookingAsync(Guid bookingId)
        {
            using var cmd = _unitOfWork.Connection.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_CancelBooking";
            if (_unitOfWork.Transaction != null) cmd.Transaction = _unitOfWork.Transaction;
            cmd.Parameters.AddWithValue("@BookingId", bookingId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
