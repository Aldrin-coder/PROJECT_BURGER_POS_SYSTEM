Imports System.Data.SqlClient

Public Class frm_salesreport4owner

    'mga variable
    Dim isLoading As Boolean = True

    'kapag nagload ang form
    Private Sub frm_salesreport4owner_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        isLoading = True
        dtdt.Value = Date.Today
        isLoading = False
        LoadRecords()
    End Sub

    'pagpalit ng date
    Private Sub dtdt_ValueChanged(sender As Object, e As EventArgs) Handles dtdt.ValueChanged
        If isLoading Then Exit Sub
        LoadRecords()
    End Sub

    'search
    Private Sub txbsearch_TextChanged(sender As Object, e As EventArgs) Handles txbsearch.TextChanged
        If isLoading Then Exit Sub
        LoadRecords()
    End Sub

    'load data sa grid at KPI cards
    Sub LoadRecords()
        datagridviewtransaction.Rows.Clear()

        Dim dailySales As Decimal = 0
        Dim totalTxns As Integer = 0
        Dim monthlySales As Decimal = 0

        Dim localCmd As SqlCommand = Nothing
        Dim localDr As SqlDataReader = Nothing

        Try
            conn()

            ' ─── 1. DAILY DATA ───
            query = "SELECT [current_date], transaction_time, transaction_id, " &
                    "cashier_fullname, payment_method, total_amount, payment_id " &
                    "FROM payments " &
                    "WHERE CAST([current_date] AS DATE) = @SelectedDate "

            If txbsearch.Text.Trim() <> "" Then
                query &= " AND (transaction_id LIKE @Search OR cashier_fullname LIKE @Search) "
            End If
            query &= " ORDER BY transaction_time DESC"

            localCmd = New SqlCommand(query, sqlconn)
            localCmd.Parameters.AddWithValue("@SelectedDate", dtdt.Value.Date)
            If txbsearch.Text.Trim() <> "" Then
                localCmd.Parameters.AddWithValue("@Search", "%" & txbsearch.Text.Trim() & "%")
            End If

            localDr = localCmd.ExecuteReader()
            While localDr.Read()
                Dim dateVal As String = CType(localDr("current_date"), DateTime).ToString("MM/dd/yyyy")
                Dim timeVal As String = DateTime.Today.Add(CType(localDr("transaction_time"), TimeSpan)).ToString("hh:mm tt")

                Dim txnId As String = If(IsDBNull(localDr("transaction_id")), "", localDr("transaction_id").ToString())

                ' ✅ CASHIER NAME — null-safe
                Dim cashier As String = If(IsDBNull(localDr("cashier_fullname")), "(unknown)", localDr("cashier_fullname").ToString().Trim())
                If cashier = "" Then cashier = "(unknown)"

                Dim payment As String = If(IsDBNull(localDr("payment_method")), "", localDr("payment_method").ToString())
                Dim total As Decimal = If(IsDBNull(localDr("total_amount")), 0D, CType(localDr("total_amount"), Decimal))
                Dim id As String = If(IsDBNull(localDr("payment_id")), "", localDr("payment_id").ToString())

                ' Column order must match Designer:
                ' 0=ID 1=TXN ID 2=DATE 3=TIME 4=CASHIER 5=METHOD 6=TOTAL 7=VIEW 8=PRINT 9=DELETE
                datagridviewtransaction.Rows.Add(
                    id,
                    txnId,
                    dateVal,
                    timeVal,
                    cashier,                 ' ← Column5 (index 4)
                    payment,
                    "₱ " & total.ToString("N2"),
                    "👁",
                    "🖨️",
                    "🗑️")

                dailySales += total
                totalTxns += 1
            End While
            localDr.Close() : localDr = Nothing
            localCmd.Dispose() : localCmd = Nothing

            ' ─── 2. MONTHLY DATA ───
            query = "SELECT ISNULL(SUM(total_amount), 0) AS MonthlyTotal " &
                    "FROM payments " &
                    "WHERE MONTH([current_date]) = @Month AND YEAR([current_date]) = @Year"

            localCmd = New SqlCommand(query, sqlconn)
            localCmd.Parameters.AddWithValue("@Month", dtdt.Value.Month)
            localCmd.Parameters.AddWithValue("@Year", dtdt.Value.Year)

            Dim result As Object = localCmd.ExecuteScalar()
            If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                monthlySales = CType(result, Decimal)
            End If
            localCmd.Dispose() : localCmd = Nothing

            ' ─── 3. UPDATE KPI CARDS ───
            lblts1.Text = "₱ " & dailySales.ToString("N2")
            lbltransaction.Text = totalTxns.ToString()
            Guna2HtmlLabel1.Text = "₱ " & monthlySales.ToString("N2")

        Catch ex As Exception
            MessageBox.Show("Error loading records: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If localDr IsNot Nothing AndAlso Not localDr.IsClosed Then localDr.Close()
            If localCmd IsNot Nothing Then localCmd.Dispose()
        End Try
    End Sub

    ' ─────────────────────────────────────────────────────────────
    ' BUTTON HANDLERS
    ' ─────────────────────────────────────────────────────────────
    Private Sub datagridviewtransaction_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridviewtransaction.CellContentClick
        If e.RowIndex < 0 Then Exit Sub

        Dim i As Integer = e.RowIndex
        Dim txnId As String = Convert.ToString(datagridviewtransaction(1, i).Value)
        Dim colName As String = datagridviewtransaction.Columns(e.ColumnIndex).Name

        If colName = "Column8" Then
            OpenReceiptPopup(txnId)
        ElseIf colName = "Column9" Then
            OpenReceiptPopup(txnId)
        ElseIf colName = "Column10" Then
            If MessageBox.Show("Are you sure you want to DELETE transaction " & txnId & "?" & vbCrLf & "This cannot be undone.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                DeleteTransaction(txnId)
            End If
        End If
    End Sub

    'delete ng transaction
    Private Sub DeleteTransaction(txnId As String)
        Dim localCmd As SqlCommand = Nothing
        Dim localTx As SqlTransaction = Nothing

        Try
            conn()
            localTx = sqlconn.BeginTransaction()

            query = "DELETE FROM transaction_records WHERE payment_id = (SELECT payment_id FROM payments WHERE transaction_id = @T)"
            localCmd = New SqlCommand(query, sqlconn, localTx)
            localCmd.Parameters.AddWithValue("@T", txnId)
            localCmd.ExecuteNonQuery()
            localCmd.Dispose() : localCmd = Nothing

            query = "DELETE FROM payments WHERE transaction_id = @T"
            localCmd = New SqlCommand(query, sqlconn, localTx)
            localCmd.Parameters.AddWithValue("@T", txnId)
            localCmd.ExecuteNonQuery()
            localCmd.Dispose() : localCmd = Nothing

            localTx.Commit() : localTx = Nothing

            MessageBox.Show("Transaction " & txnId & " deleted successfully.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadRecords()

        Catch ex As Exception
            Try
                If localTx IsNot Nothing Then localTx.Rollback()
            Catch
            End Try
            MessageBox.Show("Error deleting: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If localCmd IsNot Nothing Then localCmd.Dispose()
            If localTx IsNot Nothing Then localTx.Dispose()
        End Try
    End Sub

    'view o print ng receipt
    Private Sub OpenReceiptPopup(txnId As String)
        Dim localCmd As SqlCommand = Nothing
        Dim localDr As SqlDataReader = Nothing

        Try
            conn()

            query = "SELECT * FROM payments WHERE transaction_id = @T"
            localCmd = New SqlCommand(query, sqlconn)
            localCmd.Parameters.AddWithValue("@T", txnId)
            localDr = localCmd.ExecuteReader()

            If Not localDr.Read() Then
                localDr.Close() : localCmd.Dispose()
                MessageBox.Show("Transaction not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Exit Sub
            End If

            Dim orderId As String = If(IsDBNull(localDr("order_id")), "", localDr("order_id").ToString())
            Dim cashier As String = If(IsDBNull(localDr("cashier_fullname")), "(unknown)", localDr("cashier_fullname").ToString())
            Dim method As String = If(IsDBNull(localDr("payment_method")), "", localDr("payment_method").ToString())
            Dim subtotal As Decimal = If(IsDBNull(localDr("subtotal_amount")), 0D, CType(localDr("subtotal_amount"), Decimal))
            Dim total As Decimal = If(IsDBNull(localDr("total_amount")), 0D, CType(localDr("total_amount"), Decimal))
            Dim paid As Decimal = If(IsDBNull(localDr("amount_paid")), 0D, CType(localDr("amount_paid"), Decimal))
            Dim change As Decimal = If(IsDBNull(localDr("change_amount")), 0D, CType(localDr("change_amount"), Decimal))
            Dim tax As Decimal = total - subtotal
            Dim pid As Integer = CType(localDr("payment_id"), Integer)
            localDr.Close() : localDr = Nothing
            localCmd.Dispose() : localCmd = Nothing

            query = "SELECT product_name, product_price, product_quantity FROM transaction_records WHERE payment_id = @P"
            localCmd = New SqlCommand(query, sqlconn)
            localCmd.Parameters.AddWithValue("@P", pid)
            localDr = localCmd.ExecuteReader()

            Dim items As New List(Of frm_receipt.ReceiptItem)
            While localDr.Read()
                Dim q As Integer = CType(localDr("product_quantity"), Integer)
                Dim p As Decimal = CType(localDr("product_price"), Decimal)
                items.Add(New frm_receipt.ReceiptItem With {
                    .Name = localDr("product_name").ToString(),
                    .Qty = q,
                    .UnitPrice = p,
                    .Subtotal = p * q
                })
            End While
            localDr.Close() : localDr = Nothing
            localCmd.Dispose() : localCmd = Nothing

            Dim rpt As New frm_receipt()
            rpt.ShowReceipt(txnId, orderId, cashier, method, subtotal, tax, total, paid, change, items)

        Catch ex As Exception
            MessageBox.Show("Error opening receipt: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If localDr IsNot Nothing AndAlso Not localDr.IsClosed Then localDr.Close()
            If localCmd IsNot Nothing Then localCmd.Dispose()
        End Try
    End Sub

End Class