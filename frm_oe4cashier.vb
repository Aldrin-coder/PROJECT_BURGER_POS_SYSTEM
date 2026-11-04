Imports System.Data.SqlClient
Imports System.IO
Imports System.Drawing
Imports Guna.UI2.WinForms

Public Class frm_oe4cashier

    Dim isvalid As Boolean = True

    Private Sub frm_oe4cashier_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        cmbitems()
        LoadCategories()
        loaditems()
        isvalid = False
        PreviewNextOrderNumber()
    End Sub

    Sub cmbitems()
        cmbpm.Items.Clear()
        cmbpm.Items.AddRange({"CASH", "GCASH", "MAYA"})
    End Sub

    Sub LoadCategories()
        cmbcategory.Items.Clear()
        cmbcategory.Items.Add("ALL")

        Try
            conn()
            query = "SELECT category_name FROM categories ORDER BY category_name"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            dr = cmd.ExecuteReader()

            While dr.Read()
                cmbcategory.Items.Add(dr("category_name").ToString().ToUpper())
            End While

            dr.Close()
            cmd.Dispose()

            cmbcategory.SelectedIndex = 0
        Catch ex As Exception
            MessageBox.Show("Error loading categories: " & ex.Message)
        End Try
    End Sub

    Private Sub cmbcategory_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbcategory.SelectedIndexChanged
        loaditems()
    End Sub

    Private Sub txbsearch_TextChanged(sender As Object, e As EventArgs) Handles txbsearch.TextChanged
        loaditems()
    End Sub

    Private Sub PreviewNextOrderNumber()
        Try
            conn()
            query = "SELECT MAX(NewNumber) FROM AutoNumberOrder"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            Dim result As Object = cmd.ExecuteScalar()
            cmd.Dispose()

            If result Is Nothing OrElse IsDBNull(result) Then
                txbordern.Text = "ORD-001"
            Else
                Dim lastNum As String = result.ToString()
                Dim numPart As String = lastNum.Replace("ORD-", "")
                Dim intNum As Integer
                If Integer.TryParse(numPart, intNum) Then
                    txbordern.Text = "ORD-" & (intNum + 1).ToString("000")
                Else
                    txbordern.Text = "ORD-001"
                End If
            End If
        Catch ex As Exception
            txbordern.Text = "ORD-001"
        End Try
    End Sub

    Sub loaditems()
        FlowLayoutPanel1.Controls.Clear()
        conn()   ' <--- FIX: open the connection every time

        Dim sql As String = "SELECT m.*, c.category_name FROM menu_items m " &
                            "INNER JOIN categories c ON m.category_id = c.category_id " &
                            "WHERE m.IsActive = 1 "

        If cmbcategory.Text <> "" AndAlso cmbcategory.Text.ToUpper() <> "ALL" Then
            sql &= " AND c.category_name = @CAT "
        End If

        If txbsearch.Text.Trim() <> "" Then
            sql &= " AND m.product_name LIKE @SEARCH "
        End If

        cmd = New SqlClient.SqlCommand(sql, sqlconn)

        If cmbcategory.Text <> "" AndAlso cmbcategory.Text.ToUpper() <> "ALL" Then
            cmd.Parameters.AddWithValue("@CAT", cmbcategory.Text)
        End If

        If txbsearch.Text.Trim() <> "" Then
            cmd.Parameters.AddWithValue("@SEARCH", "%" & txbsearch.Text.Trim() & "%")
        End If

        dr = cmd.ExecuteReader()

        While dr.Read()
            Dim itemId As String = dr("item_id").ToString()
            Dim prodName As String = dr("product_name").ToString()
            Dim price As Decimal = Convert.ToDecimal(dr("price"))
            Dim catName As String = dr("category_name").ToString()
            Dim qty As Integer = Convert.ToInt32(dr("quantity"))

            If qty <= 0 Then Continue While

            Dim pnl As New Guna2Panel With {
                .Width = 219, .Height = 307, .Margin = New Padding(8, 10, 5, 5),
                .BorderThickness = 1, .BorderRadius = 5, .BorderColor = Color.DarkOrange, .FillColor = Color.White, .Cursor = Cursors.Hand
            }

            Dim pic As New Guna2PictureBox With {
                .Width = 137, .Height = 98, .Location = New Point(40, 25),
                .SizeMode = PictureBoxSizeMode.StretchImage, .BorderStyle = BorderStyle.FixedSingle, .Cursor = Cursors.Hand
            }

            If Not IsDBNull(dr("product_image")) Then
                Dim imgObj As Object = dr("product_image")
                If imgObj IsNot Nothing Then
                    Dim imgBytes As Byte() = CType(imgObj, Byte())
                    If imgBytes.Length > 0 Then
                        Using ms As New MemoryStream(imgBytes)
                            pic.Image = System.Drawing.Image.FromStream(ms)
                        End Using
                    End If
                End If
            End If

            Dim pname As New Label With {
                .Text = prodName.ToUpper(), .Width = 215, .Height = 45, .Location = New Point(2, 135),
                .TextAlign = ContentAlignment.MiddleCenter, .Font = New Font("Microsoft YaHei UI", 10, FontStyle.Bold),
                .ForeColor = Color.Black, .BackColor = Color.White, .AutoSize = False, .Cursor = Cursors.Hand
            }

            Dim pprice As New Label With {
                .Text = "₱ " & price.ToString("N2"), .Width = 215, .Height = 30, .Location = New Point(2, 180),
                .TextAlign = ContentAlignment.MiddleCenter, .Font = New Font("Microsoft YaHei UI", 11, FontStyle.Bold),
                .ForeColor = Color.OrangeRed, .BackColor = Color.White, .AutoSize = False, .Cursor = Cursors.Hand
            }

            Dim pcategory As New Label With {
                .Text = "CATEGORY: " & catName.ToUpper(), .Width = 215, .Height = 30, .Location = New Point(2, 220),
                .TextAlign = ContentAlignment.MiddleCenter, .Font = New Font("Microsoft YaHei UI", 10, FontStyle.Bold),
                .ForeColor = Color.Black, .BackColor = Color.White, .AutoSize = False, .Cursor = Cursors.Hand
            }

            Dim pquantity As New Label With {
                .Text = "QTY: " & qty.ToString(), .Width = 215, .Height = 30, .Location = New Point(2, 260),
                .TextAlign = ContentAlignment.MiddleCenter, .Font = New Font("Microsoft YaHei UI", 12, FontStyle.Italic),
                .ForeColor = Color.Gray, .BackColor = Color.White, .AutoSize = False, .Cursor = Cursors.Hand
            }

            Dim hoverIn = Sub(sender, e)
                              Dim hoverColor As Color = Color.FromArgb(255, 245, 238)
                              pnl.FillColor = hoverColor
                              pnl.BorderColor = Color.OrangeRed
                              pname.BackColor = hoverColor
                              pprice.BackColor = hoverColor
                              pcategory.BackColor = hoverColor
                              pquantity.BackColor = hoverColor
                          End Sub

            Dim hoverOut = Sub(sender, e)
                               If Not pnl.ClientRectangle.Contains(pnl.PointToClient(Control.MousePosition)) Then
                                   pnl.FillColor = Color.White
                                   pnl.BorderColor = Color.DarkOrange
                                   pname.BackColor = Color.White
                                   pprice.BackColor = Color.White
                                   pcategory.BackColor = Color.White
                                   pquantity.BackColor = Color.White
                               End If
                           End Sub

            AddHandler pnl.Click, Sub(sender, e) AddToGrid(itemId, prodName, price, catName)
            AddHandler pic.Click, Sub(sender, e) AddToGrid(itemId, prodName, price, catName)
            AddHandler pname.Click, Sub(sender, e) AddToGrid(itemId, prodName, price, catName)
            AddHandler pprice.Click, Sub(sender, e) AddToGrid(itemId, prodName, price, catName)
            AddHandler pcategory.Click, Sub(sender, e) AddToGrid(itemId, prodName, price, catName)
            AddHandler pquantity.Click, Sub(sender, e) AddToGrid(itemId, prodName, price, catName)

            Dim controlsToHover As Control() = {pnl, pic, pname, pprice, pcategory, pquantity}
            For Each ctrl As Control In controlsToHover
                AddHandler ctrl.MouseEnter, hoverIn
                AddHandler ctrl.MouseLeave, hoverOut
            Next

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

    Private Sub AddToGrid(id As String, name As String, price As Decimal, category As String)
        If datagridvieworder Is Nothing OrElse datagridvieworder.Columns.Count < 4 Then Return

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
            datagridvieworder.Rows.Add("1", name.ToUpper(), "₱" & price.ToString("N2"), "VOID ITEM")
        End If

        UpdateTotals()
    End Sub

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

    Private Sub ResetBorders()
        txbap.BorderColor = Color.Black
        cmbpm.BorderColor = Color.Black
    End Sub

    Private Function ValidateOrder() As Boolean
        ErrorProvider1.Clear()
        ResetBorders()
        isvalid = True

        If datagridvieworder.Rows.Count = 0 Then
            MessageBox.Show("Cart is empty. Add items first.", "Cannot Pay", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            isvalid = False
        End If

        If cmbpm.SelectedIndex = -1 Then
            ErrorProvider1.SetError(cmbpm, "Payment method is required.")
            cmbpm.BorderColor = Color.Red
            isvalid = False
        End If

        Dim totalText As String = lbltotal2.Text.Replace("₱", "").Replace(",", "").Trim()
        Dim total As Decimal = Convert.ToDecimal(totalText)

        Dim paid As Decimal = 0
        If txbap.Text = "" Then
            ErrorProvider1.SetError(txbap, "Amount paid is required.")
            txbap.BorderColor = Color.Red
            isvalid = False
        ElseIf Not Decimal.TryParse(txbap.Text, paid) Then
            ErrorProvider1.SetError(txbap, "Please enter a valid amount.")
            txbap.BorderColor = Color.Red
            isvalid = False
        ElseIf paid < total Then
            ErrorProvider1.SetError(txbap, "Insufficient payment. Total is ₱" & total.ToString("N2"))
            txbap.BorderColor = Color.Red
            isvalid = False
        End If

        Return isvalid
    End Function

    Sub ClearFields()
        txbap.Clear()
        cmbpm.SelectedIndex = -1
        lblchange.Text = "₱ 0.00"
        ErrorProvider1.Clear()
        ResetBorders()
    End Sub

    Private Sub cmbpm_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbpm.SelectedIndexChanged
        ErrorProvider1.SetError(cmbpm, "")
        cmbpm.BorderColor = Color.Black
    End Sub

    Private Sub txbap_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbap.KeyPress
        If Not Char.IsDigit(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) AndAlso e.KeyChar <> "."c Then
            e.Handled = True
            Exit Sub
        End If
        If e.KeyChar = "."c AndAlso txbap.Text.Contains(".") Then
            e.Handled = True
        End If
    End Sub

    Private Sub txbap_TextChanged(sender As Object, e As EventArgs) Handles txbap.TextChanged
        ErrorProvider1.SetError(txbap, "")
        txbap.BorderColor = Color.Black

        Try
            If lbltotal2 Is Nothing Then Exit Sub
            If lblchange Is Nothing Then Exit Sub

            Dim totalText As String = lbltotal2.Text
            If totalText = "" Then
                lblchange.Text = "₱ 0.00"
                Exit Sub
            End If

            totalText = totalText.Replace("₱", "").Replace(",", "").Trim()
            If totalText = "" Then
                lblchange.Text = "₱ 0.00"
                Exit Sub
            End If

            Dim total As Decimal = 0
            If Not Decimal.TryParse(totalText, total) Then
                lblchange.Text = "₱ 0.00"
                Exit Sub
            End If

            If txbap.Text = "" Then
                lblchange.Text = "₱ 0.00"
                Exit Sub
            End If

            Dim paid As Decimal = 0
            If Decimal.TryParse(txbap.Text.Trim(), paid) Then
                Dim change As Decimal = paid - total
                If change >= 0 Then
                    lblchange.Text = "₱ " & change.ToString("N2")
                Else
                    lblchange.Text = "₱ 0.00"
                End If
            Else
                lblchange.Text = "₱ 0.00"
            End If
        Catch ex As Exception
            If lblchange IsNot Nothing Then lblchange.Text = "₱ 0.00"
        End Try
    End Sub

    Private Sub datagridvieworder_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridvieworder.CellContentClick
        If e.RowIndex < 0 Then Exit Sub
        If e.ColumnIndex < 0 OrElse e.ColumnIndex >= datagridvieworder.Columns.Count Then Exit Sub

        If datagridvieworder.Columns(e.ColumnIndex).Name = "Column4" Then
            Dim row As DataGridViewRow = datagridvieworder.Rows(e.RowIndex)

            If row.Cells(0).Value IsNot Nothing Then
                Dim currentQty As Integer = Convert.ToInt32(row.Cells(0).Value)

                If currentQty > 1 Then
                    row.Cells(0).Value = (currentQty - 1).ToString()
                Else
                    datagridvieworder.Rows.Remove(row)
                End If

                UpdateTotals()
            End If
        End If

    End Sub

    Private Sub btnclear_Click(sender As Object, e As EventArgs) Handles btnclear.Click
        ErrorProvider1.Clear()
        If datagridvieworder.Rows.Count = 0 Then Return

        If MessageBox.Show("Clear the entire cart?", "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            datagridvieworder.Rows.Clear()
            UpdateTotals()
            ClearFields()
        End If
    End Sub

    Private Sub frm_oe4cashier_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.F1 Then
            btnpn.PerformClick()
        ElseIf e.KeyCode = Keys.F3 Then
            btnclear.PerformClick()
        End If
    End Sub

    Private Function GenerateNextNumber(storedProcName As String, txn As SqlTransaction) As String
        Try
            Using cmdNum As New SqlCommand(storedProcName, sqlconn, txn)
                cmdNum.CommandType = CommandType.StoredProcedure
                Dim outParam As SqlParameter = cmdNum.Parameters.Add("@NewNumber", SqlDbType.VarChar, 20)
                outParam.Direction = ParameterDirection.Output
                cmdNum.ExecuteNonQuery()
                Return Convert.ToString(outParam.Value)
            End Using
        Catch ex As Exception
            MessageBox.Show("AutoNumber Error (" & storedProcName & "): " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return ""
        End Try
    End Function

    Private Sub btnpn_Click(sender As Object, e As EventArgs) Handles btnpn.Click
        If Not ValidateOrder() Then Exit Sub

        Dim totalDisplay As String = lbltotal2.Text.Replace("₱", "").Trim()
        Dim confirm As DialogResult = MessageBox.Show(
            "Confirm payment?" & vbCrLf & vbCrLf &
            "Total: ₱" & totalDisplay & vbCrLf &
            "Payment Method: " & cmbpm.Text.Trim().ToUpper(),
            "Confirm Payment",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question)

        If confirm = DialogResult.No Then Exit Sub

        Dim totalText As String = lbltotal2.Text.Replace("₱", "").Replace(",", "").Trim()
        Dim total As Decimal = Convert.ToDecimal(totalText)
        Dim paid As Decimal = Convert.ToDecimal(txbap.Text)
        Dim change As Decimal = paid - total
        Dim subtotal As Decimal = total / 1.02D
        Dim tax As Decimal = total - subtotal

        Dim newTransactionId As String = ""
        Dim newOrderId As String = ""

        conn()
        Dim txn As SqlTransaction = sqlconn.BeginTransaction()

        Try
            newTransactionId = GenerateNextNumber("sp_AutoNumberTxn", txn)
            If newTransactionId = "" Then Throw New Exception("Failed to generate Transaction ID.")

            newOrderId = GenerateNextNumber("sp_AutoNumberOrder", txn)
            If newOrderId = "" Then Throw New Exception("Failed to generate Order ID.")

            query = "INSERT INTO payments (transaction_id, order_id, cashier_id, cashier_fullname, payment_method, subtotal_amount, amount_paid) " &
                    "OUTPUT INSERTED.payment_id " &
                    "VALUES (@TXNID, @OID, @CID, @CNAME, @METHOD, @SUB, @PAID)"

            Dim newPaymentId As Integer = 0
            Using cmdPay As New SqlCommand(query, sqlconn, txn)
                cmdPay.Parameters.AddWithValue("@TXNID", newTransactionId)
                cmdPay.Parameters.AddWithValue("@OID", newOrderId)
                cmdPay.Parameters.AddWithValue("@CID", CurrentUser.role_code)
                cmdPay.Parameters.AddWithValue("@CNAME", CurrentUser.fullname)
                cmdPay.Parameters.AddWithValue("@METHOD", cmbpm.Text.Trim().ToUpper())
                cmdPay.Parameters.AddWithValue("@SUB", subtotal)
                cmdPay.Parameters.AddWithValue("@PAID", paid)
                newPaymentId = Convert.ToInt32(cmdPay.ExecuteScalar())
            End Using

            For Each row As DataGridViewRow In datagridvieworder.Rows
                If row.IsNewRow Then Continue For

                Dim itemQty As Integer = Convert.ToInt32(row.Cells(0).Value)
                Dim itemName As String = row.Cells(1).Value.ToString()
                Dim priceText As String = row.Cells(2).Value.ToString().Replace("₱", "").Replace(",", "").Trim()
                Dim unitPrice As Decimal = Convert.ToDecimal(priceText)

                query = "INSERT INTO transaction_records (payment_id, product_name, product_price, product_quantity) " &
                        "VALUES (@PID, @PNAME, @PPRICE, @QTY)"
                Using cmdRec As New SqlCommand(query, sqlconn, txn)
                    cmdRec.Parameters.AddWithValue("@PID", newPaymentId)
                    cmdRec.Parameters.AddWithValue("@PNAME", itemName)
                    cmdRec.Parameters.AddWithValue("@PPRICE", unitPrice)
                    cmdRec.Parameters.AddWithValue("@QTY", itemQty)
                    cmdRec.ExecuteNonQuery()
                End Using

                query = "UPDATE menu_items SET quantity = quantity - @QTY WHERE product_name = @PNAME"
                Using cmdStock As New SqlCommand(query, sqlconn, txn)
                    cmdStock.Parameters.AddWithValue("@QTY", itemQty)
                    cmdStock.Parameters.AddWithValue("@PNAME", itemName)
                    cmdStock.ExecuteNonQuery()
                End Using
            Next

            txn.Commit()

            Dim receiptItems As New List(Of frm_receipt.ReceiptItem)
            For Each row As DataGridViewRow In datagridvieworder.Rows
                If row.IsNewRow Then Continue For
                Dim qty As Integer = Convert.ToInt32(row.Cells(0).Value)
                Dim nm As String = row.Cells(1).Value.ToString()
                Dim ptxt As String = row.Cells(2).Value.ToString().Replace("₱", "").Replace(",", "").Trim()
                Dim uprice As Decimal = Convert.ToDecimal(ptxt)

                receiptItems.Add(New frm_receipt.ReceiptItem With {
                    .Name = nm,
                    .Qty = qty,
                    .UnitPrice = uprice,
                    .Subtotal = uprice * qty
                })
            Next

            MessageBox.Show("Payment successful!" & vbCrLf & vbCrLf &
                            "Transaction ID: " & newTransactionId & vbCrLf &
                            "Order ID: " & newOrderId & vbCrLf & vbCrLf &
                            "Total: ₱" & total.ToString("N2") & vbCrLf &
                            "Paid: ₱" & paid.ToString("N2") & vbCrLf &
                            "Change: ₱" & change.ToString("N2"),
                            "Order Completed", MessageBoxButtons.OK, MessageBoxIcon.Information)

            Try
                Dim rptForm As New frm_receipt()
                rptForm.ShowReceipt(
                    newTransactionId, newOrderId, CurrentUser.fullname,
                    cmbpm.Text.Trim().ToUpper(),
                    subtotal, tax, total, paid, change, receiptItems)
            Catch exPrint As Exception
                MessageBox.Show("Receipt printing failed:" & vbCrLf & exPrint.Message,
                                "Printer Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try

            datagridvieworder.Rows.Clear()
            UpdateTotals()
            ClearFields()
            loaditems()
            PreviewNextOrderNumber()

        Catch ex As Exception
            Try
                txn.Rollback()
            Catch
            End Try
            MessageBox.Show("Error saving order: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If sqlconn.State = ConnectionState.Open Then sqlconn.Close()
        End Try
    End Sub

    Private Sub FlowLayoutPanel1_Paint(sender As Object, e As PaintEventArgs) Handles FlowLayoutPanel1.Paint

    End Sub
End Class