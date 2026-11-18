Imports System.Data.SqlClient
Imports System.IO

Public Class frm_mm4admin

    Dim originalItemCode As String = ""
    Dim originalCategoryId As Integer = 0

    ' ==========================================
    ' FORM LOAD & SETUP
    ' ==========================================
    Private Sub frm_mm4admin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        categoryfilter()
        SP_AutoNumber()
        readdata()
        readCategories()
        loadCategoriesToCombo()
        ResetValidation()
    End Sub

    ' ==========================================
    ' FILTER & COMBOBOX SETUP
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
    ' AUTO NUMBER GENERATOR
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
    ' READ DATA
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
            If txbsearch.Text <> "" Then .AddWithValue("@SEARCH", "%" & txbsearch.Text & "%")
        End With

        dr = cmd.ExecuteReader
        While dr.Read
            datagridviewmenulist.Rows.Add(
                dr("item_code"),
                dr("product_name"),
                dr("price"),
                dr("category_name"),
                dr("quantity"),
                If(CBool(dr("IsActive")), "ACTIVE", "INACTIVE"),
                If(IsDBNull(dr("product_image")), Nothing, Image.FromStream(New MemoryStream(CType(dr("product_image"), Byte())))), ' 6
                If(CBool(dr("IsActive")), "DISABLE", "ENABLE"),
                "UPDATE",
                "DELETE")

            datagridviewmenulist.Rows(datagridviewmenulist.Rows.Count - 1).Tag = dr("category_id")
        End While
        cmd.Dispose()
        dr.Close()

        For i = 0 To datagridviewmenulist.Rows.Count - 1
            datagridviewmenulist.Rows(i).Height = 100
        Next
    End Sub

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
    ' COMBINED VALIDATION & BORDER RESET
    ' ==========================================
    Private Sub ResetValidation()
        ErrorProvider1.Clear()

        txbpn.BorderColor = Color.Black
        txbprice.BorderColor = Color.Black
        txbpq.BorderColor = Color.Black
        txbproductcat.BorderColor = Color.Black
        txbcategoryname.BorderColor = Color.Black

        picturebox1.BackColor = Color.SlateGray
    End Sub

    ' ==========================================
    ' CLEAR FIELDS
    ' ==========================================
    Sub clear()
        ResetValidation()
        originalItemCode = ""
        txbpn.Clear() : txbprice.Clear() : txbpq.Clear()
        txbproductcat.SelectedIndex = -1
        picturebox1.Image = Nothing
        SP_AutoNumber()
    End Sub

    Sub clearCategory()
        ResetValidation()
        originalCategoryId = 0
        txbcategoryname.Clear()
    End Sub

    ' ==========================================
    ' SAVE MENU (CONTROLLER)
    ' ==========================================
    Sub saveMenu()
        If originalItemCode <> "" Then
            updateData()
        Else
            insertData()
        End If
    End Sub

    ' ==========================================
    ' INSERT DATA SUB
    ' ==========================================
    Private Sub insertData()
        Dim ms As New MemoryStream
        picturebox1.Image.Save(ms, picturebox1.Image.RawFormat)

        conn()
        query = "SELECT category_id FROM categories WHERE category_name = @NAME"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@NAME", txbproductcat.Text)
        Dim categoryId As Integer = Convert.ToInt32(cmd.ExecuteScalar())
        cmd.Dispose()

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
    End Sub

    ' ==========================================
    ' UPDATE DATA SUB
    ' ==========================================
    Private Sub updateData()
        Dim ms As New MemoryStream
        picturebox1.Image.Save(ms, picturebox1.Image.RawFormat)

        conn()
        query = "SELECT category_id FROM categories WHERE category_name = @NAME"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@NAME", txbproductcat.Text)
        Dim categoryId As Integer = Convert.ToInt32(cmd.ExecuteScalar())
        cmd.Dispose()

        query = "UPDATE menu_items SET product_name=@NAME, price=@PRICE, category_id=@CAT, " &
            "       quantity=@QTY, product_image=@IMG WHERE item_code=@CODE"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        With cmd.Parameters
            .AddWithValue("@NAME", txbpn.Text)
            .AddWithValue("@PRICE", txbprice.Text)
            .AddWithValue("@CAT", categoryId)
            .AddWithValue("@QTY", txbpq.Text)
            .AddWithValue("@IMG", ms.ToArray())
            .AddWithValue("@CODE", originalItemCode)
        End With
        cmd.ExecuteNonQuery()
        cmd.Dispose()
    End Sub

    ' ==========================================
    ' LIVE VALIDATION EVENTS
    ' ==========================================
    Private Sub txbpn_TextChanged(sender As Object, e As EventArgs) Handles txbpn.TextChanged
        If txbpn.Text.Length = 0 Then
            ErrorProvider1.SetError(txbpn, "Product name cannot be blank.")
            txbpn.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(txbpn, "")
            txbpn.BorderColor = Color.Black
        End If
    End Sub

    Private Sub txbprice_TextChanged(sender As Object, e As EventArgs) Handles txbprice.TextChanged
        Dim priceVal As Decimal
        If txbprice.Text.Length = 0 Then
            ErrorProvider1.SetError(txbprice, "Price cannot be blank.")
            txbprice.BorderColor = Color.Red
        ElseIf Not Decimal.TryParse(txbprice.Text, priceVal) OrElse priceVal <= 0 Then
            ErrorProvider1.SetError(txbprice, "Enter a valid price greater than 0.")
            txbprice.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(txbprice, "")
            txbprice.BorderColor = Color.Black
        End If
    End Sub

    Private Sub txbpq_TextChanged(sender As Object, e As EventArgs) Handles txbpq.TextChanged
        Dim qtyVal As Integer
        If txbpq.Text.Length = 0 Then
            ErrorProvider1.SetError(txbpq, "Quantity cannot be blank.")
            txbpq.BorderColor = Color.Red
        ElseIf Not Integer.TryParse(txbpq.Text, qtyVal) OrElse qtyVal < 0 Then
            ErrorProvider1.SetError(txbpq, "Enter a valid quantity.")
            txbpq.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(txbpq, "")
            txbpq.BorderColor = Color.Black
        End If
    End Sub

    Private Sub txbproductcat_SelectedIndexChanged(sender As Object, e As EventArgs) Handles txbproductcat.SelectedIndexChanged
        If txbproductcat.SelectedIndex = -1 Then
            ErrorProvider1.SetError(txbproductcat, "Please select category.")
            txbproductcat.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(txbproductcat, "")
            txbproductcat.BorderColor = Color.Black
        End If
    End Sub

    Private Sub txbcategoryname_TextChanged(sender As Object, e As EventArgs) Handles txbcategoryname.TextChanged
        If txbcategoryname.Text.Length = 0 Then
            ErrorProvider1.SetError(txbcategoryname, "Category name cannot be blank.")
            txbcategoryname.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(txbcategoryname, "")
            txbcategoryname.BorderColor = Color.Black
        End If
    End Sub

    ' ==========================================
    ' ADD BUTTON VALIDATION (FINAL CHECK)
    ' ==========================================
    Private Sub addmenubtn_Click(sender As Object, e As EventArgs) Handles addmenubtn.Click
        ResetValidation()
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

    Private Sub addbtncateg_Click(sender As Object, e As EventArgs) Handles addbtncateg.Click
        ResetValidation()

        If txbcategoryname.Text = "" Then
            ErrorProvider1.SetError(txbcategoryname, "Category name cannot be blank.")
            txbcategoryname.BorderColor = Color.Red
            Exit Sub
        End If

        query = "SELECT COUNT(*) FROM categories WHERE category_name = @NAME AND category_id <> @CID"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        With cmd.Parameters
            .AddWithValue("@NAME", txbcategoryname.Text)
            .AddWithValue("@CID", originalCategoryId)
        End With
        If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
            ErrorProvider1.SetError(txbcategoryname, "Category name already exists.")
            txbcategoryname.BorderColor = Color.Red
            cmd.Dispose()
            Exit Sub
        End If
        cmd.Dispose()

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
    End Sub

    Private Sub clearbtncateg_Click(sender As Object, e As EventArgs) Handles clearbtncateg.Click
        clearCategory()
    End Sub

    ' ==========================================
    ' GRID CLICK EVENTS
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
            ResetValidation()

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

    Private Sub datagridviewcategories_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridviewcategories.CellContentClick
        If e.RowIndex < 0 Then Return
        Dim i As Integer = e.RowIndex

        If e.ColumnIndex = 2 Then
            clearCategory()
            originalCategoryId = CInt(datagridviewcategories.Item(0, i).Value)
            txbcategoryname.Text = datagridviewcategories.Item(1, i).Value.ToString()
            ResetValidation()

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
    ' IMAGE & KEYPRESS EVENTS
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

    Private Sub ProductName_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbpn.KeyPress
        If Not (Char.IsLetter(e.KeyChar) OrElse Char.IsControl(e.KeyChar) OrElse e.KeyChar = " "c) Then
            e.Handled = True
        End If
    End Sub

    Private Sub NumericOnly_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbprice.KeyPress, txbpq.KeyPress
        If Not (Char.IsDigit(e.KeyChar) OrElse Char.IsControl(e.KeyChar) OrElse e.KeyChar = "."c) Then
            e.Handled = True
        ElseIf sender Is txbprice AndAlso e.KeyChar = "."c AndAlso txbprice.Text.Contains(".") Then
            e.Handled = True
        End If
    End Sub

    Private Sub CategoryName_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbcategoryname.KeyPress
        If Not (Char.IsLetter(e.KeyChar) OrElse Char.IsControl(e.KeyChar) OrElse e.KeyChar = " "c) Then
            e.Handled = True
        End If
    End Sub

    Private Sub cmbfilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbfilter.SelectedIndexChanged
        readdata()
    End Sub

    Private Sub txbsearch_TextChanged(sender As Object, e As EventArgs) Handles txbsearch.TextChanged
        readdata()
    End Sub

End Class