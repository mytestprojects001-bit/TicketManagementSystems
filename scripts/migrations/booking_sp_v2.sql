-- Enhanced seat availability and transactional booking stored procedures using application locks to prevent concurrency issues
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO

-- Enhanced availability check using stop sequence numbers
CREATE PROCEDURE sp_CheckSeatAvailability_V2
    @ScheduleId INT,
    @SeatId INT,
    @FromStopId INT,
    @ToStopId INT
AS
BEGIN
    SET NOCOUNT ON;

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

    -- Check overlap logic: existing.FromSeq < new.ToSeq AND existing.ToSeq > new.FromSeq means overlap
    IF EXISTS(
        SELECT 1
        FROM BookingSegments bs
        INNER JOIN Bookings b ON bs.BookingId = b.BookingId
        INNER JOIN Stops fs ON bs.FromStopId = fs.StopId
        INNER JOIN Stops ts ON bs.ToStopId = ts.StopId
        WHERE b.ScheduleId = @ScheduleId
          AND bs.SeatId = @SeatId
          AND (fs.SequenceNo < @ToSeq AND ts.SequenceNo > @FromSeq)
          AND b.Status <> 'Cancelled'
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

-- Transactional booking procedure with application lock on schedule-seat to prevent race conditions
CREATE PROCEDURE sp_CreateBooking_V2
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

    DECLARE @lockResource NVARCHAR(200) = CONCAT('schedule_', @ScheduleId, '_seat_', @SeatId);
    DECLARE @lockResult INT;

    -- Acquire exclusive app lock for this schedule-seat
    EXEC @lockResult = sp_getapplock @Resource = @lockResource, @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 10000;
    IF @lockResult < 0
    BEGIN
        RAISERROR('Unable to acquire lock for booking, please retry',16,1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Re-check availability inside lock
        DECLARE @IsAvailable INT;
        EXEC sp_CheckSeatAvailability_V2 @ScheduleId = @ScheduleId, @SeatId = @SeatId, @FromStopId = @FromStopId, @ToStopId = @ToStopId;
        SELECT @IsAvailable = IsAvailable FROM (SELECT 1 AS IsAvailable) AS tmp WHERE 1 = 1;
        -- The above select will not capture the result of sp. Instead, check using direct query
        IF EXISTS(
            SELECT 1
            FROM BookingSegments bs
            INNER JOIN Bookings b ON bs.BookingId = b.BookingId
            INNER JOIN Stops fs ON bs.FromStopId = fs.StopId
            INNER JOIN Stops ts ON bs.ToStopId = ts.StopId
            WHERE b.ScheduleId = @ScheduleId
              AND bs.SeatId = @SeatId
              AND (fs.SequenceNo < (SELECT SequenceNo FROM Stops WHERE StopId = @ToStopId) AND ts.SequenceNo > (SELECT SequenceNo FROM Stops WHERE StopId = @FromStopId))
              AND b.Status <> 'Cancelled'
        )
        BEGIN
            RAISERROR('Seat not available for selected segment',16,1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        DECLARE @BookingId UNIQUEIDENTIFIER = NEWID();
        INSERT INTO Bookings(BookingId, ScheduleId, UserId, TotalAmount, Status)
        VALUES(@BookingId, @ScheduleId, @UserId, @TotalAmount, @Status);

        INSERT INTO BookingSegments(BookingId, SeatId, FromStopId, ToStopId)
        VALUES(@BookingId, @SeatId, @FromStopId, @ToStopId);

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
    FINALLY
        -- Release app lock by ending session or explicitly
        -- sp_getapplock with LockOwner = 'Session' is released when session ends; explicit release not needed here
    END
END
GO
