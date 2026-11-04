-- =========================================================================
-- SCRIPT 3: FULL SYSTEM (Separated AutoNumbers + Payments + Transactions)
-- =========================================================================

-- Drop existing objects to prevent errors when re-running
IF OBJECT_ID('dbo.sp_AutoNumberTxn', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_AutoNumberTxn;
IF OBJECT_ID('dbo.sp_AutoNumberOrder', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_AutoNumberOrder;
IF OBJECT_ID('dbo.transaction_records', 'U') IS NOT NULL DROP TABLE dbo.transaction_records;
IF OBJECT_ID('dbo.payments', 'U') IS NOT NULL DROP TABLE dbo.payments;
IF OBJECT_ID('dbo.AutoNumberTxn', 'U') IS NOT NULL DROP TABLE dbo.AutoNumberTxn;
IF OBJECT_ID('dbo.AutoNumberOrder', 'U') IS NOT NULL DROP TABLE dbo.AutoNumberOrder;
GO

-- -------------------------------------------------------------------------
-- PART A: CREATE TABLES
-- -------------------------------------------------------------------------

-- 1. SEPARATE AUTO NUMBER FOR TXN
CREATE TABLE AutoNumberTxn (
    NewNumber VARCHAR(20) PRIMARY KEY
);
GO

-- 2. SEPARATE AUTO NUMBER FOR ORDER
CREATE TABLE AutoNumberOrder (
    NewNumber VARCHAR(20) PRIMARY KEY
);
GO

-- 3. PAYMENTS TABLE (The Receipt Header)
CREATE TABLE payments (
    payment_id          INT IDENTITY(1,1) PRIMARY KEY, 
    transaction_id      VARCHAR(20)     NOT NULL,      
    order_id            VARCHAR(20)     NOT NULL,      
    cashier_id          VARCHAR(20)     NOT NULL,      
    [current_date]      DATE            NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    transaction_time    TIME            NOT NULL DEFAULT CAST(GETDATE() AS TIME),
    cashier_fullname    VARCHAR(100)    NOT NULL,      
    payment_method      VARCHAR(50)     NOT NULL,
    subtotal_amount     DECIMAL(10,2)   NOT NULL,
    tax_rate            DECIMAL(5,4)    NOT NULL DEFAULT 0.0200,
    total_amount        AS CAST(subtotal_amount * (1 + tax_rate) AS DECIMAL(10,2)) PERSISTED,
    amount_paid         DECIMAL(10,2)   NOT NULL,
    change_amount       AS CAST(amount_paid - (subtotal_amount * (1 + tax_rate)) AS DECIMAL(10,2)) PERSISTED,
    CONSTRAINT chk_payment_method CHECK (payment_method IN ('CASH','GCASH','MAYA'))
);
GO

-- 4. TRANSACTION_RECORDS TABLE (The Items List)
CREATE TABLE transaction_records (
    id                  INT IDENTITY(1,1) PRIMARY KEY, 
    payment_id          INT             NOT NULL,      
    product_name        VARCHAR(100)    NOT NULL,
    product_price       DECIMAL(10,2)   NOT NULL,
    product_quantity    INT             NOT NULL,
    CONSTRAINT fk_payment FOREIGN KEY (payment_id) REFERENCES payments(payment_id) ON DELETE CASCADE
);
GO

-- -------------------------------------------------------------------------
-- PART B: CREATE AUTO-NUMBER PROCEDURES
-- -------------------------------------------------------------------------

-- 1. AutoNumber for TXN (Starts at TXN-1000, then TXN-1001, etc.)
CREATE PROCEDURE sp_AutoNumberTxn
    @NewNumber VARCHAR(20) OUTPUT
AS
BEGIN
    DECLARE @lastNum VARCHAR(20), @intNum INT, @newNumStr VARCHAR(20);
    DECLARE @pfx VARCHAR(10) = 'TXN-'; 
    
    SELECT TOP 1 @lastNum = NewNumber FROM AutoNumberTxn ORDER BY NewNumber DESC;
    
    IF @lastNum IS NULL
    BEGIN
        SET @intNum = 1000; -- 👈 STARTS AT TXN-1000
        SET @newNumStr = @pfx + CAST(@intNum AS VARCHAR);
        INSERT INTO AutoNumberTxn (NewNumber) VALUES (@newNumStr);
    END
    ELSE
    BEGIN
        -- Extracts the number from the last ID (e.g. gets 1000 from TXN-1000), adds 1 (1001)
        SET @intNum = CAST(SUBSTRING(@lastNum, LEN(@pfx) + 1, LEN(@lastNum)) AS INT) + 1;
        SET @newNumStr = @pfx + CAST(@intNum AS VARCHAR);
        INSERT INTO AutoNumberTxn (NewNumber) VALUES (@newNumStr);
    END
    SET @NewNumber = @newNumStr;
END
GO

-- 2. AutoNumber for ORDER (Starts at ORDER-001, then ORDER-002, etc.)
CREATE PROCEDURE sp_AutoNumberOrder
    @NewNumber VARCHAR(20) OUTPUT
AS
BEGIN
    DECLARE @lastNum VARCHAR(20), @intNum INT, @newNumStr VARCHAR(20);
    DECLARE @pfx VARCHAR(10) = 'ORDER-'; 
    
    SELECT TOP 1 @lastNum = NewNumber FROM AutoNumberOrder ORDER BY NewNumber DESC;
    
    IF @lastNum IS NULL
    BEGIN
        SET @intNum = 1; -- 👈 STARTS AT ORDER-001
        SET @newNumStr = @pfx + RIGHT('000' + CAST(@intNum AS VARCHAR), 3);
        INSERT INTO AutoNumberOrder (NewNumber) VALUES (@newNumStr);
    END
    ELSE
    BEGIN
        SET @intNum = CAST(SUBSTRING(@lastNum, LEN(@pfx) + 1, LEN(@lastNum)) AS INT) + 1;
        SET @newNumStr = @pfx + RIGHT('000' + CAST(@intNum AS VARCHAR), 3);
        INSERT INTO AutoNumberOrder (NewNumber) VALUES (@newNumStr);
    END
    SET @NewNumber = @newNumStr;
END
GO

-- -------------------------------------------------------------------------
-- PART C: INSERT TEST DATA (To test if it works)
-- -------------------------------------------------------------------------
DECLARE @TestTxn VARCHAR(20), @TestOrder VARCHAR(20), @TestPaymentID INT;

-- Get new IDs (Should be TXN-1000 and ORDER-001 on first run)
EXEC sp_AutoNumberTxn @TestTxn OUTPUT;
EXEC sp_AutoNumberOrder @TestOrder OUTPUT;

-- Insert the Payment Header
INSERT INTO payments (transaction_id, order_id, cashier_id, cashier_fullname, payment_method, subtotal_amount, amount_paid)
OUTPUT INSERTED.payment_id INTO @TestPaymentID
VALUES (@TestTxn, @TestOrder, 'C001', 'JUAN DELA CRUZ', 'CASH', 200.00, 250.00);

-- Insert the Items
INSERT INTO transaction_records (payment_id, product_name, product_price, product_quantity)
VALUES 
(@TestPaymentID, 'PAPA BEEF BURGER', 50.00, 2),
(@TestPaymentID, 'COKE ROYAL', 25.00, 4);

PRINT 'Test data inserted successfully!';
PRINT 'Transaction ID: ' + @TestTxn;
PRINT 'Order ID: ' + @TestOrder;
GO

-- -------------------------------------------------------------------------
-- PART D: JOIN (View the Receipt)
-- -------------------------------------------------------------------------
SELECT 
    p.transaction_id,
    p.order_id,
    p.[current_date],
    p.transaction_time,
    p.cashier_fullname,
    p.payment_method,
    t.product_name,
    t.product_price,
    t.product_quantity,
    (t.product_price * t.product_quantity) AS [Line Total],
    p.subtotal_amount,
    p.total_amount,
    p.amount_paid,
    p.change_amount
FROM payments p
INNER JOIN transaction_records t ON p.payment_id = t.payment_id
ORDER BY p.payment_id DESC;
GO

-- -------------------------------------------------------------------------
-- PART E: RESET / DELETE SCRIPT (For Demo Day)
-- -------------------------------------------------------------------------
-- Note: You MUST delete the child table (transaction_records) first!
DELETE FROM transaction_records;
DELETE FROM payments;

-- Reset the Identity IDs
DBCC CHECKIDENT ('transaction_records', RESEED, 0);
DBCC CHECKIDENT ('payments', RESEED, 0);

-- Clear the separated AutoNumber tables
DELETE FROM AutoNumberTxn;
DELETE FROM AutoNumberOrder;
GO