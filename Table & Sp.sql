USE [MegaMinds]
GO

/****** Object:  Table [dbo].[Users]    Script Date: 24-10-2024 13:00:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Phone] [varchar](10) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[Password] [nvarchar](100) NOT NULL,
	[IsAdmin] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Users]  WITH CHECK ADD CHECK  ((len([Phone])=(10)))
GO
-----------------------------------------------------------------------------------------------------------------------------------------
USE [MegaMinds]
GO

/****** Object:  Table [dbo].[UserCategory]    Script Date: 24-10-2024 13:00:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[UserCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[CategoryId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[UserCategory]  WITH CHECK ADD FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO

ALTER TABLE [dbo].[UserCategory]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
-----------------------------------------------------------------------------------------------------------------------------------------
USE [MegaMinds]
GO

/****** Object:  Table [dbo].[Category]    Script Date: 24-10-2024 13:01:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Category](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
-----------------------------------------------------------------------------------------------------------------------------------------
USE [MegaMinds]
GO

/****** Object:  Table [dbo].[SubCategory]    Script Date: 24-10-2024 13:01:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SubCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CategoryId] [int] NULL,
	[Name] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SubCategory]  WITH CHECK ADD FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
-----------------------------------------------------------------------------------------------------------------------------------------
USE [MegaMinds]
GO

/****** Object:  StoredProcedure [dbo].[sp_UserLogin]    Script Date: 24-10-2024 13:02:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_UserLogin]
    @Email NVARCHAR(100),
    @Password NVARCHAR(100)
AS
BEGIN
    SELECT 
        Id,
        Name,
        Email,
        IsAdmin
    FROM Users 
    WHERE Email = @Email 
    AND Password = @Password
END
GO
-----------------------------------------------------------------------------------------------------------------------------------------
USE [MegaMinds]
GO

/****** Object:  StoredProcedure [dbo].[sp_CreateUser]    Script Date: 24-10-2024 13:03:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_CreateUser]
    @Name NVARCHAR(100),
    @Phone NVARCHAR(10),
    @Email NVARCHAR(100),
    @Password NVARCHAR(256),
    @IsAdmin BIT,
    @Categories NVARCHAR(MAX) -- Comma-separated category IDs
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION

        -- Insert User
        INSERT INTO Users (Name, Phone, Email, Password, IsAdmin)
        VALUES (@Name, @Phone, @Email, @Password, @IsAdmin)
        
        DECLARE @UserId INT = SCOPE_IDENTITY()
        
        -- Insert User Categories if not admin
        IF @IsAdmin = 0 AND @Categories IS NOT NULL
        BEGIN
            INSERT INTO UserCategory (UserId, CategoryId)
            SELECT @UserId, CAST(Value AS INT) -- Ensure the column is named Value
            FROM dbo.SplitString(@Categories, ',')
        END
        
        COMMIT TRANSACTION
        SELECT @UserId AS UserId
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        THROW
    END CATCH
END
GO
-----------------------------------------------------------------------------------------------------------------------------------------
USE [MegaMinds]
GO

/****** Object:  StoredProcedure [dbo].[sp_UpdateUser]    Script Date: 24-10-2024 13:03:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_UpdateUser]
    @Id INT,
    @Name NVARCHAR(100),
    @Phone NVARCHAR(10),
    @Email NVARCHAR(100),
    @Categories NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    -- Validate phone number
    IF LEN(@Phone) <> 10 OR NOT ISNUMERIC(@Phone) = 1
    BEGIN
        RAISERROR('Phone number must be 10 digits long and numeric', 16, 1);
        RETURN;
    END

    -- Validate email format (simple format validation)
    IF @Email NOT LIKE '%_@__%.__%'
    BEGIN
        RAISERROR('Invalid email format', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Update User information
        UPDATE Users
        SET Name = @Name,
            Phone = @Phone,
            Email = @Email
        WHERE Id = @Id;

        -- Delete existing categories
        DELETE FROM UserCategory WHERE UserId = @Id;

        -- Insert new categories
        IF @Categories IS NOT NULL AND LEN(@Categories) > 0
        BEGIN
            INSERT INTO UserCategory (UserId, CategoryId)
            SELECT @Id, CAST(Value AS INT) -- Ensure to reference the correct column
            FROM dbo.SplitString(@Categories, ',')
            WHERE ISNUMERIC(Value) = 1; -- Ensure to reference the correct column
        END

        COMMIT TRANSACTION;

        SELECT 1 AS Success;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 0 AS Success, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO
-----------------------------------------------------------------------------------------------------------------------------------------
USE [MegaMinds]
GO

/****** Object:  StoredProcedure [dbo].[sp_DeleteUser]    Script Date: 24-10-2024 13:04:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_DeleteUser]
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION
            -- Delete user categories
            DELETE FROM UserCategory WHERE UserId = @Id
            
            -- Delete the user
            DELETE FROM Users WHERE Id = @Id
            
        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION

        THROW
    END CATCH
END
GO
-----------------------------------------------------------------------------------------------------------------------------------------
USE [MegaMinds]
GO

/****** Object:  StoredProcedure [dbo].[sp_GetAllUsers]    Script Date: 24-10-2024 13:04:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetAllUsers]
AS
BEGIN
    SELECT 
        U.Id,
        U.Name,
        U.Phone,
        U.Email,
        U.IsAdmin,
        STUFF((
            SELECT ', ' + C.Name
            FROM UserCategory UC 
            JOIN Category C ON UC.CategoryId = C.Id
            WHERE UC.UserId = U.Id
            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Categories
    FROM Users U
    WHERE U.IsAdmin = 0
    GROUP BY U.Id, U.Name, U.Phone, U.Email, U.IsAdmin
END
GO
-----------------------------------------------------------------------------------------------------------------------------------------
USE [MegaMinds]
GO

/****** Object:  StoredProcedure [dbo].[sp_GetSubCategoriesByCategory]    Script Date: 24-10-2024 13:04:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[sp_GetSubCategoriesByCategory]
    @CategoryId INT
AS
BEGIN
    SELECT Id, Name
    FROM SubCategory
    WHERE CategoryId = @CategoryId
END
GO
-----------------------------------------------------------------------------------------------------------------------------------------




