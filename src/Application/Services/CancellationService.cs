using System;
using TicketManagementSystem.Shared;
using TicketManagementSystem.Infrastructure.Repositories;

namespace TicketManagementSystem.Application.Services
{
    public interface ICancellationService
    {
        Task<ResponseModel<object>> CancelBookingAsync(Guid bookingId, Guid cancelledBy);
    }

    public class CancellationService : ICancellationService
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IAuditRepository _auditRepository;
        public CancellationService(IDbConnectionFactory dbConnectionFactory, IAuditRepository auditRepository)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _auditRepository = auditRepository;
        }

        public async Task<ResponseModel<object>> CancelBookingAsync(Guid bookingId, Guid cancelledBy)
        {
            try
            {
                using var conn = (SqlConnection)_dbConnectionFactory.CreateConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandText = "sp_CancelBooking_V2";
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                cmd.Parameters.AddWithValue("@CancelledBy", cancelledBy);
                await cmd.ExecuteNonQueryAsync();

                // Create audit entry as well via repository (stored proc also logs, but keep consistent)
                await _auditRepository.LogAsync("Booking", "Cancel", cancelledBy.ToString(), $