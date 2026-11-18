USE DB_DARKSYSTEM
GO


IF OBJECT_ID('dbo.SP_AutoNumberUser', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_AutoNumber;
IF OBJECT_ID('dbo.user_accounts', 'U') IS NOT NULL DROP TABLE dbo.user_accounts;
IF OBJECT_ID('dbo.AutoNumberUser', 'U') IS NOT NULL DROP TABLE dbo.AutoNumber;
GO

CREATE TABLE [dbo].[AutoNumberUser](
    [RecNo]       [int] IDENTITY(1,1) NOT NULL,
    [Pfx]         [varchar](10) NULL,              -- Changed to VARCHAR
    [NewNumber]   [varchar](20) NULL,              -- Changed to VARCHAR
    CONSTRAINT [PK_AutoNumber] PRIMARY KEY CLUSTERED ([RecNo] ASC)
)
GO


CREATE PROCEDURE [dbo].[SP_AutoNumberUser]
    @pfx       varchar(10) = '',
    @NewNumber varchar(20) = '' OUTPUT
AS
BEGIN
    Declare @MaxCode As varchar(20)
    Declare @CreateNumber As int
    Declare @RecNo As int

    select @MaxCode = case when max(NewNumber) is null then 'DSBS0000000'
                           else max(NewNumber) end
    from AutoNumberUser
    where left(NewNumber, 4) = @pfx 

    set @CreateNumber = cast(right(@maxCode, 7) as int) + 1

  
    Select @NewNumber = @pfx + Replicate('0', 11 - Len(@CreateNumber) - 4) + Cast(@CreateNumber as varchar)

    insert into AutoNumberUser(Pfx, NewNumber) Values
    (@Pfx, @NewNumber)

    Select @RecNo = @@Identity
    Select NewNumber from AutoNumberUser where RecNo = @RecNo
END
GO


CREATE TABLE user_accounts (
    userID          INT IDENTITY(1,1) PRIMARY KEY,
    user_code       VARCHAR(50) NOT NULL UNIQUE,   
    role            VARCHAR(20) NOT NULL,          
    username        VARCHAR(255) NOT NULL UNIQUE,
    password        VARCHAR(50) NOT NULL,
    gender          VARCHAR(20)  NOT NULL,
    first_name      VARCHAR(255)  NOT NULL,
    middle_name     VARCHAR(255)  NULL,
    last_name       VARCHAR(255) NOT NULL,
    extension_name  VARCHAR(20) NULL,
    shift           VARCHAR(20)  NULL,              
    created_at      DATETIME DEFAULT GETDATE(),
    userimage       image
);
GO


DECLARE @adminCode varchar(20); -- Changed to VARCHAR
EXEC [dbo].[SP_AutoNumberUser] @pfx = 'DSBS', @NewNumber = @adminCode OUTPUT;
DECLARE @cashierCode varchar(20); -- Changed to VARCHAR
EXEC [dbo].[SP_AutoNumberUser] @pfx = 'DSBS', @NewNumber =  @cashierCode OUTPUT;
DECLARE @ownerCode varchar(20); -- Changed to VARCHAR
EXEC [dbo].[SP_AutoNumberUser] @pfx = 'DSBS', @NewNumber =  @ownerCode OUTPUT;



INSERT INTO user_accounts (user_code, role, username, password, gender, first_name, middle_name, last_name, extension_name)
VALUES (@adminCode, 'admin', 'admin2', 'Admin123', 'male', 'Admin', '', 'Account', ''),
 (@cashierCode, 'cashier', 'cashier1', 'Cashier123', 'male', 'cashier', '', 'Account', ''),
 (@ownerCode, 'owner', 'owner1', 'Owner123', 'male', 'cashier', '', 'Account', '');
GO

--VIEW DATA (Use LEN to prove there are no hidden spaces)
SELECT userID, user_code, LEN(user_code) as CodeLength, role, username, password, first_name, last_name, shift, created_at 
FROM user_accounts;
GO

-- 6. RESEED IDENTITIES

-- Delete existing test data so reseed to 1 is safe
DELETE FROM dbo.user_accounts;
DELETE FROM dbo.AutoNumberUser;
GO

-- Reseed to 0 => next inserted row receives identity value 1
DBCC CHECKIDENT ('dbo.AutoNumberUser', RESEED, 0);
DBCC CHECKIDENT ('dbo.user_accounts', RESEED, 0);
GO


