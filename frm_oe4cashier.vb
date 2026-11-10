Imports System.Data.SqlClient
Imports System.IO
Imports System.Drawing
Imports Guna.UI2.WinForms

Public Class frm_oe4cashier

    Dim totalSubtotal As Decimal = 0
    Dim taxRate As Decimal = 0.02
    Dim totalAmount As Decimal = 0

    Dim transactionId As String = ""

    Private Sub frm_oe4cashier_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        loadPaymentMethods()
        loadCategories()
        SP_AutoNumber()
        loadProducts()

    End Sub

    Sub bordercolor()
        txbap.BorderColor = Color.Black
        cmbpm.BorderColor = Color.Black
    End Sub
    Private Sub btnclear_Click(sender As Object, e As EventArgs) Handles btnclear.Click
        clearOrder()
        SP_AutoNumber()

    End Sub
    Sub loadPaymentMethods()
        cmbpm.Items.Clear()
        cmbpm.Items.AddRange({"CASH", "GCASH", "MAYA"})
        cmbpm.SelectedIndex = -1
    End Sub


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
        FlowLayoutPanel1.AutoScroll = True

        query = "SELECT m.item_code, m.product_name, m.price, m.product_image, c.category_name " &
            "FROM menu_items m " &
            "INNER JOIN categories c ON m.category_id = c.category_id " &
            "WHERE m.IsActive = 1 "

        If cmbcategory.Text <> "" AndAlso cmbcategory.Text.ToUpper() <> "ALL" Then
            query &= " AND c.category_name = @FILTER "
        End If
        If txbsearch.Text.Trim() <> "" Then
            query &= " AND (LOWER(m.item_code) LIKE LOWER(@SEARCH) OR LOWER(m.product_name) LIKE LOWER(@SEARCH)) "
        End If
        query &= " ORDER BY m.product_name"

        conn()
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        If cmbcategory.Text <> "" AndAlso cmbcategory.Text.ToUpper() <> "ALL" Then cmd.Parameters.AddWithValue("@FILTER", cmbcategory.Text)
        If txbsearch.Text.Trim() <> "" Then cmd.Parameters.AddWithValue("@SEARCH", "%" & txbsearch.Text.Trim() & "%")

        dr = cmd.ExecuteReader
        While dr.Read
            ' ⭐ Card panel
            Dim card As New Panel()
            card.Width = 200
            card.Height = 280
            card.BackColor = Color.White
            card.BorderStyle = BorderStyle.FixedSingle
            card.Margin = New Padding(8)
            card.Padding = New Padding(4)
            card.Cursor = Cursors.Hand
            card.BorderStyle = BorderStyle.FixedSingle
            AddHandler card.MouseEnter, Sub(s, ev) card.BackColor = Color.Orange
            AddHandler card.MouseLeave, Sub(s, ev) card.BackColor = Color.White

            ' ⭐ Image
            Dim pic As New PictureBox()
            pic.Width = 100
            pic.Height = 100
            pic.Location = New Point(40, 11)
            pic.SizeMode = PictureBoxSizeMode.StretchImage
            pic.BackColor = Color.WhiteSmoke
            pic.BorderStyle = BorderStyle.FixedSingle
            AddHandler card.MouseEnter, Sub(s, ev) card.BackColor = Color.Orange
            AddHandler card.MouseLeave, Sub(s, ev) card.BackColor = Color.White
            If Not IsDBNull(dr("product_image")) Then
                Using ms As New MemoryStream(CType(dr("product_image"), Byte()))
                    pic.Image = Image.FromStream(ms)
                End Using
            End If
            Dim lblName As New Label()
            lblName.Text = dr("product_name").ToString()
            lblName.Font = New Font("Segoe UI", 9, FontStyle.Bold)
            lblName.TextAlign = ContentAlignment.MiddleCenter
            lblName.Width = 184
            lblName.Height = 25
            lblName.Location = New Point(4, 148)
            lblName.AutoEllipsis = True
            AddHandler lblName.MouseEnter, Sub(s, ev) card.BackColor = Color.Orange
            AddHandler lblName.MouseLeave, Sub(s, ev) card.BackColor = Color.White

            ' ⭐ CATEGORY LABEL
            Dim lblCategory As New Label()
            lblCategory.Text = dr("category_name").ToString()
            lblCategory.Font = New Font("Segoe UI", 8, FontStyle.Italic)
            lblCategory.ForeColor = Color.Gray
            lblCategory.TextAlign = ContentAlignment.MiddleCenter
            lblCategory.Width = 184
            lblCategory.Height = 20
            lblCategory.Location = New Point(4, 173)
            lblCategory.AutoEllipsis = True
            AddHandler lblCategory.MouseEnter, Sub(s, ev) card.BackColor = Color.Orange
            AddHandler lblCategory.MouseLeave, Sub(s, ev) card.BackColor = Color.White

            Dim lblPrice As New Label()
            lblPrice.Text = "₱" & Convert.ToDecimal(dr("price")).ToString("0.00")
            lblPrice.Font = New Font("Segoe UI", 11, FontStyle.Bold)
            lblPrice.ForeColor = Color.ForestGreen
            lblPrice.TextAlign = ContentAlignment.MiddleCenter
            lblPrice.Width = 184
            lblPrice.Height = 30
            lblPrice.Location = New Point(4, 195)
            AddHandler lblPrice.MouseEnter, Sub(s, ev) card.BackColor = Color.Orange
            AddHandler lblPrice.MouseLeave, Sub(s, ev) card.BackColor = Color.White
            ' ⭐ Add to card
            card.Controls.Add(pic)
            card.Controls.Add(lblName)
            card.Controls.Add(lblPrice)

            ' ⭐ Store data for click
            Dim pName As String = dr("product_name").ToString()
            Dim pPrice As Decimal = Convert.ToDecimal(dr("price"))

            AddHandler card.Click, Sub(s, ev) addToOrder(pName, pPrice)
            AddHandler pic.Click, Sub(s, ev) addToOrder(pName, pPrice)
            AddHandler lblName.Click, Sub(s, ev) addToOrder(pName, pPrice)
            AddHandler lblPrice.Click, Sub(s, ev) addToOrder(pName, pPrice)

            FlowLayoutPanel1.Controls.Add(card)
        End While
        dr.Close()
        cmd.Dispose()
    End Sub
    Sub addToOrder(productName As String, price As Decimal)
        For Each row As DataGridViewRow In datagridvieworder.Rows
            If row.Cells(1).Value.ToString() = productName Then
                row.Cells(0).Value = CInt(row.Cells(0).Value) + 1
                updateTotals()
                Return
            End If
        Next

        datagridvieworder.Rows.Add(1, productName, price, "VOID")
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
    Private Sub datagridvieworder_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridvieworder.CellContentClick
        If e.RowIndex < 0 Then Return
        If e.ColumnIndex = 3 Then
            datagridvieworder.Rows.RemoveAt(e.RowIndex)
            updateTotals()
        End If
    End Sub
    Private Sub txbap_TextChanged(sender As Object, e As EventArgs) Handles txbap.TextChanged
        updateChange()
    End Sub


    Sub clearOrder()
        datagridvieworder.Rows.Clear()
        txbap.Clear()
        cmbpm.SelectedIndex = -1
        lblchange.Text = "₱0.00"
        ErrorProvider1.Clear()
        bordercolor()
        datagridvieworder.BackgroundColor = Color.White
        updateTotals()
    End Sub
    Private Sub txbsearch_TextChanged(sender As Object, e As EventArgs) Handles txbsearch.TextChanged
        loadProducts()
    End Sub

    Private Sub cmbcategory_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbcategory.SelectedIndexChanged
        loadProducts()
    End Sub


    Private Sub FlowLayoutPanel1_Paint(sender As Object, e As PaintEventArgs) Handles FlowLayoutPanel1.Paint

    End Sub

    Private Sub btnpn_Click(sender As Object, e As EventArgs) Handles btnpn.Click
        ' ⭐ Reset old errors
        ErrorProvider1.Clear()
        bordercolor()
        datagridvieworder.BackgroundColor = Color.White

        Dim isValid As Boolean = True

        ' ⭐ CHECK 1: Order not empty
        If datagridvieworder.Rows.Count = 0 Then
            ErrorProvider1.SetError(datagridvieworder, "Order is empty — add at least 1 item.")
            datagridvieworder.BackgroundColor = Color.MistyRose
            isValid = False
        End If

        ' ⭐ CHECK 2: Payment method selected
        If cmbpm.SelectedIndex = -1 Then
            ErrorProvider1.SetError(cmbpm, "Please select a payment method.")
            cmbpm.BorderColor = Color.Red
            isValid = False
        End If

        ' ⭐ CHECK 3: Amount paid
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

        ' ⭐ Stop if any check failed
        If Not isValid Then
            MsgBox("Please fill in the required fields.", MsgBoxStyle.Exclamation, "Incomplete")
            Return
        End If

        ' ⭐ CONFIRMATION DIALOG
        Dim confirm As DialogResult = MessageBox.Show(
        "Confirm payment?" & vbCrLf & vbCrLf &
        "Transaction ID: " & txbordern.Text & vbCrLf &
        "Total: ₱" & totalAmount.ToString("0.00") & vbCrLf &
        "Paid: ₱" & paid.ToString("0.00") & vbCrLf &
        "Change: ₱" & (paid - totalAmount).ToString("0.00"),
        "Confirm Payment",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question)

        If confirm = DialogResult.No Then
            Return
        End If

        ' ⭐ Save to DB
        Try
            conn()

            query = "INSERT INTO payments (transaction_id, cashier_id, cashier_fullname, payment_method, subtotal_amount, amount_paid) " &
                "VALUES (@TXN, @CID, @NAME, @METHOD, @SUB, @PAID); SELECT SCOPE_IDENTITY();"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            With cmd.Parameters
                .AddWithValue("@TXN", txbordern.Text)
                .AddWithValue("@CID", CurrentUser.user_id.ToString())
                .AddWithValue("@NAME", CurrentUser.fullname)
                .AddWithValue("@METHOD", cmbpm.Text)
                .AddWithValue("@SUB", totalSubtotal)
                .AddWithValue("@PAID", paid)
            End With
            Dim paymentId As Integer = Convert.ToInt32(cmd.ExecuteScalar())
            cmd.Dispose()

            For Each row As DataGridViewRow In datagridvieworder.Rows
                query = "INSERT INTO transaction_records (payment_id, product_name, product_price, product_quantity) " &
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
            Next

            MsgBox("Payment successful! Transaction ID: " & txbordern.Text, MsgBoxStyle.Information, "Success")

            clearOrder()
            CreateNewAutoNumber()
            SP_AutoNumber()
        Catch ex As Exception
            MsgBox("Error: " & ex.Message, MsgBoxStyle.Critical, "Failed")
        End Try
    End Sub


End Class