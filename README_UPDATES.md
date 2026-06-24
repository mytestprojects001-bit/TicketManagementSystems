# TicketManagementSystems - README updates

## Development admin account (seeded)

A development SuperAdmin account will be automatically created on application startup if it does not already exist.

Credentials:
- Email: admin@ticketmanagement.local
- Password: Admin@123
- Role: SuperAdmin

Please change these credentials in production.

## How seeding works
On startup the API calls SeedData.InitializeAsync which uses ASP.NET Identity UserManager and RoleManager to ensure the SuperAdmin role exists and create an admin user with the above credentials if missing.

## Running the backend
1. Create SQL Server database (e.g., TicketManagement)
2. Run scripts in this order:
   - scripts/database_init.sql
   - scripts/migrations/add_refresh_tokens.sql
   - scripts/migrations/booking_sp_v2.sql
   - scripts/migrations/reports_sp.sql
   - scripts/migrations/driver_hostess.sql
   - scripts/migrations/cancel_booking_v2.sql
3. Update `src/Api/appsettings.json` with your connection string and JWT secret
4. Run API: `cd src/Api && dotnet run`

## Running the frontend (Angular)
1. cd client
2. npm install
3. ng serve --open

Note: The Angular client implementation will be populated in the repository. For now the client contains a minimal scaffold.
