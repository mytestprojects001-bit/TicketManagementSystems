-- Adds RefreshTokens table and stored procedures for refresh token management
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RefreshTokens]') AND type in (N'U'))
BEGIN
CREATE TABLE RefreshTokens (
    Token NVARCHAR(200) PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Revoked BIT NOT NULL DEFAULT 0,
    ReplacedByToken NVARCHAR(200) NULL,
    CONSTRAINT FK_RefreshTokens_User FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);
END
GO

-- Create a refresh token
CREATE PROCEDURE sp_CreateRefreshToken
    @Token NVARCHAR(200),
    @UserId UNIQUEIDENTIFIER,
    @ExpiresAt DATETIME2
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO RefreshTokens(Token, UserId, ExpiresAt) VALUES(@Token, @UserId, @ExpiresAt);
END
GO

-- Get refresh token info
CREATE PROCEDURE sp_GetRefreshToken
    @Token NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Token, UserId, ExpiresAt, Revoked, ReplacedByToken FROM RefreshTokens WHERE Token = @Token;
END
GO
