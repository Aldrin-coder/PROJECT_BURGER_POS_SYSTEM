Imports System.Data.SqlClient
Imports System.Text
Imports System.IO

Public Class frm_sales_report4owner

    ' ==========================================
    ' FORM LOAD
    ' ==========================================
    Private Sub frm_sales_report4owner_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        dtdt.Value = DateTime.Now
        readdata()
    End Sub

    ' ==========================================
    ' LOAD GRID + CARD TOTALS
    ' ==========================================
    Sub readdata()
        datagridviewsalesreport.Rows.Clear()
        conn()

        query = "SELECT payment_id, transaction_id, [current_date], transaction_time, " &
                "       cashier_fullname, payment_method, total_amount " &
                "FROM payments WHERE CAST([current_date] AS DATE) = @DATE "

        If txbsearch.Text <> "" Then
            query = query & "AND (transaction_id LIKE @SEARCH OR cashier_fullname LIKE @SEARCH) "
        End If

        query = query & "ORDER BY payment_id DESC"

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@DATE", dtdt.Value.Date)

        If txbsearch.Text <> "" Then
            cmd.Parameters.AddWithValue("@SEARCH", "%" & txbsearch.Text & "%")
        End If

        dr = cmd.ExecuteReader()

        While dr.Read
            Dim t As TimeSpan = TimeSpan.Zero
            If Not IsDBNull(dr("transaction_time")) Then
                t = DirectCast(dr("transaction_time"), TimeSpan)
            End If

            Dim d As DateTime = DateTime.Today
            If Not IsDBNull(dr("current_date")) Then
                d = Convert.ToDateTime(dr("current_date"))
            End If

            Dim total As Decimal = 0
            If Not IsDBNull(dr("total_amount")) Then
                total = Convert.ToDecimal(dr("total_amount"))
            End If

            Dim cashier As String = ""
            If Not IsDBNull(dr("cashier_fullname")) Then
                cashier = dr("cashier_fullname").ToString()
            End If

            Dim method As String = ""
            If Not IsDBNull(dr("payment_method")) Then
                method = dr("payment_method").ToString()
            End If

            datagridviewsalesreport.Rows.Add(
                dr("payment_id"),
                dr("transaction_id").ToString(),
                d.ToString("MM/dd/yyyy"),
                DateTime.Today.Add(t).ToString("hh:mm tt"),
                cashier,
                method,
                total.ToString("₱#,##0.00"),
                "VIEW",
                "DELETE")
        End While

        dr.Close()
        cmd.Dispose()

        readtoday()
        readmonth()
        readyear()
    End Sub

    ' ==========================================
    ' CARD TOTALS
    ' ==========================================
    Sub readtoday()
        query = "SELECT ISNULL(SUM(total_amount), 0) FROM payments " &
                "WHERE CAST([current_date] AS DATE) = @DATE"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@DATE", dtdt.Value.Date)
        lblst.Text = Convert.ToDecimal(cmd.ExecuteScalar()).ToString("₱#,##0.00")
        cmd.Dispose()
    End Sub

    Sub readmonth()
        query = "SELECT ISNULL(SUM(total_amount), 0) FROM payments " &
                "WHERE YEAR([current_date]) = @Y AND MONTH([current_date]) = @M"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@Y", dtdt.Value.Year)
        cmd.Parameters.AddWithValue("@M", dtdt.Value.Month)
        lblms.Text = Convert.ToDecimal(cmd.ExecuteScalar()).ToString("₱#,##0.00")
        cmd.Dispose()
    End Sub

    Sub readyear()
        query = "SELECT ISNULL(SUM(total_amount), 0) FROM payments WHERE YEAR([current_date]) = @Y"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@Y", dtdt.Value.Year)
        lbltr.Text = Convert.ToDecimal(cmd.ExecuteScalar()).ToString("₱#,##0.00")
        cmd.Dispose()
    End Sub

    ' ==========================================
    ' FILTER EVENTS
    ' ==========================================
    Private Sub dtdt_ValueChanged(sender As Object, e As EventArgs) Handles dtdt.ValueChanged
        readdata()
    End Sub

    Private Sub txbsearch_TextChanged(sender As Object, e As EventArgs) Handles txbsearch.TextChanged
        readdata()
    End Sub

    ' ==========================================
    ' VIEW / DELETE
    ' ==========================================
    Private Sub datagridviewtransaction_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridviewsalesreport.CellContentClick
        If e.RowIndex < 0 Then Exit Sub

        Dim i As Integer = e.RowIndex
        Dim paymentID As Integer = Convert.ToInt32(datagridviewsalesreport.Item(0, i).Value)
        Dim transID As String = datagridviewsalesreport.Item(1, i).Value.ToString()

        If e.ColumnIndex = 7 Then
            ShowReceipt(transID)

        ElseIf e.ColumnIndex = 8 Then
            If MessageBox.Show("Are you sure you want to delete Transaction " & transID & "?",
                               "Confirmation", MessageBoxButtons.YesNo,
                               MessageBoxIcon.Question) = DialogResult.No Then
                Exit Sub
            End If

            query = "DELETE FROM transaction_records WHERE payment_id = @ID"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@ID", paymentID)
            cmd.ExecuteNonQuery()
            cmd.Dispose()

            query = "DELETE FROM payments WHERE payment_id = @ID"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@ID", paymentID)
            cmd.ExecuteNonQuery()
            cmd.Dispose()

            readdata()
        End If
    End Sub

    ' ==========================================
    ' INLINE RECEIPT PREVIEW
    ' ==========================================
    Sub ShowReceipt(txnId As String)
        conn()

        Dim cashier As String = ""
        Dim method As String = ""
        Dim total As Decimal = 0

        query = "SELECT cashier_fullname, payment_method, total_amount " &
                "FROM payments WHERE transaction_id = @TID"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@TID", txnId)
        dr = cmd.ExecuteReader()

        If dr.Read() Then
            If Not IsDBNull(dr("cashier_fullname")) Then cashier = dr("cashier_fullname").ToString()
            If Not IsDBNull(dr("payment_method")) Then method = dr("payment_method").ToString()
            If Not IsDBNull(dr("total_amount")) Then total = Convert.ToDecimal(dr("total_amount"))
        End If

        dr.Close()
        cmd.Dispose()

        Dim sb As New StringBuilder()
        sb.AppendLine("DS BURGER SHOP")
        sb.AppendLine("TXN #  : " & txnId)
        sb.AppendLine("Cashier: " & cashier)
        sb.AppendLine("Payment: " & method)
        sb.AppendLine("--------------------------------")
        sb.AppendLine("ITEM             QTY   AMOUNT")

        ' ---- FIXED: replaced scalar subquery with INNER JOIN ----
        query = "SELECT tr.product_name, tr.product_quantity, tr.product_price " &
                "FROM transaction_records tr " &
                "INNER JOIN payments p ON p.payment_id = tr.payment_id " &
                "WHERE p.transaction_id = @TID"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@TID", txnId)
        dr = cmd.ExecuteReader()

        While dr.Read
            Dim name As String = ""
            If Not IsDBNull(dr("product_name")) Then name = dr("product_name").ToString().ToUpper()
            If name.Length > 15 Then name = name.Substring(0, 15)

            Dim qty As Integer = 0
            If Not IsDBNull(dr("product_quantity")) Then qty = Convert.ToInt32(dr("product_quantity"))

            Dim price As Decimal = 0
            If Not IsDBNull(dr("product_price")) Then price = Convert.ToDecimal(dr("product_price"))

            Dim amount As Decimal = qty * price
            sb.AppendLine(name.PadRight(17) & qty.ToString().PadLeft(3) & amount.ToString("N2").PadLeft(9))
        End While

        dr.Close()
        cmd.Dispose()

        sb.AppendLine("--------------------------------")
        sb.AppendLine("TOTAL: P " & total.ToString("N2"))

        Dim localReceiptText As String = sb.ToString()

        Dim printDoc As New System.Drawing.Printing.PrintDocument()
        printDoc.DefaultPageSettings.PaperSize = New System.Drawing.Printing.PaperSize("Thermal58mm", 220, 900)
        printDoc.DefaultPageSettings.Margins = New System.Drawing.Printing.Margins(0, 0, 0, 0)

        AddHandler printDoc.PrintPage, Sub(s As Object, ev As System.Drawing.Printing.PrintPageEventArgs)
                                           Dim font As New Font("Consolas", 8)
                                           ev.Graphics.DrawString(localReceiptText, font, Brushes.Black, 7, 80)
                                       End Sub

        Dim preview As New PrintPreviewDialog()
        preview.Document = printDoc
        preview.WindowState = FormWindowState.Maximized
        preview.ShowDialog()
    End Sub

    ' ==========================================
    ' PRINT REPORT (PREVIEW + SAVE AS PDF/WORD)
    ' ==========================================
    Private Sub printreportbtn_Click(sender As Object, e As EventArgs) Handles printreportbtn.Click
        If datagridviewsalesreport.Rows.Count = 0 Then
            MessageBox.Show("Nothing to print.", "Sales Report")
            Exit Sub
        End If

        Dim sb As New StringBuilder()

        sb.AppendLine("==========================================================================================")
        sb.AppendLine("                                 DS BURGER SHOP")
        sb.AppendLine("                               DAILY SALES REPORT")
        sb.AppendLine("==========================================================================================")
        sb.AppendLine("Date Printed: " & DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt"))
        sb.AppendLine("Report Date : " & dtdt.Value.ToString("MMMM dd, yyyy"))
        sb.AppendLine("------------------------------------------------------------------------------------------")
        sb.AppendLine("TOTAL SALES TODAY: " & lblst.Text & "    |    MONTH SALES: " & lblms.Text & "    |    YEAR REVENUE: " & lbltr.Text)
        sb.AppendLine("==========================================================================================")
        sb.AppendLine("ID     TRANSACTION ID       DATE          TIME       CASHIER          METHOD       TOTAL")
        sb.AppendLine("------------------------------------------------------------------------------------------")

        For i As Integer = 0 To datagridviewsalesreport.Rows.Count - 1
            Dim row As DataGridViewRow = datagridviewsalesreport.Rows(i)

            Dim id As String = row.Cells(0).Value.ToString()
            Dim txn As String = row.Cells(1).Value.ToString()
            Dim dateTxt As String = row.Cells(2).Value.ToString()
            Dim timeTxt As String = row.Cells(3).Value.ToString()

            Dim cashier As String = ""
            If row.Cells(4).Value IsNot Nothing Then cashier = row.Cells(4).Value.ToString()
            If cashier.Length > 15 Then cashier = cashier.Substring(0, 15)

            Dim method As String = ""
            If row.Cells(5).Value IsNot Nothing Then method = row.Cells(5).Value.ToString()

            Dim total As String = row.Cells(6).Value.ToString()

            Dim line As String = id.PadRight(7) & txn.PadRight(21) & dateTxt.PadRight(14) & timeTxt.PadRight(11) & cashier.PadRight(17) & method.PadRight(13) & total
            sb.AppendLine(line)
        Next

        sb.AppendLine("==========================================================================================")
        sb.AppendLine("End of Report")

        Dim localReportText As String = sb.ToString()

        ' 1. Setup Printer
        Dim printDoc As New System.Drawing.Printing.PrintDocument()
        printDoc.DefaultPageSettings.Landscape = True
        printDoc.DefaultPageSettings.PaperSize = New System.Drawing.Printing.PaperSize("A4", 827, 1169)
        printDoc.DefaultPageSettings.Margins = New System.Drawing.Printing.Margins(220, 20, 50, 50)

        AddHandler printDoc.PrintPage, Sub(s As Object, ev As System.Drawing.Printing.PrintPageEventArgs)
                                           Dim font As New Font("Consolas", 10)
                                           ev.Graphics.DrawString(localReportText, font, Brushes.Black, ev.MarginBounds.Left, ev.MarginBounds.Top)
                                       End Sub

        ' 2. Show Preview (User can click Printer Icon here to Print or Save as PDF)
        Dim preview As New PrintPreviewDialog()
        preview.Document = printDoc
        preview.WindowState = FormWindowState.Maximized
        preview.ShowDialog()


    End Sub



End Class