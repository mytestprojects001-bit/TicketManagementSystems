# TicketManagementSystems

This repository will contain a production-grade Bus Ticket Management System built with:

- .NET 6 Web API
- ADO.NET (no EF)
- SQL Server (full SQL script)
- ASP.NET Core Identity (custom ADO.NET stores)
- JWT Authentication + Refresh tokens
- Role-based + Permission-based Authorization
- Clean Architecture (Domain / Application / Infrastructure / API)
- Repository Pattern + UnitOfWork (ADO.NET)
- Serilog logging (file + DB)
- Swagger
- AutoMapper

Architecture and folder structure

- src/
  - TicketManagementSystem.sln
  - Api/                    -> ASP.NET Core Web API project (Startup, Controllers)
  - Application/            -> DTOs, Interfaces, Services, Validators (manual)
  - Domain/                 -> Entities
  - Infrastructure/         -> ADO.NET implementation, Repositories, DbConnectionFactory, Identity stores
  - Shared/                 -> Common models, Response model, Constants
- client/                   -> Angular 16 client (AdminLTE theme, components)
- scripts/
  - database_init.sql       -> Full SQL Server script (tables, indexes, stored procs, seed data)
- docs/
  - USER_MANUAL_TEMPLATE.md -> User manual template with screenshot placeholders
- postman/
  - TicketManagement.postman_collection.json -> Postman collection skeleton

Next steps (what I'll push next)

1. Add `scripts/database_init.sql` with complete SQL Server schema and stored procedures.
2. Add ADO.NET DbConnectionFactory and BaseRepository in Infrastructure.
3. Add Global Exception Middleware and Serilog configuration.
4. Add basic Identity ADO.NET store scaffolding and AuthService skeleton.
5. Add API controllers skeleton and central response model.

I have started the repo with this README. I will now add `scripts/database_init.sql` containing the full database DDL and stored procedures. After that I will implement the API skeleton and ADO.NET core infrastructure.

If you want me to push directly to the default branch `main`, confirm. Otherwise I will continue creating a branch `initial-skeleton` after this initial commit.