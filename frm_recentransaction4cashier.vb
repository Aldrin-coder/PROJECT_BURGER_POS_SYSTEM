Imports System.Data.SqlClient

Public Class frm_recentransaction4cashier

    'mga variable
    Dim isLoading As Boolean = True

    'kapag nagload ang form
    Private Sub frm_recentransaction4cashier_Load(sender As Object, e As EventArgs) Handles MyBase.Load
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

    'load data sa grid — ONLY the logged-in cashier's transactions
    Sub LoadRecords()
        datagridviewtransaction.Rows.Clear()

        Dim localCmd As SqlCommand = Nothing
        Dim localDr As SqlDataReader = Nothing

        Try
            conn()

            query = "SELECT [current_date], transaction_time, transaction_id, " &
                    "cashier_fullname, payment_method, total_amount, payment_id " &
                    "FROM payments " &
                    "WHERE CAST([current_date] AS DATE) = @SelectedDate " &
                    "AND cashier_fullname = @MyName "

            If txbsearch.Text.Trim() <> "" Then
                query &= " AND transaction_id LIKE @Search "
            End If
            query &= " ORDER BY transaction_time DESC"

            localCmd = New SqlCommand(query, sqlconn)
            localCmd.Parameters.AddWithValue("@SelectedDate", dtdt.Value.Date)
            localCmd.Parameters.AddWithValue("@MyName", CurrentUser.fullname)
            If txbsearch.Text.Trim() <> "" Then
                localCmd.Parameters.AddWithValue("@Search", "%" & txbsearch.Text.Trim() & "%")
            End If

            localDr = localCmd.ExecuteReader()
            While localDr.Read()
                Dim dateVal As String = CType(localDr("current_date"), DateTime).ToString("MM/dd/yyyy")
                Dim timeVal As String = DateTime.Today.Add(CType(localDr("transaction_time"), TimeSpan)).ToString("hh:mm tt")

                Dim txnId As String = If(IsDBNull(localDr("transaction_id")), "", localDr("transaction_id").ToString())
                Dim cashier As String = If(IsDBNull(localDr("cashier_fullname")), "(unknown)", localDr("cashier_fullname").ToString().Trim())
                If cashier = "" Then cashier = "(unknown)"
                Dim payment As String = If(IsDBNull(localDr("payment_method")), "", localDr("payment_method").ToString())
                Dim total As Decimal = If(IsDBNull(localDr("total_amount")), 0D, CType(localDr("total_amount"), Decimal))
                Dim id As String = If(IsDBNull(localDr("payment_id")), "", localDr("payment_id").ToString())

                ' 9 columns: ID, TXN ID, DATE, TIME, CASHIER, METHOD, TOTAL, VIEW, PRINT
                datagridviewtransaction.Rows.Add(
                    id,
                    txnId,
                    dateVal,
                    timeVal,
                    cashier,
                    payment,
                    "₱ " & total.ToString("N2"),
                    "👁",
                    "🖨️")
            End While
            localDr.Close() : localDr = Nothing
            localCmd.Dispose() : localCmd = Nothing

        Catch ex As Exception
            MessageBox.Show("Error loading transactions: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If localDr IsNot Nothing AndAlso Not localDr.IsClosed Then localDr.Close()
            If localCmd IsNot Nothing Then localCmd.Dispose()
        End Try
    End Sub

    ' ─────────────────────────────────────────────────────────────
    ' VIEW / PRINT ONLY
    ' ─────────────────────────────────────────────────────────────
    Private Sub datagridviewtransaction_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridviewtransaction.CellContentClick
        If e.RowIndex < 0 Then Exit Sub

        Dim i As Integer = e.RowIndex
        Dim txnId As String = Convert.ToString(datagridviewtransaction(1, i).Value)
        Dim colName As String = datagridviewtransaction.Columns(e.ColumnIndex).Name

        If colName = "Column8" Then          ' VIEW
            OpenReceiptPopup(txnId)
        ElseIf colName = "Column9" Then      ' PRINT
            OpenReceiptPopup(txnId)
        End If
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