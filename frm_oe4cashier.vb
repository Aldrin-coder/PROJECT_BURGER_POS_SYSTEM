Imports System.Data.SqlClient
Imports System.Drawing.Printing
Imports System.IO
Imports Guna.UI2.WinForms

Public Class frm_oe4cashier

    Dim totalSubtotal As Decimal = 0
    Dim taxRate As Decimal = 0.02
    Dim totalAmount As Decimal = 0

    ' Track the stock labels on the cards
    Private stockLabels As New Dictionary(Of String, Label)()

    ' ==========================================
    ' FORM LOAD & SETUP
    ' ==========================================
    Private Sub frm_oe4cashier_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        loadPaymentMethods()
        loadCategories()
        SP_AutoNumber()
        loadProducts()
        bordercolor()
    End Sub

    Sub bordercolor()
        txbap.BorderColor = Color.Black
        cmbpm.BorderColor = Color.Black
    End Sub

    ' ==========================================
    ' AUTO NUMBER GENERATOR
    ' ==========================================
    Sub SP_AutoNumber()
        conn()
        Dim number As String
        query = "SELECT Max(NewNumber) FROM AutoNumberTxn WHERE Pfx = 'TXN-'"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        With cmd
            If IsDBNull(cmd.ExecuteScalar) Then
                CreateNewAutoNumber()
                query = "SELECT Max(NewNumber) FROM AutoNumberTxn WHERE Pfx = 'TXN-'"
                cmd = New SqlClient.SqlCommand(query, sqlconn)
                txbordern.Text = Convert.ToString(cmd.ExecuteScalar)
                cmd.Dispose()
            Else
                number = Convert.ToString(cmd.ExecuteScalar)
                txbordern.Text = number
            End If
        End With
        cmd.Dispose()
    End Sub

    Sub CreateNewAutoNumber()
        Try
            Dim cmd As New SqlCommand
            With cmd
                .Connection = sqlconn
                .CommandText = "sp_AutoNumberTxn"
                .CommandType = CommandType.StoredProcedure
                .Parameters.AddWithValue("@pfx", "TXN-")
            End With
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
        cmbpm.Items.AddRange({"CASH", "GCASH", "MAYA"})
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

        If cmbcategory.Items.Count > 0 Then cmbcategory.SelectedIndex = 0
    End Sub

    Sub loadProducts()
        FlowLayoutPanel1.Controls.Clear()
        FlowLayoutPanel1.Padding = New Padding(15)
        stockLabels.Clear()

        ' Pull the quantity (stock) from the database
        query = "SELECT m.item_code, m.product_name, m.price, m.quantity, m.product_image, c.category_name " &
            "FROM menu_items m " &
            "INNER JOIN categories c ON m.category_id = c.category_id " &
            "WHERE m.IsActive = 1 "

        If cmbcategory.Text <> "" AndAlso cmbcategory.Text.ToUpper() <> "ALL" Then
            query &= " AND c.category_name = @FILTER "
        End If
        If txbsearch.Text <> "" Then
            query &= " AND (LOWER(m.item_code) LIKE LOWER(@SEARCH) OR LOWER(m.product_name) LIKE LOWER(@SEARCH)) "
        End If
        query &= " ORDER BY m.product_name"

        conn()
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        If cmbcategory.Text <> "" AndAlso cmbcategory.Text.ToUpper() <> "ALL" Then cmd.Parameters.AddWithValue("@FILTER", cmbcategory.Text)
        If txbsearch.Text <> "" Then cmd.Parameters.AddWithValue("@SEARCH", "%" & txbsearch.Text & "%")

        dr = cmd.ExecuteReader
        While dr.Read
            ' --- MAIN CARD ---
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

            ' --- PICTURE BOX ---
            Dim pic As New Guna2PictureBox()
            pic.Width = 146
            pic.Height = 116
            pic.Location = New Point(22, 13)
            pic.SizeMode = PictureBoxSizeMode.StretchImage
            pic.BackColor = Color.White
            pic.Cursor = Cursors.Hand

            If Not IsDBNull(dr("product_image")) Then
                Using ms As New MemoryStream(CType(dr("product_image"), Byte()))
                    pic.Image = Image.FromStream(ms)
                End Using
            End If

            ' --- LABELS ---
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

            ' --- STOCK DISPLAY LABEL ---
            Dim pName As String = dr("product_name").ToString()
            Dim pPrice As Decimal = Convert.ToDecimal(dr("price"))
            Dim pStock As Integer = Convert.ToInt32(dr("quantity"))

            Dim lblStock As New Label()
            lblStock.Text = "STOCK: " & pStock.ToString()
            lblStock.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            lblStock.ForeColor = Color.White
            lblStock.BackColor = If(pStock > 0, Color.DarkOrange, Color.Red)
            lblStock.TextAlign = ContentAlignment.MiddleCenter
            lblStock.Width = 100
            lblStock.Height = 25
            lblStock.Location = New Point(50, 258)
            lblStock.Cursor = Cursors.Hand

            ' Save the label reference
            stockLabels(pName) = lblStock

            ' --- ADD TO MAIN CARD ---
            card.Controls.Add(pic)
            card.Controls.Add(lblName)
            card.Controls.Add(lblCategory)
            card.Controls.Add(lblPrice)
            card.Controls.Add(lblStock)

            ' --- CLICK EVENTS ---
            AddHandler card.Click, Sub(s, ev) addToOrder(pName, pPrice)
            AddHandler pic.Click, Sub(s, ev) addToOrder(pName, pPrice)
            AddHandler lblName.Click, Sub(s, ev) addToOrder(pName, pPrice)
            AddHandler lblCategory.Click, Sub(s, ev) addToOrder(pName, pPrice)
            AddHandler lblPrice.Click, Sub(s, ev) addToOrder(pName, pPrice)
            AddHandler lblStock.Click, Sub(s, ev) addToOrder(pName, pPrice)

            ' --- HOVER EFFECT ---
            ApplyHover(card, Color.FromArgb(236, 112, 60), Color.White, pic, lblName, lblCategory, lblPrice, lblStock)

            FlowLayoutPanel1.Controls.Add(card)
        End While
        dr.Close()
        cmd.Dispose()
    End Sub

    Private Sub ApplyHover(card As Guna2Panel, hoverColor As Color, baseColor As Color, ParamArray children As Control())
        Dim onEnter As EventHandler = Sub(s, ev) card.FillColor = hoverColor

        Dim onLeave As EventHandler = Sub(s, ev)
                                          If Not card.ClientRectangle.Contains(card.PointToClient(Control.MousePosition)) Then
                                              card.FillColor = baseColor
                                          End If
                                      End Sub

        AddHandler card.MouseEnter, onEnter
        AddHandler card.MouseLeave, onLeave

        For Each c As Control In children
            AddHandler c.MouseEnter, onEnter
            AddHandler c.MouseLeave, onLeave
        Next
    End Sub

    ' ==========================================
    ' ORDER LOGIC (INSTANT STOCK DEDUCTION)
    ' ==========================================
    Sub addToOrder(productName As String, price As Decimal)
        ' 1. Check current stock from the label
        Dim currentStock As Integer = 0
        If stockLabels.ContainsKey(productName) Then
            currentStock = Convert.ToInt32(stockLabels(productName).Text.Replace("STOCK: ", ""))
        End If

        ' 2. Block if out of stock
        If currentStock <= 0 Then
            MessageBox.Show("Cannot add item. Out of stock!", "Out of Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        ' 3. Add to grid (Increment if exists)
        Dim existingRow As DataGridViewRow = Nothing
        For Each row As DataGridViewRow In datagridvieworder.Rows
            If row.Cells(1).Value IsNot Nothing AndAlso
               row.Cells(1).Value.ToString() = productName Then
                existingRow = row
                Exit For
            End If
        Next

        If existingRow IsNot Nothing Then
            existingRow.Cells(0).Value = CInt(existingRow.Cells(0).Value) + 1
        Else
            datagridvieworder.Rows.Add(1, productName, price, "VOID")
        End If

        ' 4. Deduct 1 from the UI stock label instantly
        Dim newStock As Integer = currentStock - 1
        stockLabels(productName).Text = "STOCK: " & newStock.ToString()

        ' Change color to red if stock hits 0
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

        Dim tax As Decimal = totalSubtotal * taxRate
        totalAmount = totalSubtotal + tax

        lblsubtotal2.Text = "₱" & totalSubtotal.ToString("0.00")
        lbltr2.Text = "₱" & tax.ToString("0.00")
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
        bordercolor()
        updateTotals()

        ' Reload products to reset stock display to original DB values
        loadProducts()
    End Sub

    ' ==========================================
    ' SAVE PAYMENT (DEDUCTS FROM DB STOCK)
    ' ==========================================
    Sub savePayment(paid As Decimal)
        query = "INSERT INTO payments " &
                "(transaction_id, cashier_id, cashier_fullname, payment_method, " &
                " subtotal_amount, amount_paid) " &
                "VALUES (@TXN, @CID, @NAME, @METHOD, @SUB, @PAID); " &
                "SELECT SCOPE_IDENTITY();"

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        With cmd.Parameters
            .AddWithValue("@TXN", txbordern.Text)
            .AddWithValue("@CID", CurrentUser.user_code)
            .AddWithValue("@NAME", CurrentUser.fullname)
            .AddWithValue("@METHOD", cmbpm.Text)
            .AddWithValue("@SUB", totalSubtotal)
            .AddWithValue("@PAID", paid)
        End With
        Dim paymentId As Integer = Convert.ToInt32(cmd.ExecuteScalar())
        cmd.Dispose()

        For Each row As DataGridViewRow In datagridvieworder.Rows
            ' 1. Insert into transaction records
            query = "INSERT INTO transaction_records " &
                    "(payment_id, product_name, product_price, product_quantity) " &
                    "VALUES (@PID, @NAME, @PRICE, @QTY)"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            With cmd.Parameters
                .AddWithValue("@PID", paymentId)
                .AddWithValue("@NAME", row.Cells(1).Value.ToString())
                .AddWithValue("@PRICE", CDec(row.Cells(2).Value))
                .AddWithValue("@QTY", CInt(row.Cells(0).Value))
            End With
            cmd.ExecuteNonQuery()
            cmd.Dispose()

            ' 2. Deduct the stock permanently from the database
            Dim productName As String = row.Cells(1).Value.ToString()
            Dim qtyPurchased As Integer = CInt(row.Cells(0).Value)

            query = "UPDATE menu_items SET quantity = quantity - @QTY WHERE product_name = @NAME"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            With cmd.Parameters
                .AddWithValue("@QTY", qtyPurchased)
                .AddWithValue("@NAME", productName)
            End With
            cmd.ExecuteNonQuery()
            cmd.Dispose()
        Next
    End Sub

    ' ==========================================
    ' PAY NOW EVENT (CENTERED & PUSHED DOWN RECEIPT)
    ' ==========================================
    Private Sub btnpn_Click(sender As Object, e As EventArgs) Handles btnpn.Click
        ErrorProvider1.Clear()
        bordercolor()
        Dim isValid As Boolean = True

        If datagridvieworder.Rows.Count = 0 Then
            ErrorProvider1.SetError(datagridvieworder, "Order is empty — add at least 1 item.")
            isValid = False
        End If

        If cmbpm.SelectedIndex = -1 Then
            ErrorProvider1.SetError(cmbpm, "Please select a payment method.")
            cmbpm.BorderColor = Color.Red
            isValid = False
        End If

        Dim paid As Decimal
        If txbap.Text = "" Then
            ErrorProvider1.SetError(txbap, "Enter the amount paid.")
            txbap.BorderColor = Color.Red
            isValid = False
        ElseIf Not Decimal.TryParse(txbap.Text, paid) OrElse paid < totalAmount Then
            ErrorProvider1.SetError(txbap, "Amount paid must be at least ₱" & totalAmount.ToString("0.00"))
            txbap.BorderColor = Color.Red
            isValid = False
        End If

        If Not isValid Then
            MsgBox("Please fill in the required fields.", MsgBoxStyle.Exclamation, "Incomplete")
            Return
        End If

        Dim confirm As DialogResult = MessageBox.Show(
            "Confirm payment?" & vbCrLf & vbCrLf &
            "Transaction ID: " & txbordern.Text & vbCrLf &
            "Total: ₱" & totalAmount.ToString("0.00") & vbCrLf &
            "Paid: ₱" & paid.ToString("0.00") & vbCrLf &
            "Change: ₱" & (paid - totalAmount).ToString("0.00"),
            "Confirm Payment", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If confirm = DialogResult.No Then Return

        Try
            conn()
            savePayment(paid)

            ' --- Build Receipt Text (CENTERED) ---
            Dim sb As New System.Text.StringBuilder()
            Dim lineWidth As Integer = 32 ' Characters per line for Consolas 9 on 58mm paper

            ' Helper function to center text
            Dim CenterText As Func(Of String, String) = Function(txt)
                                                            If txt.Length >= lineWidth Then Return txt
                                                            Dim leftPad As Integer = (lineWidth - txt.Length) \ 2
                                                            Return txt.PadLeft(txt.Length + leftPad).PadRight(lineWidth)
                                                        End Function

            ' --- HEADER ---
            sb.AppendLine(CenterText("DS BURGER SHOP"))
            sb.AppendLine(CenterText("Fresh Burger, Great Taste!"))
            sb.AppendLine("".PadRight(lineWidth, "-"c))

            ' --- TRANSACTION INFO ---
            sb.AppendLine("TXN #  : " & txbordern.Text)
            sb.AppendLine("Date   : " & DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"))
            sb.AppendLine("Cashier: " & CurrentUser.fullname)
            sb.AppendLine("Cash ID: " & CurrentUser.user_code)
            sb.AppendLine("Payment: " & cmbpm.Text)
            sb.AppendLine("".PadRight(lineWidth, "-"c))

            ' --- ITEMS HEADER ---
            sb.AppendLine("ITEM             QTY   AMOUNT")

            ' --- ITEM LIST ---
            For Each row As DataGridViewRow In datagridvieworder.Rows
                Dim itemName As String = row.Cells(1).Value.ToString().ToUpper()
                If itemName.Length > 15 Then itemName = itemName.Substring(0, 15)
                Dim qty As Integer = CInt(row.Cells(0).Value)
                Dim price As Decimal = CDec(row.Cells(2).Value)
                sb.AppendLine(itemName.PadRight(17) & qty.ToString().PadLeft(3) & (qty * price).ToString("N2").PadLeft(9))
            Next

            sb.AppendLine("".PadRight(lineWidth, "-"c))

            ' --- TOTALS ---
            sb.AppendLine("Subtotal:".PadRight(20) & "P " & totalSubtotal.ToString("N2"))
            sb.AppendLine("Tax (2%):".PadRight(20) & "P " & (totalAmount - totalSubtotal).ToString("N2"))
            sb.AppendLine("TOTAL:".PadRight(20) & "P " & totalAmount.ToString("N2"))
            sb.AppendLine("Paid:".PadRight(20) & "P " & paid.ToString("N2"))
            sb.AppendLine("Change:".PadRight(20) & "P " & (paid - totalAmount).ToString("N2"))
            sb.AppendLine("".PadRight(lineWidth, "-"c))

            ' --- FOOTER ---
            sb.AppendLine(CenterText("THANK YOU FOR YOUR ORDER!"))
            sb.AppendLine(CenterText("Please come again!"))

            ' --- Setup Printer ---
            Dim printDoc As New System.Drawing.Printing.PrintDocument()
            ' Increased height to 900 so it doesn't cut off
            printDoc.DefaultPageSettings.PaperSize = New System.Drawing.Printing.PaperSize("Thermal58mm", 220, 900)
            printDoc.DefaultPageSettings.Margins = New System.Drawing.Printing.Margins(0, 0, 0, 0)

            ' X = 10 (centered), Y = 80 (pushed down from top)
            AddHandler printDoc.PrintPage, Sub(s, ev)
                                               ev.Graphics.DrawString(sb.ToString(), New Font("Consolas", 8), Brushes.Black, 7, 80)
                                           End Sub



            ' Dim preview As New PrintPreviewDialog()
            'preview.Document = printDoc
            'preview.WindowState = FormWindowState.Maximized
            'preview.ShowDialog()




            printDoc.PrinterSettings.PrinterName = "Name of your printer"
            printDoc.Print()

            clearOrder()
            CreateNewAutoNumber()
            SP_AutoNumber()

        Catch ex As Exception
            MsgBox("Error processing payment: " & ex.Message, MsgBoxStyle.Critical, "Error")
        End Try
    End Sub

    Private Sub btnclear_Click(sender As Object, e As EventArgs) Handles btnclear.Click
        clearOrder()
        SP_AutoNumber()
    End Sub

    ' If the user clicks VOID on the grid, add 1 back to the display stock
    Private Sub datagridvieworder_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridvieworder.CellContentClick
        If e.RowIndex < 0 Then Return
        If e.ColumnIndex = 3 Then
            Dim productName As String = datagridvieworder.Rows(e.RowIndex).Cells(1).Value.ToString()
            Dim qtyVoided As Integer = CInt(datagridvieworder.Rows(e.RowIndex).Cells(0).Value)

            ' Add the voided quantity back to the stock label
            If stockLabels.ContainsKey(productName) Then
                Dim currentStock As Integer = Convert.ToInt32(stockLabels(productName).Text.Replace("STOCK: ", ""))
                Dim newStock As Integer = currentStock + qtyVoided
                stockLabels(productName).Text = "STOCK: " & newStock

                If newStock > 0 Then stockLabels(productName).BackColor = Color.DarkOrange
            End If

            datagridvieworder.Rows.RemoveAt(e.RowIndex)
            updateTotals()
        End If
    End Sub

    Private Sub txbap_TextChanged(sender As Object, e As EventArgs) Handles txbap.TextChanged
        updateChange()
    End Sub

    Private Sub txbap_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbap.KeyPress
        If Not (Char.IsDigit(e.KeyChar) OrElse Char.IsControl(e.KeyChar) OrElse e.KeyChar = "."c) Then
            e.Handled = True
            Return
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