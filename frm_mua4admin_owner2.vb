Imports System.Data.SqlClient
Imports System.IO
Imports System.Text.RegularExpressions

Public Class frm_mua4admin_owner2

    Dim originalUsername As String = ""

    ' ==========================================
    ' FORM LOAD & SETUP
    ' ==========================================
    Private Sub frm_mua4admin_owner2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        rolefilter()
        cmbitems()
        readdata()
        ResetValidation()
        cmbshift.Enabled = False
    End Sub

    Private Sub rolefilter()
        Dim role As String = CurrentUser.role.ToLower()
        If role = "owner" Then
            lblmua.Text = "MANAGE USER ACCOUNTS - ADMIN/CASHIER"
            cmbrole.Items.Clear() : cmbrole.Items.AddRange({"admin", "cashier"})
            cmbfilter.Items.Clear() : cmbfilter.Items.AddRange({"all", "admin", "cashier"})
        ElseIf role = "admin" Then
            lblmua.Text = "MANAGE USER ACCOUNTS - OWNER/CASHIER"
            cmbrole.Items.Clear() : cmbrole.Items.AddRange({"owner", "cashier"})
            cmbfilter.Items.Clear() : cmbfilter.Items.AddRange({"all", "owner", "cashier"})
        Else
            lblmua.Text = "MANAGE USER ACCOUNTS"
            cmbrole.Items.Clear() : cmbrole.Items.AddRange({"admin", "owner", "cashier"})
            cmbfilter.Items.Clear() : cmbfilter.Items.AddRange({"all", "admin", "owner", "cashier"})
        End If
        If cmbfilter.Items.Count > 0 Then cmbfilter.SelectedIndex = 0
    End Sub

    Private Sub cmbitems()
        cmbshift.Items.Clear() : cmbshift.Items.AddRange({"am", "pm"})
        cmbgender.Items.Clear() : cmbgender.Items.AddRange({"male", "female"})
        cmben.Items.Clear() : cmben.Items.AddRange({"Jr.", "Sr.", "I", "II", "III", "IV", "V"})
    End Sub

    Private Sub UpdateShiftColumn()
        Dim r As String = cmbrole.Text.ToLower()
        Dim f As String = cmbfilter.Text.ToLower()
        datagridviewaccounts.Columns("Column3").Visible = Not (r = "owner" OrElse r = "admin" OrElse f = "owner" OrElse f = "admin")
    End Sub

    ' ==========================================
    ' READ DATA (FIXED FOR DBNULL)
    ' ==========================================
    Private Sub readdata()
        datagridviewaccounts.Rows.Clear()
        conn()
        query = "SELECT role, user_code, shift, gender, first_name, middle_name, last_name, extension_name, username, password, created_at, userimage FROM user_accounts WHERE 1 = 1 "

        If CurrentUser.role.ToLower() = "owner" Then query &= " AND LOWER(role) IN ('admin', 'cashier') "
        If CurrentUser.role.ToLower() = "admin" Then query &= " AND LOWER(role) IN ('owner', 'cashier') "
        If cmbfilter.Text <> "" AndAlso cmbfilter.Text.ToUpper() <> "ALL" Then query &= " AND LOWER(role) = LOWER(@ROLE) "
        If txbsearch.Text <> "" Then query &= " AND (username LIKE @SEARCH OR user_code LIKE @SEARCH) "
        query &= " ORDER BY created_at DESC "

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        With cmd.Parameters
            If cmbfilter.Text <> "" AndAlso cmbfilter.Text.ToUpper() <> "ALL" Then .AddWithValue("@ROLE", cmbfilter.Text)
            If txbsearch.Text <> "" Then .AddWithValue("@SEARCH", "%" & txbsearch.Text & "%")
        End With

        dr = cmd.ExecuteReader
        While dr.Read
            datagridviewaccounts.Rows.Add(
                dr("role"),
                dr("user_code"),
                If(IsDBNull(dr("shift")), "", dr("shift")),
                dr("gender"),
                dr("first_name"),
                If(IsDBNull(dr("middle_name")), "", dr("middle_name")),
                dr("last_name"),
                If(IsDBNull(dr("extension_name")), "", dr("extension_name")),
                dr("username"),
                dr("password"),
                dr("created_at"),
                If(IsDBNull(dr("userimage")), Nothing, Image.FromStream(New MemoryStream(CType(dr("userimage"), Byte())))),
                "UPDATE",
                "DELETE")
        End While
        cmd.Dispose()
        dr.Close()

        For i = 0 To datagridviewaccounts.Rows.Count - 1
            datagridviewaccounts.Rows(i).Height = 60
        Next
    End Sub

    ' ==========================================
    ' CLEAR FIELDS & AUTO NUMBER
    ' ==========================================
    Private Sub clear()
        ResetValidation()

        cmbrole.SelectedIndex = -1 : cmbshift.SelectedIndex = -1 : cmbgender.SelectedIndex = -1 : cmben.SelectedIndex = -1
        txbfn.Clear() : txbmn.Clear() : txbln.Clear() : txbusername.Clear() : txbpass.Clear() : txbcp.Clear()
        picturebox1.Image = Nothing
        originalUsername = ""
        chknoneen.Checked = False : chkbnonemn.Checked = False
        cmben.Enabled = True : txbmn.Enabled = True : cmbshift.Enabled = False
    End Sub

    Private Sub SP_AutoNumber()
        conn()
        Dim number As String
        query = "SELECT Max(NewNumber) FROM AutoNumberUser WHERE Pfx = 'DSBS'"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        With cmd
            If IsDBNull(cmd.ExecuteScalar) Then
                CreateNewAutoNumber()
                query = "SELECT Max(NewNumber) FROM AutoNumberUser WHERE Pfx = 'DSBS'"
                cmd = New SqlClient.SqlCommand(query, sqlconn)
                txbrole.Text = Convert.ToString(cmd.ExecuteScalar)
                cmd.Dispose()
            Else
                number = Convert.ToString(cmd.ExecuteScalar)
                txbrole.Text = number
            End If
        End With
        cmd.Dispose()
    End Sub

    Private Sub CreateNewAutoNumber()
        Try
            Dim cmd As New SqlCommand
            With cmd
                .Connection = sqlconn
                .CommandText = "SP_AutoNumberUser"
                .CommandType = CommandType.StoredProcedure
                .Parameters.AddWithValue("@pfx", "DSBS")
            End With
            cmd.ExecuteScalar()
            cmd.Dispose()
        Catch ex As Exception
        End Try
    End Sub

    Private Function IsUsernameExists() As Boolean
        conn()
        query = "SELECT COUNT(*) FROM user_accounts WHERE username = @username"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@username", txbusername.Text)
        Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
        cmd.Dispose()
        Return count > 0
    End Function

    ' ==========================================
    ' COMBINED VALIDATION & BORDER RESET
    ' ==========================================
    Private Sub ResetValidation()
        ErrorProvider1.Clear()

        txbusername.BorderColor = Color.Black
        txbpass.BorderColor = Color.Black
        txbcp.BorderColor = Color.Black
        txbfn.BorderColor = Color.Black
        txbln.BorderColor = Color.Black
        txbmn.BorderColor = Color.Black

        cmbrole.BorderColor = Color.Black
        cmbgender.BorderColor = Color.Black
        cmbshift.BorderColor = Color.Black
        cmben.BorderColor = Color.Black

        picturebox1.BackColor = Color.SlateGray
    End Sub

    ' ==========================================
    ' SAVE DATA (CONTROLLER)
    ' ==========================================
    Private Sub savedata()
        If originalUsername <> "" Then
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

        Dim mnnone As Object = DBNull.Value
        If Not chkbnonemn.Checked AndAlso txbmn.Text <> "" Then mnnone = txbmn.Text

        Dim ennone As Object = DBNull.Value
        If Not chknoneen.Checked AndAlso cmben.Text <> "" Then ennone = cmben.Text

        Dim shiftnone As Object = DBNull.Value
        If cmbrole.Text.ToLower() = "cashier" AndAlso cmbshift.Text <> "" Then shiftnone = cmbshift.Text

        query = "INSERT INTO user_accounts (role, user_code, username, password, gender, first_name, middle_name, last_name, extension_name, shift, created_at, userimage) VALUES (@ROLE, @CODE, @USERNAME, @PASSWORD, @GENDER, @FIRSTNAME, @MIDDLENAME, @LASTNAME, @EXTENSIONNAME, @SHIFT, @CREATED_AT, @USERIMAGE)"

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        With cmd.Parameters
            .AddWithValue("@ROLE", cmbrole.Text)
            .AddWithValue("@CODE", txbrole.Text)
            .AddWithValue("@USERNAME", txbusername.Text)
            .AddWithValue("@PASSWORD", txbpass.Text)
            .AddWithValue("@GENDER", cmbgender.Text)
            .AddWithValue("@FIRSTNAME", txbfn.Text)
            .AddWithValue("@MIDDLENAME", mnnone)
            .AddWithValue("@LASTNAME", txbln.Text)
            .AddWithValue("@EXTENSIONNAME", ennone)
            .AddWithValue("@SHIFT", shiftnone)
            .AddWithValue("@CREATED_AT", DateTime.Now)
            .AddWithValue("@USERIMAGE", ms.ToArray())
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

        Dim mnnone As Object = DBNull.Value
        If Not chkbnonemn.Checked AndAlso txbmn.Text <> "" Then mnnone = txbmn.Text

        Dim ennone As Object = DBNull.Value
        If Not chknoneen.Checked AndAlso cmben.Text <> "" Then ennone = cmben.Text

        Dim shiftnone As Object = DBNull.Value
        If cmbrole.Text.ToLower() = "cashier" AndAlso cmbshift.Text <> "" Then shiftnone = cmbshift.Text

        query = "UPDATE user_accounts SET role=@ROLE, user_code=@CODE, username=@USERNAME, password=@PASSWORD, gender=@GENDER, first_name=@FIRSTNAME, middle_name=@MIDDLENAME, last_name=@LASTNAME, extension_name=@EXTENSIONNAME, shift=@SHIFT, userimage=@USERIMAGE WHERE username=@ORIGINAL_USERNAME"

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        With cmd.Parameters
            .AddWithValue("@ROLE", cmbrole.Text)
            .AddWithValue("@CODE", txbrole.Text)
            .AddWithValue("@USERNAME", txbusername.Text)
            .AddWithValue("@PASSWORD", txbpass.Text)
            .AddWithValue("@GENDER", cmbgender.Text)
            .AddWithValue("@FIRSTNAME", txbfn.Text)
            .AddWithValue("@MIDDLENAME", mnnone)
            .AddWithValue("@LASTNAME", txbln.Text)
            .AddWithValue("@EXTENSIONNAME", ennone)
            .AddWithValue("@SHIFT", shiftnone)
            .AddWithValue("@USERIMAGE", ms.ToArray())
            .AddWithValue("@ORIGINAL_USERNAME", originalUsername)
        End With
        cmd.ExecuteNonQuery()
        cmd.Dispose()
    End Sub

    ' ==========================================
    ' LIVE VALIDATION EVENTS 
    ' ==========================================
    Private Sub txbusername_TextChanged(sender As Object, e As EventArgs) Handles txbusername.TextChanged
        If txbusername.Text.Length > 0 AndAlso txbusername.Text.Length < 6 Then
            ErrorProvider1.SetError(txbusername, "Minimum 6 characters.")
            txbusername.BorderColor = Color.Red
        ElseIf txbusername.Text.Length >= 6 AndAlso IsUsernameExists() And txbusername.Text <> originalUsername Then
            ErrorProvider1.SetError(txbusername, "Username already exists.")
            txbusername.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(txbusername, "")
            txbusername.BorderColor = Color.Black
        End If
    End Sub

    Private Sub txbpass_TextChanged(sender As Object, e As EventArgs) Handles txbpass.TextChanged
        If txbpass.Text.Length > 0 AndAlso txbpass.Text.Length < 8 Then
            ErrorProvider1.SetError(txbpass, "Minimum 8 characters.")
            txbpass.BorderColor = Color.Red
        ElseIf txbpass.Text.Length >= 8 AndAlso Not Regex.IsMatch(txbpass.Text, "[A-Z]") Then
            ErrorProvider1.SetError(txbpass, "Needs 1 uppercase letter.")
            txbpass.BorderColor = Color.Red
        ElseIf txbpass.Text.Length >= 8 AndAlso Not Regex.IsMatch(txbpass.Text, "[0-9]") Then
            ErrorProvider1.SetError(txbpass, "Needs 1 number.")
            txbpass.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(txbpass, "")
            txbpass.BorderColor = Color.Black
        End If
    End Sub

    Private Sub txbcp_TextChanged(sender As Object, e As EventArgs) Handles txbcp.TextChanged
        If txbcp.Text.Length > 0 AndAlso txbcp.Text <> txbpass.Text Then
            ErrorProvider1.SetError(txbcp, "Passwords do not match.")
            txbcp.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(txbcp, "")
            txbcp.BorderColor = Color.Black
        End If
    End Sub

    Private Sub txbfn_TextChanged(sender As Object, e As EventArgs) Handles txbfn.TextChanged
        If txbfn.Text.Length = 0 Then
            ErrorProvider1.SetError(txbfn, "First name is required.")
            txbfn.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(txbfn, "")
            txbfn.BorderColor = Color.Black
        End If
    End Sub

    Private Sub txbln_TextChanged(sender As Object, e As EventArgs) Handles txbln.TextChanged
        If txbln.Text.Length = 0 Then
            ErrorProvider1.SetError(txbln, "Last name is required.")
            txbln.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(txbln, "")
            txbln.BorderColor = Color.Black
        End If
    End Sub

    Private Sub txbmn_TextChanged(sender As Object, e As EventArgs) Handles txbmn.TextChanged
        If Not chkbnonemn.Checked AndAlso txbmn.Text.Length = 0 Then
            ErrorProvider1.SetError(txbmn, "Middle name is required.")
            txbmn.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(txbmn, "")
            txbmn.BorderColor = Color.Black
        End If
    End Sub

    Private Sub cmbrole_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbrole.SelectedIndexChanged
        UpdateShiftColumn()
        cmbshift.Enabled = (cmbrole.Text.ToLower() = "cashier")
        If Not cmbshift.Enabled Then cmbshift.SelectedIndex = -1
        If originalUsername = "" Then SP_AutoNumber()

        If cmbrole.SelectedIndex = -1 Then
            ErrorProvider1.SetError(cmbrole, "Role is required.")
            cmbrole.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(cmbrole, "")
            cmbrole.BorderColor = Color.Black
        End If
    End Sub

    Private Sub cmbgender_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbgender.SelectedIndexChanged
        If cmbgender.SelectedIndex = -1 Then
            ErrorProvider1.SetError(cmbgender, "Gender is required.")
            cmbgender.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(cmbgender, "")
            cmbgender.BorderColor = Color.Black
        End If
    End Sub

    Private Sub cmbshift_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbshift.SelectedIndexChanged
        If cmbrole.Text.ToLower() = "cashier" AndAlso cmbshift.SelectedIndex = -1 Then
            ErrorProvider1.SetError(cmbshift, "Shift is required.")
            cmbshift.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(cmbshift, "")
            cmbshift.BorderColor = Color.Black
        End If
    End Sub

    Private Sub cmben_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmben.SelectedIndexChanged
        If Not chknoneen.Checked AndAlso cmben.SelectedIndex = -1 Then
            ErrorProvider1.SetError(cmben, "Extension is required.")
            cmben.BorderColor = Color.Red
        Else
            ErrorProvider1.SetError(cmben, "")
            cmben.BorderColor = Color.Black
        End If
    End Sub

    ' ==========================================
    ' ADD BUTTON VALIDATION (FINAL CHECK)
    ' ==========================================
    Private Sub addbtn_Click(sender As Object, e As EventArgs) Handles addbtn.Click
        ResetValidation()
        Dim isValid As Boolean = True

        ' Username
        If txbusername.Text = "" Then
            ErrorProvider1.SetError(txbusername, "Username cannot be blank.")
            txbusername.BorderColor = Color.Red
            isValid = False
        ElseIf txbusername.Text.Length < 6 Then
            ErrorProvider1.SetError(txbusername, "Username should be 6 characters above.")
            txbusername.BorderColor = Color.Red
            isValid = False
        ElseIf IsUsernameExists() And txbusername.Text <> originalUsername Then
            ErrorProvider1.SetError(txbusername, "Existing Username.")
            txbusername.BorderColor = Color.Red
            isValid = False
        End If

        ' Password
        If txbpass.Text = "" Then
            ErrorProvider1.SetError(txbpass, "Password cannot be blank.")
            txbpass.BorderColor = Color.Red
            isValid = False
        ElseIf txbpass.Text.Length < 8 Then
            ErrorProvider1.SetError(txbpass, "Password should be 8 characters above.")
            txbpass.BorderColor = Color.Red
            isValid = False
        ElseIf Not Regex.IsMatch(txbpass.Text, "[A-Z]") Then
            ErrorProvider1.SetError(txbpass, "Password needs at least 1 uppercase letter.")
            txbpass.BorderColor = Color.Red
            isValid = False
        ElseIf Not Regex.IsMatch(txbpass.Text, "[0-9]") Then
            ErrorProvider1.SetError(txbpass, "Password needs at least 1 number.")
            txbpass.BorderColor = Color.Red
            isValid = False
        End If

        ' Confirm Password
        If txbcp.Text = "" Then
            ErrorProvider1.SetError(txbcp, "Confirm password cannot be blank.")
            txbcp.BorderColor = Color.Red
            isValid = False
        ElseIf txbpass.Text <> txbcp.Text Then
            ErrorProvider1.SetError(txbcp, "Passwords do not match.")
            txbcp.BorderColor = Color.Red
            isValid = False
        End If

        ' Names
        If txbfn.Text = "" Then
            ErrorProvider1.SetError(txbfn, "First name cannot be blank.")
            txbfn.BorderColor = Color.Red
            isValid = False
        End If

        If txbln.Text = "" Then
            ErrorProvider1.SetError(txbln, "Last name cannot be blank.")
            txbln.BorderColor = Color.Red
            isValid = False
        End If

        If Not chkbnonemn.Checked Then
            If txbmn.Text = "" Then
                ErrorProvider1.SetError(txbmn, "Middle name cannot be blank or check 'No Middle Name'.")
                txbmn.BorderColor = Color.Red
                isValid = False
            End If
        End If

        ' Dropdowns & Image
        If Not chknoneen.Checked AndAlso cmben.Text = "" Then
            ErrorProvider1.SetError(cmben, "Please select extension name or check 'None'.")
            cmben.BorderColor = Color.Red
            isValid = False
        End If

        If cmbrole.SelectedIndex = -1 Then
            ErrorProvider1.SetError(cmbrole, "Please select role.")
            cmbrole.BorderColor = Color.Red
            isValid = False
        End If

        If cmbgender.SelectedIndex = -1 Then
            ErrorProvider1.SetError(cmbgender, "Please select gender.")
            cmbgender.BorderColor = Color.Red
            isValid = False
        End If

        If cmbrole.Text.ToLower() = "cashier" AndAlso cmbshift.SelectedIndex = -1 Then
            ErrorProvider1.SetError(cmbshift, "Please select shift for cashier.")
            cmbshift.BorderColor = Color.Red
            isValid = False
        End If

        If picturebox1.Image Is Nothing Then
            ErrorProvider1.SetError(picturebox1, "Image cannot be empty.")
            isValid = False
        End If

        ' Save
        If isValid Then
            savedata()
            MsgBox("Data added successfully", MsgBoxStyle.Information, "Success")
            readdata()
            clear()
        End If
    End Sub

    Private Sub clearbtn_Click(sender As Object, e As EventArgs) Handles clearbtn.Click
        clear()
    End Sub

    ' ==========================================
    ' GRID CLICK EVENTS (FIXED FOR DBNULL)
    ' ==========================================
    Private Sub datagridviewaccounts_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridviewaccounts.CellContentClick
        If e.RowIndex < 0 Then Exit Sub
        Dim i As Integer = datagridviewaccounts.CurrentRow.Index

        ' UPDATE
        If e.ColumnIndex = 12 Then
            clear()

            ' Safely read text column values to prevent InvalidCastException
            cmbrole.Text = If(IsDBNull(datagridviewaccounts.Item(0, i).Value), "", datagridviewaccounts.Item(0, i).Value.ToString())
            txbrole.Text = If(IsDBNull(datagridviewaccounts.Item(1, i).Value), "", datagridviewaccounts.Item(1, i).Value.ToString())
            cmbshift.Text = If(IsDBNull(datagridviewaccounts.Item(2, i).Value), "", datagridviewaccounts.Item(2, i).Value.ToString())
            cmbgender.Text = If(IsDBNull(datagridviewaccounts.Item(3, i).Value), "", datagridviewaccounts.Item(3, i).Value.ToString())
            txbfn.Text = If(IsDBNull(datagridviewaccounts.Item(4, i).Value), "", datagridviewaccounts.Item(4, i).Value.ToString())

            ' Middle Name safe check
            If IsDBNull(datagridviewaccounts.Item(5, i).Value) OrElse datagridviewaccounts.Item(5, i).Value.ToString() = "" Then
                chkbnonemn.Checked = True
                txbmn.Text = ""
            Else
                chkbnonemn.Checked = False
                txbmn.Text = datagridviewaccounts.Item(5, i).Value.ToString()
            End If

            txbln.Text = If(IsDBNull(datagridviewaccounts.Item(6, i).Value), "", datagridviewaccounts.Item(6, i).Value.ToString())

            ' Extension Name safe check
            If IsDBNull(datagridviewaccounts.Item(7, i).Value) OrElse datagridviewaccounts.Item(7, i).Value.ToString() = "" Then
                chknoneen.Checked = True
                cmben.Text = ""
            Else
                chknoneen.Checked = False
                cmben.Text = datagridviewaccounts.Item(7, i).Value.ToString()
            End If

            txbusername.Text = If(IsDBNull(datagridviewaccounts.Item(8, i).Value), "", datagridviewaccounts.Item(8, i).Value.ToString())
            originalUsername = txbusername.Text
            txbpass.Text = If(IsDBNull(datagridviewaccounts.Item(9, i).Value), "", datagridviewaccounts.Item(9, i).Value.ToString())
            txbcp.Text = If(IsDBNull(datagridviewaccounts.Item(9, i).Value), "", datagridviewaccounts.Item(9, i).Value.ToString())

            viewimage()
            ResetValidation()

            ' DELETE
        ElseIf e.ColumnIndex = 13 Then
            Dim targetRole As String = datagridviewaccounts.Item(0, i).Value.ToString().ToLower()
            Dim targetUsername As String = datagridviewaccounts.Item(8, i).Value.ToString()

            If CurrentUser.role.ToLower() = "admin" AndAlso targetRole = "owner" Then
                Dim inputPass As String = InputBox("Enter your ADMIN password to authorize this action:", "Authorization Required", "")
                If inputPass = "" Then Exit Sub
                If Not IsPasswordCorrect(inputPass) Then Exit Sub
            Else
                If MessageBox.Show("Are you sure you want to delete this Account?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
                    Exit Sub
                End If
            End If

            query = "DELETE FROM user_accounts WHERE username = @USERNAME"
            cmd = New SqlClient.SqlCommand(query, sqlconn)
            With cmd.Parameters
                .AddWithValue("@USERNAME", targetUsername)
            End With
            cmd.ExecuteNonQuery()
            cmd.Dispose()
            readdata()
        End If
    End Sub

    ' ==========================================
    ' VIEW IMAGE & OTHER EVENTS
    ' ==========================================
    Private Sub viewimage()
        If txbusername.Text = "" Then
            picturebox1.Image = Nothing
            Exit Sub
        End If

        Dim img() As Byte
        query = "SELECT * FROM user_accounts WHERE username = '" & txbusername.Text & "'"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        dr = cmd.ExecuteReader

        While dr.Read
            If IsDBNull(dr("userimage")) Then
                picturebox1.Image = Nothing
            Else
                img = dr("userimage")
                Dim ms As New MemoryStream(img)
                picturebox1.Image = Image.FromStream(ms)

            End If
        End While

        cmd.Dispose()
        dr.Close()
    End Sub

    Private Function IsPasswordCorrect(inputPass As String) As Boolean
        query = "SELECT COUNT(*) FROM user_accounts WHERE userID = @userID AND password = @password"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        With cmd.Parameters
            .AddWithValue("@userID", CurrentUser.user_id)
            .AddWithValue("@password", inputPass)
        End With
        Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
        cmd.Dispose()

        If count > 0 Then Return True Else Return False
    End Function

    Private Sub chknoneen_CheckedChanged(sender As Object, e As EventArgs) Handles chknoneen.CheckedChanged
        cmben.Enabled = Not chknoneen.Checked
        If chknoneen.Checked Then cmben.Text = ""
        ErrorProvider1.SetError(cmben, "")
        cmben.BorderColor = Color.Black
    End Sub

    Private Sub chkbnonemn_CheckedChanged(sender As Object, e As EventArgs) Handles chkbnonemn.CheckedChanged
        txbmn.Enabled = Not chkbnonemn.Checked
        If chkbnonemn.Checked Then txbmn.Text = ""
        ErrorProvider1.SetError(txbmn, "")
        txbmn.BorderColor = Color.Black
    End Sub

    Private Sub cmbfilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbfilter.SelectedIndexChanged
        UpdateShiftColumn()
        readdata()
    End Sub

    Private Sub txbsearch_TextChanged(sender As Object, e As EventArgs) Handles txbsearch.TextChanged
        readdata()
    End Sub

    Private Sub picturebox1_Click(sender As Object, e As EventArgs) Handles picturebox1.Click
        Dim ofd As New OpenFileDialog()
        ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
        If ofd.ShowDialog() = DialogResult.OK Then
            picturebox1.SizeMode = PictureBoxSizeMode.StretchImage
            picturebox1.Image = Image.FromFile(ofd.FileName)
            ErrorProvider1.SetError(picturebox1, "")


        End If
    End Sub

    ' ==========================================
    ' KEYPRESS (LETTERS ONLY)
    ' ==========================================
    Private Sub OnlyLetters_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbfn.KeyPress, txbmn.KeyPress, txbln.KeyPress
        If Not (Char.IsLetter(e.KeyChar) OrElse Char.IsControl(e.KeyChar) OrElse e.KeyChar = " "c) Then
            e.Handled = True
        End If
    End Sub

End Class