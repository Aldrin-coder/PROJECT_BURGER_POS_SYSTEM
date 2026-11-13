Imports System.Data.SqlClient
Imports Guna.Charts.WinForms

Public Class frm_dashboard1

    ' ==========================================
    ' LOAD
    ' ==========================================
    Private Sub frm_dashboard1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        loadWelcome()
        loadrecenttrans()
        readdata()
        readMonthlyTrend()
        readPaymentMethod()
        ApplyRoleVisibility()
    End Sub

    ' ==========================================
    ' HEADERS
    ' ==========================================
    Sub loadrecenttrans()
        If CurrentUser.role.ToLower() = "cashier" Then
            lblrt.Text = "MY RECENT TRANSACTIONS"
        Else
            lblrt.Text = "RECENT TRANSACTIONS"
        End If
    End Sub

    Sub loadWelcome()
        lblgg.Text = "WELCOME, " & CurrentUser.fullname.ToUpper() & "!"
        If CurrentUser.role.ToLower() = "cashier" Then
            lblrole.Text = "Here is the cashier overview for today."
        Else
            lblrole.Text = "Here is the shop overview for today."
        End If
    End Sub

    ' ==========================================
    ' VISIBILITY
    ' ==========================================
    Sub ApplyRoleVisibility()
        If CurrentUser.role.ToLower() = "cashier" Then
            Column4.Visible = False
            Column5.Visible = False
            panelshift.Visible = True
            panelshiftransac.Visible = True
            panelconh.Visible = True
            paneltotalsales.Visible = False
            paneltransactoday.Visible = False
            panelms.Visible = False
            panelmni.Visible = False
        Else
            Column4.Visible = True
            Column5.Visible = True
            panelshift.Visible = False
            panelshiftransac.Visible = False
            panelconh.Visible = False
            paneltotalsales.Visible = True
            paneltransactoday.Visible = True
            panelms.Visible = True
            panelmni.Visible = True
        End If
    End Sub

    ' ==========================================
    ' GRID
    ' ==========================================
    Sub readdata()
        If datagridviewrecenttransac.Columns.Count = 0 Then Return

        datagridviewrecenttransac.Rows.Clear()

        For Each c As DataGridViewColumn In datagridviewrecenttransac.Columns
            If c.Name Like "Column[1-7]" Then c.Visible = True
        Next

        conn()

        query = "SELECT transaction_id, cashier_id, cashier_fullname, " &
                "       payment_method, subtotal_amount, amount_paid " &
                "FROM payments "

        If CurrentUser.role.ToLower() = "cashier" Then
            query &= " WHERE cashier_id = @CID "
        Else
            query &= " WHERE 1 = 1 "
        End If
        If searchtxb.Text <> "" Then
            query &= " AND transaction_id LIKE @SEARCH "
        End If
        query &= " ORDER BY payment_id DESC"

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        If CurrentUser.role.ToLower() = "cashier" Then cmd.Parameters.AddWithValue("@CID", CurrentUser.user_code)
        If searchtxb.Text <> "" Then cmd.Parameters.AddWithValue("@SEARCH", "%" & searchtxb.Text & "%")

        dr = cmd.ExecuteReader
        While dr.Read
            Dim subtotal As Decimal = 0
            If Not IsDBNull(dr("subtotal_amount")) Then subtotal = Convert.ToDecimal(dr("subtotal_amount"))
            Dim total As Decimal = subtotal * 1.02D

            datagridviewrecenttransac.Rows.Add(
                dr("transaction_id"),
                Date.Today.ToString("MM/dd/yyyy"),
                DateTime.Now.ToString("hh:mm tt"),
                dr("cashier_id"),
                dr("cashier_fullname"),
                dr("payment_method"),
                "₱" & total.ToString("N2"))
        End While
        dr.Close()
        cmd.Dispose()

        AddViewButtonColumn()
        ApplyRoleVisibility()

        If CurrentUser.role.ToLower() = "cashier" Then
            readMyShift()
            readMyTransactions()
            readCashOnHand()
        Else
            readTotalSales()
            readTotalTransactions()
            readMonthlySales()
            readMenuItems()
        End If
    End Sub

    ' ==========================================
    ' VIEW BUTTON COLUMN
    ' ==========================================
    Sub AddViewButtonColumn()
        If datagridviewrecenttransac.Columns.Contains("VIEW") Then Return

        Dim btn As New DataGridViewButtonColumn()
        btn.Name = "VIEW"
        btn.HeaderText = "VIEW"
        btn.Text = "View"
        btn.UseColumnTextForButtonValue = True
        btn.Width = 70
        btn.FlatStyle = FlatStyle.Flat
        datagridviewrecenttransac.Columns.Add(btn)
    End Sub

    ' ==========================================
    ' VIEW CLICK — Show Receipt Preview
    ' ==========================================
    Private Sub datagridviewrecenttransac_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridviewrecenttransac.CellContentClick
        If e.RowIndex < 0 Then Return
        If e.ColumnIndex <> datagridviewrecenttransac.Columns("VIEW").Index Then Return

        Dim cellVal = datagridviewrecenttransac.Item(0, e.RowIndex).Value
        If cellVal Is Nothing OrElse IsDBNull(cellVal) Then Return

        ShowReceipt(cellVal.ToString())
    End Sub

    ' ==========================================
    ' RECEIPT PREVIEW (FIXED)
    ' ==========================================
    Sub ShowReceipt(txnId As String)
        conn()

        Dim cashierName As String = ""
        Dim cashierId As String = ""
        Dim paymentMethod As String = ""
        Dim subtotalAmt As Decimal = 0
        Dim totalAmount As Decimal = 0
        Dim paid As Decimal = 0
        Dim changeAmt As Decimal = 0

        query = "SELECT cashier_id, cashier_fullname, payment_method, " &
                "       subtotal_amount, amount_paid " &
                "FROM payments WHERE transaction_id = @TID"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@TID", txnId)
        dr = cmd.ExecuteReader
        If dr.Read() Then
            cashierId = dr("cashier_id").ToString()
            cashierName = dr("cashier_fullname").ToString()
            paymentMethod = dr("payment_method").ToString()

            If Not IsDBNull(dr("subtotal_amount")) Then
                subtotalAmt = Convert.ToDecimal(dr("subtotal_amount"))
            End If
            If Not IsDBNull(dr("amount_paid")) Then
                paid = Convert.ToDecimal(dr("amount_paid"))
            End If

            totalAmount = subtotalAmt * 1.02D
            changeAmt = paid - totalAmount
        End If
        dr.Close()
        cmd.Dispose()

        Dim sb As New System.Text.StringBuilder()
        Dim lineWidth As Integer = 32

        Dim CenterText As Func(Of String, String) = Function(txt)
                                                        If txt.Length >= lineWidth Then Return txt
                                                        Dim leftPad As Integer = (lineWidth - txt.Length) \ 2
                                                        Return txt.PadLeft(txt.Length + leftPad).PadRight(lineWidth)
                                                    End Function

        sb.AppendLine(CenterText("DS BURGER SHOP"))
        sb.AppendLine(CenterText("Fresh Burger, Great Taste!"))
        sb.AppendLine("".PadRight(lineWidth, "-"c))

        sb.AppendLine("TXN #  : " & txnId)
        sb.AppendLine("Date   : " & DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"))
        sb.AppendLine("Cashier: " & cashierName)
        sb.AppendLine("Cash ID: " & cashierId)
        sb.AppendLine("Payment: " & paymentMethod)
        sb.AppendLine("".PadRight(lineWidth, "-"c))

        sb.AppendLine("ITEM             QTY   AMOUNT")

        ' --- FIX: ADDED TOP 1 TO PREVENT MULTIPLE ROW ERROR ---
        query = "SELECT product_name, product_quantity, product_price " &
                "FROM transaction_records " &
                "WHERE payment_id = (SELECT TOP 1 payment_id FROM payments WHERE transaction_id = @TID)"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@TID", txnId)
        dr = cmd.ExecuteReader
        While dr.Read
            Dim itemName As String = dr("product_name").ToString().ToUpper()
            If itemName.Length > 15 Then itemName = itemName.Substring(0, 15)
            Dim qty As Integer = Convert.ToInt32(dr("product_quantity"))
            Dim price As Decimal = Convert.ToDecimal(dr("product_price"))
            sb.AppendLine(itemName.PadRight(17) & qty.ToString().PadLeft(3) & (qty * price).ToString("N2").PadLeft(9))
        End While
        dr.Close()
        cmd.Dispose()

        sb.AppendLine("".PadRight(lineWidth, "-"c))

        Dim taxAmt As Decimal = totalAmount - subtotalAmt
        sb.AppendLine("Subtotal:".PadRight(20) & "P " & subtotalAmt.ToString("N2"))
        sb.AppendLine("Tax (2%):".PadRight(20) & "P " & taxAmt.ToString("N2"))
        sb.AppendLine("TOTAL:".PadRight(20) & "P " & totalAmount.ToString("N2"))
        sb.AppendLine("Paid:".PadRight(20) & "P " & paid.ToString("N2"))
        sb.AppendLine("Change:".PadRight(20) & "P " & changeAmt.ToString("N2"))
        sb.AppendLine("".PadRight(lineWidth, "-"c))

        sb.AppendLine(CenterText("THANK YOU FOR YOUR ORDER!"))
        sb.AppendLine(CenterText("Please come again!"))

        Dim printDoc As New System.Drawing.Printing.PrintDocument()
        printDoc.DefaultPageSettings.PaperSize = New System.Drawing.Printing.PaperSize("Thermal58mm", 220, 900)
        printDoc.DefaultPageSettings.Margins = New System.Drawing.Printing.Margins(0, 0, 0, 0)

        Dim localReceiptText As String = sb.ToString()

        AddHandler printDoc.PrintPage, Sub(s As Object, ev As System.Drawing.Printing.PrintPageEventArgs)
                                           ev.Graphics.DrawString(localReceiptText, New Font("Consolas", 8), Brushes.Black, 7, 80)
                                       End Sub

        Dim preview As New PrintPreviewDialog()
        preview.Document = printDoc
        preview.WindowState = FormWindowState.Maximized
        preview.ShowDialog()
    End Sub

    ' ==========================================
    ' CARDS
    ' ==========================================
    Sub readMyShift()
        conn()
        query = "SELECT ISNULL(SUM(subtotal_amount * 1.02), 0) FROM payments " &
                "WHERE cashier_id = @CID"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@CID", CurrentUser.user_code)
        dr = cmd.ExecuteReader
        If dr.Read() Then lblmys.Text = "₱" & Convert.ToDecimal(dr(0)).ToString("N2")
        dr.Close()
        cmd.Dispose()
    End Sub

    Sub readMyTransactions()
        conn()
        query = "SELECT COUNT(*) FROM payments WHERE cashier_id = @CID"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@CID", CurrentUser.user_code)
        dr = cmd.ExecuteReader
        If dr.Read() Then lblmy.Text = dr(0).ToString()
        dr.Close()
        cmd.Dispose()
    End Sub

    Sub readCashOnHand()
        conn()
        query = "SELECT ISNULL(SUM(amount_paid - (subtotal_amount * 1.02)), 0) FROM payments " &
                "WHERE payment_method = 'CASH' AND cashier_id = @CID"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@CID", CurrentUser.user_code)
        dr = cmd.ExecuteReader
        If dr.Read() Then lblcoh.Text = "₱" & Convert.ToDecimal(dr(0)).ToString("N2")
        dr.Close()
        cmd.Dispose()
    End Sub

    Sub readTotalSales()
        conn()
        query = "SELECT ISNULL(SUM(subtotal_amount * 1.02), 0) FROM payments"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        dr = cmd.ExecuteReader
        If dr.Read() Then lblts.Text = "₱" & Convert.ToDecimal(dr(0)).ToString("N2")
        dr.Close()
        cmd.Dispose()
    End Sub

    Sub readTotalTransactions()
        conn()
        query = "SELECT COUNT(*) FROM payments"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        dr = cmd.ExecuteReader
        If dr.Read() Then lbltr.Text = dr(0).ToString()
        dr.Close()
        cmd.Dispose()
    End Sub

    Sub readMonthlySales()
        conn()
        query = "SELECT ISNULL(SUM(subtotal_amount * 1.02), 0) FROM payments"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        dr = cmd.ExecuteReader
        If dr.Read() Then lblms.Text = "₱" & Convert.ToDecimal(dr(0)).ToString("N2")
        dr.Close()
        cmd.Dispose()
    End Sub

    Sub readMenuItems()
        conn()
        query = "SELECT (SELECT COUNT(*) FROM menu_items WHERE IsActive = 1), (SELECT COUNT(*) FROM menu_items)"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        dr = cmd.ExecuteReader
        If dr.Read() Then lblitems.Text = dr(0).ToString() & "/" & dr(1).ToString()
        dr.Close()
        cmd.Dispose()
    End Sub

    ' ==========================================
    ' CHARTS
    ' ==========================================
    Sub readMonthlyTrend()
        chartmsl.Datasets.Clear()
        chartmsl.YAxes.Ticks.Display = True
        chartmsl.XAxes.Ticks.Display = True

        Dim titleFont As New Guna.Charts.WinForms.ChartFont()
        titleFont.FontName = "Arial"
        titleFont.Size = 12
        titleFont.Style = Guna.Charts.WinForms.ChartFontStyle.Bold
        chartmsl.Title.Font = titleFont
        chartmsl.Title.Text = "Monthly Sales Trend (" & DateTime.Now.Year.ToString() & ")"

        conn()
        query = "SELECT ISNULL(SUM(subtotal_amount * 1.02), 0) AS amt FROM payments "
        If CurrentUser.role.ToLower() = "cashier" Then query &= "WHERE cashier_id = @CID "

        Dim totalAmt As Decimal = 0

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        If CurrentUser.role.ToLower() = "cashier" Then cmd.Parameters.AddWithValue("@CID", CurrentUser.user_code)
        dr = cmd.ExecuteReader
        If dr.Read() Then totalAmt = Convert.ToDecimal(dr("amt"))
        dr.Close()
        cmd.Dispose()

        Dim dataset As New GunaBarDataset()
        dataset.Label = "Monthly Sales"
        dataset.FillColors.Add(Color.LimeGreen)

        Dim monthNames() As String = {"Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                      "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"}

        For m As Integer = 1 To 12
            Dim amt As Decimal = If(m = DateTime.Now.Month, totalAmt, 0D)
            dataset.DataPoints.Add(New LPoint With {
                .Label = monthNames(m - 1) & " (" & amt.ToString("N0") & ")",
                .Y = CDbl(amt)
            })
        Next

        chartmsl.Datasets.Add(dataset)
        chartmsl.Update()
    End Sub

    Sub readPaymentMethod()
        chartpm.Datasets.Clear()
        chartpm.XAxes.Ticks.Display = False
        chartpm.YAxes.Ticks.Display = False

        Dim titleFont As New Guna.Charts.WinForms.ChartFont()
        titleFont.FontName = "Arial"
        titleFont.Size = 12
        titleFont.Style = Guna.Charts.WinForms.ChartFontStyle.Bold
        chartpm.Title.Font = titleFont
        chartpm.Title.Text = "Payment Methods"

        conn()
        query = "SELECT payment_method, ISNULL(SUM(subtotal_amount * 1.02), 0) AS amt " &
                "FROM payments "
        If CurrentUser.role.ToLower() = "cashier" Then query &= "WHERE cashier_id = @CID "
        query &= " GROUP BY payment_method"

        Dim cash As Decimal = 0
        Dim gcash As Decimal = 0
        Dim maya As Decimal = 0

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        If CurrentUser.role.ToLower() = "cashier" Then cmd.Parameters.AddWithValue("@CID", CurrentUser.user_code)
        dr = cmd.ExecuteReader
        While dr.Read
            If dr("payment_method").ToString().ToUpper() = "CASH" Then
                cash = Convert.ToDecimal(dr("amt"))
            ElseIf dr("payment_method").ToString().ToUpper() = "GCASH" Then
                gcash = Convert.ToDecimal(dr("amt"))
            ElseIf dr("payment_method").ToString().ToUpper() = "MAYA" Then
                maya = Convert.ToDecimal(dr("amt"))
            End If
        End While
        dr.Close()
        cmd.Dispose()

        Dim totalSales As Decimal = cash + gcash + maya
        If totalSales = 0 Then totalSales = 1

        Dim dataset As New GunaDoughnutDataset()
        dataset.Label = "Payment Methods"
        dataset.FillColors.AddRange(New Color() {Color.Orange, Color.SteelBlue, Color.LimeGreen})
        dataset.YFormat = "{0}%"
        dataset.DataPoints.Add(New LPoint With {.Label = "Cash", .Y = CDbl(cash / totalSales * 100)})
        dataset.DataPoints.Add(New LPoint With {.Label = "GCash", .Y = CDbl(gcash / totalSales * 100)})
        dataset.DataPoints.Add(New LPoint With {.Label = "Maya", .Y = CDbl(maya / totalSales * 100)})

        chartpm.Datasets.Add(dataset)
        chartpm.Update()
    End Sub

    ' ==========================================
    ' SEARCH
    ' ==========================================
    Private Sub searchtxb_TextChanged(sender As Object, e As EventArgs) Handles searchtxb.TextChanged
        readdata()
    End Sub

End Class