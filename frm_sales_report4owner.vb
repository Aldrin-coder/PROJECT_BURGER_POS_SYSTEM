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

        ' Selects BOTH Cashier ID and Cashier Fullname
        Dim q As String = "SELECT payment_id, transaction_id, [current_date], transaction_time, " &
                          "cashier_id, cashier_fullname, payment_method, total_amount " &
                          "FROM payments WHERE CAST([current_date] AS DATE) = @DATE "

        If txbsearch.Text <> "" Then
            q &= "AND (transaction_id LIKE @SEARCH OR cashier_fullname LIKE @SEARCH) "
        End If
        q &= "ORDER BY payment_id DESC"

        cmd = New SqlClient.SqlCommand(q, sqlconn)
        cmd.Parameters.AddWithValue("@DATE", dtdt.Value.Date)
        If txbsearch.Text <> "" Then cmd.Parameters.AddWithValue("@SEARCH", "%" & txbsearch.Text & "%")

        dr = cmd.ExecuteReader()

        While dr.Read
            Dim t As TimeSpan = If(IsDBNull(dr("transaction_time")), TimeSpan.Zero, DirectCast(dr("transaction_time"), TimeSpan))
            Dim d As DateTime = If(IsDBNull(dr("current_date")), DateTime.Today, Convert.ToDateTime(dr("current_date")))
            Dim total As Decimal = If(IsDBNull(dr("total_amount")), 0, Convert.ToDecimal(dr("total_amount")))

            Dim cID As String = If(IsDBNull(dr("cashier_id")), "", dr("cashier_id").ToString())
            Dim cName As String = If(IsDBNull(dr("cashier_fullname")), "", dr("cashier_fullname").ToString())
            Dim method As String = If(IsDBNull(dr("payment_method")), "", dr("payment_method").ToString())

            ' Matches your EXACT 10-column Designer layout
            datagridviewsalesreport.Rows.Add(
                dr("payment_id"),                            ' 0: ID
                dr("transaction_id").ToString(),             ' 1: TRANSACTION ID
                d.ToString("MM/dd/yyyy"),                    ' 2: DATE
                DateTime.Today.Add(t).ToString("hh:mm tt"),  ' 3: TRANSACTION TIME
                cID,                                         ' 4: CASHIER ID
                cName,                                       ' 5: CASHIER NAME
                method,                                      ' 6: PAYMENT METHOD
                total.ToString("₱#,##0.00"),                 ' 7: TOTAL
                "VIEW",                                      ' 8: VIEW
                "DELETE")                                    ' 9: DELETE
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
        cmd = New SqlClient.SqlCommand("SELECT ISNULL(SUM(total_amount), 0) FROM payments WHERE CAST([current_date] AS DATE) = @DATE", sqlconn)
        cmd.Parameters.AddWithValue("@DATE", dtdt.Value.Date)
        lblst.Text = Convert.ToDecimal(cmd.ExecuteScalar()).ToString("₱#,##0.00")
        cmd.Dispose()
    End Sub

    Sub readmonth()
        cmd = New SqlClient.SqlCommand("SELECT ISNULL(SUM(total_amount), 0) FROM payments WHERE YEAR([current_date]) = @Y AND MONTH([current_date]) = @M", sqlconn)
        cmd.Parameters.AddWithValue("@Y", dtdt.Value.Year)
        cmd.Parameters.AddWithValue("@M", dtdt.Value.Month)
        lblms.Text = Convert.ToDecimal(cmd.ExecuteScalar()).ToString("₱#,##0.00")
        cmd.Dispose()
    End Sub

    Sub readyear()
        cmd = New SqlClient.SqlCommand("SELECT ISNULL(SUM(total_amount), 0) FROM payments WHERE YEAR([current_date]) = @Y", sqlconn)
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
    ' VIEW / DELETE (Indices match your Designer)
    ' ==========================================
    Private Sub datagridviewtransaction_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridviewsalesreport.CellContentClick
        If e.RowIndex < 0 Then Exit Sub

        Dim payID As Integer = Convert.ToInt32(datagridviewsalesreport.Item(0, e.RowIndex).Value)
        Dim txnID As String = datagridviewsalesreport.Item(1, e.RowIndex).Value.ToString()

        If e.ColumnIndex = 8 Then
            ShowReceipt(txnID)
        ElseIf e.ColumnIndex = 9 Then
            If MessageBox.Show("Delete Transaction " & txnID & "?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                cmd = New SqlClient.SqlCommand("DELETE FROM transaction_records WHERE payment_id = @ID; DELETE FROM payments WHERE payment_id = @ID;", sqlconn)
                cmd.Parameters.AddWithValue("@ID", payID)
                cmd.ExecuteNonQuery()
                cmd.Dispose()
                readdata()
            End If
        End If
    End Sub

    ' ==========================================
    ' RECEIPT PREVIEW (Shows Subtotal & Tax)
    ' ==========================================
    Sub ShowReceipt(txnId As String)
        conn()

        cmd = New SqlClient.SqlCommand("SELECT cashier_fullname, payment_method, subtotal_amount, tax_amount, total_amount FROM payments WHERE transaction_id = @TID", sqlconn)
        cmd.Parameters.AddWithValue("@TID", txnId)
        dr = cmd.ExecuteReader()

        Dim cashier As String = "", method As String = ""
        Dim subT As Decimal = 0, tax As Decimal = 0, tot As Decimal = 0

        If dr.Read() Then
            cashier = If(IsDBNull(dr("cashier_fullname")), "", dr("cashier_fullname").ToString())
            method = If(IsDBNull(dr("payment_method")), "", dr("payment_method").ToString())
            subT = If(IsDBNull(dr("subtotal_amount")), 0, Convert.ToDecimal(dr("subtotal_amount")))
            tax = If(IsDBNull(dr("tax_amount")), 0, Convert.ToDecimal(dr("tax_amount")))
            tot = If(IsDBNull(dr("total_amount")), 0, Convert.ToDecimal(dr("total_amount")))
        End If
        dr.Close()

        Dim sb As New StringBuilder()
        sb.AppendLine("DS BURGER SHOP")
        sb.AppendLine("TXN #  : " & txnId)
        sb.AppendLine("Cashier: " & cashier)
        sb.AppendLine("Payment: " & method)
        sb.AppendLine("--------------------------------")
        sb.AppendLine("ITEM             QTY   AMOUNT")

        cmd = New SqlClient.SqlCommand("SELECT tr.product_name, tr.product_quantity, tr.product_price FROM transaction_records tr INNER JOIN payments p ON p.payment_id = tr.payment_id WHERE p.transaction_id = @TID", sqlconn)
        cmd.Parameters.AddWithValue("@TID", txnId)
        dr = cmd.ExecuteReader()

        While dr.Read
            Dim name As String = If(IsDBNull(dr("product_name")), "", dr("product_name").ToString().ToUpper())
            If name.Length > 15 Then name = name.Substring(0, 15)
            Dim qty As Integer = If(IsDBNull(dr("product_quantity")), 0, Convert.ToInt32(dr("product_quantity")))
            Dim price As Decimal = If(IsDBNull(dr("product_price")), 0, Convert.ToDecimal(dr("product_price")))
            sb.AppendLine(name.PadRight(17) & qty.ToString().PadLeft(3) & (qty * price).ToString("N2").PadLeft(9))
        End While
        dr.Close()
        cmd.Dispose()

        sb.AppendLine("--------------------------------")
        ' Receipt still shows the tax breakdown as required by the PDF
        sb.AppendLine("Subtotal:".PadRight(16) & "P " & subT.ToString("N2"))
        sb.AppendLine("Tax (2%):".PadRight(16) & "P " & tax.ToString("N2"))
        sb.AppendLine("TOTAL:".PadRight(16) & "P " & tot.ToString("N2"))

        Dim printDoc As New System.Drawing.Printing.PrintDocument()
        printDoc.DefaultPageSettings.PaperSize = New System.Drawing.Printing.PaperSize("Thermal58mm", 220, 900)
        printDoc.DefaultPageSettings.Margins = New System.Drawing.Printing.Margins(0, 0, 0, 0)
        AddHandler printDoc.PrintPage, Sub(s As Object, ev As System.Drawing.Printing.PrintPageEventArgs)
                                           ev.Graphics.DrawString(sb.ToString(), New Font("Consolas", 8), Brushes.Black, 7, 80)
                                       End Sub

        Dim preview As New PrintPreviewDialog()
        preview.Document = printDoc
        preview.WindowState = FormWindowState.Maximized
        preview.ShowDialog()
    End Sub

    ' ==========================================
    ' PRINT BUTTON MENU (Daily and Monthly only)
    ' ==========================================
    Private Sub printreportbtn_Click(sender As Object, e As EventArgs) Handles printreportbtn.Click
        Dim menu As New ContextMenuStrip()

        Dim itemDaily As New ToolStripMenuItem("Daily Sales Report (Selected Date)")
        Dim itemMonthly As New ToolStripMenuItem("Monthly Sales Report (Day-by-Day)")

        AddHandler itemDaily.Click, Sub(s, ev) PrintDailyReport()
        AddHandler itemMonthly.Click, Sub(s, ev) PrintDailyInMonthReport()

        menu.Items.AddRange({itemDaily, itemMonthly})
        menu.Show(printreportbtn, New Point(0, printreportbtn.Height))
    End Sub

    ' ==========================================
    ' REPORT 1: DAILY SALES REPORT
    ' ==========================================
    Private Sub PrintDailyReport()
        If datagridviewsalesreport.Rows.Count = 0 Then
            MessageBox.Show("Nothing to print for this date.", "Sales Report")
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
        sb.AppendLine("ID     TRANSACTION ID       DATE          TIME       CASHIER ID   CASHIER NAME      METHOD       TOTAL")
        sb.AppendLine("------------------------------------------------------------------------------------------")

        For i As Integer = 0 To datagridviewsalesreport.Rows.Count - 1
            Dim row As DataGridViewRow = datagridviewsalesreport.Rows(i)
            Dim cName As String = row.Cells(5).Value.ToString()
            If cName.Length > 15 Then cName = cName.Substring(0, 15)

            sb.AppendLine(row.Cells(0).Value.ToString().PadRight(7) &
                          row.Cells(1).Value.ToString().PadRight(21) &
                          row.Cells(2).Value.ToString().PadRight(14) &
                          row.Cells(3).Value.ToString().PadRight(11) &
                          row.Cells(4).Value.ToString().PadRight(13) &
                          cName.PadRight(18) &
                          row.Cells(6).Value.ToString().PadRight(13) &
                          row.Cells(7).Value.ToString())
        Next

        sb.AppendLine("==========================================================================================")
        ShowPrintPreview(sb.ToString())
    End Sub

    ' ==========================================
    ' REPORT 2: MONTHLY SALES REPORT (Day-by-Day)
    ' ==========================================
    Private Sub PrintDailyInMonthReport()
        Dim yr As Integer = dtdt.Value.Year, mo As Integer = dtdt.Value.Month
        Dim days As Integer = Date.DaysInMonth(yr, mo)
        Dim sb As New StringBuilder()
        Dim grandTot As Decimal = 0

        conn()
        cmd = New SqlClient.SqlCommand("SELECT DAY([current_date]) AS Dy, COUNT(*) AS Cnt, ISNULL(SUM(total_amount), 0) AS Tot FROM payments WHERE YEAR([current_date]) = @Y AND MONTH([current_date]) = @M GROUP BY DAY([current_date])", sqlconn)
        cmd.Parameters.AddWithValue("@Y", yr)
        cmd.Parameters.AddWithValue("@M", mo)
        dr = cmd.ExecuteReader()

        Dim tot(31) As Decimal, cnt(31) As Integer
        While dr.Read
            tot(Convert.ToInt32(dr("Dy"))) = Convert.ToDecimal(dr("Tot"))
            cnt(Convert.ToInt32(dr("Dy"))) = Convert.ToInt32(dr("Cnt"))
            grandTot += Convert.ToDecimal(dr("Tot"))
        End While
        dr.Close()

        sb.AppendLine("==========================================================================================")
        sb.AppendLine("                                 DS BURGER SHOP")
        sb.AppendLine("                         MONTHLY SALES REPORT (DAY-BY-DAY)")
        sb.AppendLine("==========================================================================================")
        sb.AppendLine("Month        : " & MonthName(mo) & " " & yr.ToString())
        sb.AppendLine("Date Printed : " & DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt"))
        sb.AppendLine("------------------------------------------------------------------------------------------")
        sb.AppendLine("DATE           DAY              TRANSACTIONS        TOTAL SALES        % OF MONTH")
        sb.AppendLine("------------------------------------------------------------------------------------------")

        For dy As Integer = 1 To days
            Dim pct As String = If(grandTot > 0, (tot(dy) / grandTot * 100D).ToString("0.00") & "%", "0.00%")
            Dim dayName As String = New DateTime(yr, mo, dy).ToString("ddd")
            Dim dateStr As String = New DateTime(yr, mo, dy).ToString("MM/dd/yyyy")
            sb.AppendLine(dateStr.PadRight(15) & dayName.PadRight(10) & cnt(dy).ToString().PadLeft(12) & tot(dy).ToString("₱#,##0.00").PadLeft(20) & pct.PadLeft(15))
        Next

        sb.AppendLine("------------------------------------------------------------------------------------------")
        sb.AppendLine("TOTAL".PadRight(25) & cnt.Sum().ToString().PadLeft(12) & grandTot.ToString("₱#,##0.00").PadLeft(20) & "100.00%".PadLeft(15))
        sb.AppendLine("==========================================================================================")
        ShowPrintPreview(sb.ToString())
    End Sub

    ' ==========================================
    ' SHARED PRINT PREVIEW
    ' ==========================================
    Private Sub ShowPrintPreview(txt As String)
        Dim p As New System.Drawing.Printing.PrintDocument()
        p.DefaultPageSettings.Landscape = True
        p.DefaultPageSettings.PaperSize = New System.Drawing.Printing.PaperSize("A4", 827, 1169)
        p.DefaultPageSettings.Margins = New System.Drawing.Printing.Margins(220, 20, 50, 50)

        AddHandler p.PrintPage, Sub(s, ev)
                                    ev.Graphics.DrawString(txt, New Font("Consolas", 10, FontStyle.Bold), Brushes.Black, ev.MarginBounds.Left, ev.MarginBounds.Top)
                                End Sub

        Dim prev As New PrintPreviewDialog()
        prev.Document = p
        prev.WindowState = FormWindowState.Maximized
        prev.ShowDialog()
    End Sub

End Class