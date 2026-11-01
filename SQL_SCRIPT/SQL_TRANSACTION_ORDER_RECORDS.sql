-- =========================================================================
-- SCRIPT 3: SEPARATED SYSTEM (Payments + Transaction Records)
-- =========================================================================

-- Drop existing objects to prevent errors when re-running
IF OBJECT_ID('dbo.sp_AutoNumberTxn', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_AutoNumberTxn;
IF OBJECT_ID('dbo.sp_AutoNumberOrder', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_AutoNumberOrder;
IF OBJECT_ID('dbo.transaction_records', 'U') IS NOT NULL DROP TABLE dbo.transaction_records;
IF OBJECT_ID('dbo.payments', 'U') IS NOT NULL DROP TABLE dbo.payments;
GO

-- -------------------------------------------------------------------------
-- PART A: CREATE TABLES
-- -------------------------------------------------------------------------

-- 1. PAYMENTS TABLE (The Receipt Header)
CREATE TABLE payments (
    payment_id          INT IDENTITY(1,1) PRIMARY KEY, 
    transaction_id      VARCHAR(20)     NOT NULL,      -- e.g., TXN-1000
    order_id            VARCHAR(20)     NOT NULL,      -- e.g., ORDER-1000
    cashier_id          VARCHAR(20)     NOT NULL,      
    current_date        DATE            NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    transaction_time    TIME            NOT NULL DEFAULT CAST(GETDATE() AS TIME),
    cashier_fullname    VARCHAR(100)    NOT NULL,      
    payment_method      VARCHAR(50)     NOT NULL,
    subtotal_amount     DECIMAL(10,2)   NOT NULL,
    tax_rate            DECIMAL(5,4)    NOT NULL DEFAULT 0.0200,
    total_amount        AS CAST(subtotal_amount * (1 + tax_rate) AS DECIMAL(10,2)) PERSISTED,
    amount_paid         DECIMAL(10,2)   NOT NULL,
    change_amount       AS CAST(amount_paid - (subtotal_amount * (1 + tax_rate)) AS DECIMAL(10,2)) PERSISTED,
    CONSTRAINT chk_payment_method CHECK (payment_method IN ('CASH','GCASH','CARD',))
);
GO

-- 2. TRANSACTION_RECORDS TABLE (The Items List)
CREATE TABLE transaction_records (
    id                  INT IDENTITY(1,1) PRIMARY KEY, 
    transaction_id      VARCHAR(20)     NOT NULL,      -- Links to payments table
    product_name        VARCHAR(100)    NOT NULL,
    product_price       DECIMAL(10,2)   NOT NULL,
    product_quantity    INT             NOT NULL,
    CONSTRAINT fk_payment FOREIGN KEY (transaction_id) REFERENCES payments(transaction_id) ON DELETE CASCADE
);
GO

-- -------------------------------------------------------------------------
-- PART B: CREATE AUTO-NUMBER PROCEDURES
-- -------------------------------------------------------------------------

-- 1. AutoNumber for TXN
CREATE PROCEDURE sp_AutoNumberTxn
    @NewNumber VARCHAR(20) OUTPUT
AS
BEGIN
    DECLARE @lastNum VARCHAR(20), @intNum INT, @newNumStr VARCHAR(20);
    DECLARE @pfx VARCHAR(10) = 'TXN-'; 
    
    SELECT TOP 1 @lastNum = NewNumber FROM AutoNumberUsers WHERE pfx = @pfx ORDER BY NewNumber DESC;
    
    IF @lastNum IS NULL
    BEGIN
        SET @intNum = 1000;
        SET @newNumStr = @pfx + CAST(@intNum AS VARCHAR);
        INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES (@pfx, @newNumStr);
    END
    ELSE
    BEGIN
        SET @intNum = CAST(SUBSTRING(@lastNum, LEN(@pfx) + 1, LEN(@lastNum)) AS INT) + 1;
        SET @newNumStr = @pfx + CAST(@intNum AS VARCHAR);
        INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES (@pfx, @newNumStr);
    END
    SET @NewNumber = @newNumStr;
END
GO

-- 2. AutoNumber for ORDER
CREATE PROCEDURE sp_AutoNumberOrder
    @NewNumber VARCHAR(20) OUTPUT
AS
BEGIN
    DECLARE @lastNum VARCHAR(20), @intNum INT, @newNumStr VARCHAR(20);
    DECLARE @pfx VARCHAR(10) = 'ORDER-'; 
    
    SELECT TOP 1 @lastNum = NewNumber FROM AutoNumberUsers WHERE pfx = @pfx ORDER BY NewNumber DESC;
    
    IF @lastNum IS NULL
    BEGIN
        SET @intNum = 1000;
        SET @newNumStr = @pfx + CAST(@intNum AS VARCHAR);
        INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES (@pfx, @newNumStr);
    END
    ELSE
    BEGIN
        SET @intNum = CAST(SUBSTRING(@lastNum, LEN(@pfx) + 1, LEN(@lastNum)) AS INT) + 1;
        SET @newNumStr = @pfx + CAST(@intNum AS VARCHAR);
        INSERT INTO AutoNumberUsers (pfx, NewNumber) VALUES (@pfx, @newNumStr);
    END
    SET @NewNumber = @newNumStr;
END
GO