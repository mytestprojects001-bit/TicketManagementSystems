using TicketManagementSystem.Shared;
using TicketManagementSystem.Application.DTOs;
using TicketManagementSystem.Infrastructure.Repositories;
using System;

namespace TicketManagementSystem.Application.Services
{
    public partial class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly UnitOfWork _unitOfWork;
        public BookingService(IBookingRepository bookingRepository, UnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel<object>> CreateBookingAsync(CreateBookingDto model)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var newBookingId = await _bookingRepository.CreateBookingTransactionalAsync(model.ScheduleId, model.UserId, model.TotalAmount, model.SeatId, model.FromStopId, model.ToStopId);
                // commit
                _unitOfWork.Commit();
                return new ResponseModel<object>{ Success = true, Message = "Booking created", Data = new { BookingId = newBookingId } };
            }
            catch(Exception ex)
            {
                _unitOfWork.Rollback();
                return new ResponseModel<object>{ Success = false, Message = ex.Message };
            }
            finally
            {
                _unitOfWork.Dispose();
            }
        }
    }
}
