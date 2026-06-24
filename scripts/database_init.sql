-- Database initialization script for TicketManagementSystems
-- Includes Identity-like tables, permissions, buses, routes, stops, schedules, seats, bookings, payments, audit and serilog logs

SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO

-- Users and Roles (simplified ASP.NET Identity tables adapted for ADO.NET stores)
CREATE TABLE AspNetRoles (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(256) NOT NULL,
    NormalizedName NVARCHAR(256) NOT NULL,
    ConcurrencyStamp NVARCHAR(50) NULL
);

CREATE UNIQUE INDEX IX_Roles_NormalizedName ON AspNetRoles(NormalizedName);

CREATE TABLE AspNetUsers (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserName NVARCHAR(256) NULL,
    NormalizedUserName NVARCHAR(256) NULL,
    Email NVARCHAR(256) NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    PasswordHash NVARCHAR(MAX) NULL,
    SecurityStamp NVARCHAR(50) NULL,
    ConcurrencyStamp NVARCHAR(50) NULL,
    PhoneNumber NVARCHAR(50) NULL,
    PhoneNumberConfirmed BIT NOT NULL DEFAULT 0,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    LockoutEnd DATETIMEOFFSET NULL,
    LockoutEnabled BIT NOT NULL DEFAULT 1,
    AccessFailedCount INT NOT NULL DEFAULT 0,
    FullName NVARCHAR(200) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE UNIQUE INDEX IX_Users_NormalizedUserName ON AspNetUsers(NormalizedUserName);
CREATE INDEX IX_Users_NormalizedEmail ON AspNetUsers(NormalizedEmail);

CREATE TABLE AspNetUserRoles (
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id)
);

CREATE TABLE UserPermissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Permission NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_UserPermissions_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

CREATE TABLE RolePermissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    Permission NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id)
);

-- Buses
CREATE TABLE Buses (
    BusId INT IDENTITY(1,1) PRIMARY KEY,
    BusNo NVARCHAR(50) NOT NULL,
    Capacity INT NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    FullCapacity INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

-- Routes and Stops
CREATE TABLE Routes (
    RouteId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Source NVARCHAR(200) NOT NULL,
    Destination NVARCHAR(200) NOT NULL,
    DistanceKm DECIMAL(10,2) NOT NULL,
    DurationMinutes INT NOT NULL
);

CREATE TABLE Stops (
    StopId INT IDENTITY(1,1) PRIMARY KEY,
    RouteId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    SequenceNo INT NOT NULL,
    ArrivalOffsetMinutes INT NOT NULL,
    CONSTRAINT FK_Stops_Route FOREIGN KEY (RouteId) REFERENCES Routes(RouteId)
);

-- RouteSegments: segments between consecutive stops
CREATE TABLE RouteSegments (
    SegmentId INT IDENTITY(1,1) PRIMARY KEY,
    RouteId INT NOT NULL,
    FromStopId INT NOT NULL,
    ToStopId INT NOT NULL,
    DistanceKm DECIMAL(10,2) NOT NULL,
    DurationMinutes INT NOT NULL,
    CONSTRAINT FK_Segments_Route FOREIGN KEY (RouteId) REFERENCES Routes(RouteId),
    CONSTRAINT FK_Segments_FromStop FOREIGN KEY (FromStopId) REFERENCES Stops(StopId),
    CONSTRAINT FK_Segments_ToStop FOREIGN KEY (ToStopId) REFERENCES Stops(StopId)
);

-- Schedules
CREATE TABLE Schedules (
    ScheduleId INT IDENTITY(1,1) PRIMARY KEY,
    RouteId INT NOT NULL,
    BusId INT NOT NULL,
    DepartureDateTime DATETIME2 NOT NULL,
    ArrivalDateTime DATETIME2 NOT NULL,
    CONSTRAINT FK_Schedules_Route FOREIGN KEY (RouteId) REFERENCES Routes(RouteId),
    CONSTRAINT FK_Schedules_Bus FOREIGN KEY (BusId) REFERENCES Buses(BusId)
);

-- Seats
CREATE TABLE Seats (
    SeatId INT IDENTITY(1,1) PRIMARY KEY,
    BusId INT NOT NULL,
    SeatNumber NVARCHAR(20) NOT NULL,
    IsWindow BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Seats_Bus FOREIGN KEY (BusId) REFERENCES Buses(BusId)
);

-- Bookings
CREATE TABLE Bookings (
    BookingId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ScheduleId INT NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    BookingDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CancellationRequested BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Bookings_Schedule FOREIGN KEY (ScheduleId) REFERENCES Schedules(ScheduleId),
    CONSTRAINT FK_Bookings_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);

-- BookingSegments: which segment(s) a booking covers and which seat
CREATE TABLE BookingSegments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BookingId UNIQUEIDENTIFIER NOT NULL,
    SeatId INT NOT NULL,
    FromStopId INT NOT NULL,
    ToStopId INT NOT NULL,
    CONSTRAINT FK_BookingSegments_Booking FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId),
    CONSTRAINT FK_BookingSegments_Seat FOREIGN KEY (SeatId) REFERENCES Seats(SeatId),
    CONSTRAINT FK_BookingSegments_FromStop FOREIGN KEY (FromStopId) REFERENCES Stops(StopId),
    CONSTRAINT FK_BookingSegments_ToStop FOREIGN KEY (ToStopId) REFERENCES Stops(StopId)
);

-- Payments
CREATE TABLE Payments (
    PaymentId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    BookingId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    TransactionRef NVARCHAR(200) NULL,
    PaidAt DATETIME2 NULL,
    CONSTRAINT FK_Payments_Booking FOREIGN KEY (BookingId) REFERENCES Bookings(BookingId)
);

-- Audit logs
CREATE TABLE AuditLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Entity NVARCHAR(200) NOT NULL,
    Action NVARCHAR(100) NOT NULL,
    PerformedBy NVARCHAR(200) NULL,
    PerformedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Details NVARCHAR(MAX) NULL
);

-- Serilog logs table
CREATE TABLE SerilogLogs (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Message NVARCHAR(MAX) NULL,
    MessageTemplate NVARCHAR(MAX) NULL,
    Level NVARCHAR(128) NULL,
    TimeStamp DATETIME2 NOT NULL,
    Exception NVARCHAR(MAX) NULL,
    Properties NVARCHAR(MAX) NULL
);

-- Indexes for performance
CREATE INDEX IX_Schedules_RouteId_Departure ON Schedules(RouteId, DepartureDateTime);
CREATE INDEX IX_Bookings_ScheduleId ON Bookings(ScheduleId);
CREATE INDEX IX_BookingSegments_SeatId ON BookingSegments(SeatId);

-- Stored Procedures
GO

-- User registration (simple; password hashing should be done in app code)
CREATE PROCEDURE sp_RegisterUser
    @UserName NVARCHAR(256),
    @NormalizedUserName NVARCHAR(256),
    @Email NVARCHAR(256),
    @NormalizedEmail NVARCHAR(256),
    @PasswordHash NVARCHAR(MAX),
    @FullName NVARCHAR(200),
    @NewUserId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO AspNetUsers(Id, UserName, NormalizedUserName, Email, NormalizedEmail, PasswordHash, FullName)
    VALUES(@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @PasswordHash, @FullName);
    SET @NewUserId = @Id;
END
GO

-- Login validation: finds user by username/email and returns id and password hash
CREATE PROCEDURE sp_ValidateLogin
    @UserNameOrEmail NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1 Id, UserName, NormalizedUserName, Email, NormalizedEmail, PasswordHash, IsActive
    FROM AspNetUsers
    WHERE NormalizedUserName = UPPER(@UserNameOrEmail) OR NormalizedEmail = UPPER(@UserNameOrEmail);
END
GO

-- Check seat availability for a schedule and segment (prevent overlap)
CREATE PROCEDURE sp_CheckSeatAvailability
    @ScheduleId INT,
    @SeatId INT,
    @FromStopId INT,
    @ToStopId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- If there exists any booking segment on same schedule and seat where segments overlap, return 0
    -- Overlap logic: NOT (existing.ToStopSeq <= new.FromStopSeq OR existing.FromStopSeq >= new.ToStopSeq)

    DECLARE @FromSeq INT = (SELECT SequenceNo FROM Stops WHERE StopId = @FromStopId);
    DECLARE @ToSeq INT = (SELECT SequenceNo FROM Stops WHERE StopId = @ToStopId);

    IF @FromSeq IS NULL OR @ToSeq IS NULL
    BEGIN
        RAISERROR('Invalid stop ids',16,1);
        RETURN;
    END

    IF @FromSeq >= @ToSeq
    BEGIN
        RAISERROR('FromStop must be before ToStop',16,1);
        RETURN;
    END

    -- Check for overlapping booking segments for same schedule and seat
    IF EXISTS(
        SELECT 1 FROM BookingSegments bs
        INNER JOIN Bookings b ON bs.BookingId = b.BookingId
        INNER JOIN Seats s ON bs.SeatId = s.SeatId
        WHERE b.ScheduleId = @ScheduleId AND bs.SeatId = @SeatId
        AND NOT ( ( (SELECT SequenceNo FROM Stops WHERE StopId = bs.ToStopId) <= @FromSeq )
                 OR ( (SELECT SequenceNo FROM Stops WHERE StopId = bs.FromStopId) >= @ToSeq ) )
    )
    BEGIN
        SELECT 0 AS IsAvailable;
    END
    ELSE
    BEGIN
        SELECT 1 AS IsAvailable;
    END
END
GO

-- Create booking transactional: inserts booking, booking segments, payment stub
CREATE PROCEDURE sp_CreateBooking
    @ScheduleId INT,
    @UserId UNIQUEIDENTIFIER,
    @TotalAmount DECIMAL(18,2),
    @Status NVARCHAR(50),
    @SeatId INT,
    @FromStopId INT,
    @ToStopId INT,
    @NewBookingId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @BookingId UNIQUEIDENTIFIER = NEWID();
        INSERT INTO Bookings(BookingId, ScheduleId, UserId, TotalAmount, Status)
        VALUES(@BookingId, @ScheduleId, @UserId, @TotalAmount, @Status);

        INSERT INTO BookingSegments(BookingId, SeatId, FromStopId, ToStopId)
        VALUES(@BookingId, @SeatId, @FromStopId, @ToStopId);

        -- Insert a payment placeholder (mock)
        INSERT INTO Payments(BookingId, Amount, Status)
        VALUES(@BookingId, @TotalAmount, 'Pending');

        SET @NewBookingId = @BookingId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrMsg,16,1);
    END CATCH
END
GO

-- Cancel booking (basic); refund logic will be handled in app logic but stub here
CREATE PROCEDURE sp_CancelBooking
    @BookingId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Bookings SET Status = 'Cancelled' WHERE BookingId = @BookingId;
    UPDATE Payments SET Status = 'Failed' WHERE BookingId = @BookingId AND Status = 'Pending';
END
GO

-- Seed roles and admin user
DECLARE @adminRoleId UNIQUEIDENTIFIER = NEWID();
INSERT INTO AspNetRoles(Id, Name, NormalizedName, ConcurrencyStamp) VALUES(@adminRoleId, 'Admin', 'ADMIN', NEWID());
DECLARE @staffRoleId UNIQUEIDENTIFIER = NEWID();
INSERT INTO AspNetRoles(Id, Name, NormalizedName, ConcurrencyStamp) VALUES(@staffRoleId, 'Staff', 'STAFF', NEWID());
DECLARE @customerRoleId UNIQUEIDENTIFIER = NEWID();
INSERT INTO AspNetRoles(Id, Name, NormalizedName, ConcurrencyStamp) VALUES(@customerRoleId, 'Customer', 'CUSTOMER', NEWID());

-- Create sample route, stops, bus, schedule, seats
INSERT INTO Routes(Name, Source, Destination, DistanceKm, DurationMinutes) VALUES('HYD-LHR', 'HYD', 'LHR', 1500, 1200);
DECLARE @routeId INT = SCOPE_IDENTITY();

INSERT INTO Stops(RouteId, Name, SequenceNo, ArrivalOffsetMinutes) VALUES(@routeId, 'HYD', 1, 0);
INSERT INTO Stops(RouteId, Name, SequenceNo, ArrivalOffsetMinutes) VALUES(@routeId, 'KHI', 2, 240);
INSERT INTO Stops(RouteId, Name, SequenceNo, ArrivalOffsetMinutes) VALUES(@routeId, 'MULTAN', 3, 480);
INSERT INTO Stops(RouteId, Name, SequenceNo, ArrivalOffsetMinutes) VALUES(@routeId, 'LHR', 4, 720);

INSERT INTO Buses(BusNo, Capacity, Type, FullCapacity) VALUES('BUS-1001', 40, 'AC', 40);
DECLARE @busId INT = SCOPE_IDENTITY();

-- Create seats for bus
DECLARE @i INT = 1;
WHILE @i <= 40
BEGIN
    INSERT INTO Seats(BusId, SeatNumber, IsWindow) VALUES(@busId, CONCAT('S', @i), CASE WHEN @i % 4 IN (1,0) THEN 1 ELSE 0 END);
    SET @i = @i + 1;
END

-- Add schedule
INSERT INTO Schedules(RouteId, BusId, DepartureDateTime, ArrivalDateTime) VALUES(@routeId, @busId, DATEADD(day,1,GETDATE()), DATEADD(day,1,DATEADD(minute,1200,GETDATE())));

PRINT 'Database initialization completed.';
