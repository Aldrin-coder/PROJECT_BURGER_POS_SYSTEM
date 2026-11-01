Imports System.Data.SqlClient
Imports System.IO
Imports System.Drawing
Imports Guna.UI2.WinForms

Public Class frm_oe4cashier

    ' === VARIABLES (like your reference) ===
    Dim originalItemId As String = ""     ' If not empty → we are editing
    Dim isLoading As Boolean = True

    Private Sub frm_oe4cashier_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        loaditems()
        isLoading = False
    End Sub

    ' ============================================================
    ' LOAD PRODUCT CARDS (same as before, unchanged)
    ' ============================================================
    Sub loaditems()
        FlowLayoutPanel1.Controls.Clear()

        query = "SELECT m.*, c.category_name FROM menu_items m " &
                "INNER JOIN categories c ON m.category_id = c.category_id " &
                "WHERE m.IsActive = 1"

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        dr = cmd.ExecuteReader()

        While dr.Read()
            Dim itemId As String = dr("item_id").ToString()
            Dim prodName As String = dr("product_name").ToString()
            Dim price As Decimal = Convert.ToDecimal(dr("price"))
            Dim catName As String = dr("category_name").ToString()
            Dim qty As Integer = Convert.ToInt32(dr("quantity"))

            Dim pnl As New Guna2Panel With {
                .Width = 219,
                .Height = 307,
                .Margin = New Padding(8, 10, 5, 5),
                .BorderThickness = 1,
                .BorderRadius = 5,
                .BorderColor = Color.DarkOrange,
                .FillColor = Color.White,
                .Cursor = Cursors.Hand
            }

            Dim pic As New Guna2PictureBox With {
                .Width = 137,
                .Height = 98,
                .Location = New Point(40, 25),
                .SizeMode = PictureBoxSizeMode.StretchImage,
                .BorderStyle = BorderStyle.FixedSingle,
                .Cursor = Cursors.Hand
            }

            If Not IsDBNull(dr("product_image")) Then
                Dim imgBytes As Byte() = CType(dr("product_image"), Byte())
                If imgBytes.Length > 0 Then
                    Using ms As New MemoryStream(imgBytes)
                        pic.Image = System.Drawing.Image.FromStream(ms)
                    End Using
                End If
            End If

            Dim pname As New Label With {
                .Text = prodName.ToUpper(),
                .Width = 215, .Height = 45,
                .Location = New Point(2, 135),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Font = New Font("Microsoft YaHei UI", 10, FontStyle.Bold),
                .ForeColor = Color.Black, .BackColor = Color.White,
                .AutoSize = False, .Cursor = Cursors.Hand
            }

            Dim pprice As New Label With {
                .Text = "₱ " & price.ToString("N2"),
                .Width = 215, .Height = 30,
                .Location = New Point(2, 180),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Font = New Font("Microsoft YaHei UI", 11, FontStyle.Bold),
                .ForeColor = Color.OrangeRed, .BackColor = Color.White,
                .AutoSize = False, .Cursor = Cursors.Hand
            }

            Dim pcategory As New Label With {
                .Text = "CAT: " & catName.ToUpper(),
                .Width = 215, .Height = 30,
                .Location = New Point(2, 220),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Font = New Font("Microsoft YaHei UI", 10, FontStyle.Bold),
                .ForeColor = Color.Black, .BackColor = Color.White,
                .AutoSize = False, .Cursor = Cursors.Hand
            }

            Dim pquantity As New Label With {
                .Text = "QTY: " & qty.ToString(),
                .Width = 215, .Height = 30,
                .Location = New Point(2, 260),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Font = New Font("Microsoft YaHei UI", 12, FontStyle.Italic),
                .ForeColor = Color.Gray, .BackColor = Color.White,
                .AutoSize = False, .Cursor = Cursors.Hand
            }

            AddHandler pnl.Click, Sub(sender, e) AddToGrid(itemId, prodName, price, catName)
            AddHandler pic.Click, Sub(sender, e) AddToGrid(itemId, prodName, price, catName)
            AddHandler pname.Click, Sub(sender, e) AddToGrid(itemId, prodName, price, catName)
            AddHandler pprice.Click, Sub(sender, e) AddToGrid(itemId, prodName, price, catName)
            AddHandler pcategory.Click, Sub(sender, e) AddToGrid(itemId, prodName, price, catName)
            AddHandler pquantity.Click, Sub(sender, e) AddToGrid(itemId, prodName, price, catName)

            pnl.Controls.Add(pic)
            pnl.Controls.Add(pname)
            pnl.Controls.Add(pprice)
            pnl.Controls.Add(pcategory)
            pnl.Controls.Add(pquantity)

            FlowLayoutPanel1.Controls.Add(pnl)
        End While

        dr.Close()
        cmd.Dispose()
    End Sub

    ' ============================================================
    ' ADD / INCREASE QUANTITY IN CART
    ' ============================================================
    Private Sub AddToGrid(id As String, name As String, price As Decimal, category As String)

        Dim existingRow As DataGridViewRow = Nothing

        For Each row As DataGridViewRow In datagridvieworder.Rows
            If row.IsNewRow Then Continue For
            If row.Cells(1).Value IsNot Nothing AndAlso row.Cells(1).Value.ToString() = name.ToUpper() Then
                existingRow = row
                Exit For
            End If
        Next

        If existingRow IsNot Nothing Then
            Dim currentQty As Integer = Convert.ToInt32(existingRow.Cells(0).Value)
            existingRow.Cells(0).Value = (currentQty + 1).ToString()
        Else
            datagridvieworder.Rows.Add(
                "1",
                name.ToUpper(),
                "₱" & price.ToString("N2")
            )
        End If

        UpdateTotals()
    End Sub

    ' ============================================================
    ' CALCULATE SUBTOTAL / TAX / TOTAL
    ' ============================================================
    Private Sub UpdateTotals()
        Dim subtotal As Decimal = 0

        For Each row As DataGridViewRow In datagridvieworder.Rows
            If row.IsNewRow Then Continue For
            If row.Cells(0).Value Is Nothing OrElse row.Cells(2).Value Is Nothing Then Continue For

            Dim qty As Integer = Convert.ToInt32(row.Cells(0).Value)
            Dim priceText As String = row.Cells(2).Value.ToString().Replace("₱", "").Replace(",", "").Trim()
            Dim price As Decimal = Convert.ToDecimal(priceText)

            subtotal += qty * price
        Next

        Dim tax As Decimal = subtotal * 0.02D
        Dim total As Decimal = subtotal + tax

        lblsubtotal2.Text = " ₱" & subtotal.ToString("N2")
        lbltr2.Text = " ₱" & tax.ToString("N2")
        lbltotal2.Text = " ₱" & total.ToString("N2")
    End Sub

    ' ============================================================
    ' VOID ITEM (Guna2Button2)
    ' ============================================================
    Private Sub Guna2Button2_Click(sender As Object, e As EventArgs) Handles btnvi.Click

        If datagridvieworder.SelectedRows.Count = 0 Then
            MessageBox.Show("Please select an item to void.", "No Item Selected",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim row As DataGridViewRow = datagridvieworder.SelectedRows(0)
        Dim currentQty As Integer = Convert.ToInt32(row.Cells(0).Value)

        If currentQty > 1 Then
            row.Cells(0).Value = (currentQty - 1).ToString()
        Else
            Dim itemName As String = row.Cells(1).Value.ToString()
            Dim result As DialogResult = MessageBox.Show(
                "Remove " & itemName & " from cart?",
                "Confirm Void", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

            If result = DialogResult.Yes Then
                datagridvieworder.Rows.Remove(row)
            End If
        End If

        UpdateTotals()
    End Sub

    ' ============================================================
    ' CLEAR CART (Guna2Button3)
    ' ============================================================
    Private Sub Guna2Button3_Click(sender As Object, e As EventArgs) Handles btnclear.Click

        If datagridvieworder.Rows.Count = 0 Then Return

        Dim result As DialogResult = MessageBox.Show(
            "Clear the entire cart?", "Confirm Clear",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If result = DialogResult.Yes Then
            datagridvieworder.Rows.Clear()
            UpdateTotals()
            txbap.Clear()
        End If
    End Sub

    ' ============================================================
    ' PRINT BILL (Guna2Button1) — placeholder
    ' ============================================================
    Private Sub Guna2Button1_Click(sender As Object, e As EventArgs) Handles btnpb.Click
        If datagridvieworder.Rows.Count = 0 Then
            MessageBox.Show("Cart is empty.", "Nothing to Print",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        MessageBox.Show("Receipt printed! (Implement actual printing here)",
                        "Print Bill", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ' ============================================================
    ' PAY NOW (addbtncateg) — SAVE ORDER TO DATABASE
    ' ============================================================
    Private Sub addbtncateg_Click(sender As Object, e As EventArgs) Handles btnpn.Click

        ' 1. Validate cart is not empty
        If datagridvieworder.Rows.Count = 0 Then
            MessageBox.Show("Cart is empty. Add items first.", "Cannot Pay",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' 2. Validate amount paid
        Dim totalText As String = lbltotal2.Text.Replace("₱", "").Replace(",", "").Trim()
        Dim total As Decimal = Convert.ToDecimal(totalText)

        Dim paid As Decimal = 0
        If Not Decimal.TryParse(txbap.Text, paid) Then
            MessageBox.Show("Please enter a valid amount paid.", "Invalid Payment",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If paid < total Then
            MessageBox.Show("Insufficient payment. Total is ₱" & total.ToString("N2"),
                            "Insufficient", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Dim change As Decimal = paid - total

        ' 3. Save to database
        Try
            conn()

            ' 3a. Insert main order
            query = "INSERT INTO orders (order_date, total_amount, amount_paid, change_amount, cashier_id) " &
                    "OUTPUT INSERTED.order_id " &
                    "VALUES (@DATE, @TOTAL, @PAID, @CHANGE, @CASHIER)"
            cmd = New SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@DATE", DateTime.Now)
            cmd.Parameters.AddWithValue("@TOTAL", total)
            cmd.Parameters.AddWithValue("@PAID", paid)
            cmd.Parameters.AddWithValue("@CHANGE", change)
            cmd.Parameters.AddWithValue("@CASHIER", CurrentUser.user_id)

            Dim newOrderId As Integer = Convert.ToInt32(cmd.ExecuteScalar())
            cmd.Dispose()

            ' 3b. Insert order items
            For Each row As DataGridViewRow In datagridvieworder.Rows
                If row.IsNewRow Then Continue For

                Dim itemQty As Integer = Convert.ToInt32(row.Cells(0).Value)
                Dim itemName As String = row.Cells(1).Value.ToString()
                Dim priceText As String = row.Cells(2).Value.ToString().Replace("₱", "").Replace(",", "").Trim()
                Dim unitPrice As Decimal = Convert.ToDecimal(priceText)

                ' Get product_id from name
                query = "SELECT item_id FROM menu_items WHERE product_name = @NAME"
                cmd = New SqlCommand(query, sqlconn)
                cmd.Parameters.AddWithValue("@NAME", itemName)
                Dim itemId As String = Convert.ToString(cmd.ExecuteScalar())
                cmd.Dispose()

                query = "INSERT INTO order_details (order_id, item_id, quantity, unit_price, subtotal) " &
                        "VALUES (@OID, @IID, @QTY, @PRICE, @SUB)"
                cmd = New SqlCommand(query, sqlconn)
                cmd.Parameters.AddWithValue("@OID", newOrderId)
                cmd.Parameters.AddWithValue("@IID", itemId)
                cmd.Parameters.AddWithValue("@QTY", itemQty)
                cmd.Parameters.AddWithValue("@PRICE", unitPrice)
                cmd.Parameters.AddWithValue("@SUB", itemQty * unitPrice)
                cmd.ExecuteNonQuery()
                cmd.Dispose()

                ' Deduct stock
                query = "UPDATE menu_items SET quantity = quantity - @QTY WHERE item_id = @IID"
                cmd = New SqlCommand(query, sqlconn)
                cmd.Parameters.AddWithValue("@QTY", itemQty)
                cmd.Parameters.AddWithValue("@IID", itemId)
                cmd.ExecuteNonQuery()
                cmd.Dispose()
            Next

            MessageBox.Show("Payment successful!" & vbCrLf &
                            "Total: ₱" & total.ToString("N2") & vbCrLf &
                            "Paid: ₱" & paid.ToString("N2") & vbCrLf &
                            "Change: ₱" & change.ToString("N2"),
                            "Order Completed", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' 4. Reset
            datagridvieworder.Rows.Clear()
            txbap.Clear()
            UpdateTotals()
            loaditems()   ' Reload products (stock changed)

        Catch ex As Exception
            MessageBox.Show("Error saving order: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class