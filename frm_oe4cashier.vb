Imports System.Data.SqlClient
Imports System.Drawing.Printing
Imports System.IO
Imports Guna.UI2.WinForms

Public Class frm_oe4cashier

    ' Variables to hold the totals
    Dim totalSubtotal As Decimal = 0
    Dim totalTax As Decimal = 0
    Dim totalAmount As Decimal = 0

    ' 2% Tax Rate (0.02)
    Dim taxRate As Decimal = 0.02D

    ' Dictionary to keep track of the stock labels
    Private stockLabels As New Dictionary(Of String, Label)()

    ' ==========================================
    ' FORM LOAD
    ' ==========================================
    Private Sub frm_oe4cashier_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        loadPaymentMethods()
        loadCategories()
        SP_AutoNumber()
        loadProducts()
        txbap.BorderColor = Color.Black
        cmbpm.BorderColor = Color.Black
    End Sub

    ' ==========================================
    ' AUTO NUMBER GENERATOR
    ' ==========================================
    Sub SP_AutoNumber()
        conn()
        Dim result As Object

        query = "SELECT Max(NewNumber) FROM AutoNumberTxn WHERE Pfx = 'TXN-'"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        result = cmd.ExecuteScalar()

        If IsDBNull(result) Then
            CreateNewAutoNumber()
            query = "SELECT Max(NewNumber) FROM AutoNumberTxn WHERE Pfx = 'TXN-'"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            txbordern.Text = Convert.ToString(cmd.ExecuteScalar)
        Else
            txbordern.Text = Convert.ToString(result)
        End If

        cmd.Dispose()
    End Sub

    Sub CreateNewAutoNumber()
        Try
            Dim cmd As New SqlCommand
            cmd.Connection = sqlconn
            cmd.CommandText = "sp_AutoNumberTxn"
            cmd.CommandType = CommandType.StoredProcedure
            cmd.Parameters.AddWithValue("@pfx", "TXN-")
            cmd.ExecuteScalar()
            cmd.Dispose()
        Catch ex As Exception
        End Try
    End Sub

    ' ==========================================
    ' LOAD DATA
    ' ==========================================
    Sub loadPaymentMethods()
        cmbpm.Items.Clear()
        cmbpm.Items.Add("CASH")
        cmbpm.Items.Add("GCASH")
        cmbpm.Items.Add("MAYA")
        cmbpm.SelectedIndex = -1
    End Sub

    Sub loadCategories()
        cmbcategory.Items.Clear()
        cmbcategory.Items.Add("all")

        conn()
        query = "SELECT category_name FROM categories ORDER BY category_name"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        dr = cmd.ExecuteReader

        While dr.Read
            cmbcategory.Items.Add(dr("category_name").ToString())
        End While

        dr.Close()
        cmd.Dispose()

        If cmbcategory.Items.Count > 0 Then
            cmbcategory.SelectedIndex = 0
        End If
    End Sub

    Sub loadProducts()
        FlowLayoutPanel1.Controls.Clear()
        FlowLayoutPanel1.Padding = New Padding(15)
        stockLabels.Clear()

        query = "SELECT m.item_code, m.product_name, m.price, m.quantity, m.product_image, c.category_name " &
                "FROM menu_items m " &
                "INNER JOIN categories c ON m.category_id = c.category_id " &
                "WHERE m.IsActive = 1 "

        If cmbcategory.Text <> "" And cmbcategory.Text.ToUpper() <> "ALL" Then
            query = query & " AND c.category_name = @FILTER "
        End If
        If txbsearch.Text <> "" Then
            query = query & " AND (LOWER(m.item_code) LIKE LOWER(@SEARCH) OR LOWER(m.product_name) LIKE LOWER(@SEARCH)) "
        End If
        query = query & " ORDER BY m.product_name"

        conn()
        cmd = New SqlClient.SqlCommand(query, sqlconn)

        If cmbcategory.Text <> "" And cmbcategory.Text.ToUpper() <> "ALL" Then
            cmd.Parameters.AddWithValue("@FILTER", cmbcategory.Text)
        End If
        If txbsearch.Text <> "" Then
            cmd.Parameters.AddWithValue("@SEARCH", "%" & txbsearch.Text & "%")
        End If

        dr = cmd.ExecuteReader

        While dr.Read
            Dim card As New Guna2Panel()
            card.Width = 200
            card.Height = 300
            card.BorderRadius = 15
            card.FillColor = Color.White
            card.BorderColor = Color.DarkOrange
            card.BorderThickness = 2
            card.Margin = New Padding(12)
            card.Padding = New Padding(4)
            card.Cursor = Cursors.Hand

            Dim pic As New Guna2PictureBox()
            pic.Width = 146
            pic.Height = 116
            pic.Location = New Point(22, 13)
            pic.SizeMode = PictureBoxSizeMode.StretchImage
            pic.BackColor = Color.White
            pic.Cursor = Cursors.Hand
            pic.BorderStyle = BorderStyle.FixedSingle

            If Not IsDBNull(dr("product_image")) Then
                Using ms As New MemoryStream(CType(dr("product_image"), Byte()))
                    pic.Image = Image.FromStream(ms)
                End Using
            End If

            Dim lblName As New Label()
            lblName.Text = dr("product_name").ToString().ToUpper()
            lblName.Font = New Font("Segoe UI", 10, FontStyle.Bold)
            lblName.TextAlign = ContentAlignment.MiddleCenter
            lblName.Width = 184
            lblName.Height = 25
            lblName.Location = New Point(4, 148)
            lblName.Cursor = Cursors.Hand
            lblName.BackColor = Color.Transparent

            Dim lblCategory As New Label()
            lblCategory.Text = dr("category_name").ToString().ToUpper()
            lblCategory.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            lblCategory.ForeColor = Color.Red
            lblCategory.TextAlign = ContentAlignment.MiddleCenter
            lblCategory.Width = 184
            lblCategory.Height = 30
            lblCategory.Location = New Point(4, 180)
            lblCategory.Cursor = Cursors.Hand
            lblCategory.BackColor = Color.Transparent

            Dim lblPrice As New Label()
            lblPrice.Text = "₱" & Convert.ToDecimal(dr("price")).ToString("0.00")
            lblPrice.Font = New Font("Segoe UI", 11, FontStyle.Bold)
            lblPrice.ForeColor = Color.ForestGreen
            lblPrice.TextAlign = ContentAlignment.MiddleCenter
            lblPrice.Width = 184
            lblPrice.Height = 30
            lblPrice.Location = New Point(2, 220)
            lblPrice.Cursor = Cursors.Hand
            lblPrice.BackColor = Color.Transparent

            Dim pName As String = dr("product_name").ToString()
            Dim pPrice As Decimal = Convert.ToDecimal(dr("price"))
            Dim pStock As Integer = Convert.ToInt32(dr("quantity"))

            Dim lblStock As New Label()
            lblStock.Text = "STOCK: " & pStock.ToString()
            lblStock.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            lblStock.ForeColor = Color.White

            If pStock > 0 Then
                lblStock.BackColor = Color.DarkOrange
            Else
                lblStock.BackColor = Color.Red
            End If

            lblStock.TextAlign = ContentAlignment.MiddleCenter
            lblStock.Width = 100
            lblStock.Height = 25
            lblStock.Location = New Point(50, 258)
            lblStock.Cursor = Cursors.Hand

            stockLabels(pName) = lblStock

            card.Controls.Add(pic)
            card.Controls.Add(lblName)
            card.Controls.Add(lblCategory)
            card.Controls.Add(lblPrice)
            card.Controls.Add(lblStock)

            AddHandler card.Click, Sub(s, ev) addToOrder(pName, pPrice)
            AddHandler pic.Click, Sub(s, ev) addToOrder(pName, pPrice)
            AddHandler lblName.Click, Sub(s, ev) addToOrder(pName, pPrice)
            AddHandler lblCategory.Click, Sub(s, ev) addToOrder(pName, pPrice)
            AddHandler lblPrice.Click, Sub(s, ev) addToOrder(pName, pPrice)
            AddHandler lblStock.Click, Sub(s, ev) addToOrder(pName, pPrice)

            ' ==========================================
            ' HOVER EFFECTS — Card only
            ' ==========================================
            Dim enterCard As EventHandler = Sub(s, ev)
                                                card.FillColor = Color.FromArgb(255, 248, 220) ' light cream
                                                card.BorderColor = Color.OrangeRed
                                                card.BorderThickness = 3
                                            End Sub

            Dim leaveCard As EventHandler = Sub(s, ev)
                                                ' Only reset if the cursor truly left the whole card
                                                If Not card.ClientRectangle.Contains(card.PointToClient(Cursor.Position)) Then
                                                    card.FillColor = Color.White
                                                    card.BorderColor = Color.DarkOrange
                                                    card.BorderThickness = 2
                                                End If
                                            End Sub

            AddHandler card.MouseEnter, enterCard
            AddHandler card.MouseLeave, leaveCard
            AddHandler pic.MouseEnter, enterCard
            AddHandler pic.MouseLeave, leaveCard
            AddHandler lblName.MouseEnter, enterCard
            AddHandler lblName.MouseLeave, leaveCard
            AddHandler lblCategory.MouseEnter, enterCard
            AddHandler lblCategory.MouseLeave, leaveCard
            AddHandler lblPrice.MouseEnter, enterCard
            AddHandler lblPrice.MouseLeave, leaveCard
            AddHandler lblStock.MouseEnter, enterCard
            AddHandler lblStock.MouseLeave, leaveCard

            FlowLayoutPanel1.Controls.Add(card)
        End While

        dr.Close()
        cmd.Dispose()
    End Sub

    ' ==========================================
    ' ORDER LOGIC
    ' ==========================================
    Sub addToOrder(productName As String, price As Decimal)
        Dim currentStock As Integer = 0
        If stockLabels.ContainsKey(productName) Then
            currentStock = Convert.ToInt32(stockLabels(productName).Text.Replace("STOCK: ", ""))
        End If

        If currentStock <= 0 Then
            MessageBox.Show("Cannot add item. Out of stock!", "Out of Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim existingRow As DataGridViewRow = Nothing
        For Each row As DataGridViewRow In datagridvieworder.Rows
            If row.Cells(1).Value.ToString() = productName Then
                existingRow = row
                Exit For
            End If
        Next

        If existingRow IsNot Nothing Then
            existingRow.Cells(0).Value = CInt(existingRow.Cells(0).Value) + 1
        Else
            datagridvieworder.Rows.Add(1, productName, price, "VOID")
        End If

        Dim newStock As Integer = currentStock - 1
        stockLabels(productName).Text = "STOCK: " & newStock.ToString()

        If newStock <= 0 Then
            stockLabels(productName).BackColor = Color.Red
        Else
            stockLabels(productName).BackColor = Color.DarkOrange
        End If

        updateTotals()
    End Sub

    Sub updateTotals()
        totalSubtotal = 0

        For Each row As DataGridViewRow In datagridvieworder.Rows
            totalSubtotal += CInt(row.Cells(0).Value) * CDec(row.Cells(2).Value)
        Next

        totalTax = totalSubtotal * taxRate
        totalAmount = totalSubtotal + totalTax

        lblsubtotal2.Text = "₱" & totalSubtotal.ToString("0.00")
        lblrtpercentage.Text = "₱" & totalTax.ToString("0.00")
        lbltotal2.Text = "₱" & totalAmount.ToString("0.00")

        updateChange()
    End Sub

    Sub updateChange()
        Dim paid As Decimal = 0
        If Decimal.TryParse(txbap.Text, paid) Then
            Dim change As Decimal = paid - totalAmount
            If change < 0 Then change = 0
            lblchange.Text = "₱" & change.ToString("0.00")
        Else
            lblchange.Text = "₱0.00"
        End If
    End Sub

    Sub clearOrder()
        datagridvieworder.Rows.Clear()
        txbap.Clear()
        cmbpm.SelectedIndex = -1
        lblchange.Text = "₱0.00"
        ErrorProvider1.Clear()
        txbap.BorderColor = Color.Black
        cmbpm.BorderColor = Color.Black
        updateTotals()
        loadProducts()
    End Sub

    ' ==========================================
    ' SAVE TO DATABASE
    ' ==========================================
    Sub savePayment(paid As Decimal)

        query = "INSERT INTO payments " &
                "(transaction_id, cashier_id, cashier_fullname, payment_method, " &
                " subtotal_amount, tax_rate, amount_paid) " &
                "VALUES (@TXN, @CID, @NAME, @METHOD, @SUB, @RATE, @PAID); " &
                "SELECT SCOPE_IDENTITY();"

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@TXN", txbordern.Text)
        cmd.Parameters.AddWithValue("@CID", CurrentUser.user_code)
        cmd.Parameters.AddWithValue("@NAME", CurrentUser.fullname)
        cmd.Parameters.AddWithValue("@METHOD", cmbpm.Text)
        cmd.Parameters.AddWithValue("@SUB", totalSubtotal)
        cmd.Parameters.AddWithValue("@RATE", taxRate)
        cmd.Parameters.AddWithValue("@PAID", paid)

        Dim paymentId As Integer = Convert.ToInt32(cmd.ExecuteScalar())
        cmd.Dispose()

        For Each row As DataGridViewRow In datagridvieworder.Rows
            Dim pName As String = row.Cells(1).Value.ToString()
            Dim pPrice As Decimal = CDec(row.Cells(2).Value)
            Dim pQty As Integer = CInt(row.Cells(0).Value)

            query = "INSERT INTO transaction_records " &
                    "(payment_id, product_name, product_price, product_quantity) " &
                    "VALUES (@PID, @NAME, @PRICE, @QTY)"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@PID", paymentId)
            cmd.Parameters.AddWithValue("@NAME", pName)
            cmd.Parameters.AddWithValue("@PRICE", pPrice)
            cmd.Parameters.AddWithValue("@QTY", pQty)
            cmd.ExecuteNonQuery()
            cmd.Dispose()

            query = "UPDATE menu_items SET quantity = quantity - @QTY WHERE product_name = @NAME"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@QTY", pQty)
            cmd.Parameters.AddWithValue("@NAME", pName)
            cmd.ExecuteNonQuery()
            cmd.Dispose()
        Next
    End Sub

    ' ==========================================
    ' PRINT RECEIPT (DIRECT PRINT TO DEFAULT PRINTER)
    ' ==========================================
    Private Sub PrintReceipt(paid As Decimal)
        Dim sb As New System.Text.StringBuilder()
        Dim line As String = "--------------------------------"

        sb.AppendLine("        DS BURGER SHOP        ")
        sb.AppendLine("   Fresh Burger, Great Taste!  ")
        sb.AppendLine(line)
        sb.AppendLine("TXN #  : " & txbordern.Text)
        sb.AppendLine("Date   : " & DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"))
        sb.AppendLine("Cashier: " & CurrentUser.fullname)
        sb.AppendLine("Payment: " & cmbpm.Text)
        sb.AppendLine(line)
        sb.AppendLine("ITEM             QTY   AMOUNT")

        For Each row As DataGridViewRow In datagridvieworder.Rows
            Dim itemName As String = row.Cells(1).Value.ToString().ToUpper()
            If itemName.Length > 15 Then
                itemName = itemName.Substring(0, 15)
            End If
            Dim qty As Integer = CInt(row.Cells(0).Value)
            Dim price As Decimal = CDec(row.Cells(2).Value)

            sb.AppendLine(itemName.PadRight(17) & qty.ToString().PadLeft(3) & (qty * price).ToString("N2").PadLeft(9))
        Next

        sb.AppendLine(line)
        sb.AppendLine("Subtotal:".PadRight(20) & "P " & totalSubtotal.ToString("N2"))
        sb.AppendLine("Tax (2%):".PadRight(20) & "P " & totalTax.ToString("N2"))
        sb.AppendLine("TOTAL:".PadRight(20) & "P " & totalAmount.ToString("N2"))
        sb.AppendLine("Paid:".PadRight(20) & "P " & paid.ToString("N2"))
        sb.AppendLine("Change:".PadRight(20) & "P " & (paid - totalAmount).ToString("N2"))
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
    ' BUTTONS
    ' ==========================================
    Private Sub btnpn_Click(sender As Object, e As EventArgs) Handles btnpn.Click
        ' 1. Check if order is empty
        If datagridvieworder.Rows.Count = 0 Then
            MsgBox("Order is empty. Please add items.", MsgBoxStyle.Exclamation, "Warning")
            Return
        End If

        ' 2. Check if payment method is selected
        If cmbpm.SelectedIndex = -1 Then
            MsgBox("Please select a payment method.", MsgBoxStyle.Exclamation, "Warning")
            Return
        End If

        ' 3. Check if amount paid is valid
        Dim paid As Decimal = 0
        If Not Decimal.TryParse(txbap.Text, paid) Or paid < totalAmount Then
            MsgBox("Amount paid must be at least ₱" & totalAmount.ToString("0.00"), MsgBoxStyle.Exclamation, "Warning")
            Return
        End If

        ' 4. Confirm Payment
        Dim confirm As DialogResult = MessageBox.Show(
            "Confirm payment?" & vbCrLf & vbCrLf &
            "Transaction ID: " & txbordern.Text & vbCrLf &
            "Subtotal: ₱" & totalSubtotal.ToString("0.00") & vbCrLf &
            "Tax (2%): ₱" & totalTax.ToString("0.00") & vbCrLf &
            "Total: ₱" & totalAmount.ToString("0.00") & vbCrLf &
            "Paid: ₱" & paid.ToString("0.00") & vbCrLf &
            "Change: ₱" & (paid - totalAmount).ToString("0.00"),
            "Confirm Payment", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If confirm = DialogResult.No Then Return

        ' 5. Save to Database
        Try
            conn()
            savePayment(paid)
        Catch ex As Exception
            MsgBox("Error saving: " & ex.Message, MsgBoxStyle.Critical, "Error")
            Return
        End Try

        ' 6. Print Receipt (Direct Print)
        PrintReceipt(paid)

        ' 7. Show Success Message on Top
        MsgBox("Payment Successful!" & vbCrLf & "Transaction #: " & txbordern.Text, MsgBoxStyle.Information, "Success")

        ' 8. Clear Screen and Reset TXN #
        clearOrder()
        CreateNewAutoNumber()
        SP_AutoNumber()
    End Sub

    Private Sub btnclear_Click(sender As Object, e As EventArgs) Handles btnclear.Click
        clearOrder()
        SP_AutoNumber()
    End Sub

    ' ==========================================
    ' VOID ITEM — REMOVES 1 QTY PER CLICK
    ' ==========================================
    Private Sub datagridvieworder_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridvieworder.CellContentClick
        If e.RowIndex < 0 Then Return

        ' If "VOID ITEM" button is clicked
        If e.ColumnIndex = 3 Then
            Dim row As DataGridViewRow = datagridvieworder.Rows(e.RowIndex)
            Dim productName As String = row.Cells(1).Value.ToString()
            Dim currentQty As Integer = CInt(row.Cells(0).Value)

            ' Return 1 stock to the label
            If stockLabels.ContainsKey(productName) Then
                Dim currentStock As Integer = Convert.ToInt32(stockLabels(productName).Text.Replace("STOCK: ", ""))
                Dim newStock As Integer = currentStock + 1
                stockLabels(productName).Text = "STOCK: " & newStock

                ' Update stock label color
                If newStock > 0 Then
                    stockLabels(productName).BackColor = Color.DarkOrange
                Else
                    stockLabels(productName).BackColor = Color.Red
                End If
            End If

            ' Decrease quantity by 1
            If currentQty > 1 Then
                row.Cells(0).Value = currentQty - 1
            Else
                ' Quantity is 1 → remove the row entirely
                datagridvieworder.Rows.RemoveAt(e.RowIndex)
            End If

            updateTotals()
        End If
    End Sub

    Private Sub txbap_TextChanged(sender As Object, e As EventArgs) Handles txbap.TextChanged
        updateChange()
    End Sub

    Private Sub txbap_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbap.KeyPress
        ' Only allow numbers, backspace, and one decimal point
        If Not (Char.IsDigit(e.KeyChar) OrElse Char.IsControl(e.KeyChar) OrElse e.KeyChar = "."c) Then
            e.Handled = True
        End If
        If e.KeyChar = "."c AndAlso txbap.Text.Contains(".") Then
            e.Handled = True
        End If
    End Sub

    Private Sub txbsearch_TextChanged(sender As Object, e As EventArgs) Handles txbsearch.TextChanged
        loadProducts()
    End Sub

    Private Sub cmbcategory_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbcategory.SelectedIndexChanged
        loadProducts()
    End Sub

End Class