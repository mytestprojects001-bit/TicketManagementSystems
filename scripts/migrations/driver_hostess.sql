-- Drivers, Hostesses and assignments tables + sample SPs
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Drivers]') AND type in (N'U'))
BEGIN
CREATE TABLE Drivers (
    DriverId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    LicenseNumber NVARCHAR(100) NULL,
    Phone NVARCHAR(50) NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Hostesses]') AND type in (N'U'))
BEGIN
CREATE TABLE Hostesses (
    HostessId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(50) NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DriverAssignments]') AND type in (N'U'))
BEGIN
CREATE TABLE DriverAssignments (
    AssignmentId INT IDENTITY(1,1) PRIMARY KEY,
    DriverId INT NOT NULL,
    ScheduleId INT NOT NULL,
    AssignedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Notes NVARCHAR(MAX) NULL,
    CONSTRAINT FK_DriverAssign_Driver FOREIGN KEY (DriverId) REFERENCES Drivers(DriverId),
    CONSTRAINT FK_DriverAssign_Schedule FOREIGN KEY (ScheduleId) REFERENCES Schedules(ScheduleId)
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HostessAssignments]') AND type in (N'U'))
BEGIN
CREATE TABLE HostessAssignments (
    AssignmentId INT IDENTITY(1,1) PRIMARY KEY,
    HostessId INT NOT NULL,
    ScheduleId INT NOT NULL,
    AssignedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Notes NVARCHAR(MAX) NULL,
    CONSTRAINT FK_HostessAssign_Hostess FOREIGN KEY (HostessId) REFERENCES Hostesses(HostessId),
    CONSTRAINT FK_HostessAssign_Schedule FOREIGN KEY (ScheduleId) REFERENCES Schedules(ScheduleId)
);
END
GO

-- Driver assignment history SP
CREATE PROCEDURE sp_GetDriverAssignmentHistory
    @DriverId INT,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT da.AssignmentId, da.DriverId, d.FullName AS DriverName, da.ScheduleId, s.DepartureDateTime, da.AssignedAt, da.Notes
    FROM DriverAssignments da
    INNER JOIN Drivers d ON da.DriverId = d.DriverId
    INNER JOIN Schedules s ON da.ScheduleId = s.ScheduleId
    WHERE da.DriverId = @DriverId
    AND (@StartDate IS NULL OR da.AssignedAt >= @StartDate)
    AND (@EndDate IS NULL OR da.AssignedAt <= @EndDate)
    ORDER BY da.AssignedAt DESC;
END
GO

-- Hostess assignment history SP
CREATE PROCEDURE sp_GetHostessAssignmentHistory
    @HostessId INT,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ha.AssignmentId, ha.HostessId, h.FullName AS HostessName, ha.ScheduleId, s.DepartureDateTime, ha.AssignedAt, ha.Notes
    FROM HostessAssignments ha
    INNER JOIN Hostesses h ON ha.HostessId = h.HostessId
    INNER JOIN Schedules s ON ha.ScheduleId = s.ScheduleId
    WHERE ha.HostessId = @HostessId
    AND (@StartDate IS NULL OR ha.AssignedAt >= @StartDate)
    AND (@EndDate IS NULL OR ha.AssignedAt <= @EndDate)
    ORDER BY ha.AssignedAt DESC;
END
GO
