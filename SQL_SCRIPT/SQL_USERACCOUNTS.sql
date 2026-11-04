-- =========================================================================
-- SCRIPT 1: USER ACCOUNTS SYSTEM
-- =========================================================================

-- -------------------------------------------------------------------------
-- PART A: CREATE TABLES, PROCEDURES, AND DEFAULT DATA
-- -------------------------------------------------------------------------
IF OBJECT_ID('dbo.SP_AutoNumberUser', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_AutoNumberUser;
IF OBJECT_ID('dbo.cashier_accounts', 'U') IS NOT NULL DROP TABLE dbo.cashier_accounts;
IF OBJECT_ID('dbo.owner_accounts', 'U') IS NOT NULL DROP TABLE dbo.owner_accounts;
IF OBJECT_ID('dbo.admin_accounts', 'U') IS NOT NULL DROP TABLE dbo.admin_accounts;
IF OBJECT_ID('dbo.user_accounts', 'U') IS NOT NULL DROP TABLE dbo.user_accounts;
IF OBJECT_ID('dbo.AutoNumberUsers', 'U') IS NOT NULL DROP TABLE dbo.AutoNumberUsers;
GO

CREATE TABLE user_accounts (
    userID INT IDENTITY(1,1) PRIMARY KEY,
    role VARCHAR(50) NOT NULL,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(50) NOT NULL,
    gender VARCHAR(20),
    first_name VARCHAR(50),
    middle_name VARCHAR(50),
    last_name VARCHAR(50),
    extension_name VARCHAR(10),
    created_at DATETIME DEFAULT GETDATE(),
    userimage VARBINARY(MAX)
);

CREATE TABLE admin_accounts (
    userID INT PRIMARY KEY,
    admin_code VARCHAR(20) UNIQUE,
    FOREIGN KEY (userID) REFERENCES user_accounts(userID) ON DELETE CASCADE
);

CREATE TABLE owner_accounts (
    userID INT PRIMARY KEY,
    owner_code VARCHAR(20) UNIQUE,
    FOREIGN KEY (userID) REFERENCES user_accounts(userID) ON DELETE CASCADE
);

CREATE TABLE cashier_accounts (
    userID INT PRIMARY KEY,
    cashier_code VARCHAR(20) UNIQUE,
    shift VARCHAR(20),
    FOREIGN KEY (userID) REFERENCES user_accounts(userID) ON DELETE CASCADE
);


CREATE TABLE AutoNumberUsers (
    pfx VARCHAR(5) NOT NULL,
    NewNumber VARCHAR(15) NOT NULL
);
GO

CREATE PROCEDURE SP_AutoNumberUser
    @pfx VARCHAR(4),
    @NewNumber VARCHAR(15) OUTPUT
AS
BEGIN
    DECLARE @lastNum VARCHAR(15), @intNum INT, @newNumStr VARCHAR(15);
    SELECT TOP 1 @lastNum = NewNumber FROM AutoNumberUsers WHERE pfx = @pfx ORDER BY NewNumber DESC;
    IF @lastNum IS NULL
    BEGIN
        SET @intNum = 1;
        SET @newNumStr = @pfx + '10000';
        INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES (@pfx, @newNumStr);
    END
    ELSE
    BEGIN
        SET @intNum = CAST(SUBSTRING(@lastNum, 2, LEN(@lastNum)) AS INT) + 1;
        SET @newNumStr = @pfx + RIGHT('1000' + CAST(@intNum AS VARCHAR), 4);
        INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES (@pfx, @newNumStr);
    END
    SET @NewNumber = @newNumStr;
END
GO

-- Insert Default Admin
INSERT INTO user_accounts (role, username, password, gender, first_name, middle_name, last_name, extension_name)
VALUES ('admin', 'admin1', 'Admin123', 'male', 'Admin', '', 'Account', '');
DECLARE @adminUserID INT = SCOPE_IDENTITY();
INSERT INTO admin_accounts (userID, admin_code) VALUES (@adminUserID, 'A1000');

-- Initialize Sequences
INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES ('A', 'A1001'); 
INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES ('C', 'C1000'); 
INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES ('O', 'O1000'); 
GO

-- -------------------------------------------------------------------------
-- PART B: SELECT (VIEW DATA)
-- -------------------------------------------------------------------------
SELECT 
    u.userID, u.role, u.username, u.password, u.first_name, u.last_name,
    COALESCE(a.admin_code, o.owner_code, c.cashier_code) AS RoleCode,
    c.shift, u.created_at
FROM user_accounts u
LEFT JOIN admin_accounts a ON a.userID = u.userID
LEFT JOIN owner_accounts o ON o.userID = u.userID
LEFT JOIN cashier_accounts c ON c.userID = u.userID;
GO

-- -------------------------------------------------------------------------
-- PART C: RESET / RESEED SCRIPT 
-- (Remove the -- to run. This resets the Table AND the SP_AutoNumberUser)
-- -------------------------------------------------------------------------

-- 1. Delete all users and their role records
---DELETE FROM user_accounts;

-- 2. Reset the Identity ID back to 1
-- DBCC CHECKIDENT ('user_accounts', RESEED, 0);

-- 3. Clear the AutoNumber history completely
-- DELETE FROM AutoNumberUsers;

-- 4. Put the starting seeds back so the SP generates A001, C001, O001 again
-- INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES ('A', 'A001'); 
-- INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES ('C', 'C000'); 
-- INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES ('O', 'O000'); 

-- 5. Create the default Admin again
-- INSERT INTO user_accounts (role, username, password, gender, first_name, middle_name, last_name, extension_name)
-- VALUES ('admin', 'admin', 'Admin123', 'male', 'Admin', '', 'Account', '');
-- DECLARE @adminUserID INT = SCOPE_IDENTITY();
-- INSERT INTO admin_accounts (userID, admin_code) VALUES (@adminUserID, 'A001');
GO