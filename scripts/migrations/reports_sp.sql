-- Reporting stored procedures: bookings and revenue aggregates
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO

-- Daily bookings between date range
CREATE PROCEDURE sp_GetBookingsDaily
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(BookingDate AS DATE) AS DayDate,
           COUNT(*) AS TotalBookings,
           SUM(TotalAmount) AS TotalAmount
    FROM Bookings
    WHERE BookingDate >= @StartDate AND BookingDate <= @EndDate
    GROUP BY CAST(BookingDate AS DATE)
    ORDER BY DayDate;
END
GO

-- Monthly revenue (year-month)
CREATE PROCEDURE sp_GetRevenueMonthly
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MONTH(BookingDate) AS Month, SUM(TotalAmount) AS Revenue
    FROM Bookings
    WHERE YEAR(BookingDate) = @Year
    GROUP BY MONTH(BookingDate)
    ORDER BY Month;
END
GO

-- Bookings route-wise between dates
CREATE PROCEDURE sp_GetBookingsByRoute
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.RouteId, r.Name, COUNT(b.BookingId) AS TotalBookings, SUM(b.TotalAmount) AS TotalAmount
    FROM Bookings b
    INNER JOIN Schedules s ON b.ScheduleId = s.ScheduleId
    INNER JOIN Routes r ON s.RouteId = r.RouteId
    WHERE b.BookingDate BETWEEN @StartDate AND @EndDate
    GROUP BY r.RouteId, r.Name
    ORDER BY TotalBookings DESC;
END
GO

-- Bookings bus-wise between dates
CREATE PROCEDURE sp_GetBookingsByBus
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    SELECT bus.BusId, bus.BusNo, COUNT(b.BookingId) AS TotalBookings, SUM(b.TotalAmount) AS TotalAmount
    FROM Bookings b
    INNER JOIN Schedules s ON b.ScheduleId = s.ScheduleId
    INNER JOIN Buses bus ON s.BusId = bus.BusId
    WHERE b.BookingDate BETWEEN @StartDate AND @EndDate
    GROUP BY bus.BusId, bus.BusNo
    ORDER BY TotalBookings DESC;
END
GO

-- Occupancy rate for a schedule (seats booked / total seats) between dates
CREATE PROCEDURE sp_GetOccupancyRate
    @ScheduleId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TotalSeats INT = (SELECT COUNT(*) FROM Seats WHERE BusId = (SELECT BusId FROM Schedules WHERE ScheduleId = @ScheduleId));
    DECLARE @BookedSeats INT = (
        SELECT COUNT(DISTINCT bs.SeatId)
        FROM BookingSegments bs
        INNER JOIN Bookings b ON bs.BookingId = b.BookingId
        WHERE b.ScheduleId = @ScheduleId AND b.Status <> 'Cancelled'
    );
    SELECT @TotalSeats AS TotalSeats, @BookedSeats AS BookedSeats, CASE WHEN @TotalSeats = 0 THEN 0 ELSE CAST(100.0 * @BookedSeats / @TotalSeats AS DECIMAL(5,2)) END AS OccupancyPercent;
END
GO

-- Driver/hostess assignment history placeholder (requires driver/hostess tables to be present)
CREATE PROCEDURE sp_GetDriverAssignmentHistory
    @DriverId INT,
    @StartDate DATETIME2,
    @EndDate DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    -- Placeholder: If you have DriverAssignments table, query it. Returning empty result for now.
    SELECT CAST(NULL AS DATETIME2) AS Date, CAST(NULL AS INT) AS ScheduleId, CAST(NULL AS NVARCHAR(200)) AS Notes WHERE 1 = 0;
END
GO
