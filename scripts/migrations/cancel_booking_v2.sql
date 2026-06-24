-- Cancellation stored procedure with refund calculation and audit log entry
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE sp_CancelBooking_V2
    @BookingId UNIQUEIDENTIFIER,
    @CancelledBy UNIQUEIDENTIFIER -- user performing cancellation
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @BookingStatus NVARCHAR(50);
    DECLARE @ScheduleId INT;
    DECLARE @TotalAmount DECIMAL(18,2);
    DECLARE @Departure DATETIME2;
    DECLARE @RefundAmount DECIMAL(18,2);
    DECLARE @Now DATETIME2 = SYSUTCDATETIME();

    SELECT @BookingStatus = Status, @ScheduleId = ScheduleId, @TotalAmount = TotalAmount
    FROM Bookings WHERE BookingId = @BookingId;

    IF @BookingStatus IS NULL
    BEGIN
        RAISERROR('Booking not found',16,1);
        RETURN;
    END

    IF @BookingStatus = 'Cancelled'
    BEGIN
        RAISERROR('Booking already cancelled',16,1);
        RETURN;
    END

    SELECT @Departure = DepartureDateTime FROM Schedules WHERE ScheduleId = @ScheduleId;
    IF @Departure IS NULL
    BEGIN
        RAISERROR('Schedule not found',16,1);
        RETURN;
    END

    DECLARE @HoursDiff INT = DATEDIFF(hour, @Now, @Departure);

    -- If cancellation happens within 48 hours of departure (i.e., HoursDiff < 48), deduct 30%
    IF @HoursDiff < 48
        SET @RefundAmount = ROUND(@TotalAmount * 0.7, 2);
    ELSE
        SET @RefundAmount = @TotalAmount;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE Bookings SET Status = 'Cancelled', CancellationRequested = 1 WHERE BookingId = @BookingId;

        -- Update payment record(s) to reflect refund/failed
        UPDATE Payments SET Status = 'Refunded', PaidAt = SYSUTCDATETIME() WHERE BookingId = @BookingId;

        -- Insert audit log
        INSERT INTO AuditLogs(Entity, Action, PerformedBy, PerformedAt, Details)
        VALUES('Booking', 'Cancel', @CancelledBy, SYSUTCDATETIME(), CONCAT('RefundAmount=', CAST(@RefundAmount AS NVARCHAR(50)), '; BookingId=', CAST(@BookingId AS NVARCHAR(50))));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@Err,16,1);
    END CATCH
END
GO
