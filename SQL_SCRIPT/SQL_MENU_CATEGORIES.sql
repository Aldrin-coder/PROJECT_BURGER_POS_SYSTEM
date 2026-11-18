SET NOCOUNT ON;
USE DB_DARKSYSTEM
GO


IF OBJECT_ID('dbo.SP_AutoNumberMenu', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_AutoNumberMenu;
IF OBJECT_ID('dbo.menu_items',        'U') IS NOT NULL DROP TABLE dbo.menu_items;
IF OBJECT_ID('dbo.categories',        'U') IS NOT NULL DROP TABLE dbo.categories;
IF OBJECT_ID('dbo.AutoNumberMenu',    'U') IS NOT NULL DROP TABLE dbo.AutoNumberMenu;
GO


CREATE TABLE [dbo].[AutoNumberMenu](
    [RecNo]     [int] IDENTITY(1,1) PRIMARY KEY,
    [Pfx]       [varchar](10) NULL,
    [NewNumber] [varchar](20) NULL
);
GO

CREATE PROCEDURE [dbo].[SP_AutoNumberMenu]
    @pfx       varchar(10) = '',
    @NewNumber varchar(20) = '' OUTPUT
AS
BEGIN
    Declare @MaxCode As varchar(20)
    Declare @CreateNumber As int
    Declare @RecNo As int

    select @MaxCode = case when max(NewNumber) is null then 'M000000000'
                           else max(NewNumber) end
    from AutoNumberMenu
    where left(NewNumber, 1) = @pfx 

    set @CreateNumber = cast(right(@maxCode, 9) as int) + 1

    Select @NewNumber = @pfx + Replicate('0', 10 - Len(@CreateNumber) - 1) + Cast(@CreateNumber as varchar)

    insert into AutoNumberMenu(Pfx, NewNumber) Values
    (@Pfx, @NewNumber)

    Select @RecNo = @@Identity
    Select NewNumber from AutoNumberMenu where RecNo = @RecNo
END
GO

CREATE TABLE categories (
    category_id   INT IDENTITY(1,1) PRIMARY KEY,
    category_name VARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE menu_items (
    item_id       INT IDENTITY(1,1) PRIMARY KEY,
    item_code     VARCHAR(20) UNIQUE NOT NULL,
    product_name  VARCHAR(100) NOT NULL,
    price         DECIMAL(10, 2) NOT NULL,
    category_id   INT NOT NULL,
    quantity      INT NOT NULL DEFAULT 0,
    product_image IMAGE,
    IsActive      BIT DEFAULT 1,
    FOREIGN KEY (category_id) REFERENCES categories(category_id)
);
GO


INSERT INTO categories (category_name) VALUES
('Burgers'), ('Hotdogs'), ('Sandwiches'), ('Fries'), ('Add-ons'), ('Drinks');
GO


-- PART B: SELECT (VIEW DATA)
SELECT
    m.item_id, m.item_code, m.product_name, m.price,
    m.quantity, m.IsActive, m.product_image,
    c.category_id, c.category_name
FROM menu_items m
INNER JOIN categories c ON m.category_id = c.category_id
ORDER BY m.item_id;

SELECT * FROM categories ORDER BY category_id;

SELECT RecNo, Pfx, NewNumber FROM AutoNumberMenu ORDER BY RecNo;
GO

-- PART C: RESET / RESEED SCRIPT (MENU-SIDE ONLY)
GO
DELETE FROM menu_items;
DELETE FROM categories;
DELETE FROM AutoNumberMenu;

DBCC CHECKIDENT ('menu_items',     RESEED, 0);
DBCC CHECKIDENT ('categories',     RESEED, 0);
DBCC CHECKIDENT ('AutoNumberMenu', RESEED, 0);

INSERT INTO categories (category_name) VALUES
('Burgers'), ('Hotdogs'), ('Sandwiches'), ('Fries'), ('Add-ons'), ('Drinks');

DECLARE @ResetItemCode VARCHAR(20);
EXEC [dbo].[SP_AutoNumberMenu] 'M', @ResetItemCode OUTPUT;

IF @ResetItemCode IS NULL
BEGIN
    RAISERROR('Reset failed: SP_AutoNumberMenu did not return a code.', 16, 1);
    RETURN;
END

INSERT INTO menu_items (item_code, product_name, price, category_id, quantity, product_image, IsActive)
VALUES (@ResetItemCode, 'Classic Burger', 50.00, 1, 100, 'C:\Images\burger.png', 1);
GO