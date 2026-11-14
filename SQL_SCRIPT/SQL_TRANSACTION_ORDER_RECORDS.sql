USE DB_DARKSYSTEM
GO

-- ============================================================
-- DROP (FK-safe order)
-- ============================================================
IF OBJECT_ID('dbo.sp_AutoNumberTxn', 'P')    IS NOT NULL DROP PROCEDURE dbo.sp_AutoNumberTxn;
IF OBJECT_ID('dbo.transaction_records', 'U') IS NOT NULL DROP TABLE dbo.transaction_records;
IF OBJECT_ID('dbo.payments', 'U')            IS NOT NULL DROP TABLE dbo.payments;
IF OBJECT_ID('dbo.AutoNumberTxn', 'U')       IS NOT NULL DROP TABLE dbo.AutoNumberTxn;
GO

-- ============================================================
-- 1. AutoNumberTxn
-- ============================================================
CREATE TABLE [dbo].[AutoNumberTxn](
    [RecNo]     [int] IDENTITY(1,1) NOT NULL,
    [Pfx]       [varchar](10) NULL,
    [NewNumber] [varchar](20) NULL,
    CONSTRAINT [PK_AutoNumberTxn] PRIMARY KEY CLUSTERED ([RecNo] ASC)
)
GO

-- ============================================================
-- 2. sp_AutoNumberTxn
-- ============================================================
CREATE PROCEDURE [dbo].[sp_AutoNumberTxn]
    @pfx       varchar(10) = '',
    @NewNumber varchar(20) = '' OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MaxCode      varchar(30)
    DECLARE @CreateNumber int

    SELECT @MaxCode = CASE WHEN MAX(NewNumber) IS NULL THEN @pfx + '000000000'
                           ELSE MAX(NewNumber) END
    FROM AutoNumberTxn
    WHERE LEFT(NewNumber, LEN(@pfx)) = @pfx

    SET @CreateNumber = CAST(RIGHT(@MaxCode, 9) AS int) + 1

    SELECT @NewNumber = @pfx + REPLICATE('0', 13 - LEN(@CreateNumber) - LEN(@pfx))
                              + CAST(@CreateNumber AS varchar)

    INSERT INTO AutoNumberTxn(Pfx, NewNumber) VALUES (@pfx, @NewNumber)
END
GO

-- ============================================================
-- 3. payments — current_date is a REAL DATE column (fixes GROUP BY)
-- ============================================================
CREATE TABLE payments (
    payment_id          INT IDENTITY(1,1) PRIMARY KEY,
    transaction_id      VARCHAR(20)     NOT NULL,
    cashier_id          VARCHAR(20)     NOT NULL,
    [current_date]      DATE            NOT NULL CONSTRAINT DF_payments_current_date DEFAULT (CONVERT(date, GETDATE())),
    transaction_time    TIME            NOT NULL CONSTRAINT DF_payments_transaction_time DEFAULT (CAST(GETDATE() AS TIME)),
    cashier_fullname    VARCHAR(100)    NOT NULL,
    payment_method      VARCHAR(50)     NOT NULL,
    subtotal_amount     DECIMAL(10,2)   NOT NULL,
    tax_rate            DECIMAL(5,4)    NOT NULL CONSTRAINT DF_payments_tax_rate DEFAULT (0.0200),
    total_amount        AS CAST(subtotal_amount * (1 + tax_rate) AS DECIMAL(10,2)) PERSISTED,
    amount_paid         DECIMAL(10,2)   NOT NULL,
    change_amount       AS CAST(amount_paid - (subtotal_amount * (1 + tax_rate)) AS DECIMAL(10,2)) PERSISTED,
    CONSTRAINT chk_payment_method CHECK (payment_method IN ('CASH','GCASH','MAYA'))
);
GO

-- ============================================================
-- 4. transaction_records
-- ============================================================
CREATE TABLE transaction_records (
    id                  INT IDENTITY(1,1) PRIMARY KEY,
    payment_id          INT             NOT NULL,
    product_name        VARCHAR(100)    NOT NULL,
    product_price       DECIMAL(10,2)   NOT NULL,
    product_quantity    INT             NOT NULL,
    CONSTRAINT fk_payment FOREIGN KEY (payment_id)
        REFERENCES payments(payment_id) ON DELETE CASCADE
);
GO

-- ============================================================
-- 5. Insert test data
-- ============================================================
DECLARE @TestTxn       varchar(20);
DECLARE @TestPaymentID INT;

EXEC [dbo].[sp_AutoNumberTxn] @pfx = 'TXN-', @NewNumber = @TestTxn OUTPUT;

INSERT INTO payments
    (transaction_id, cashier_id, cashier_fullname, payment_method, subtotal_amount, amount_paid)
VALUES
    (@TestTxn, 'DSBS0000002', 'CASHIER ACCOUNT', 'CASH', 200.00, 250.00);

SET @TestPaymentID = SCOPE_IDENTITY();

INSERT INTO transaction_records (payment_id, product_name, product_price, product_quantity)
VALUES
    (@TestPaymentID, 'PAPA BEEF BURGER', 50.00, 2),
    (@TestPaymentID, 'COKE ROYAL',       25.00, 4);

PRINT 'Test data inserted successfully!';
PRINT 'Transaction ID: ' + @TestTxn;
PRINT 'Payment ID:     ' + CAST(@TestPaymentID AS VARCHAR(10));
GO

-- ============================================================
-- 6. Verify data
-- ============================================================
SELECT
    p.payment_id, p.transaction_id, p.[current_date], p.transaction_time,
    p.cashier_id, p.cashier_fullname, p.payment_method,
    t.product_name, t.product_price, t.product_quantity,
    p.subtotal_amount, p.total_amount, p.amount_paid, p.change_amount
FROM payments p
INNER JOIN transaction_records t ON p.payment_id = t.payment_id
ORDER BY p.payment_id DESC;
GO

-- ============================================================
-- 7. VERIFY FIX — is_computed MUST be 0
-- ============================================================
SELECT c.name, t.name AS data_type, c.is_computed
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('payments') AND c.name = 'current_date';
GO

-- ============================================================
-- 8. VERIFY FIX — the exact query from readMonthlySales1() must run
-- ============================================================
SELECT DATEPART(MONTH, current_date) AS mth,
       ISNULL(SUM(subtotal_amount * 1.02), 0) AS amt
FROM payments
WHERE current_date >= '2026-01-01' AND current_date < '2027-01-01'
GROUP BY DATEPART(MONTH, current_date);
GO

-- ============================================================
-- 9. DEMO RESET — uncomment ALL lines below ONLY to wipe data.
--    (Do NOT uncomment if you want to keep test data.)
-- ============================================================

DELETE FROM dbo.transaction_records;
DELETE FROM dbo.payments;
DELETE FROM dbo.AutoNumberTxn;
DBCC CHECKIDENT ('dbo.transaction_records', RESEED, 0);
DBCC CHECKIDENT ('dbo.payments',           RESEED, 0);

GO

USE DB_DARKSYSTEM
GO

-- ============================================================
-- INSERT TEST DATA FOR ALL 12 MONTHS
-- ============================================================
DECLARE @CurrentYear INT = YEAR(GETDATE());
DECLARE @MonthLoop INT = 1;
DECLARE @DayLoop INT;

-- Create a temporary loop to generate data for each month
WHILE @MonthLoop <= 12
BEGIN
    -- Create 3 transactions for each month (adjust the number to add more data)
    SET @DayLoop = 1;
    WHILE @DayLoop <= 3
    BEGIN
        DECLARE @TestTxn VARCHAR(20);
        DECLARE @TestPaymentID INT;
        DECLARE @TestDate DATE = DATEFROMPARTS(@CurrentYear, @MonthLoop, (@DayLoop * 5));

        -- Generate a new transaction ID
        EXEC [dbo].[sp_AutoNumberTxn] @pfx = 'TXN-', @NewNumber = @TestTxn OUTPUT;

        -- Insert the payment record with the specific month date
        INSERT INTO payments
            (transaction_id, cashier_id, [current_date], transaction_time, cashier_fullname, payment_method, subtotal_amount, amount_paid)
        VALUES
            (
                @TestTxn, 
                'DSBS0000002', 
                @TestDate, -- Force the date to the specific month
                CAST('12:00:00' AS TIME), 
                'CASHIER ACCOUNT', 
                CASE WHEN @DayLoop = 1 THEN 'CASH' 
                     WHEN @DayLoop = 2 THEN 'GCASH' 
                     ELSE 'MAYA' END, 
                -- Generate random subtotal between 100 and 500
                (ABS(CHECKSUM(NEWID())) % 400) + 100.00, 
                -- Amount paid is just higher than the subtotal
                ((ABS(CHECKSUM(NEWID())) % 400) + 100.00) + 50.00
            );

        SET @TestPaymentID = SCOPE_IDENTITY();

        -- Insert 2 items for this payment
        INSERT INTO transaction_records (payment_id, product_name, product_price, product_quantity)
        VALUES
            (@TestPaymentID, 'PAPA BEEF BURGER', 50.00, 2),
            (@TestPaymentID, 'COKE ROYAL',       25.00, 4);

        SET @DayLoop = @DayLoop + 1;
    END
    SET @MonthLoop = @MonthLoop + 1;
END

PRINT 'Successfully inserted 36 test transactions across all 12 months of ' + CAST(@CurrentYear AS VARCHAR(4)) + '!';
GO

-- ============================================================
-- VERIFY THE DATA 
-- ============================================================
SELECT 
    MONTH([current_date]) AS [Month],
    DATENAME(MONTH, [current_date]) AS [Month Name],
    COUNT(payment_id) AS [Total Transactions],
    SUM(subtotal_amount) AS [Total Subtotal],
    SUM(total_amount) AS [Total With Tax]
FROM payments
WHERE YEAR([current_date]) = YEAR(GETDATE())
GROUP BY MONTH([current_date]), DATENAME(MONTH, [current_date])
ORDER BY MONTH([current_date]);
GO