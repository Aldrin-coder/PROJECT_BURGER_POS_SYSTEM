Imports System.Data.SqlClient
Imports Guna.Charts.WinForms
Imports System.Text

Public Class frm_dashboard1

    ' ==========================================
    ' LOAD
    ' ==========================================
    Private Sub frm_dashboard1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        loadWelcome()
        loadrecenttrans()
        readdata()
        readMonthlySales1()
        readPaymentMethod()
        ApplyRoleVisibility()
        chartmsl.Update()
        chartpm.Update()
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
        ' Show the VIEW button column for everyone
        Column8.Visible = True

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
            panelmst.Visible = True
            panelpm.Visible = True
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
            panelmst.Visible = True
            panelpm.Visible = True
        End If
    End Sub

    ' ==========================================
    ' GRID
    ' ==========================================
    Sub readdata()
        If datagridviewrecenttransac.Columns.Count = 0 Then Return

        datagridviewrecenttransac.Rows.Clear()

        ' THIS MAKES SURE COLUMN 1 TO 8 ARE VISIBLE
        For Each c As DataGridViewColumn In datagridviewrecenttransac.Columns
            If c.Name Like "Column[1-8]" Then c.Visible = True
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
                "₱" & total.ToString("N2"),
                "VIEW")
        End While
        dr.Close()
        cmd.Dispose()

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
    ' VIEW CLICK (Trigger Preview)
    ' ==========================================
    Private Sub datagridviewrecenttransac_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridviewrecenttransac.CellContentClick
        If e.RowIndex < 0 Then Exit Sub

        ' Check if the clicked column is a Button column (Column8)
        If TypeOf datagridviewrecenttransac.Columns(e.ColumnIndex) Is DataGridViewButtonColumn Then
            Dim cellVal = datagridviewrecenttransac.Item(0, e.RowIndex).Value
            If cellVal IsNot Nothing AndAlso Not IsDBNull(cellVal) Then
                ShowReceipt(cellVal.ToString())
            End If
        End If
    End Sub

    ' ==========================================
    ' RECEIPT PREVIEW (May Print Preview Dialog)
    ' ==========================================
    Sub ShowReceipt(txnId As String)
        conn()

        Dim cashierName As String = ""
        Dim paymentMethod As String = ""
        Dim subtotalAmt As Decimal = 0
        Dim taxRate As Decimal = 0
        Dim totalAmount As Decimal = 0
        Dim paid As Decimal = 0

        ' 1. Get Payment Details
        cmd = New SqlClient.SqlCommand("SELECT cashier_fullname, payment_method, subtotal_amount, tax_rate, total_amount, amount_paid FROM payments WHERE transaction_id = @TID", sqlconn)
        cmd.Parameters.AddWithValue("@TID", txnId)
        dr = cmd.ExecuteReader()

        If dr.Read() Then
            If Not IsDBNull(dr("cashier_fullname")) Then cashierName = dr("cashier_fullname").ToString()
            If Not IsDBNull(dr("payment_method")) Then paymentMethod = dr("payment_method").ToString()
            If Not IsDBNull(dr("subtotal_amount")) Then subtotalAmt = Convert.ToDecimal(dr("subtotal_amount"))
            If Not IsDBNull(dr("tax_rate")) Then taxRate = Convert.ToDecimal(dr("tax_rate"))
            If Not IsDBNull(dr("total_amount")) Then totalAmount = Convert.ToDecimal(dr("total_amount"))
            If Not IsDBNull(dr("amount_paid")) Then paid = Convert.ToDecimal(dr("amount_paid"))
        End If
        dr.Close()
        cmd.Dispose()

        Dim changeAmt As Decimal = paid - totalAmount
        Dim taxPeso As Decimal = totalAmount - subtotalAmt

        ' 2. Build the receipt text (Standard Padding)
        Dim sb As New System.Text.StringBuilder()
        Dim line As String = "--------------------------------"

        sb.AppendLine("        DS BURGER SHOP        ")
        sb.AppendLine("   Fresh Burger, Great Taste!  ")
        sb.AppendLine(line)
        sb.AppendLine("TXN #  : " & txnId)
        sb.AppendLine("Date   : " & DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"))
        sb.AppendLine("Cashier: " & cashierName)
        sb.AppendLine("Payment: " & paymentMethod)
        sb.AppendLine(line)
        sb.AppendLine("ITEM             QTY   AMOUNT")

        ' 3. Loop through items (Using TOP 1 to prevent errors)
        cmd = New SqlClient.SqlCommand("SELECT product_name, product_quantity, product_price FROM transaction_records WHERE payment_id = (SELECT TOP 1 payment_id FROM payments WHERE transaction_id = @TID)", sqlconn)
        cmd.Parameters.AddWithValue("@TID", txnId)
        dr = cmd.ExecuteReader()

        While dr.Read
            Dim itemName As String = dr("product_name").ToString().ToUpper()
            If itemName.Length > 15 Then
                itemName = itemName.Substring(0, 15)
            End If
            Dim qty As Integer = Convert.ToInt32(dr("product_quantity"))
            Dim price As Decimal = Convert.ToDecimal(dr("product_price"))

            sb.AppendLine(itemName.PadRight(17) & qty.ToString().PadLeft(3) & (qty * price).ToString("N2").PadLeft(9))
        End While
        dr.Close()
        cmd.Dispose()

        ' 4. Add Totals
        sb.AppendLine(line)
        sb.AppendLine("Subtotal:".PadRight(20) & "P " & subtotalAmt.ToString("N2"))
        sb.AppendLine("Tax (" & (taxRate * 100).ToString("0.##") & "%):".PadRight(20) & "P " & taxPeso.ToString("N2"))
        sb.AppendLine("TOTAL:".PadRight(20) & "P " & totalAmount.ToString("N2"))
        sb.AppendLine("Paid:".PadRight(20) & "P " & paid.ToString("N2"))
        sb.AppendLine("Change:".PadRight(20) & "P " & changeAmt.ToString("N2"))
        sb.AppendLine(line)
        sb.AppendLine("   THANK YOU FOR YOUR ORDER!  ")
        sb.AppendLine("      Please come again!      ")

        ' 5. Show Print Preview Dialog (ETO ANG PREVIEW)
        Dim printDoc As New System.Drawing.Printing.PrintDocument()
        printDoc.DefaultPageSettings.PaperSize = New System.Drawing.Printing.PaperSize("Thermal58mm", 220, 600)
        printDoc.DefaultPageSettings.Margins = New System.Drawing.Printing.Margins(0, 0, 0, 0)

        AddHandler printDoc.PrintPage, Sub(s As Object, ev As System.Drawing.Printing.PrintPageEventArgs)
                                           ev.Graphics.DrawString(sb.ToString(), New Font("Consolas", 8), Brushes.Black, 7, 20)
                                       End Sub

        Dim preview As New PrintPreviewDialog()
        preview.Document = printDoc
        preview.WindowState = FormWindowState.Maximized
        preview.ShowDialog() ' <--- ITO ANG NAGPAPALABAS NG PREVIEW
    End Sub

    ' ==========================================
    ' CARDS
    ' ==========================================
    Sub readMyShift()
        conn()
        query = "SELECT ISNULL(SUM(subtotal_amount * 1.02), 0) FROM payments " &
                "WHERE cashier_id = @CID " &
                "AND CAST(current_date AS DATE) = CAST(GETDATE() AS DATE)"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@CID", CurrentUser.user_code)
        dr = cmd.ExecuteReader
        If dr.Read() Then lblmys.Text = "₱" & Convert.ToDecimal(dr(0)).ToString("N2")
        dr.Close()
        cmd.Dispose()
    End Sub

    Sub readMyTransactions()
        conn()
        query = "SELECT COUNT(*) FROM payments WHERE cashier_id = @CID " &
                "AND CAST(current_date AS DATE) = CAST(GETDATE() AS DATE)"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@CID", CurrentUser.user_code)
        dr = cmd.ExecuteReader
        If dr.Read() Then lblmy.Text = dr(0).ToString()
        dr.Close()
        cmd.Dispose()
    End Sub

    Sub readCashOnHand()
        conn()
        query = "SELECT ISNULL(SUM(subtotal_amount * 1.02), 0) FROM payments " &
                "WHERE payment_method = 'CASH' AND cashier_id = @CID " &
                "AND CAST(current_date AS DATE) = CAST(GETDATE() AS DATE)"
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
        query = "SELECT ISNULL(SUM(subtotal_amount * 1.02), 0) FROM payments " &
                "WHERE YEAR(current_date) = YEAR(GETDATE()) AND MONTH(current_date) = MONTH(GETDATE())"
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
    Sub readMonthlySales1()
        chartmsl.Datasets.Clear()
        chartmsl.YAxes.Ticks.Display = True
        chartmsl.XAxes.Ticks.Display = True

        Dim titleFont As New Guna.Charts.WinForms.ChartFont()
        titleFont.FontName = "Arial"
        titleFont.Size = 12
        titleFont.Style = Guna.Charts.WinForms.ChartFontStyle.Bold
        chartmsl.Title.Font = titleFont

        If CurrentUser.role.ToLower() = "cashier" Then
            chartmsl.Title.Text = "My Monthly Sales  (" & DateTime.Now.Year.ToString() & ")"
        Else
            chartmsl.Title.Text = "Monthly Sales  (" & DateTime.Now.Year.ToString() & ")"
        End If

        conn()

        query = "SELECT MONTH([current_date]) AS m, ISNULL(SUM(subtotal_amount * 1.02), 0) AS amt " &
                          "FROM dbo.payments " &
                          "WHERE YEAR([current_date]) = @Y "

        If CurrentUser.role.ToLower() = "cashier" Then
            query &= " AND cashier_id = @CID "
        End If

        query &= " GROUP BY MONTH([current_date])"

        Dim monthlyData(11) As Decimal

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@Y", DateTime.Now.Year)
        If CurrentUser.role.ToLower() = "cashier" Then cmd.Parameters.AddWithValue("@CID", CurrentUser.user_code)

        dr = cmd.ExecuteReader
        While dr.Read
            monthlyData(Convert.ToInt32(dr("m")) - 1) = Convert.ToDecimal(dr("amt"))
        End While
        dr.Close()
        cmd.Dispose()

        Dim dataset As New GunaBarDataset()
        dataset.Label = "Monthly Sales"
        dataset.FillColors.Add(Color.LimeGreen)

        Dim monthNames() As String = {"Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                      "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"}

        For m As Integer = 1 To 12
            Dim amt As Decimal = monthlyData(m - 1)
            dataset.DataPoints.Add(New LPoint With {
                .Label = monthNames(m - 1) & " (" & amt.ToString("N0") & ")",
                .Y = CDbl(amt)
            })
        Next

        chartmsl.Datasets.Add(dataset)
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
        dataset.YFormat = "{0:0}%"
        dataset.DataPoints.Add(New LPoint With {.Label = "Cash (₱" & cash.ToString("#,##0") & ")", .Y = CDbl(cash / totalSales * 100)})
        dataset.DataPoints.Add(New LPoint With {.Label = "GCash (₱" & gcash.ToString("#,##0") & ")", .Y = CDbl(gcash / totalSales * 100)})
        dataset.DataPoints.Add(New LPoint With {.Label = "Maya (₱" & maya.ToString("#,##0") & ")", .Y = CDbl(maya / totalSales * 100)})

        chartpm.Datasets.Add(dataset)
    End Sub

    ' ==========================================
    ' SEARCH
    ' ==========================================
    Private Sub searchtxb_TextChanged(sender As Object, e As EventArgs) Handles searchtxb.TextChanged
        readdata()
    End Sub

    Private Sub Guna2Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Guna2Panel1.Paint

    End Sub
End Class