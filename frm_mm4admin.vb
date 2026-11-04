Imports System.Data.SqlClient
Imports System.IO
Imports System.Text.RegularExpressions

Public Class frm_mm4admin

    Dim currentCategoryId As Integer = 0
    Dim currentItemId As Integer = 0
    Dim isvalid As Boolean = True
    Dim isLoading As Boolean = True


    Dim selectedImageBytes As Byte() = Nothing

    Private Sub frm_mm4admin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        isvalid = False

        readdatacategories()
        readdatamenuitems()
        previewautonumber()

        ErrorProvider1.Clear()
        isLoading = False
    End Sub

    Sub previewautonumber()
        conn()

        ' Read from AutoNumberUsers (unified table) instead of AutoNumberMenu
        query = "SELECT MAX(NewNumber) FROM AutoNumberUsers WHERE pfx = 'M'"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        Dim result As Object = cmd.ExecuteScalar()
        cmd.Dispose()

        If result Is Nothing OrElse IsDBNull(result) Then
            ' No menu items yet, preview the first one
            txbiditem.Text = "M1001"
        Else
            Dim lastNum As String = result.ToString()
            Dim numPart As String = lastNum.Substring(1) ' Remove the "M"
            Dim intNum As Integer

            If Integer.TryParse(numPart, intNum) Then
                txbiditem.Text = "M" & (intNum + 1).ToString()
            Else
                txbiditem.Text = "M1001"
            End If
        End If


    End Sub
    Sub readdatacategories()
        If datagridviewcategories.Columns.Count = 0 Then Exit Sub
        conn()
        datagridviewcategories.Rows.Clear()

        query = "SELECT category_id, category_name FROM categories ORDER BY category_name"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        dr = cmd.ExecuteReader()

        txbproductcat.Items.Clear()
        cmbfilter.Items.Clear()
        cmbfilter.Items.Add("ALL")

        While dr.Read()
            Dim catName As String = StrConv(dr("category_name").ToString(), VbStrConv.ProperCase)

            datagridviewcategories.Rows.Add(
                dr("category_id"),
                catName,
                "UPDATE",
                "DELETE"
            )
            txbproductcat.Items.Add(catName)
            cmbfilter.Items.Add(catName)
        End While

        dr.Close()
        cmd.Dispose()
    End Sub

    Sub readdatamenuitems()
        If datagridviewmenulist.Columns.Count = 0 Then Exit Sub
        conn()
        datagridviewmenulist.Rows.Clear()

        Dim searchText As String = txbsearch.Text()
        Dim categoryFilter As String = If(cmbfilter.Text, "").Trim()

        query = "SELECT m.item_id, m.item_code, m.product_name, m.price, m.quantity, " &
                "c.category_name, m.IsActive, m.product_image " &
                "FROM menu_items m " &
                "INNER JOIN categories c ON m.category_id = c.category_id " &
                "WHERE 1 = 1 "

        If categoryFilter <> "" AndAlso categoryFilter.ToUpper() <> "ALL" Then
            query &= " AND LOWER(c.category_name) = LOWER(@CATEGORY) "
        End If

        If searchText <> "" Then
            query &= " AND ( m.item_code LIKE @SEARCH OR m.product_name LIKE @SEARCH) "
        End If

        query &= " ORDER BY m.item_id"

        cmd = New SqlClient.SqlCommand(query, sqlconn)

        If categoryFilter <> "" AndAlso categoryFilter.ToUpper() <> "ALL" Then
            cmd.Parameters.AddWithValue("@CATEGORY", categoryFilter)
        End If

        If searchText <> "" Then
            cmd.Parameters.AddWithValue("@SEARCH", "%" & searchText & "%")
        End If

        dr = cmd.ExecuteReader()

        While dr.Read()
            Dim img As Image = Nothing

            ' ⭐ LOADING IMAGE FROM DATABASE BYTES ⭐
            If Not IsDBNull(dr("product_image")) Then
                Dim imgBytes As Byte() = CType(dr("product_image"), Byte())
                If imgBytes.Length > 0 Then
                    Try
                        Using ms As New MemoryStream(imgBytes)
                            Using tempImg As Image = Image.FromStream(ms)
                                img = New Bitmap(tempImg, 100, 100)
                            End Using
                        End Using
                    Catch
                        img = Nothing
                    End Try
                End If
            End If

            Dim status As String = If(Convert.ToBoolean(dr("IsActive")), "ACTIVE", "INACTIVE")
            Dim actionText As String = If(status = "ACTIVE", "DEACTIVATE", "ACTIVATE")
            Dim displayPrice As String = "₱" & Convert.ToDecimal(dr("price")).ToString("N2")
            Dim catNameDisplay As String = StrConv(dr("category_name").ToString(), VbStrConv.ProperCase)

            datagridviewmenulist.Rows.Add(
                dr("item_code"),
                dr("product_name"),
                displayPrice,
                catNameDisplay,
                dr("quantity"),
                status,
                img,
                actionText,
                "UPDATE",
                "DELETE"
            )
        End While

        dr.Close()
        cmd.Dispose()

        For i = 0 To datagridviewmenulist.Rows.Count - 1
            datagridviewmenulist.Rows(i).Height = 100
        Next
    End Sub

    Private Sub txbsearch_TextChanged(sender As Object, e As EventArgs) Handles txbsearch.TextChanged
        If isLoading Then Exit Sub
        readdatamenuitems()
    End Sub

    Private Sub datagridviewmenulist_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles datagridviewmenulist.CellFormatting
        If e.RowIndex >= 0 AndAlso e.Value IsNot Nothing Then
            If e.ColumnIndex = 5 Then
                Dim status As String = e.Value.ToString().ToUpper()
                If status = "ACTIVE" Then
                    e.CellStyle.ForeColor = Color.Green
                    e.CellStyle.Font = New Font(datagridviewmenulist.Font, FontStyle.Bold)
                ElseIf status = "INACTIVE" Then
                    e.CellStyle.ForeColor = Color.Red
                    e.CellStyle.Font = New Font(datagridviewmenulist.Font, FontStyle.Bold)
                End If
            End If
        End If
    End Sub

    Private Sub ResetBorders()
        txbpn.BorderColor = Color.Black
        txbprice.BorderColor = Color.Black
        txbpq.BorderColor = Color.Black
        txbproductcat.BorderColor = Color.Black
        txbcategoryname.BorderColor = Color.Black
    End Sub

    Private Function ValidateMenuItem() As Boolean
        ErrorProvider1.Clear()
        ResetBorders()
        isvalid = True

        If txbpn.Text.Trim() = "" Then
            ErrorProvider1.SetError(txbpn, "Product name is required.")
            txbpn.BorderColor = Color.Red
            isvalid = False
        ElseIf IsProductNameDuplicate(txbpn.Text.Trim(), currentItemId) Then
            ErrorProvider1.SetError(txbpn, "Product name already exists.")
            txbpn.BorderColor = Color.Red
            isvalid = False
        End If

        Dim price As Decimal
        If txbprice.Text.Trim() = "" Then
            ErrorProvider1.SetError(txbprice, "Price is required.")
            txbprice.BorderColor = Color.Red
            isvalid = False
        ElseIf Not Decimal.TryParse(txbprice.Text, price) OrElse price <= 0 Then
            ErrorProvider1.SetError(txbprice, "Price must be greater than 0.")
            txbprice.BorderColor = Color.Red
            isvalid = False
        End If

        If txbproductcat.SelectedIndex = -1 Then
            ErrorProvider1.SetError(txbproductcat, "Please select a category.")
            txbproductcat.BorderColor = Color.Red
            isvalid = False
        End If

        Dim qty As Integer
        If txbpq.Text.Trim() = "" Then
            ErrorProvider1.SetError(txbpq, "Quantity is required.")
            txbpq.BorderColor = Color.Red
            isvalid = False
        ElseIf Not Integer.TryParse(txbpq.Text, qty) OrElse qty < 0 Then
            ErrorProvider1.SetError(txbpq, "Quantity must be 0 or greater.")
            txbpq.BorderColor = Color.Red
            isvalid = False
        End If

        If picturebox1.Image Is Nothing AndAlso selectedImageBytes Is Nothing Then
            ErrorProvider1.SetError(picturebox1, "Product image is required.")
            isvalid = False
        End If

        Return isvalid
    End Function

    Private Function ValidateCategory() As Boolean
        ErrorProvider1.Clear()
        ResetBorders()
        isvalid = True

        If txbcategoryname.Text.Trim() = "" Then
            ErrorProvider1.SetError(txbcategoryname, "Category name is required.")
            txbcategoryname.BorderColor = Color.Red
            isvalid = False
        ElseIf IsCategoryNameDuplicate(txbcategoryname.Text.Trim(), currentCategoryId) Then
            ErrorProvider1.SetError(txbcategoryname, "Category name already exists.")
            txbcategoryname.BorderColor = Color.Red
            isvalid = False
        End If

        Return isvalid
    End Function

    Private Function IsProductNameDuplicate(productName As String, currentId As Integer) As Boolean
        conn()
        Dim checkQuery As String
        If currentId = 0 Then
            checkQuery = "SELECT COUNT(*) FROM menu_items WHERE LOWER(product_name) = LOWER(@name)"
        Else
            checkQuery = "SELECT COUNT(*) FROM menu_items WHERE LOWER(product_name) = LOWER(@name) AND item_id <> @id"
        End If

        Using checkCmd As New SqlCommand(checkQuery, sqlconn)
            checkCmd.Parameters.AddWithValue("@name", productName)
            If currentId > 0 Then checkCmd.Parameters.AddWithValue("@id", currentId)
            Return Convert.ToInt32(checkCmd.ExecuteScalar()) > 0
        End Using
    End Function

    Private Function IsCategoryNameDuplicate(categoryName As String, currentId As Integer) As Boolean
        conn()
        Dim checkQuery As String
        If currentId = 0 Then
            checkQuery = "SELECT COUNT(*) FROM categories WHERE LOWER(category_name) = LOWER(@name)"
        Else
            checkQuery = "SELECT COUNT(*) FROM categories WHERE LOWER(category_name) = LOWER(@name) AND category_id <> @id"
        End If

        Using checkCmd As New SqlCommand(checkQuery, sqlconn)
            checkCmd.Parameters.AddWithValue("@name", categoryName)
            If currentId > 0 Then checkCmd.Parameters.AddWithValue("@id", currentId)
            Return Convert.ToInt32(checkCmd.ExecuteScalar()) > 0
        End Using
    End Function

    Sub clearMenuForm(Optional keepCode As Boolean = False)
        If Not keepCode Then txbiditem.Clear()
        txbpn.Clear()
        txbprice.Clear()
        txbpq.Clear()
        txbproductcat.SelectedIndex = -1
        picturebox1.Image = Nothing
        selectedImageBytes = Nothing
        currentItemId = 0

        ErrorProvider1.Clear()
        ResetBorders()
    End Sub

    Sub clearCategoryForm()
        txbcategoryname.Clear()
        currentCategoryId = 0
        ErrorProvider1.Clear()
        ResetBorders()
    End Sub

    Private Sub txbpn_TextChanged(sender As Object, e As EventArgs) Handles txbpn.TextChanged
        txbpn.BorderColor = Color.Black
        ErrorProvider1.SetError(txbpn, "")
    End Sub

    Private Sub txbprice_TextChanged(sender As Object, e As EventArgs) Handles txbprice.TextChanged
        txbprice.BorderColor = Color.Black
        ErrorProvider1.SetError(txbprice, "")
    End Sub

    Private Sub txbpq_TextChanged(sender As Object, e As EventArgs) Handles txbpq.TextChanged
        txbpq.BorderColor = Color.Black
        ErrorProvider1.SetError(txbpq, "")
    End Sub

    Private Sub txbcategoryname_TextChanged(sender As Object, e As EventArgs) Handles txbcategoryname.TextChanged
        txbcategoryname.BorderColor = Color.Black
        ErrorProvider1.SetError(txbcategoryname, "")
    End Sub

    Private Sub txbproductcat_SelectedIndexChanged(sender As Object, e As EventArgs) Handles txbproductcat.SelectedIndexChanged
        txbproductcat.BorderColor = Color.Black
        ErrorProvider1.SetError(txbproductcat, "")
    End Sub

    Private Sub cmbfilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbfilter.SelectedIndexChanged
        If isLoading Then Exit Sub
        readdatamenuitems()
    End Sub

    Private Sub txbpn_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbpn.KeyPress
        If Not Char.IsLetter(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) AndAlso e.KeyChar <> " "c Then
            e.Handled = True
        End If
    End Sub

    Private Sub txbprice_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbprice.KeyPress
        If Not Char.IsDigit(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) AndAlso e.KeyChar <> "."c Then
            e.Handled = True
            Exit Sub
        End If
        If e.KeyChar = "."c AndAlso txbprice.Text.Contains(".") Then
            e.Handled = True
        End If
    End Sub

    Private Sub txbpq_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbpq.KeyPress
        If Not Char.IsDigit(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub

    Private Sub txbcategoryname_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbcategoryname.KeyPress
        If Not Char.IsLetter(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) AndAlso e.KeyChar <> " "c Then
            e.Handled = True
        End If
    End Sub

    ' ⭐ CONVERT IMAGE TO BYTES ON SELECT ⭐
    Private Sub picturebox1_Click(sender As Object, e As EventArgs) Handles picturebox1.Click
        Using ofd As New OpenFileDialog()
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            If ofd.ShowDialog() = DialogResult.OK Then
                picturebox1.SizeMode = PictureBoxSizeMode.StretchImage
                picturebox1.Image = Image.FromFile(ofd.FileName)

                ' Convert image to byte array
                Using ms As New MemoryStream()
                    picturebox1.Image.Save(ms, picturebox1.Image.RawFormat)
                    selectedImageBytes = ms.ToArray()
                End Using

                ErrorProvider1.SetError(picturebox1, "")
            End If
        End Using
    End Sub

    Private Sub addmenubtn_Click(sender As Object, e As EventArgs) Handles addmenubtn.Click
        If Not ValidateMenuItem() Then Exit Sub

        conn()
        Dim catId As Integer = GetCategoryId(txbproductcat.Text)
        If catId = 0 Then
            MsgBox("Category not found in the database. Please re-select the category or refresh.", MsgBoxStyle.Critical, "Category Error")
            Exit Sub
        End If

        Dim priceValue As Decimal
        Dim qtyValue As Integer
        Decimal.TryParse(txbprice.Text, priceValue)
        Integer.TryParse(txbpq.Text, qtyValue)

        If currentItemId = 0 Then
            Dim actualItemCode As String = ""
            Using spCmd As New SqlCommand("sp_AutoNumbermenu", sqlconn)
                spCmd.CommandType = CommandType.StoredProcedure
                spCmd.Parameters.AddWithValue("@pfx", "M")
                Dim outParam As SqlParameter = spCmd.Parameters.Add("@NewNumber", SqlDbType.NVarChar, 20)
                outParam.Direction = ParameterDirection.Output
                spCmd.ExecuteNonQuery()
                actualItemCode = outParam.Value.ToString()
            End Using

            query = "INSERT INTO menu_items (item_code, product_name, price, category_id, quantity, product_image, IsActive) " &
                    "VALUES (@code, @name, @price, @catid, @qty, @img, 1)"

            cmd = New SqlClient.SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@code", actualItemCode)
            cmd.Parameters.AddWithValue("@name", txbpn.Text.Trim())
            cmd.Parameters.AddWithValue("@price", priceValue)
            cmd.Parameters.AddWithValue("@catid", catId)
            cmd.Parameters.AddWithValue("@qty", qtyValue)
            ' ⭐ SAVE BYTES ⭐
            If selectedImageBytes IsNot Nothing Then
                cmd.Parameters.AddWithValue("@img", selectedImageBytes)
            Else
                cmd.Parameters.AddWithValue("@img", DBNull.Value)
            End If
            cmd.ExecuteNonQuery()
            cmd.Dispose()
            MsgBox("Menu item added successfully with code " & actualItemCode & "!", MsgBoxStyle.Information, "Success")
        Else
            query = "UPDATE menu_items SET product_name=@name, price=@price, category_id=@catid, " &
                    "quantity=@qty, product_image=@img WHERE item_id=@id"

            cmd = New SqlClient.SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@name", txbpn.Text.Trim())
            cmd.Parameters.AddWithValue("@price", priceValue)
            cmd.Parameters.AddWithValue("@catid", catId)
            cmd.Parameters.AddWithValue("@qty", qtyValue)
            ' ⭐ SAVE BYTES ⭐
            If selectedImageBytes IsNot Nothing Then
                cmd.Parameters.AddWithValue("@img", selectedImageBytes)
            Else
                cmd.Parameters.AddWithValue("@img", DBNull.Value)
            End If
            cmd.Parameters.AddWithValue("@id", currentItemId)
            cmd.ExecuteNonQuery()
            cmd.Dispose()
            MsgBox("Menu item updated successfully!", MsgBoxStyle.Information, "Success")
        End If

        clearMenuForm()
        readdatamenuitems()
        previewautonumber()
    End Sub

    Private Sub clearbtn_Click(sender As Object, e As EventArgs) Handles clearbtn.Click
        clearMenuForm(keepCode:=True)
        previewautonumber()
    End Sub

    Private Sub addbtncateg_Click(sender As Object, e As EventArgs) Handles addbtncateg.Click
        If Not ValidateCategory() Then Exit Sub
        conn()

        Dim catName As String = StrConv(txbcategoryname.Text.Trim(), VbStrConv.ProperCase)

        If currentCategoryId = 0 Then
            query = "INSERT INTO categories (category_name) VALUES (@name)"
        Else
            query = "UPDATE categories SET category_name = @name WHERE category_id = @id"
        End If

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@name", catName)

        If currentCategoryId > 0 Then
            cmd.Parameters.AddWithValue("@id", currentCategoryId)
        End If

        cmd.ExecuteNonQuery()
        cmd.Dispose()

        MsgBox(If(currentCategoryId = 0, "Category added successfully!", "Category updated successfully!"), MsgBoxStyle.Information, "Success")

        currentCategoryId = 0
        readdatacategories()
        clearCategoryForm()
    End Sub

    Private Sub clearbtncateg_Click(sender As Object, e As EventArgs) Handles clearbtncateg.Click
        clearCategoryForm()
    End Sub

    Private Sub datagridviewmenulist_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridviewmenulist.CellContentClick
        If e.RowIndex < 0 Then Exit Sub

        Dim itemCode As String = datagridviewmenulist(0, e.RowIndex).Value.ToString()
        Dim itemId As Integer = GetItemIdByCode(itemCode)

        If e.ColumnIndex = 7 Then
            Dim status As String = datagridviewmenulist(5, e.RowIndex).Value.ToString()
            Dim newStatus As Integer = If(status = "ACTIVE", 0, 1)

            conn()
            query = "UPDATE menu_items SET IsActive = @s WHERE item_id = @id"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@s", newStatus)
            cmd.Parameters.AddWithValue("@id", itemId)
            cmd.ExecuteNonQuery()
            cmd.Dispose()
            readdatamenuitems()

        ElseIf e.ColumnIndex = 8 Then
            currentItemId = itemId
            txbiditem.Text = itemCode
            txbpn.Text = datagridviewmenulist(1, e.RowIndex).Value.ToString()
            txbprice.Text = datagridviewmenulist(2, e.RowIndex).Value.ToString().Replace("₱", "").Trim()
            txbproductcat.Text = datagridviewmenulist(3, e.RowIndex).Value.ToString()
            txbpq.Text = datagridviewmenulist(4, e.RowIndex).Value.ToString()

            conn()
            query = "SELECT product_image FROM menu_items WHERE item_id = @id"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@id", currentItemId)
            Dim imgObj As Object = cmd.ExecuteScalar()
            cmd.Dispose()

            ' ⭐ LOAD BYTES INTO PICTUREBOX ON UPDATE ⭐
            If imgObj IsNot Nothing AndAlso Not IsDBNull(imgObj) Then
                selectedImageBytes = CType(imgObj, Byte())
                Using ms As New MemoryStream(selectedImageBytes)
                    picturebox1.Image = Image.FromStream(ms)
                End Using
            Else
                picturebox1.Image = Nothing
                selectedImageBytes = Nothing
            End If

        ElseIf e.ColumnIndex = 9 Then
            Dim result As MsgBoxResult = MsgBox("Are you sure you want to delete product '" & itemCode & "'?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Confirm Delete")

            If result = MsgBoxResult.Yes Then
                conn()
                query = "DELETE FROM menu_items WHERE item_id = @id"
                cmd = New SqlClient.SqlCommand(query, sqlconn)
                cmd.Parameters.AddWithValue("@id", itemId)
                cmd.ExecuteNonQuery()
                cmd.Dispose()

                readdatamenuitems()
                MsgBox("Menu item deleted successfully!", MsgBoxStyle.Information, "Deleted")
            End If
        End If
    End Sub

    Private Sub datagridviewcategories_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridviewcategories.CellContentClick
        If e.RowIndex < 0 Then Exit Sub

        If e.ColumnIndex = 2 Then
            currentCategoryId = Convert.ToInt32(datagridviewcategories(0, e.RowIndex).Value)
            txbcategoryname.Text = datagridviewcategories(1, e.RowIndex).Value.ToString()

        ElseIf e.ColumnIndex = 3 Then
            Dim catId As Integer = Convert.ToInt32(datagridviewcategories(0, e.RowIndex).Value)
            Dim catName As String = datagridviewcategories(1, e.RowIndex).Value.ToString()

            conn()
            query = "SELECT COUNT(*) FROM menu_items WHERE category_id = @id"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@id", catId)
            Dim itemCount As Integer = Convert.ToInt32(cmd.ExecuteScalar())
            cmd.Dispose()

            If itemCount > 0 Then
                MsgBox("Cannot delete '" & catName & "' because it has " & itemCount & " products. Please delete those products first.", MsgBoxStyle.Exclamation, "Action Required")
                Exit Sub
            End If

            Dim result As MsgBoxResult = MsgBox("Are you sure you want to delete category '" & catName & "'?", MsgBoxStyle.YesNo + MsgBoxStyle.Question, "Confirm Delete")

            If result = MsgBoxResult.Yes Then
                conn()
                query = "DELETE FROM categories WHERE category_id = @id"
                cmd = New SqlClient.SqlCommand(query, sqlconn)
                cmd.Parameters.AddWithValue("@id", catId)
                cmd.ExecuteNonQuery()
                cmd.Dispose()

                readdatacategories()
                clearCategoryForm()
                MsgBox("Category deleted successfully!", MsgBoxStyle.Information, "Deleted")
            End If
        End If
    End Sub

    Private Function GetCategoryId(categoryName As String) As Integer
        conn()
        query = "SELECT category_id FROM categories WHERE LOWER(category_name) = LOWER(@name)"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@name", categoryName)
        Dim id As Object = cmd.ExecuteScalar()
        cmd.Dispose()

        If id IsNot Nothing Then Return Convert.ToInt32(id) Else Return 0
    End Function

    Private Function GetItemIdByCode(itemCode As String) As Integer
        conn()
        query = "SELECT item_id FROM menu_items WHERE item_code = @code"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@code", itemCode)
        Dim id As Integer = Convert.ToInt32(cmd.ExecuteScalar())
        cmd.Dispose()
        Return id
    End Function

End Class