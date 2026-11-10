Imports System.Data.SqlClient
Imports System.IO

Public Class frm_mm4admin

    Dim originalItemCode As String = ""
    Dim originalCategoryId As Integer = 0

    ' ==========================================
    ' LOAD
    ' ==========================================
    Private Sub frm_mm4admin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        categoryfilter()
        SP_AutoNumber()
        readdata()
        readCategories()
        loadCategoriesToCombo()
        bordercolor()
    End Sub

    ' ==========================================
    ' SETUP FILTER COMBO
    ' ==========================================
    Sub categoryfilter()
        cmbfilter.Items.Clear()
        cmbfilter.Items.Add("all")

        conn()
        query = "SELECT category_name FROM categories ORDER BY category_id"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        dr = cmd.ExecuteReader
        While dr.Read
            cmbfilter.Items.Add(dr("category_name").ToString())
        End While
        dr.Close()
        cmd.Dispose()

        If cmbfilter.Items.Count > 0 Then cmbfilter.SelectedIndex = 0
    End Sub

    ' ==========================================
    ' AUTO NUMBER (same pattern as frm_mua4admin_owner2)
    ' ==========================================
    Sub SP_AutoNumber()
        conn()
        Dim number As String
        query = "SELECT Max(NewNumber) FROM AutoNumberMenu WHERE Pfx = 'M'"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        With cmd
            If IsDBNull(cmd.ExecuteScalar) Then
                CreateNewAutoNumber()
                query = "SELECT Max(NewNumber) FROM AutoNumberMenu WHERE Pfx = 'M'"
                cmd = New SqlClient.SqlCommand(query, sqlconn)
                txbiditem.Text = Convert.ToString(cmd.ExecuteScalar)
                cmd.Dispose()
            Else
                number = Convert.ToString(cmd.ExecuteScalar)
                txbiditem.Text = number
            End If
        End With
        cmd.Dispose()
    End Sub

    ' ==========================================
    ' CREATE NEW AUTO NUMBER (same pattern as user form)
    ' ==========================================
    Sub CreateNewAutoNumber()
        Try
            Dim cmd As New SqlCommand
            With cmd
                .Connection = sqlconn
                .CommandText = "SP_AutoNumberMenu"
                .CommandType = CommandType.StoredProcedure
                .Parameters.AddWithValue("@pfx", "M")
            End With
            cmd.ExecuteScalar()
            cmd.Dispose()
        Catch ex As Exception
        End Try
    End Sub

    ' ==========================================
    ' READ DATA (MENU GRID)
    ' ==========================================
    Sub readdata()
        If datagridviewmenulist.Columns.Count = 0 Then Return
        datagridviewmenulist.Rows.Clear()
        conn()

        query = "SELECT m.item_code, m.product_name, m.price, c.category_name, " &
            "       m.quantity, m.IsActive, m.product_image, c.category_id " &
            "FROM menu_items m " &
            "INNER JOIN categories c ON m.category_id = c.category_id " &
            "WHERE 1 = 1 "

        If cmbfilter.Text <> "" AndAlso cmbfilter.Text.ToUpper() <> "ALL" Then
            query &= " AND c.category_name = @FILTER "
        End If
        If txbsearch.Text() <> "" Then
            query &= " AND (LOWER(m.item_code) LIKE LOWER(@SEARCH) OR LOWER(m.product_name) LIKE LOWER(@SEARCH)) "
        End If
        query &= " ORDER BY m.item_id"

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        With cmd.Parameters
            If cmbfilter.Text <> "" AndAlso cmbfilter.Text.ToUpper() <> "ALL" Then .AddWithValue("@FILTER", cmbfilter.Text)
            If txbsearch.Text.Trim() <> "" Then .AddWithValue("@SEARCH", "%" & txbsearch.Text.Trim() & "%")
        End With

        dr = cmd.ExecuteReader
        While dr.Read
            Dim img As Image = Nothing
            If Not IsDBNull(dr("product_image")) Then
                Using ms As New MemoryStream(CType(dr("product_image"), Byte()))
                    img = Image.FromStream(ms)
                End Using
            End If

            Dim isActive As Boolean = CBool(dr("IsActive"))

            datagridviewmenulist.Rows.Add(
            dr("item_code"),                                              ' 0
            dr("product_name"),                                           ' 1
            dr("price"),                                                  ' 2
            dr("category_name"),                                          ' 3
            dr("quantity"),                                               ' 4
            If(isActive, "ACTIVE", "INACTIVE"),                           ' 5
            img,                                                          ' 6
            If(isActive, "DISABLE", "ENABLE"),                            ' 7
            "UPDATE",                                                     ' 8
            "DELETE")                                                     ' 9

            datagridviewmenulist.Rows(datagridviewmenulist.Rows.Count - 1).Tag = dr("category_id")
        End While
        cmd.Dispose()
        dr.Close()

        For i = 0 To datagridviewmenulist.Rows.Count - 1
            datagridviewmenulist.Rows(i).Height = 100
        Next
    End Sub

    ' ==========================================
    ' READ DATA (CATEGORIES GRID)
    ' ==========================================
    Sub readCategories()
        If datagridviewcategories.Columns.Count = 0 Then Return
        datagridviewcategories.Rows.Clear()
        conn()

        query = "SELECT category_id, category_name FROM categories ORDER BY category_id"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        dr = cmd.ExecuteReader
        While dr.Read
            datagridviewcategories.Rows.Add(dr("category_id"), dr("category_name"), "UPDATE", "DELETE")
        End While
        cmd.Dispose()
        dr.Close()

        For i = 0 To datagridviewcategories.Rows.Count - 1
            datagridviewcategories.Rows(i).Height = 30
        Next
    End Sub

    ' ==========================================
    ' LOAD CATEGORIES TO COMBO
    ' ==========================================
    Sub loadCategoriesToCombo()
        conn()
        txbproductcat.Items.Clear()

        query = "SELECT category_name FROM categories ORDER BY category_name"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        dr = cmd.ExecuteReader
        While dr.Read
            txbproductcat.Items.Add(dr("category_name").ToString())
        End While
        dr.Close()
        cmd.Dispose()

        categoryfilter()
    End Sub

    ' ==========================================
    ' CLEAR (MENU)
    ' ==========================================
    Sub clear()
        ErrorProvider1.Clear()
        bordercolor()
        originalItemCode = ""
        txbpn.Clear() : txbprice.Clear() : txbpq.Clear()
        txbproductcat.SelectedIndex = -1
        picturebox1.Image = Nothing
        SP_AutoNumber()
    End Sub

    ' ==========================================
    ' CLEAR (CATEGORY)
    ' ==========================================
    Sub clearCategory()
        ErrorProvider1.Clear()
        bordercolor()
        originalCategoryId = 0
        txbcategoryname.Clear()
    End Sub

    Sub bordercolor()
        txbpn.BorderColor = Color.Black
        txbprice.BorderColor = Color.Black
        txbpq.BorderColor = Color.Black
        txbproductcat.BorderColor = Color.Black
        txbcategoryname.BorderColor = Color.Black
        picturebox1.BackColor = Color.SlateGray
    End Sub

    ' ==========================================
    ' SAVE MENU (INSERT / UPDATE)
    ' ==========================================
    Sub saveMenu()
        Dim ms As New MemoryStream
        picturebox1.Image.Save(ms, picturebox1.Image.RawFormat)

        ' ⭐ get category_id from the selected name
        conn()
        query = "SELECT category_id FROM categories WHERE category_name = @NAME"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@NAME", txbproductcat.Text)
        Dim categoryId As Integer = Convert.ToInt32(cmd.ExecuteScalar())
        cmd.Dispose()

        If originalItemCode <> "" Then
            ' UPDATE
            query = "UPDATE menu_items SET product_name=@NAME, price=@PRICE, category_id=@CAT, " &
                "       quantity=@QTY, product_image=@IMG WHERE item_code=@CODE"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            With cmd.Parameters
                .AddWithValue("@NAME", txbpn.Text)
                .AddWithValue("@PRICE", txbprice.Text)
                .AddWithValue("@CAT", categoryId)               ' ⭐ use lookup
                .AddWithValue("@QTY", txbpq.Text)
                .AddWithValue("@IMG", ms.ToArray())
                .AddWithValue("@CODE", originalItemCode)
            End With
            cmd.ExecuteNonQuery()
            cmd.Dispose()
        Else
            ' INSERT — same
            cmd = New SqlClient.SqlCommand("SP_AutoNumberMenu", sqlconn)
            cmd.CommandType = CommandType.StoredProcedure
            cmd.Parameters.AddWithValue("@pfx", "M")
            Dim outP As New SqlParameter("@NewNumber", SqlDbType.VarChar, 20)
            outP.Direction = ParameterDirection.Output
            cmd.Parameters.Add(outP)
            cmd.ExecuteScalar()
            Dim newCode As String = outP.Value.ToString()
            cmd.Dispose()

            query = "INSERT INTO menu_items (item_code, product_name, price, category_id, quantity, product_image, IsActive) " &
                "VALUES (@CODE, @NAME, @PRICE, @CAT, @QTY, @IMG, 1)"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            With cmd.Parameters
                .AddWithValue("@CODE", newCode)
                .AddWithValue("@NAME", txbpn.Text)
                .AddWithValue("@PRICE", txbprice.Text)
                .AddWithValue("@CAT", categoryId)
                .AddWithValue("@QTY", txbpq.Text)
                .AddWithValue("@IMG", ms.ToArray())
            End With
            cmd.ExecuteNonQuery()
            cmd.Dispose()

            CreateNewAutoNumber()
            SP_AutoNumber()
        End If
    End Sub
    ' ==========================================
    ' ADD / SAVE MENU BUTTON
    ' ==========================================
    Private Sub addmenubtn_Click(sender As Object, e As EventArgs) Handles addmenubtn.Click
        ErrorProvider1.Clear()
        bordercolor()
        Dim isValid As Boolean = True

        If txbpn.Text = "" Then
            ErrorProvider1.SetError(txbpn, "Product name cannot be blank.")
            txbpn.BorderColor = Color.Red
            isValid = False
        End If

        Dim priceVal As Decimal
        If txbprice.Text = "" Then
            ErrorProvider1.SetError(txbprice, "Price cannot be blank.")
            txbprice.BorderColor = Color.Red
            isValid = False
        ElseIf Not Decimal.TryParse(txbprice.Text, priceVal) OrElse priceVal <= 0 Then
            ErrorProvider1.SetError(txbprice, "Enter a valid price greater than 0.")
            txbprice.BorderColor = Color.Red
            isValid = False
        End If

        Dim qtyVal As Integer
        If txbpq.Text = "" Then
            ErrorProvider1.SetError(txbpq, "Quantity cannot be blank.")
            txbpq.BorderColor = Color.Red
            isValid = False
        ElseIf Not Integer.TryParse(txbpq.Text, qtyVal) OrElse qtyVal < 0 Then
            ErrorProvider1.SetError(txbpq, "Enter a valid quantity.")
            txbpq.BorderColor = Color.Red
            isValid = False
        End If

        If txbproductcat.SelectedIndex = -1 Then
            ErrorProvider1.SetError(txbproductcat, "Please select category.")
            txbproductcat.BorderColor = Color.Red
            isValid = False
        End If

        If picturebox1.Image Is Nothing Then
            ErrorProvider1.SetError(picturebox1, "Image cannot be empty.")
            isValid = False
        End If

        If isValid Then
            saveMenu()
            MsgBox("Data saved successfully", MsgBoxStyle.Information, "Success")
            readdata()
            clear()
        End If
    End Sub

    Private Sub clearbtn_Click(sender As Object, e As EventArgs) Handles clearbtn.Click
        clear()
    End Sub

    ' ==========================================
    ' MENU GRID CLICK — ACTION(7) / UPDATE(8) / DELETE(9)
    ' ==========================================
    Private Sub datagridviewmenulist_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridviewmenulist.CellContentClick
        If e.RowIndex < 0 Then Return
        Dim i As Integer = e.RowIndex
        Dim itemCode As String = datagridviewmenulist.Item(0, i).Value.ToString()

        If e.ColumnIndex = 7 Then
            If MessageBox.Show("Toggle status of " & itemCode & "?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                query = "UPDATE menu_items SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE item_code = @CODE"
                cmd = New SqlClient.SqlCommand(query, sqlconn)
                cmd.Parameters.AddWithValue("@CODE", itemCode)
                cmd.ExecuteNonQuery()
                cmd.Dispose()
                readdata()
            End If

        ElseIf e.ColumnIndex = 8 Then
            clear()
            originalItemCode = itemCode
            txbiditem.Text = originalItemCode
            txbpn.Text = datagridviewmenulist.Item(1, i).Value.ToString()
            txbprice.Text = datagridviewmenulist.Item(2, i).Value.ToString()
            txbpq.Text = datagridviewmenulist.Item(4, i).Value.ToString()
            txbproductcat.Text = datagridviewmenulist.Item(3, i).Value.ToString()
            viewimage()
            bordercolor()

        ElseIf e.ColumnIndex = 9 Then
            If MessageBox.Show("Delete this item permanently?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                query = "DELETE FROM menu_items WHERE item_code = @CODE"
                cmd = New SqlClient.SqlCommand(query, sqlconn)
                cmd.Parameters.AddWithValue("@CODE", itemCode)
                cmd.ExecuteNonQuery()
                cmd.Dispose()
                readdata()
            End If
        End If
    End Sub

    ' ==========================================
    ' VIEW IMAGE
    ' ==========================================
    Sub viewimage()
        If txbiditem.Text = "" Then
            picturebox1.Image = Nothing
            Return
        End If

        query = "SELECT product_image FROM menu_items WHERE item_code = @CODE"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@CODE", txbiditem.Text)
        dr = cmd.ExecuteReader

        While dr.Read
            If IsDBNull(dr("product_image")) Then
                picturebox1.Image = Nothing
            Else
                Using ms As New MemoryStream(CType(dr("product_image"), Byte()))
                    picturebox1.Image = Image.FromStream(ms)
                End Using
            End If
        End While

        cmd.Dispose()
        dr.Close()
    End Sub

    Private Sub picturebox1_Click(sender As Object, e As EventArgs) Handles picturebox1.Click
        Dim ofd As New OpenFileDialog()
        ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
        If ofd.ShowDialog() = DialogResult.OK Then
            picturebox1.SizeMode = PictureBoxSizeMode.StretchImage
            picturebox1.Image = Image.FromFile(ofd.FileName)
            ErrorProvider1.SetError(picturebox1, "")
            picturebox1.BackColor = Color.SlateGray
        End If
    End Sub

    ' ==========================================
    ' KEYPRESS — PRODUCT NAME
    ' ==========================================
    Private Sub ProductName_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbpn.KeyPress
        If Not (Char.IsLetter(e.KeyChar) OrElse Char.IsControl(e.KeyChar) OrElse e.KeyChar = " "c) Then
            e.Handled = True
        ElseIf txbpn.Text.Length = 0 OrElse (txbpn.SelectionStart > 0 AndAlso txbpn.Text(txbpn.SelectionStart - 1) = " "c) Then
            e.KeyChar = Char.ToUpper(e.KeyChar)
        End If
    End Sub

    ' ==========================================
    ' KEYPRESS — NUMERIC ONLY
    ' ==========================================
    Private Sub NumericOnly_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbprice.KeyPress, txbpq.KeyPress
        If Not (Char.IsDigit(e.KeyChar) OrElse Char.IsControl(e.KeyChar) OrElse e.KeyChar = "."c) Then
            e.Handled = True
        ElseIf sender Is txbprice AndAlso e.KeyChar = "."c AndAlso txbprice.Text.Contains(".") Then
            e.Handled = True
        End If
    End Sub

    ' ==========================================
    ' ADD / UPDATE CATEGORY
    ' ==========================================
    Private Sub addbtncateg_Click(sender As Object, e As EventArgs) Handles addbtncateg.Click
        ErrorProvider1.Clear()
        bordercolor()
        Dim isValid As Boolean = True

        If txbcategoryname.Text = "" Then
            ErrorProvider1.SetError(txbcategoryname, "Category name cannot be blank.")
            txbcategoryname.BorderColor = Color.Red
            isValid = False
        End If

        If isValid Then
            query = "SELECT COUNT(*) FROM categories WHERE category_name = @NAME AND category_id <> @CID"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            With cmd.Parameters
                .AddWithValue("@NAME", txbcategoryname.Text)
                .AddWithValue("@CID", originalCategoryId)
            End With
            If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
                ErrorProvider1.SetError(txbcategoryname, "Category name already exists.")
                txbcategoryname.BorderColor = Color.Red
                isValid = False
            End If
            cmd.Dispose()
        End If

        If isValid Then
            If originalCategoryId > 0 Then
                query = "UPDATE categories SET category_name=@NAME WHERE category_id=@CID"
                cmd = New SqlClient.SqlCommand(query, sqlconn)
                With cmd.Parameters
                    .AddWithValue("@NAME", txbcategoryname.Text)
                    .AddWithValue("@CID", originalCategoryId)
                End With
            Else
                query = "INSERT INTO categories (category_name) VALUES (@NAME)"
                cmd = New SqlClient.SqlCommand(query, sqlconn)
                With cmd.Parameters
                    .AddWithValue("@NAME", txbcategoryname.Text)
                End With
            End If
            cmd.ExecuteNonQuery()
            cmd.Dispose()

            MsgBox("Data saved successfully", MsgBoxStyle.Information, "Success")
            readCategories()
            loadCategoriesToCombo()
            clearCategory()
        End If
    End Sub

    Private Sub clearbtncateg_Click(sender As Object, e As EventArgs) Handles clearbtncateg.Click
        clearCategory()
    End Sub

    ' ==========================================
    ' CATEGORY GRID CLICK — UPDATE(2) / DELETE(3)
    ' ==========================================
    Private Sub datagridviewcategories_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridviewcategories.CellContentClick
        If e.RowIndex < 0 Then Return
        Dim i As Integer = e.RowIndex

        If e.ColumnIndex = 2 Then
            clearCategory()
            originalCategoryId = CInt(datagridviewcategories.Item(0, i).Value)
            txbcategoryname.Text = datagridviewcategories.Item(1, i).Value.ToString()
            bordercolor()

        ElseIf e.ColumnIndex = 3 Then
            query = "SELECT COUNT(*) FROM menu_items WHERE category_id = @CID"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            With cmd.Parameters
                .AddWithValue("@CID", CInt(datagridviewcategories.Item(0, i).Value))
            End With
            Dim inUse As Boolean = Convert.ToInt32(cmd.ExecuteScalar()) > 0
            cmd.Dispose()

            If inUse Then
                MsgBox("Cannot delete - category is in use.", MsgBoxStyle.Exclamation, "In Use")
            ElseIf MessageBox.Show("Delete this category?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                query = "DELETE FROM categories WHERE category_id = @CID"
                cmd = New SqlClient.SqlCommand(query, sqlconn)
                With cmd.Parameters
                    .AddWithValue("@CID", CInt(datagridviewcategories.Item(0, i).Value))
                End With
                cmd.ExecuteNonQuery()
                cmd.Dispose()
                readCategories()
                loadCategoriesToCombo()
            End If
        End If
    End Sub

    ' ==========================================
    ' CATEGORY NAME KEYPRESS
    ' ==========================================
    Private Sub CategoryName_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbcategoryname.KeyPress
        If Not (Char.IsLetter(e.KeyChar) OrElse Char.IsControl(e.KeyChar) OrElse e.KeyChar = " "c) Then
            e.Handled = True
        ElseIf txbcategoryname.Text.Length = 0 OrElse (txbcategoryname.SelectionStart > 0 AndAlso txbcategoryname.Text(txbcategoryname.SelectionStart - 1) = " "c) Then
            e.KeyChar = Char.ToUpper(e.KeyChar)
        End If
    End Sub

    ' ==========================================
    ' FILTER + SEARCH
    ' ==========================================
    Private Sub cmbfilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbfilter.SelectedIndexChanged
        readdata()
    End Sub

    Private Sub txbsearch_TextChanged(sender As Object, e As EventArgs) Handles txbsearch.TextChanged
        readdata()
    End Sub

End Class