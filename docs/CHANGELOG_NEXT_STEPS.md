# Run instructions and notes for the next development batch

This commit adds:
- New stored procedures for robust availability checks and transactional booking (sp_CheckSeatAvailability_V2, sp_CreateBooking_V2)
- UnitOfWork implementation (UnitOfWork.cs)
- GenericRepository skeleton
- BookingRepository using transactional stored procedure
- BookingService updated to use UnitOfWork + BookingRepository transactional flow
- Identity stores and role services registered via ServiceRegistration extension
- Several API controllers scaffolds for buses, routes, schedules and payments

Next steps you can run locally:
1. Apply new SQL migrations in order:
   - scripts/database_init.sql
   - scripts/migrations/add_refresh_tokens.sql
   - scripts/migrations/booking_sp_v2.sql

2. Update src/Api/appsettings.json DefaultConnection and Jwt:Key
3. Build and run the API (dotnet run from src/Api)

Notes:
- The sp_CreateBooking_V2 uses sp_getapplock to serialize booking attempts for the same schedule-seat to avoid race conditions. This is safe but tune timeout as needed.
- UnitOfWork uses Serializable isolation for safety; monitor performance and adjust.
- Password hashing should be replaced with ASP.NET Identity PasswordHasher for production; I will switch in the next commit if you confirm.
- PDF/Excel and reporting endpoints will be added next along with Angular client.
