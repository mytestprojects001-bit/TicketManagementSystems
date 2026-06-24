# PR #1 - Backend Core Implementation

Title: backend-core: Initial backend core implementation (ADO.NET stores, booking, reporting, auth, audit)

Branch: initial-skeleton -> main

This Pull Request contains the initial backend core implementation for TicketManagementSystems. It includes database scripts, ADO.NET infrastructure, Identity stores (custom), JWT auth with refresh tokens, booking/cancellation transactional procedures, reporting and export capabilities, audit logging, dynamic permission system and base API controllers.

---

Complete commit list included in this PR:

(Commit list is the commits present on the `initial-skeleton` branch - latest first)

1. feat: seed admin on startup, update README, add frontend scaffold (Angular minimal app), add driver/hostess and cancellation pieces
   - commit: eeea566846cefdd1b79bb6f54353115ec0d418e8
2. feat: add driver/hostess schema, cancellation SP, audit repository, cancellation service, permission service and authorization policy/provider/handler, register services
   - commit: 0d8b2b02ce21850b7ec0def0794677af5a6f0dcd
3. feat: add reporting stored procs, report repository/service/controllers, serilog mssql sink, unitofwork wiring, password hasher, client skeleton
   - commit: a1e76661184554480b821139957bd9496782034d
4. feat: add unitofwork, repositories, transactional booking SP, identity wiring, controllers scaffolding
   - commit: 5005bb08f1d19e452319d8cc7a70d3d578766819
5. feat: add identity stores, role/user services, refresh tokens support, role APIs
   - commit: 97577c7fbb1cc1fe54255c4dc00fa4294116b5e1
6. chore: add initial skeleton with SQL script, infra, API controllers, services, middleware, postman, docs
   - commit: 3dbe60d441b6a641710ca75e97349829192d8087
7. chore: add README with architecture and folder structure
   - commit: a90f135bcc2c300e448e0cf46728f0c1a364a374


---

Architecture summary (detailed)

- Solution layout (src/):
  - Api/ (ASP.NET Core Web API)
    - Program.cs
    - Controllers/ (Auth, Booking, Reports, Roles, Users, Cancellation, etc.)
    - Middleware/ (ExceptionMiddleware)
    - Authorization/ (PermissionRequirement, PermissionHandler, PermissionPolicyProvider)
    - ServiceRegistration.cs (DI registration)
    - SeedData.cs (development admin seeding)
  - Application/ (Services, DTOs)
    - Services: AuthService, BookingService, CancellationService, RoleService, UserService, ReportService, PermissionService
    - DTOs: Auth DTOs, Booking DTOs, Refresh DTO
  - Infrastructure/ (ADO.NET implementations)
    - DbConnectionFactory, SqlHelper, BaseRepository, GenericRepository
    - UnitOfWork
    - Repositories: BookingRepository, ReportRepository, AuditRepository
    - Identity: SqlUserStore, SqlRoleStore
  - Shared/ (common models): ResponseModel
  - scripts/ (database SQL scripts & migrations)
  - client/ (Angular 16 client skeleton)

Design principles
- Clean Architecture: separation of Domain/Application/Infrastructure/API concerns
- No ORM: raw ADO.NET (SqlConnection, SqlCommand, SqlDataReader) used throughout
- Stored Procedures used for transactional and complex operations (booking, availability, cancellation)
- UnitOfWork pattern for transactional operations where needed (BookingService)
- ASP.NET Core Identity used with custom ADO.NET stores (no EF)
- JWT + Refresh token support; refresh tokens persisted in DB
- Serilog for file and MSSQL sink (writes to SerilogLogs table)
- Centralized response model and global exception middleware
- Permission-based authorization using dynamic policies backed by RolePermissions and UserPermissions


Database migration order (run in this sequence)
1. scripts/database_init.sql (creates base schema: AspNetUsers, AspNetRoles, Buses, Routes, Stops, Schedules, Seats, Bookings, Payments, BookingSegments, AuditLogs, SerilogLogs, seed roles and sample data)
2. scripts/migrations/add_refresh_tokens.sql (creates RefreshTokens table and SPs)
3. scripts/migrations/booking_sp_v2.sql (enhanced availability and transactional booking SP using sp_getapplock)
4. scripts/migrations/reports_sp.sql (reporting stored procedures)
5. scripts/migrations/driver_hostess.sql (drivers, hostesses, assignments tables and SPs)
6. scripts/migrations/cancel_booking_v2.sql (cancellation stored procedure with refund calculation and audit logging)


API endpoint documentation (implemented / scaffolded)
- Authentication
  - POST /api/auth/register
    - Body: RegisterDto { UserName, Email, Password, FullName }
    - Returns: central ResponseModel with Data = { UserId } or Auth tokens in RegisterAndLogin flow
  - POST /api/auth/login
    - Body: LoginDto { UserNameOrEmail, Password }
    - Returns: central ResponseModel with Data = AuthResultDto { Token, RefreshToken, ExpiresAt }
  - POST /api/auth/refresh
    - Body: RefreshRequestDto { RefreshToken }
    - Returns: new JWT + refresh token

- Booking
  - POST /api/booking/create
    - Body: CreateBookingDto { ScheduleId, UserId, SeatId, FromStopId, ToStopId, TotalAmount }
    - Returns: booking id in central ResponseModel

- Cancellation
  - POST /api/cancellation/{bookingId}
    - Authorization required
    - Cancels booking and applies refund policy (48-hour rule, 30% deduction if within 48 hours)

- Reports
  - GET /api/reports/bookings/daily?start={date}&end={date}
    - Returns daily bookings aggregate
  - GET /api/reports/bookings/daily/export?start={date}&end={date}&format=excel|pdf
    - Returns streamed Excel or PDF

- Role & User Management
  - POST /api/users/{userId}/roles/{roleId}
  - DELETE /api/users/{userId}/roles/{roleId}
  - POST /api/roles/{roleId}/permissions
  - DELETE /api/roles/{roleId}/permissions

- Scaffolds (to be implemented)
  - GET/POST /api/buses
  - GET /api/routes
  - GET /api/schedules
  - POST /api/payments/mock


Run instructions (backend)
1. Ensure .NET 6 SDK installed.
2. Prepare SQL Server and create a database, e.g., TicketManagement.
3. Apply SQL scripts in the order specified in "Database migration order".
4. Edit src/Api/appsettings.json:
   - Set ConnectionStrings:DefaultConnection to your DB connection string
   - Replace Jwt:Key with a secure random secret
5. From command line, run:
   - cd src/Api
   - dotnet restore
   - dotnet build
   - dotnet run
6. On startup the API will seed a development admin user (admin@ticketmanagement.local / Admin@123) with role SuperAdmin.
7. Swagger will be available at https://localhost:{port}/swagger (in Development)

Run instructions (frontend - Angular)
1. Ensure Node.js and Angular CLI installed.
2. cd client
3. npm install
4. ng serve --open


Seeded admin account
- Email: admin@ticketmanagement.local
- Password: Admin@123
- Role: SuperAdmin

Notes: This account is for development/test only. Change password and disable seeding in production.


Known limitations and pending tasks
- Frontend is a minimal scaffold; AdminLTE integration, dashboard, booking UI, seat selector, reports UI, and role management UI are pending.
- Not all CRUD endpoints for Buses/Routes/Schedules/Seats/Drivers/Hostesses are implemented; controllers contain scaffolds and will be completed in subsequent commits.
- Tests: unit and integration tests are missing (will be added in next milestone).
- Payment gateway: mock payments only; integrate real payment processor as needed.
- Email/SMS notifications: not implemented.
- Performance: transactional booking uses sp_getapplock and serializable isolation; monitor under load and tune locks/timeouts.


Testing checklist for reviewers
- DB: Verify scripts run in specified order and create expected tables and stored procedures.
- Auth:
  - Register a user via POST /api/auth/register
  - Login via POST /api/auth/login and verify JWT returned
  - Use JWT to access protected endpoints
- Admin user:
  - Use seeded admin credentials to login and verify role membership
- Booking concurrency (manual test):
  - Attempt concurrent POST /api/booking/create requests for same schedule/seat and overlapping segments; ensure only one booking succeeds
- Cancellation:
  - Create a booking and call POST /api/cancellation/{bookingId}; verify Booking status set to 'Cancelled' and AuditLogs updated
- Reports:
  - Call GET /api/reports/bookings/daily and export endpoints; verify files download and content
- Permissions:
  - Assign permission to a role and verify PermissionHandler enforces access


Files added/modified (high level)
- src/Api/*: Program.cs, Controllers, Middleware, Authorization
- src/Application/*: Services, DTOs, ReportService
- src/Infrastructure/*: DbConnectionFactory, Repositories, UnitOfWork, Identity stores
- scripts/*: SQL scripts and migrations
- client/*: Angular scaffold


---

Please review this PR. After you approve, we will not merge until frontend milestones and tests are complete. I will continue development on `initial-skeleton` and push the next commits implementing the frontend (AdminLTE, auth flow, booking UI) and remaining backend endpoints (CRUD and refresh endpoint completed).
