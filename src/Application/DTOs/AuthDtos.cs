namespace TicketManagementSystem.Application.DTOs
{
    public class RegisterDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
    }

    public class LoginDto
    {
        public string UserNameOrEmail { get; set; }
        public string Password { get; set; }
    }

    public class AuthResultDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class CreateBookingDto
    {
        public int ScheduleId { get; set; }
        public Guid UserId { get; set; }
        public int SeatId { get; set; }
        public int FromStopId { get; set; }
        public int ToStopId { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
