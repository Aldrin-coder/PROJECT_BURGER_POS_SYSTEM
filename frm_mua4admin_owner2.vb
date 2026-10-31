
Imports System.Data.SqlClient
Imports System.IO
Imports System.Text.RegularExpressions


Public Class frm_mua4admin_owner2

    'mga variable
    Dim originalUsername As String = ""
    Dim isvalid As Boolean = True
    Dim isLoading As Boolean = True

    'kapag nagload ang form
    Private Sub frm_mua4admin_owner2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        SetupRoleUI()
        cmbitems()
        isvalid = False


        readdata()
        cmbshift.Enabled = False
        isLoading = False
    End Sub

    'setup ng role dropdown
    Private Sub SetupRoleUI()
        If CurrentUser.role.ToLower = "owner" Then
            lblmua.Text = "MANAGE USER ACCOUNTS - ADMIN/CASHIER"
            cmbrole.Items.Clear()
            cmbrole.Items.AddRange({"admin", "cashier"})
            cmbfilter.Items.Clear()
            cmbfilter.Items.AddRange({"all", "admin", "cashier"})
        ElseIf CurrentUser.role.ToLower = "admin" Then
            lblmua.Text = "MANAGE USER ACCOUNTS - OWNER/CASHIER"
            cmbrole.Items.Clear()
            cmbrole.Items.AddRange({"owner", "cashier"})
            cmbfilter.Items.Clear()
            cmbfilter.Items.AddRange({"all", "owner", "cashier"})
        Else
            lblmua.Text = "MANAGE USER ACCOUNTS"
            cmbrole.Items.Clear()
            cmbrole.Items.AddRange({"admin", "owner", "cashier"})
            cmbfilter.Items.Clear()
            cmbfilter.Items.AddRange({"all", "admin", "owner", "cashier"})
        End If

        lblmua.AutoSize = True
        If cmbfilter.Items.Count > 0 Then cmbfilter.SelectedIndex = 0
    End Sub

    'mga items sa combobox
    Sub cmbitems()
        cmbshift.Items.Clear()
        cmbshift.Items.AddRange({"am", "pm"})
        cmbgender.Items.Clear()
        cmbgender.Items.AddRange({"male", "female"})
        cmben.Items.Clear()
        cmben.Items.AddRange({"Jr.", "Sr.", "I", "II", "III", "IV", "V"})
    End Sub

    'itago ang shift column kung owner o admin
    Private Sub UpdateShiftColumn()
        Dim r As String = cmbrole.Text.ToLower()
        Dim f As String = cmbfilter.Text.ToLower()

        If r = "owner" OrElse r = "admin" OrElse f = "owner" OrElse f = "admin" Then
            datagridviewaccounts.Columns("Column3").Visible = False
        Else
            datagridviewaccounts.Columns("Column3").Visible = True
        End If
    End Sub

    'load data sa grid
    Sub readdata()
        datagridviewaccounts.Rows.Clear()
        Dim searchText As String = txbsearch.Text()
        Dim roleFilter As String = If(cmbfilter.Text, "")
        Dim curRole As String = CurrentUser.role.ToLower()

        conn()

        query = "SELECT u.role, u.userID, u.username, u.password, u.gender, u.first_name, u.middle_name, u.last_name, " &
                "u.extension_name, u.userimage, u.created_at, ISNULL(c.shift, '') AS [SHIFT], " &
                "COALESCE(a.admin_code, o.owner_code, c.cashier_code) AS [ROLE ID] " &
                "FROM user_accounts u " &
                "LEFT JOIN admin_accounts a ON a.userID = u.userID " &
                "LEFT JOIN owner_accounts o ON o.userID = u.userID " &
                "LEFT JOIN cashier_accounts c ON c.userID = u.userID " &
                "WHERE 1 = 1 "

        If curRole = "owner" Then
            query &= " AND LOWER(u.role) IN ('admin', 'cashier') "
        ElseIf curRole = "admin" Then
            query &= " AND LOWER(u.role) IN ('owner', 'cashier') "
        End If

        If roleFilter <> "" AndAlso roleFilter.ToUpper() <> "ALL" Then
            query &= " AND LOWER(u.role) = LOWER(@ROLE) "
        End If

        If searchText <> "" Then
            query &= " AND ( u.username LIKE @SEARCH OR u.first_name LIKE @SEARCH OR u.middle_name LIKE @SEARCH OR u.last_name LIKE @SEARCH OR COALESCE(a.admin_code, o.owner_code, c.cashier_code) LIKE @SEARCH ) "
        End If

        query &= " ORDER BY u.created_at DESC "

        cmd = New SqlCommand(query, sqlconn)
        If roleFilter <> "" AndAlso roleFilter.ToUpper() <> "ALL" Then cmd.Parameters.AddWithValue("@ROLE", roleFilter)
        If searchText <> "" Then cmd.Parameters.AddWithValue("@SEARCH", "%" & searchText & "%")

        dr = cmd.ExecuteReader()

        While dr.Read()
            Dim img As Image = Nothing
            If Not IsDBNull(dr("userimage")) Then
                Dim imgBytes() As Byte = CType(dr("userimage"), Byte())
                Using ms As New MemoryStream(imgBytes)
                    img = Image.FromStream(ms)
                End Using
            End If

            datagridviewaccounts.Rows.Add(
                dr("role"), dr("ROLE ID"), dr("SHIFT"), dr("gender"),
                dr("first_name"), dr("middle_name"), dr("last_name"),
                dr("extension_name"), dr("username"), dr("password"),
                dr("created_at"), dr("userimage"), "UPDATE", "DELETE")
        End While

        dr.Close()
        cmd.Dispose()

        For i = 0 To datagridviewaccounts.Rows.Count - 1
            datagridviewaccounts.Rows(i).Height = 100
        Next
    End Sub

    'reset borders
    Private Sub ResetBorders()
        txbusername.BorderColor = Color.Black
        txbpass.BorderColor = Color.Black
        txbcp.BorderColor = Color.Black
        txbfn.BorderColor = Color.Black
        txbln.BorderColor = Color.Black
        txbmn.BorderColor = Color.Black
        cmben.BorderColor = Color.Black
        cmbrole.BorderColor = Color.Black
        cmbgender.BorderColor = Color.Black
        cmbshift.BorderColor = Color.Black
    End Sub

    'validation bago magsave
    Private Function ValidateFields() As Boolean
        ErrorProvider1.Clear()
        ResetBorders()
        isvalid = True

        If txbusername.Text().Trim() = "" Then
            ErrorProvider1.SetError(txbusername, "Username is required.")
            txbusername.BorderColor = Color.Red
            isvalid = False
        ElseIf txbusername.Text().Trim().Length < 6 Then
            ErrorProvider1.SetError(txbusername, "Username must be at least 6 characters.")
            txbusername.BorderColor = Color.Red
            isvalid = False
        ElseIf IsUsernameDuplicate(txbusername.Text().Trim(), originalUsername) Then
            ErrorProvider1.SetError(txbusername, "Username already exists.")
            txbusername.BorderColor = Color.Red
            isvalid = False
        End If

        If txbpass.Text = "" Then
            ErrorProvider1.SetError(txbpass, "Password is required.")
            txbpass.BorderColor = Color.Red
            isvalid = False
        ElseIf txbpass.Text.Length < 8 Then
            ErrorProvider1.SetError(txbpass, "Password must be at least 8 characters.")
            txbpass.BorderColor = Color.Red
            isvalid = False
        ElseIf Not Regex.IsMatch(txbpass.Text, "[A-Z]") Then
            ErrorProvider1.SetError(txbpass, "Password needs at least 1 uppercase letter.")
            txbpass.BorderColor = Color.Red
            isvalid = False
        ElseIf Not Regex.IsMatch(txbpass.Text, "[0-9]") Then
            ErrorProvider1.SetError(txbpass, "Password needs at least 1 number.")
            txbpass.BorderColor = Color.Red
            isvalid = False
        End If

        If txbcp.Text <> txbpass.Text Then
            ErrorProvider1.SetError(txbcp, "Passwords do not match.")
            txbcp.BorderColor = Color.Red
            isvalid = False
        End If

        If txbfn.Text = "" Then
            ErrorProvider1.SetError(txbfn, "First name is required.")
            txbfn.BorderColor = Color.Red
            isvalid = False
        End If

        If txbln.Text = "" Then
            ErrorProvider1.SetError(txbln, "Last name is required.")
            txbln.BorderColor = Color.Red
            isvalid = False
        End If

        If Not chkbnonemn.Checked AndAlso txbmn.Text = "" Then
            ErrorProvider1.SetError(txbmn, "Middle name is required.")
            txbmn.BorderColor = Color.Red
            isvalid = False
        End If

        If Not chknoneen.Checked AndAlso cmben.Text = "" Then
            ErrorProvider1.SetError(cmben, "Extension name is required.")
            cmben.BorderColor = Color.Red
            isvalid = False
        End If

        If cmbrole.SelectedIndex = -1 Then
            ErrorProvider1.SetError(cmbrole, "Role is required.")
            cmbrole.BorderColor = Color.Red
            isvalid = False
        End If

        If cmbgender.SelectedIndex = -1 Then
            ErrorProvider1.SetError(cmbgender, "Gender is required.")
            cmbgender.BorderColor = Color.Red
            isvalid = False
        End If

        If cmbrole.Text.ToLower() = "cashier" AndAlso cmbshift.SelectedIndex = -1 Then
            ErrorProvider1.SetError(cmbshift, "Shift is required for cashiers.")
            cmbshift.BorderColor = Color.Red
            isvalid = False
        End If

        If picturebox1.Image Is Nothing Then
            ErrorProvider1.SetError(picturebox1, "User image is required.")
            isvalid = False
        End If

        Return isvalid
    End Function

    'check duplicate username
    Private Function IsUsernameDuplicate(username As String, originalUser As String) As Boolean
        If username = originalUser Then Return False

        conn()
        query = "SELECT COUNT(*) FROM user_accounts WHERE username = @USERNAME"
        cmd = New SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@USERNAME", username)
        Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
        cmd.Dispose()
        Return count > 0
    End Function

    'clear fields
    Sub ClearFields()
        cmbrole.SelectedIndex = -1
        cmbshift.SelectedIndex = -1
        cmbgender.SelectedIndex = -1
        cmben.SelectedIndex = -1

        txbfn.Clear()
        txbmn.Clear()
        txbln.Clear()
        txbusername.Clear()
        txbpass.Clear()
        txbcp.Clear()

        picturebox1.Image = Nothing
        originalUsername = ""

        chknoneen.Checked = False
        chkbnonemn.Checked = False
        cmben.Enabled = True
        txbmn.Enabled = True
        cmbshift.Enabled = False

        ErrorProvider1.Clear()
        ResetBorders()
    End Sub

    'events para sa UI
    Private Sub cmbrole_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbrole.SelectedIndexChanged
        UpdateShiftColumn()
        If cmbrole.Text.ToLower() = "cashier" Then
            cmbshift.Enabled = True
        Else
            cmbshift.Enabled = False
            cmbshift.SelectedIndex = -1
        End If
        AutoNumber()
        ErrorProvider1.SetError(cmbrole, "")
        cmbrole.BorderColor = Color.Black
    End Sub

    Private Sub chknoneen_CheckedChanged(sender As Object, e As EventArgs) Handles chknoneen.CheckedChanged
        If chknoneen.Checked Then
            cmben.Text = ""
            cmben.Enabled = False
            ErrorProvider1.SetError(cmben, "")
            cmben.BorderColor = Color.Black
        Else
            cmben.Enabled = True
        End If
    End Sub

    Private Sub chkbnonemn_CheckedChanged(sender As Object, e As EventArgs) Handles chkbnonemn.CheckedChanged
        If chkbnonemn.Checked Then
            txbmn.Text = ""
            txbmn.Enabled = False
            ErrorProvider1.SetError(txbmn, "")
            txbmn.BorderColor = Color.Black
        Else
            txbmn.Enabled = True
        End If
    End Sub

    Private Sub txbusername_TextChanged(sender As Object, e As EventArgs) Handles txbusername.TextChanged
        ErrorProvider1.SetError(txbusername, "")
        txbusername.BorderColor = Color.Black
    End Sub

    Private Sub txbpass_TextChanged(sender As Object, e As EventArgs) Handles txbpass.TextChanged
        ErrorProvider1.SetError(txbpass, "")
        txbpass.BorderColor = Color.Black
    End Sub

    Private Sub txbcp_TextChanged(sender As Object, e As EventArgs) Handles txbcp.TextChanged
        ErrorProvider1.SetError(txbcp, "")
        txbcp.BorderColor = Color.Black
    End Sub

    Private Sub txbfn_TextChanged(sender As Object, e As EventArgs) Handles txbfn.TextChanged
        ErrorProvider1.SetError(txbfn, "")
        txbfn.BorderColor = Color.Black
    End Sub

    Private Sub txbln_TextChanged(sender As Object, e As EventArgs) Handles txbln.TextChanged
        ErrorProvider1.SetError(txbln, "")
        txbln.BorderColor = Color.Black
    End Sub

    Private Sub txbmn_TextChanged(sender As Object, e As EventArgs) Handles txbmn.TextChanged
        If txbmn.Text <> "" Then
            ErrorProvider1.SetError(txbmn, "")
            txbmn.BorderColor = Color.Black
        End If
    End Sub

    Private Sub cmben_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmben.SelectedIndexChanged
        If cmben.Text <> "" Then
            ErrorProvider1.SetError(cmben, "")
            cmben.BorderColor = Color.Black
        End If
    End Sub

    Private Sub cmbgender_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbgender.SelectedIndexChanged
        ErrorProvider1.SetError(cmbgender, "")
        cmbgender.BorderColor = Color.Black
    End Sub

    Private Sub cmbshift_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbshift.SelectedIndexChanged
        ErrorProvider1.SetError(cmbshift, "")
        cmbshift.BorderColor = Color.Black
    End Sub

    Private Sub txbfn_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbfn.KeyPress
        If Not Char.IsLetter(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) AndAlso e.KeyChar <> " "c Then e.Handled = True
    End Sub

    Private Sub txbmn_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbmn.KeyPress
        If Not Char.IsLetter(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) AndAlso e.KeyChar <> " "c Then e.Handled = True
    End Sub

    Private Sub txbln_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbln.KeyPress
        If Not Char.IsLetter(e.KeyChar) AndAlso Not Char.IsControl(e.KeyChar) AndAlso e.KeyChar <> " "c Then e.Handled = True
    End Sub

    Private Sub cmbfilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbfilter.SelectedIndexChanged
        UpdateShiftColumn()
        If isLoading Then Exit Sub
        readdata()
    End Sub

    Private Sub txbsearch_TextChanged(sender As Object, e As EventArgs) Handles txbsearch.TextChanged
        If isLoading Then Exit Sub
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

    Private Sub addbtn_Click(sender As Object, e As EventArgs) Handles addbtn.Click
        If Not ValidateFields() Then Exit Sub

        Dim isNewUser As Boolean = (originalUsername = "")
        Dim savedRoleID As String = txbrole.Text

        savedata()
        readdata()

        If isNewUser Then
            Dim pfx As String = GetPrefix(cmbrole.Text.ToLower())
            If pfx <> "" Then
                CreateNewAutoNumber(pfx)
                AutoNumber()
            End If
        End If

        ClearFields()
        MsgBox("User account successfully saved with Role ID: " & savedRoleID, MsgBoxStyle.Information, "Success")
    End Sub

    Private Sub clearbtn_Click(sender As Object, e As EventArgs) Handles clearbtn.Click
        ClearFields()
    End Sub

    Private Sub datagridviewaccounts_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles datagridviewaccounts.CellContentClick
        If e.RowIndex < 0 Then Exit Sub
        Dim i As Integer = e.RowIndex

        If e.ColumnIndex = 12 Then
            cmbrole.Text = Convert.ToString(datagridviewaccounts(0, i).Value)
            txbrole.Text = Convert.ToString(datagridviewaccounts(1, i).Value)
            cmbshift.Text = Convert.ToString(datagridviewaccounts(2, i).Value)
            cmbgender.Text = Convert.ToString(datagridviewaccounts(3, i).Value)
            txbfn.Text = Convert.ToString(datagridviewaccounts(4, i).Value)

            Dim midName As String = Convert.ToString(datagridviewaccounts(5, i).Value)
            If midName = "" Then
                chkbnonemn.Checked = True
                txbmn.Text = ""
            Else
                chkbnonemn.Checked = False
                txbmn.Text = midName
            End If

            txbln.Text = Convert.ToString(datagridviewaccounts(6, i).Value)

            Dim extName As String = Convert.ToString(datagridviewaccounts(7, i).Value)
            If extName = "" Then
                chknoneen.Checked = True
                cmben.Text = ""
            Else
                chknoneen.Checked = False
                cmben.Text = extName
            End If

            txbusername.Text = Convert.ToString(datagridviewaccounts(8, i).Value)
            originalUsername = txbusername.Text()
            txbpass.Text = Convert.ToString(datagridviewaccounts(9, i).Value)
            txbcp.Text = Convert.ToString(datagridviewaccounts(9, i).Value)
            viewimage()

            ErrorProvider1.Clear()
            ResetBorders()

        ElseIf e.ColumnIndex = 13 Then
            Dim targetRole As String = Convert.ToString(datagridviewaccounts(0, i).Value).ToLower()
            Dim currentRole As String = CurrentUser.role.ToLower()

            If currentRole = "admin" AndAlso targetRole = "owner" Then
                Dim inputPass As String = InputBox("SECURITY ALERT:" & vbCrLf & vbCrLf & "Enter your ADMIN password to authorize this action:", "Authorization Required", "")
                If inputPass = "" Then Exit Sub
                If Not IsPasswordCorrect(inputPass) Then
                    MsgBox("Incorrect password. Deletion cancelled.", MsgBoxStyle.Critical, "Access Denied")
                    Exit Sub
                End If
                MsgBox("Password verified. Proceeding.", MsgBoxStyle.Information, "Authorized")
            Else
                If MessageBox.Show("Are you sure you want to delete this Account?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then Exit Sub
            End If

            Dim targetUsername As String = Convert.ToString(datagridviewaccounts(8, i).Value)
            conn()

            query = "DELETE FROM admin_accounts WHERE userID = (SELECT userID FROM user_accounts WHERE username = @USERNAME); " &
                    "DELETE FROM owner_accounts WHERE userID = (SELECT userID FROM user_accounts WHERE username = @USERNAME); " &
                    "DELETE FROM cashier_accounts WHERE userID = (SELECT userID FROM user_accounts WHERE username = @USERNAME); " &
                    "DELETE FROM user_accounts WHERE username = @USERNAME;"

            cmd = New SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@USERNAME", targetUsername)
            cmd.ExecuteNonQuery()
            cmd.Dispose()

            MsgBox("Account successfully deleted.", MsgBoxStyle.Information, "Deleted")
            readdata()
        End If
    End Sub

    Sub savedata()
        datagridviewaccounts.Rows.Clear()
        conn()

        Dim ms As New MemoryStream
        picturebox1.Image.Save(ms, picturebox1.Image.RawFormat)

        Dim midName As Object = DBNull.Value
        If Not chkbnonemn.Checked AndAlso txbmn.Text() <> "" Then midName = txbmn.Text()

        Dim extName As Object = DBNull.Value
        If Not chknoneen.Checked AndAlso cmben.Text <> "" Then extName = cmben.Text

        Dim shiftVal As Object = DBNull.Value
        If cmbrole.Text.ToLower() = "cashier" AndAlso cmbshift.Text <> "" Then shiftVal = cmbshift.Text

        If originalUsername <> "" Then
            ' UPDATE
            query = "UPDATE user_accounts SET role=@ROLE, username=@USERNAME, password=@PASSWORD, gender=@GENDER, first_name=@FIRSTNAME, middle_name=@MIDDLENAME, last_name=@LASTNAME, extension_name=@EXTENSIONNAME, userimage=@USERIMAGE WHERE username=@ORIGINAL_USERNAME"
            cmd = New SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@ROLE", cmbrole.Text)
            cmd.Parameters.AddWithValue("@USERNAME", txbusername.Text())
            cmd.Parameters.AddWithValue("@PASSWORD", txbpass.Text)
            cmd.Parameters.AddWithValue("@GENDER", cmbgender.Text)
            cmd.Parameters.AddWithValue("@FIRSTNAME", txbfn.Text())
            cmd.Parameters.AddWithValue("@MIDDLENAME", midName)
            cmd.Parameters.AddWithValue("@LASTNAME", txbln.Text())
            cmd.Parameters.AddWithValue("@EXTENSIONNAME", extName)
            cmd.Parameters.AddWithValue("@USERIMAGE", ms.ToArray)
            cmd.Parameters.AddWithValue("@ORIGINAL_USERNAME", originalUsername)
            cmd.ExecuteNonQuery()
            cmd.Dispose()

            query = "DELETE FROM admin_accounts WHERE userID = (SELECT userID FROM user_accounts WHERE username = @USERNAME); DELETE FROM owner_accounts WHERE userID = (SELECT userID FROM user_accounts WHERE username = @USERNAME); DELETE FROM cashier_accounts WHERE userID = (SELECT userID FROM user_accounts WHERE username = @USERNAME);"
            cmd = New SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@USERNAME", txbusername.Text())
            cmd.ExecuteNonQuery()
            cmd.Dispose()

            Dim role As String = cmbrole.Text.ToLower()
            Dim currentUserID As Integer = 0

            query = "SELECT userID FROM user_accounts WHERE username = @USERNAME"
            cmd = New SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@USERNAME", txbusername.Text())
            currentUserID = Convert.ToInt32(cmd.ExecuteScalar())
            cmd.Dispose()

            If role = "admin" Then
                query = "INSERT INTO admin_accounts (userID, admin_code) VALUES (@uid, @code)"
                cmd = New SqlCommand(query, sqlconn)
                cmd.Parameters.AddWithValue("@uid", currentUserID)
                cmd.Parameters.AddWithValue("@code", txbrole.Text)
                cmd.ExecuteNonQuery()
                cmd.Dispose()
            ElseIf role = "owner" Then
                query = "INSERT INTO owner_accounts (userID, owner_code) VALUES (@uid, @code)"
                cmd = New SqlCommand(query, sqlconn)
                cmd.Parameters.AddWithValue("@uid", currentUserID)
                cmd.Parameters.AddWithValue("@code", txbrole.Text)
                cmd.ExecuteNonQuery()
                cmd.Dispose()
            ElseIf role = "cashier" Then
                query = "INSERT INTO cashier_accounts (userID, cashier_code, shift) VALUES (@uid, @code, @shift)"
                cmd = New SqlCommand(query, sqlconn)
                cmd.Parameters.AddWithValue("@uid", currentUserID)
                cmd.Parameters.AddWithValue("@code", txbrole.Text)
                cmd.Parameters.AddWithValue("@shift", shiftVal)
                cmd.ExecuteNonQuery()
                cmd.Dispose()
            End If

        Else
            ' INSERT
            query = "INSERT INTO user_accounts (role, username, password, gender, first_name, middle_name, last_name, extension_name,created_at ,userimage) OUTPUT INSERTED.userID VALUES (@ROLE, @USERNAME, @PASSWORD, @GENDER, @FIRSTNAME, @MIDDLENAME, @LASTNAME, @EXTENSIONNAME, @CREATED_AT, @USERIMAGE)"
            cmd = New SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@ROLE", cmbrole.Text)
            cmd.Parameters.AddWithValue("@USERNAME", txbusername.Text())
            cmd.Parameters.AddWithValue("@PASSWORD", txbpass.Text)
            cmd.Parameters.AddWithValue("@GENDER", cmbgender.Text)
            cmd.Parameters.AddWithValue("@FIRSTNAME", txbfn.Text())
            cmd.Parameters.AddWithValue("@MIDDLENAME", midName)
            cmd.Parameters.AddWithValue("@LASTNAME", txbln.Text())
            cmd.Parameters.AddWithValue("@EXTENSIONNAME", extName)
            cmd.Parameters.AddWithValue("@CREATED_AT", DateTime.Now)
            cmd.Parameters.AddWithValue("@USERIMAGE", ms.ToArray)

            Dim newUserID As Integer = Convert.ToInt32(cmd.ExecuteScalar())
            cmd.Dispose()

            Dim role As String = cmbrole.Text.ToLower()

            If role = "admin" Then
                query = "INSERT INTO admin_accounts (userID, admin_code) VALUES (@uid, @code)"
                cmd = New SqlCommand(query, sqlconn)
                cmd.Parameters.AddWithValue("@uid", newUserID)
                cmd.Parameters.AddWithValue("@code", txbrole.Text)
                cmd.ExecuteNonQuery()
                cmd.Dispose()
            ElseIf role = "owner" Then
                query = "INSERT INTO owner_accounts (userID, owner_code) VALUES (@uid, @code)"
                cmd = New SqlCommand(query, sqlconn)
                cmd.Parameters.AddWithValue("@uid", newUserID)
                cmd.Parameters.AddWithValue("@code", txbrole.Text)
                cmd.ExecuteNonQuery()
                cmd.Dispose()
            ElseIf role = "cashier" Then
                query = "INSERT INTO cashier_accounts (userID, cashier_code, shift) VALUES (@uid, @code, @shift)"
                cmd = New SqlCommand(query, sqlconn)
                cmd.Parameters.AddWithValue("@uid", newUserID)
                cmd.Parameters.AddWithValue("@code", txbrole.Text)
                cmd.Parameters.AddWithValue("@shift", shiftVal)
                cmd.ExecuteNonQuery()
                cmd.Dispose()
            End If
        End If
    End Sub

    Sub viewimage()
        If txbusername.Text() = "" Then
            picturebox1.Image = Nothing
            Exit Sub
        End If
        conn()
        Dim img() As Byte
        query = "SELECT userimage FROM user_accounts WHERE username = @USERNAME"
        cmd = New SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@USERNAME", txbusername.Text())
        dr = cmd.ExecuteReader()
        While dr.Read()
            If IsDBNull(dr("userimage")) Then
                picturebox1.Image = Nothing
            Else
                img = CType(dr("userimage"), Byte())
                Dim ms As New MemoryStream(img)
                picturebox1.Image = Image.FromStream(ms)
            End If
        End While
        dr.Close()
        cmd.Dispose()
    End Sub

    Private Function IsPasswordCorrect(inputPass As String) As Boolean
        conn()
        query = "SELECT COUNT(*) FROM user_accounts WHERE userID = @uid AND password = @pass"
        cmd = New SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@uid", CurrentUser.user_id)
        cmd.Parameters.AddWithValue("@pass", inputPass)
        Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
        cmd.Dispose()
        Return count > 0
    End Function

    Private Function GetPrefix(role As String) As String
        If role = "admin" Then
            Return "A"
        ElseIf role = "owner" Then
            Return "O"
        ElseIf role = "cashier" Then
            Return "C"
        Else
            Return ""
        End If
    End Function

    Sub AutoNumber()
        Dim pfx As String = GetPrefix(cmbrole.Text.ToLower())
        If pfx = "" Then
            txbrole.Text = ""
            Exit Sub
        End If
        conn()
        Dim number As String
        query = "SELECT Max(NewNumber) FROM AutoNumberUsers where pfx = @pfx"
        cmd = New SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@pfx", pfx)
        Dim result As Object = cmd.ExecuteScalar()
        cmd.Dispose()

        If result Is Nothing OrElse IsDBNull(result) Then
            CreateNewAutoNumber(pfx)
            query = "SELECT Max(NewNumber) FROM AutoNumberUsers where pfx = @pfx"
            cmd = New SqlCommand(query, sqlconn)
            cmd.Parameters.AddWithValue("@pfx", pfx)
            number = Convert.ToString(cmd.ExecuteScalar())
            cmd.Dispose()
        Else
            number = Convert.ToString(result)
        End If
        txbrole.Text = number
    End Sub

    Sub CreateNewAutoNumber(pfx As String)
        Try
            Dim cmd As New SqlCommand
            cmd.Connection = sqlconn
            cmd.CommandText = "SP_AutoNumberUser"
            cmd.CommandType = CommandType.StoredProcedure
            cmd.Parameters.AddWithValue("@pfx", pfx)
            Dim outParam As SqlParameter = cmd.Parameters.Add("@NewNumber", SqlDbType.VarChar, 15)
            outParam.Direction = ParameterDirection.Output
            cmd.ExecuteNonQuery()
            cmd.Dispose()
        Catch ex As Exception
            MsgBox("AutoNumber Error: " & ex.Message)
        End Try
    End Sub

End Class