-- =========================================================================
-- SCRIPT 2: MENU & CATEGORIES SYSTEM
-- =========================================================================

-- -------------------------------------------------------------------------
-- PART A: CREATE TABLES, PROCEDURES, AND DEFAULT DATA
-- -------------------------------------------------------------------------
IF OBJECT_ID('dbo.sp_AutoNumbermenu', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_AutoNumbermenu;
IF OBJECT_ID('dbo.menu_items', 'U') IS NOT NULL DROP TABLE dbo.menu_items;
IF OBJECT_ID('dbo.categories', 'U') IS NOT NULL DROP TABLE dbo.categories;
GO

CREATE TABLE categories (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    category_name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE menu_items (
    item_id INT IDENTITY(1,1) PRIMARY KEY,
    item_code VARCHAR(20) UNIQUE NOT NULL,
    product_name VARCHAR(100) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    category_id INT NOT NULL,
    quantity INT NOT NULL DEFAULT 0,
    product_image VARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (category_id) REFERENCES categories(category_id)
);
GO

CREATE PROCEDURE sp_AutoNumbermenu
    @pfx VARCHAR(5), @NewNumber VARCHAR(15) OUTPUT
AS
BEGIN
    DECLARE @lastNum VARCHAR(15), @intNum INT, @newNumStr VARCHAR(15);
    
    -- Look in AutoNumberUsers table for the last 'M' number
    SELECT TOP 1 @lastNum = NewNumber FROM AutoNumberUsers WHERE pfx = @pfx ORDER BY NewNumber DESC;
    
    IF @lastNum IS NULL
    BEGIN
        -- IF RESET: Start at M1000
        SET @intNum = 1000;
        SET @newNumStr = @pfx + CAST(@intNum AS VARCHAR);
        INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES (@pfx, @newNumStr);
    END
    ELSE
    BEGIN
        -- Add 1 to the last number (e.g., M1000 -> M1001)
        SET @intNum = CAST(SUBSTRING(@lastNum, LEN(@pfx) + 1, LEN(@lastNum)) AS INT) + 1;
        SET @newNumStr = @pfx + CAST(@intNum AS VARCHAR);
        INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES (@pfx, @newNumStr);
    END
    SET @NewNumber = @newNumStr;
END
GO

-- Insert Default Categories
INSERT INTO categories (category_name) VALUES 
('Burgers'), ('Hotdogs'), ('Sandwiches'), ('Fries'), ('Add-ons'), ('Drinks');
GO

-- -------------------------------------------------------------------------
-- PART B: SELECT (VIEW DATA)
-- -------------------------------------------------------------------------

-- View all Menu Items with Category Names
SELECT 
    m.item_id, m.item_code, m.product_name, m.price, 
    m.quantity, m.IsActive, m.product_image,
    c.category_name
FROM menu_items m
INNER JOIN categories c ON m.category_id = c.category_id
ORDER BY m.item_id;

-- View all Categories
SELECT * FROM categories ORDER BY category_id;
GO

-- -------------------------------------------------------------------------
-- PART C: RESET / RESEED SCRIPT 
-- (Remove the -- to run. This resets the Table AND the sp_AutoNumbermenu)
-- -------------------------------------------------------------------------

-- 1. Delete all menu items and categories
-- DELETE FROM menu_items;
-- DELETE FROM categories;

-- 2. Reset the Identity IDs back to 1
-- DBCC CHECKIDENT ('menu_items', RESEED, 0);
-- DBCC CHECKIDENT ('categories', RESEED, 0);

-- 3. Clear the 'M' history from AutoNumberUsers
-- DELETE FROM AutoNumberUsers WHERE pfx = 'M';

-- 4. Add back the default categories
-- INSERT INTO categories (category_name) VALUES 
-- ('Burgers'), ('Hotdogs'), ('Sandwiches'), ('Fries'), ('Add-ons'), ('Drinks');
GO