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
    ' PRINT BUTTON → SHOW REPORT TYPE MENU
    ' ==========================================
    Private Sub printreportbtn_Click(sender As Object, e As EventArgs) Handles printreportbtn.Click
        Dim menu As New ContextMenuStrip()

        Dim itemDaily As New ToolStripMenuItem("Daily Sales Report (Selected Date)")
        Dim itemMonthly As New ToolStripMenuItem("Monthly Sales Report (Jan–Dec of Selected Year)")
        Dim itemDailyInMonth As New ToolStripMenuItem("Daily Sales Inside the Month (Day-by-Day)")

        AddHandler itemDaily.Click, Sub(s, ev) PrintDailyReport()
        AddHandler itemMonthly.Click, Sub(s, ev) PrintMonthlyReport()
        AddHandler itemDailyInMonth.Click, Sub(s, ev) PrintDailyInMonthReport()

        menu.Items.Add(itemDaily)
        menu.Items.Add(itemMonthly)
        menu.Items.Add(itemDailyInMonth)

        ' Show the menu under the button
        menu.Show(printreportbtn, New Point(0, printreportbtn.Height))
    End Sub

    ' ==========================================
    ' REPORT 1: DAILY SALES REPORT (SELECTED DATE)
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

        ShowPrintPreview(sb.ToString())
    End Sub

    ' ==========================================
    ' REPORT 2: MONTHLY SALES REPORT (JAN–DEC)
    ' ==========================================
    Private Sub PrintMonthlyReport()
        Dim reportYear As Integer = dtdt.Value.Year

        Dim monthTotals(12) As Decimal
        Dim monthCounts(12) As Integer

        conn()
        query = "SELECT MONTH([current_date]) AS Mn, " &
                "       COUNT(*) AS TxnCount, " &
                "       ISNULL(SUM(total_amount), 0) AS Total " &
                "FROM payments " &
                "WHERE YEAR([current_date]) = @Y " &
                "GROUP BY MONTH([current_date]) " &
                "ORDER BY Mn"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@Y", reportYear)
        dr = cmd.ExecuteReader()

        While dr.Read
            Dim mn As Integer = Convert.ToInt32(dr("Mn"))
            monthTotals(mn) = Convert.ToDecimal(dr("Total"))
            monthCounts(mn) = Convert.ToInt32(dr("TxnCount"))
        End While

        dr.Close()
        cmd.Dispose()

        Dim sb As New StringBuilder()
        Dim grandTotal As Decimal = 0
        Dim grandCount As Integer = 0

        sb.AppendLine("==========================================================================================")
        sb.AppendLine("                                 DS BURGER SHOP")
        sb.AppendLine("                               MONTHLY SALES REPORT")
        sb.AppendLine("==========================================================================================")
        sb.AppendLine("Year         : " & reportYear.ToString())
        sb.AppendLine("Date Printed : " & DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt"))
        sb.AppendLine("------------------------------------------------------------------------------------------")
        sb.AppendLine("MONTH           TRANSACTIONS        TOTAL SALES        % OF YEAR")
        sb.AppendLine("------------------------------------------------------------------------------------------")

        For m As Integer = 1 To 12
            grandTotal += monthTotals(m)
            grandCount += monthCounts(m)
        Next

        For m As Integer = 1 To 12
            Dim pct As String = "0.00%"
            If grandTotal > 0 Then
                pct = (monthTotals(m) / grandTotal * 100D).ToString("0.00") & "%"
            End If

            sb.AppendLine(MonthName(m).PadRight(15) &
                          monthCounts(m).ToString().PadLeft(12) &
                          monthTotals(m).ToString("₱#,##0.00").PadLeft(20) &
                          pct.PadLeft(15))
        Next

        sb.AppendLine("------------------------------------------------------------------------------------------")
        sb.AppendLine("TOTAL".PadRight(15) &
                      grandCount.ToString().PadLeft(12) &
                      grandTotal.ToString("₱#,##0.00").PadLeft(20) &
                      "100.00%".PadLeft(15))
        sb.AppendLine("==========================================================================================")

        Dim bestMonth As Integer = 0
        Dim bestTotal As Decimal = 0
        For m As Integer = 1 To 12
            If monthTotals(m) > bestTotal Then
                bestTotal = monthTotals(m)
                bestMonth = m
            End If
        Next
        If bestMonth > 0 Then
            sb.AppendLine("BEST MONTH : " & MonthName(bestMonth) & "  (" & bestTotal.ToString("₱#,##0.00") & ")")
        End If

        sb.AppendLine("------------------------------------------------------------------------------------------")
        sb.AppendLine("SALES PER CASHIER (Year " & reportYear.ToString() & ")")
        sb.AppendLine("------------------------------------------------------------------------------------------")
        sb.AppendLine("CASHIER                  TRANSACTIONS        TOTAL SALES")
        sb.AppendLine("------------------------------------------------------------------------------------------")

        query = "SELECT ISNULL(cashier_fullname,'(none)') AS Cashier, " &
                "       COUNT(*) AS TxnCount, " &
                "       ISNULL(SUM(total_amount),0) AS Total " &
                "FROM payments " &
                "WHERE YEAR([current_date]) = @Y " &
                "GROUP BY cashier_fullname " &
                "ORDER BY Total DESC"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@Y", reportYear)
        dr = cmd.ExecuteReader()

        While dr.Read
            Dim cName As String = dr("Cashier").ToString()
            If cName.Length > 22 Then cName = cName.Substring(0, 22)
            sb.AppendLine(cName.PadRight(23) &
                          Convert.ToInt32(dr("TxnCount")).ToString().PadLeft(12) &
                          Convert.ToDecimal(dr("Total")).ToString("₱#,##0.00").PadLeft(20))
        End While

        dr.Close()
        cmd.Dispose()

        sb.AppendLine("==========================================================================================")
        sb.AppendLine("End of Report")

        ShowPrintPreview(sb.ToString())
    End Sub

    ' ==========================================
    ' REPORT 3: DAILY SALES INSIDE THE MONTH
    ' ==========================================
    Private Sub PrintDailyInMonthReport()
        Dim reportYear As Integer = dtdt.Value.Year
        Dim reportMonth As Integer = dtdt.Value.Month

        Dim dayTotals(31) As Decimal
        Dim dayCounts(31) As Integer

        conn()
        query = "SELECT DAY([current_date]) AS Dy, " &
                "       COUNT(*) AS TxnCount, " &
                "       ISNULL(SUM(total_amount), 0) AS Total " &
                "FROM payments " &
                "WHERE YEAR([current_date]) = @Y " &
                "  AND MONTH([current_date]) = @M " &
                "GROUP BY DAY([current_date]) " &
                "ORDER BY Dy"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@Y", reportYear)
        cmd.Parameters.AddWithValue("@M", reportMonth)
        dr = cmd.ExecuteReader()

        While dr.Read
            Dim dy As Integer = Convert.ToInt32(dr("Dy"))
            dayTotals(dy) = Convert.ToDecimal(dr("Total"))
            dayCounts(dy) = Convert.ToInt32(dr("TxnCount"))
        End While

        dr.Close()
        cmd.Dispose()

        Dim daysInMonth As Integer = Date.DaysInMonth(reportYear, reportMonth)
        Dim sb As New StringBuilder()
        Dim grandTotal As Decimal = 0
        Dim grandCount As Integer = 0

        sb.AppendLine("==========================================================================================")
        sb.AppendLine("                                 DS BURGER SHOP")
        sb.AppendLine("                         DAILY SALES IN THE MONTH REPORT")
        sb.AppendLine("==========================================================================================")
        sb.AppendLine("Month        : " & MonthName(reportMonth) & " " & reportYear.ToString())
        sb.AppendLine("Date Printed : " & DateTime.Now.ToString("MMMM dd, yyyy hh:mm tt"))
        sb.AppendLine("------------------------------------------------------------------------------------------")
        sb.AppendLine("DATE           DAY              TRANSACTIONS        TOTAL SALES        % OF MONTH")
        sb.AppendLine("------------------------------------------------------------------------------------------")

        For dy As Integer = 1 To daysInMonth
            grandTotal += dayTotals(dy)
            grandCount += dayCounts(dy)
        Next

        For dy As Integer = 1 To daysInMonth
            Dim pct As String = "0.00%"
            If grandTotal > 0 Then
                pct = (dayTotals(dy) / grandTotal * 100D).ToString("0.00") & "%"
            End If

            Dim dt As New DateTime(reportYear, reportMonth, dy)
            Dim dayName As String = dt.ToString("ddd")
            Dim dateStr As String = dt.ToString("MM/dd/yyyy")

            sb.AppendLine(dateStr.PadRight(15) &
                          dayName.PadRight(10) &
                          dayCounts(dy).ToString().PadLeft(12) &
                          dayTotals(dy).ToString("₱#,##0.00").PadLeft(20) &
                          pct.PadLeft(15))
        Next

        sb.AppendLine("------------------------------------------------------------------------------------------")
        sb.AppendLine("TOTAL".PadRight(25) &
                      grandCount.ToString().PadLeft(12) &
                      grandTotal.ToString("₱#,##0.00").PadLeft(20) &
                      "100.00%".PadLeft(15))
        sb.AppendLine("==========================================================================================")

        Dim bestDay As Integer = 0
        Dim bestDayTotal As Decimal = 0
        Dim worstDay As Integer = 0
        Dim worstDayTotal As Decimal = Decimal.MaxValue

        For dy As Integer = 1 To daysInMonth
            If dayTotals(dy) > bestDayTotal Then
                bestDayTotal = dayTotals(dy)
                bestDay = dy
            End If
            If dayTotals(dy) < worstDayTotal Then
                worstDayTotal = dayTotals(dy)
                worstDay = dy
            End If
        Next

        If bestDay > 0 Then
            sb.AppendLine("BEST DAY  : " & New DateTime(reportYear, reportMonth, bestDay).ToString("MMMM dd") &
                          "  (" & bestDayTotal.ToString("₱#,##0.00") & ")")
        End If
        If worstDay > 0 Then
            sb.AppendLine("WORST DAY : " & New DateTime(reportYear, reportMonth, worstDay).ToString("MMMM dd") &
                          "  (" & worstDayTotal.ToString("₱#,##0.00") & ")")
        End If

        sb.AppendLine("------------------------------------------------------------------------------------------")
        sb.AppendLine("SALES PER CASHIER (" & MonthName(reportMonth) & " " & reportYear.ToString() & ")")
        sb.AppendLine("------------------------------------------------------------------------------------------")
        sb.AppendLine("CASHIER                  TRANSACTIONS        TOTAL SALES        % OF MONTH")
        sb.AppendLine("------------------------------------------------------------------------------------------")

        query = "SELECT ISNULL(cashier_fullname,'(none)') AS Cashier, " &
                "       COUNT(*) AS TxnCount, " &
                "       ISNULL(SUM(total_amount),0) AS Total " &
                "FROM payments " &
                "WHERE YEAR([current_date]) = @Y " &
                "  AND MONTH([current_date]) = @M " &
                "GROUP BY cashier_fullname " &
                "ORDER BY Total DESC"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@Y", reportYear)
        cmd.Parameters.AddWithValue("@M", reportMonth)
        dr = cmd.ExecuteReader()

        While dr.Read
            Dim cName As String = dr("Cashier").ToString()
            If cName.Length > 22 Then cName = cName.Substring(0, 22)
            Dim cTotal As Decimal = Convert.ToDecimal(dr("Total"))
            Dim cPct As String = "0.00%"
            If grandTotal > 0 Then
                cPct = (cTotal / grandTotal * 100D).ToString("0.00") & "%"
            End If
            sb.AppendLine(cName.PadRight(23) &
                          Convert.ToInt32(dr("TxnCount")).ToString().PadLeft(12) &
                          cTotal.ToString("₱#,##0.00").PadLeft(20) &
                          cPct.PadLeft(15))
        End While

        dr.Close()
        cmd.Dispose()

        sb.AppendLine("==========================================================================================")
        sb.AppendLine("End of Report")

        ShowPrintPreview(sb.ToString())
    End Sub

    ' ==========================================
    ' SHARED PRINT PREVIEW
    ' ==========================================
    Private Sub ShowPrintPreview(reportText As String)
        Dim printDoc As New System.Drawing.Printing.PrintDocument()
        printDoc.DefaultPageSettings.Landscape = True
        printDoc.DefaultPageSettings.PaperSize = New System.Drawing.Printing.PaperSize("A4", 827, 1169)
        printDoc.DefaultPageSettings.Margins = New System.Drawing.Printing.Margins(220, 20, 50, 50)

        AddHandler printDoc.PrintPage, Sub(s As Object, ev As System.Drawing.Printing.PrintPageEventArgs)
                                           Dim font As New Font("Consolas", 10, FontStyle.Bold)
                                           ev.Graphics.DrawString(reportText, font, Brushes.Black, ev.MarginBounds.Left, ev.MarginBounds.Top)
                                       End Sub

        Dim preview As New PrintPreviewDialog()
        preview.Document = printDoc
        preview.WindowState = FormWindowState.Maximized
        preview.ShowDialog()
    End Sub

End Class