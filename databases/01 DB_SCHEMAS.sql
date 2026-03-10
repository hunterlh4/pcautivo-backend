
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


CREATE TABLE Users
(
	Id INT PRIMARY KEY IDENTITY(1,1),
	Username NVARCHAR(100) NOT NULL,
	PasswordHash NVARCHAR(200) NOT NULL,
	SuperUser BIT NOT NULL DEFAULT 0,
	CreatedAt DATETIME NOT NULL,
	UpdatedAt DATETIME NULL,
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
	CreatedAt DATETIME NOT NULL,
	UpdatedAt DATETIME NULL
)
GO
-- =============================================
-- Datos de prueba
--   admin    -> 102030
--   cliente1   -> 102030
--   cliente2   -> 102030
-- =============================================

INSERT INTO Users (Username, PasswordHash, SuperUser, CreatedAt)
VALUES
    ('admin',  '$2a$13$pHywiuK9AY4X/BORNdpNaeINFbvePylHLH.d6NiLEr.lUKWNEbooW', 1, GETDATE()),
    ('cliente1', '$2a$13$pHywiuK9AY4X/BORNdpNaeINFbvePylHLH.d6NiLEr.lUKWNEbooW', 0, GETDATE()),
    ('cliente2', '$2a$13$pHywiuK9AY4X/BORNdpNaeINFbvePylHLH.d6NiLEr.lUKWNEbooW', 0, GETDATE());
GO

INSERT INTO UserDetails (UserId, FirstName, LastName, Email, PhoneNumber, CountryCode, CreatedAt)
VALUES
    (1, 'Administrador', 'Sistema',  'admin@pcautivo.com',  '933054810', '+51', GETDATE()),
    (2, 'pedro',          'pedro2',    'jperez@pcautivo.com', '933054811', '+51', GETDATE()),
    (3, 'jose',         'jose2',    'mlopez@pcautivo.com', '933054812', '+51', GETDATE());
GO