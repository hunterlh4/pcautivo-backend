
IF NOT EXISTS (
    SELECT name 
    FROM sys.databases 
    WHERE name = 'DB_PCAUTIVO'
)
BEGIN
    CREATE DATABASE DB_PCAUTIVO;
END
GO

USE DB_PCAUTIVO;
GO


/* CREATE TABLE Users
(
	Id INT PRIMARY KEY IDENTITY(1,1),
	Username NVARCHAR(100) NOT NULL,
	PasswordHash NVARCHAR(200) NOT NULL,
	SuperUser BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2(7) NOT NULL,
    UpdatedAt DATETIME2(7) NULL,
	CONSTRAINT UC_Users_Username UNIQUE (Username)
)
GO

CREATE TABLE UserDetails
(
	UserId INT PRIMARY KEY,
	FirstName NVARCHAR(100) NOT NULL,
	LastName NVARCHAR(100) NOT NULL,
	Email NVARCHAR(200) NOT NULL,
	PhoneNumber NVARCHAR(50) NULL,
	CountryCode  NVARCHAR(10) null,
    CreatedAt DATETIME2(7) NOT NULL,
    UpdatedAt DATETIME2(7) NULL
)
GO */

-- Script para crear las tablas necesarias para el tracking de Webhooks Omada
-- 1. Tabla Devices
CREATE TABLE Devices (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MacAddress VARCHAR(50) NOT NULL UNIQUE,
    Dni NVARCHAR(20) NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO
-- 2. Tabla DeviceSessions
CREATE TABLE DeviceSessions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DeviceId INT NOT NULL,
    SessionId NVARCHAR(100) NULL,
    StartTime DATETIME2(7) NOT NULL,
    EndTime DATETIME2(7) NULL,
    DurationSeconds INT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

/* CREATE TABLE UserDevices (
    UserId INT NOT NULL,
    DeviceId INT NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_UserDevices PRIMARY KEY (UserId, DeviceId),
    CONSTRAINT FK_UserDevices_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserDevices_Devices FOREIGN KEY (DeviceId) REFERENCES Devices(Id)
);
GO */

-- Migracion para bases existentes: convierte DATETIME a DATETIME2(7)
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'CreatedAt' AND system_type_id = 61)
BEGIN
    ALTER TABLE dbo.Users ALTER COLUMN CreatedAt DATETIME2(7) NOT NULL;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'UpdatedAt' AND system_type_id = 61)
BEGIN
    ALTER TABLE dbo.Users ALTER COLUMN UpdatedAt DATETIME2(7) NULL;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.UserDetails') AND name = 'CreatedAt' AND system_type_id = 61)
BEGIN
    ALTER TABLE dbo.UserDetails ALTER COLUMN CreatedAt DATETIME2(7) NOT NULL;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.UserDetails') AND name = 'UpdatedAt' AND system_type_id = 61)
BEGIN
    ALTER TABLE dbo.UserDetails ALTER COLUMN UpdatedAt DATETIME2(7) NULL;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Devices') AND name = 'CreatedAt' AND system_type_id = 61)
BEGIN
    ALTER TABLE dbo.Devices ALTER COLUMN CreatedAt DATETIME2(7) NOT NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Devices') AND name = 'Dni')
BEGIN
    ALTER TABLE dbo.Devices ADD Dni NVARCHAR(20) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DeviceSessions') AND name = 'StartTime')
BEGIN
    ALTER TABLE dbo.DeviceSessions ADD StartTime DATETIME2(7) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DeviceSessions') AND name = 'EndTime')
BEGIN
    ALTER TABLE dbo.DeviceSessions ADD EndTime DATETIME2(7) NULL;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DeviceSessions') AND name = 'StartTimeUtc')
BEGIN
    UPDATE dbo.DeviceSessions
    SET StartTime = ISNULL(StartTime, StartTimeUtc)
    WHERE StartTime IS NULL;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DeviceSessions') AND name = 'EndTimeUtc')
BEGIN
    UPDATE dbo.DeviceSessions
    SET EndTime = ISNULL(EndTime, EndTimeUtc)
    WHERE EndTime IS NULL;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DeviceSessions') AND name = 'EndTimee')
BEGIN

    UPDATE dbo.DeviceSessions
    SET EndTime = ISNULL(EndTime, EndTimee)
    WHERE EndTime IS NULL;
END


IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DeviceSessions') AND name = 'StartTime')
BEGIN
    ALTER TABLE dbo.DeviceSessions ALTER COLUMN StartTime DATETIME2(7) NOT NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DeviceSessions') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE dbo.DeviceSessions ADD CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_DeviceSessions_CreatedAt DEFAULT SYSUTCDATETIME();
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DeviceSessions') AND name = 'SessionId')
BEGIN
    ALTER TABLE dbo.DeviceSessions ADD SessionId NVARCHAR(100) NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DeviceSessions') AND name = 'DurationSeconds')
BEGIN
    ALTER TABLE dbo.DeviceSessions ADD DurationSeconds INT NULL;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DeviceSessions') AND name = 'StartTimeUtc')
BEGIN
    ALTER TABLE dbo.DeviceSessions DROP COLUMN StartTimeUtc;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DeviceSessions') AND name = 'EndTimeUtc')
BEGIN
    ALTER TABLE dbo.DeviceSessions DROP COLUMN EndTimeUtc;
END

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DeviceSessions') AND name = 'EndTimee')
BEGIN
    ALTER TABLE dbo.DeviceSessions DROP COLUMN EndTimee;
END


IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.DeviceSessions') AND name = 'SessionType')
BEGIN
    ALTER TABLE dbo.DeviceSessions DROP COLUMN SessionType;
END
GO
-- Índices recomendados para búsquedas rápidas por MAC y Fechas
CREATE INDEX IX_Devices_MacAddress ON Devices(MacAddress);
CREATE INDEX IX_Devices_Dni ON Devices(Dni);
CREATE INDEX IX_DeviceSessions_StartTime ON DeviceSessions(StartTime);
CREATE INDEX IX_DeviceSessions_DeviceId ON DeviceSessions(DeviceId);
CREATE INDEX IX_DeviceSessions_SessionId ON DeviceSessions(SessionId);
CREATE UNIQUE INDEX UX_DeviceSessions_DeviceSessionId
ON DeviceSessions(DeviceId, SessionId)
WHERE SessionId IS NOT NULL;
CREATE INDEX IX_UserDevices_DeviceId ON UserDevices(DeviceId);
GO

-- =============================================
-- Datos de prueba
--   admin    -> 102030
--   cliente1   -> 102030
--   cliente2   -> 102030
-- =============================================

INSERT INTO Users (Username, PasswordHash, SuperUser, CreatedAt)
VALUES
    ('admin',  '$2a$13$pHywiuK9AY4X/BORNdpNaeINFbvePylHLH.d6NiLEr.lUKWNEbooW', 1, SYSDATETIME()),
    ('cliente1', '$2a$13$pHywiuK9AY4X/BORNdpNaeINFbvePylHLH.d6NiLEr.lUKWNEbooW', 0, SYSDATETIME()),
    ('cliente2', '$2a$13$pHywiuK9AY4X/BORNdpNaeINFbvePylHLH.d6NiLEr.lUKWNEbooW', 0, SYSDATETIME());
GO

INSERT INTO UserDetails (UserId, FirstName, LastName, Email, PhoneNumber, CountryCode, CreatedAt)
VALUES
    (1, 'Administrador', 'Sistema',  'admin@pcautivo.com',  '933054810', '+51', SYSDATETIME()),
    (2, 'pedro',          'pedro2',    'jperez@pcautivo.com', '933054811', '+51', SYSDATETIME()),
    (3, 'jose',         'jose2',    'mlopez@pcautivo.com', '933054812', '+51', SYSDATETIME());
GO