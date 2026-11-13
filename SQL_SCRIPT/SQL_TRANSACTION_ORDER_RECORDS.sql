USE DB_DARKSYSTEM
GO

-- Drop if exists to avoid errors
IF OBJECT_ID('dbo.sp_AutoNumberTxn', 'P')    IS NOT NULL DROP PROCEDURE dbo.sp_AutoNumberTxn;
IF OBJECT_ID('dbo.transaction_records', 'U') IS NOT NULL DROP TABLE dbo.transaction_records;
IF OBJECT_ID('dbo.payments', 'U')            IS NOT NULL DROP TABLE dbo.payments;
IF OBJECT_ID('dbo.AutoNumberTxn', 'U')       IS NOT NULL DROP TABLE dbo.AutoNumberTxn;
GO

-- ============================================================
-- 1. CREATE AutoNumberTxn TABLE
-- ============================================================
CREATE TABLE [dbo].[AutoNumberTxn](
    [RecNo]     [int] IDENTITY(1,1) NOT NULL,
    [Pfx]       [varchar](10) NULL,
    [NewNumber] [varchar](20) NULL,
    CONSTRAINT [PK_AutoNumberTxn] PRIMARY KEY CLUSTERED ([RecNo] ASC)
)
GO

-- ============================================================
-- 2. CREATE sp_AutoNumberTxn PROCEDURE
-- ============================================================
CREATE PROCEDURE [dbo].[sp_AutoNumberTxn]
    @pfx       varchar(10) = '',
    @NewNumber varchar(20) = '' OUTPUT
AS
BEGIN
    Declare @MaxCode      As varchar(20)
    Declare @CreateNumber As int
    Declare @RecNo        As int

    -- Default 'TXN-000000000' => first generated number is TXN-000000001
    select @MaxCode = case when max(NewNumber) is null then 'TXN-000000000'
                           else max(NewNumber) end
    from AutoNumberTxn
    where left(NewNumber, 4) = @pfx

    set @CreateNumber = cast(right(@maxCode, 9) as int) + 1

    -- Generate the new number (padded to 13 chars, e.g. TXN-000000001)
    Select @NewNumber = @pfx + Replicate('0', 13 - Len(@CreateNumber) - 4) + Cast(@CreateNumber as varchar)

    insert into AutoNumberTxn(Pfx, NewNumber) Values
    (@Pfx, @NewNumber)

    Select @RecNo = @@Identity
    Select NewNumber from AutoNumberTxn where RecNo = @RecNo
END
GO

-- ============================================================
-- 3. CREATE payments TABLE (Receipt Header)
-- ============================================================
CREATE TABLE payments (
    payment_id          INT IDENTITY(1,1) PRIMARY KEY,
    transaction_id      VARCHAR(20)     NOT NULL,
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

-- ============================================================
-- 4. CREATE transaction_records TABLE (Items List)
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
-- 5. INSERT TEST DATA
-- ============================================================
DECLARE @TestTxn       varchar(20);
DECLARE @TestPaymentID INT;

-- Get new TXN ID (Should be TXN-000000001 on first run)
EXEC [dbo].[sp_AutoNumberTxn] @pfx = 'TXN-', @NewNumber = @TestTxn OUTPUT;

-- Insert the Payment Header
INSERT INTO payments
    (transaction_id, cashier_id, cashier_fullname, payment_method, subtotal_amount, amount_paid)
VALUES
    (@TestTxn, 'DSBS0000002', 'CASHIER ACCOUNT', 'CASH', 200.00, 250.00);

-- Capture the new payment_id
SET @TestPaymentID = SCOPE_IDENTITY();

-- Insert the Items
INSERT INTO transaction_records (payment_id, product_name, product_price, product_quantity)
VALUES
    (@TestPaymentID, 'PAPA BEEF BURGER', 50.00, 2),
    (@TestPaymentID, 'COKE ROYAL',       25.00, 4);

PRINT 'Test data inserted successfully!';
PRINT 'Transaction ID: ' + @TestTxn;
PRINT 'Payment ID:     ' + CAST(@TestPaymentID AS VARCHAR(10));
GO

-- ============================================================
-- 6. VIEW DATA
-- ============================================================
SELECT
    p.payment_id,
    p.transaction_id,
    p.[current_date],
    p.transaction_time,
    p.cashier_id,
    p.cashier_fullname,
    p.payment_method,
    t.product_name,
    t.product_price,
    t.product_quantity,
    p.subtotal_amount,
    p.total_amount,
    p.amount_paid,
    p.change_amount
FROM payments p
INNER JOIN transaction_records t ON p.payment_id = t.payment_id
ORDER BY p.payment_id DESC;
GO

SELECT payment_id, transaction_id, cashier_id, cashier_fullname FROM payments

-- ============================================================
-- 7. RESEED IDENTITIES (Demo Reset)
-- ============================================================
-- ⚠️ I COMMENTED OUT THE DELETE LINES SO YOUR TEST DATA REMAINS
-- Uncomment them only if you want to wipe the tables.
-- ============================================================

 DELETE FROM dbo.transaction_records;
 DELETE FROM dbo.payments;
 DELETE FROM dbo.AutoNumberTxn;
-- GO

-- DBCC CHECKIDENT ('dbo.transaction_records', RESEED, 0);
-- DBCC CHECKIDENT ('dbo.payments',           RESEED, 0);
-- GO

-- Verify
SELECT
    'transaction_records' AS TableName,
    IDENT_CURRENT('dbo.transaction_records') AS CurrentIdentity,
    IDENT_SEED('dbo.transaction_records')    AS SeedValue,
    IDENT_INCR('dbo.transaction_records')    AS IncrementValue
UNION ALL
SELECT
    'payments' AS TableName,
    IDENT_CURRENT('dbo.payments') AS CurrentIdentity,
    IDENT_SEED('dbo.payments')    AS SeedValue,
    IDENT_INCR('dbo.payments')    AS IncrementValue;
GO
SELECT payment_id, transaction_id, cashier_id, cashier_fullname FROM payments ORDER BY payment_id DESC;