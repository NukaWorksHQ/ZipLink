-- Script d'initialisation de la base de données ZipLink
USE master;
GO

-- Créer la base de données si elle n'existe pas
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ZIPLINK')
BEGIN
    CREATE DATABASE [ZIPLINK]
END
GO

USE [ZIPLINK];
GO

-- Configuration pour la réplication
-- Activer la réplication transactionnelle
EXEC sp_replicationdboption 
    @dbname = N'ZIPLINK', 
    @optname = N'publish', 
    @value = N'true'
GO

-- Créer un utilisateur pour l'application
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'ziplink_user')
BEGIN
    CREATE LOGIN [ziplink_user] WITH PASSWORD = N'$(DB_USER_PASSWORD)', 
        DEFAULT_DATABASE = [ZIPLINK], 
        CHECK_EXPIRATION = OFF, 
        CHECK_POLICY = OFF
END
GO

USE [ZIPLINK];
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'ziplink_user')
BEGIN
    CREATE USER [ziplink_user] FOR LOGIN [ziplink_user]
    ALTER ROLE [db_owner] ADD MEMBER [ziplink_user]
END
GO

-- Configuration des paramètres de performance
ALTER DATABASE [ZIPLINK] SET RECOVERY FULL
GO

-- Configuration pour les connexions multiples
ALTER DATABASE [ZIPLINK] SET ALLOW_SNAPSHOT_ISOLATION ON
GO

ALTER DATABASE [ZIPLINK] SET READ_COMMITTED_SNAPSHOT ON
GO

PRINT 'Database ZIPLINK initialized successfully'