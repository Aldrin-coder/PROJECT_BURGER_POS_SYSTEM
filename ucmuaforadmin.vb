Imports System.Data
Imports System.Data.SqlClient
Imports System.Text.RegularExpressions

Public Class ucmuaforadmin1

    Dim selectedUser As String = ""


    ' ===================== LOAD =====================
    Private Sub ucmuaforadmin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler DataGridView1.DataError, Sub(s, ev) CType(ev, DataGridViewDataErrorEventArgs).ThrowException = False

        loadcomboitems()
        DisableShift()
        HideShiftColumn()
        txbcode.Text = ""

        filtercbx.Items.Clear()
        filtercbx.Items.AddRange({"all", "owner", "cashier"})
        filtercbx.SelectedIndex = 0

        readdata()
    End Sub


    ' ===================== NAME KEYPRESS =====================
    Private Sub NameOnly_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txbfirstname.KeyPress,
                                                                                txbmiddlename.KeyPress,
                                                                                txblastname.KeyPress
        If Char.IsLetter(e.KeyChar) OrElse e.KeyChar = " "c OrElse e.KeyChar = "."c OrElse e.KeyChar = ChrW(8) Then
            ' allow
        Else
            e.Handled = True
        End If
    End Sub


    ' ===================== NO MIDDLE NAME CHECKBOX =====================
    Private Sub mnnonecb_CheckedChanged(sender As Object, e As EventArgs) Handles mnnonecb.CheckedChanged
        If mnnonecb.Checked Then
            txbmiddlename.Text = ""
            txbmiddlename.Enabled = False
            SetError(txbmiddlename, "", Errormn)
        Else
            txbmiddlename.Enabled = True
        End If
    End Sub


    ' ===================== AUTO-CLEAR ERRORS + BORDER =====================
    Private Sub ClearErrorOnChange(sender As Object, e As EventArgs) Handles _
            txbfirstname.TextChanged, txbmiddlename.TextChanged, txblastname.TextChanged,
            txbusername.TextChanged, txbpass.TextChanged, cnpass.TextChanged

        Dim tb As Guna.UI2.WinForms.Guna2TextBox = CType(sender, Guna.UI2.WinForms.Guna2TextBox)
        tb.BorderColor = Color.Black
        tb.BorderThickness = 1

        Errorfn.SetError(tb, "")
        Errormn.SetError(tb, "")
        errorln.SetError(tb, "")
        errorpass.SetError(tb, "")
        errorcnpass.SetError(tb, "")
    End Sub


    ' ===================== HELPERS =====================
    Sub DisableShift()
        shiftcmb.Enabled = False
        shiftcmb.SelectedIndex = -1
    End Sub

    Sub HideShiftColumn()
        If DataGridView1.Columns.Contains("SHIFT") Then
            DataGridView1.Columns("SHIFT").Visible = False
        End If
    End Sub

    Sub ShowShiftColumn()
        If DataGridView1.Columns.Contains("SHIFT") Then
            DataGridView1.Columns("SHIFT").Visible = True
        End If
    End Sub

    Sub GetNextId(seq As String, prefix As String, ByRef result As String)
        result = prefix & "1000"
        Using c As New SqlCommand("SELECT CAST(current_value AS BIGINT)+1 FROM sys.sequences WHERE name=@n", sqlconn)
            c.Parameters.AddWithValue("@n", seq)
            Dim r = c.ExecuteScalar()
            If r IsNot Nothing AndAlso Not IsDBNull(r) Then
                result = prefix & r.ToString()
            End If
        End Using
    End Sub


    ' ===================== SET ERROR + RED BORDER =====================
    Sub SetError(tb As Guna.UI2.WinForms.Guna2TextBox, msg As String, ep As ErrorProvider)
        ep.SetError(tb, msg)
        If msg = "" Then
            tb.BorderColor = Color.Black
            tb.BorderThickness = 1
        Else
            tb.BorderColor = Color.Red
            tb.BorderThickness = 2
        End If
    End Sub


    ' ===================== ROLE CHANGE =====================
    Private Sub rolecb_SelectedIndexChanged(sender As Object, e As EventArgs) Handles rolecb.SelectedIndexChanged
        Dim role As String = rolecb.Text.Trim().ToLower()
        Dim cash As Boolean = (role = "cashier")

        shiftcmb.Enabled = cash

        If Not cash Then
            shiftcmb.SelectedIndex = -1
        End If

        If cash Then
            ShowShiftColumn()
        Else
            HideShiftColumn()
        End If

        If role = "owner" Then
            GetNextId("seq_owner", "O", txbcode.Text)
        ElseIf role = "cashier" Then
            GetNextId("seq_cashier", "C", txbcode.Text)
        Else
            txbcode.Text = ""
        End If
    End Sub


    ' ===================== FILTER =====================
    Private Sub filtercbx_SelectedIndexChanged(sender As Object, e As EventArgs) Handles filtercbx.SelectedIndexChanged
        readdata()
    End Sub


    ' ===================== READ GRID =====================
    Sub readdata()
        Dim rf As String = filtercbx.Text.Trim().ToLower()
        Dim kw As String = searchtxb.Text.Trim()

        query = "SELECT u.role AS [ROLE],COALESCE(a.admin_id,o.owner_id,c.cashier_id) AS [ROLE ID]," &
                "c.shift AS [SHIFT],u.first_name AS [FIRST NAME],u.middle_name AS [MIDDLE NAME]," &
                "u.last_name AS [LAST NAME],u.gender AS [GENDER],u.username AS [USERNAME]," &
                "u.password_hash AS [PASSWORD] " &
                "FROM user_accounts u " &
                "LEFT JOIN admin_accounts a ON a.user_id=u.user_id " &
                "LEFT JOIN owner_accounts o ON o.user_id=u.user_id " &
                "LEFT JOIN cashier_accounts c ON c.user_id=u.user_id " &
                "WHERE u.role<>'admin'"

        If rf = "owner" Or rf = "cashier" Then
            query &= " AND u.role='" & rf & "'"
        End If

        If kw <> "" Then
            query &= " AND (u.first_name LIKE @kw OR u.middle_name LIKE @kw OR u.last_name LIKE @kw OR " &
                     "u.username LIKE @kw OR COALESCE(a.admin_id,o.owner_id,c.cashier_id) LIKE @kw)"
        End If

        cmd = New SqlCommand(query, sqlconn)

        If kw <> "" Then
            cmd.Parameters.AddWithValue("@kw", "%" & kw & "%")
        End If

        Dim dt As New DataTable
        Dim da As New SqlDataAdapter(cmd)
        da.Fill(dt)

        DataGridView1.DataSource = Nothing
        DataGridView1.Columns.Clear()
        DataGridView1.AutoGenerateColumns = True
        DataGridView1.DataSource = dt

        da.Dispose()
        cmd.Dispose()

        If rolecb.Text.Trim().ToLower() = "cashier" Then
            ShowShiftColumn()
        Else
            HideShiftColumn()
        End If
    End Sub


    ' ===================== SEARCH =====================
    Private Sub searchtxb_TextChanged(sender As Object, e As EventArgs) Handles searchtxb.TextChanged
        readdata()
    End Sub


    ' ===================== GRID CLICK =====================
    Private Sub DataGridView1_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellClick
        If e.RowIndex < 0 Then Return

        Dim r = DataGridView1.Rows(e.RowIndex)

        rolecb.Text = r.Cells("ROLE").Value.ToString()
        txbcode.Text = r.Cells("ROLE ID").Value.ToString()

        If DataGridView1.Columns.Contains("SHIFT") Then
            If r.Cells("SHIFT").Value Is Nothing Then
                shiftcmb.Text = ""
            Else
                shiftcmb.Text = r.Cells("SHIFT").Value.ToString()
            End If
        End If

        txbfirstname.Text = r.Cells("FIRST NAME").Value.ToString()
        txblastname.Text = r.Cells("LAST NAME").Value.ToString()
        gendercmb.Text = r.Cells("GENDER").Value.ToString()
        txbusername.Text = r.Cells("USERNAME").Value.ToString()
        txbpass.Text = r.Cells("PASSWORD").Value.ToString()
        cnpass.Text = r.Cells("PASSWORD").Value.ToString()
        selectedUser = r.Cells("USERNAME").Value.ToString()

        Dim mn As String = r.Cells("MIDDLE NAME").Value.ToString()
        If mn = "" OrElse mn Is Nothing Then
            txbmiddlename.Text = ""
            mnnonecb.Checked = True
            txbmiddlename.Enabled = False
        Else
            txbmiddlename.Text = mn
            mnnonecb.Checked = False
            txbmiddlename.Enabled = True
        End If
    End Sub


    ' ===================== CLEAR =====================
    Sub clear()
        txbfirstname.Clear()
        txblastname.Clear()
        txbmiddlename.Clear()
        txbusername.Clear()
        txbpass.Clear()
        cnpass.Clear()
        rolecb.SelectedIndex = -1
        gendercmb.SelectedIndex = -1
        DisableShift()
        HideShiftColumn()
        selectedUser = ""
        txbcode.Clear()
        mnnonecb.Checked = False
        txbmiddlename.Enabled = True
        ClearAllErrors()
    End Sub


    ' ===================== CLEAR ALL ERRORS + BORDERS =====================
    Sub ClearAllErrors()
        SetError(txbfirstname, "", Errorfn)
        SetError(txbusername, "", Errorfn)
        SetError(txbmiddlename, "", Errormn)
        SetError(txblastname, "", errorln)
        SetError(txbpass, "", errorpass)
        SetError(cnpass, "", errorcnpass)
    End Sub


    ' ===================== COMBO ITEMS =====================
    Sub loadcomboitems()
        rolecb.Items.Clear()
        rolecb.Items.AddRange({"owner", "cashier"})
        gendercmb.Items.Clear()
        gendercmb.Items.AddRange({"male", "female"})
        shiftcmb.Items.Clear()
        shiftcmb.Items.AddRange({"am", "pm"})
    End Sub


    ' ===================== VALIDATION =====================
    Sub Valid(ByRef ok As Boolean)

        ClearAllErrors()
        ok = True

        ' ROLE — MsgBox
        If rolecb.Text = "" Then
            MsgBox("Select a role.", MsgBoxStyle.Exclamation, "Error")
            ok = False
        End If

        ' SHIFT — MsgBox
        If rolecb.Text.ToLower() = "cashier" AndAlso shiftcmb.Text = "" Then
            MsgBox("Select a shift for cashier.", MsgBoxStyle.Exclamation, "Error")
            ok = False
        End If

        ' GENDER — MsgBox
        If gendercmb.Text = "" Then
            MsgBox("Select a gender.", MsgBoxStyle.Exclamation, "Error")
            ok = False
        End If

        ' FIRST NAME — ErrorProvider + red border
        If txbfirstname.Text = "" Then
            SetError(txbfirstname, "First name is required.", Errorfn)
            ok = False
        End If

        ' MIDDLE NAME — ErrorProvider + red border (skip if "no middle name" checked)
        If Not mnnonecb.Checked AndAlso txbmiddlename.Text = "" Then
            SetError(txbmiddlename, "Middle name is required.", Errormn)
            ok = False
        End If

        ' LAST NAME — ErrorProvider + red border
        If txblastname.Text = "" Then
            SetError(txblastname, "Last name is required.", errorln)
            ok = False
        End If

        ' USERNAME — ErrorProvider + red border
        If txbusername.Text = "" Then
            SetError(txbusername, "Username is required.", Errorfn)
            ok = False
        ElseIf txbusername.Text.Trim().Length < 6 Then
            SetError(txbusername, "Username must be at least 6 characters.", Errorfn)
            ok = False
        End If

        ' PASSWORD — ErrorProvider + red border
        If txbpass.Text = "" Then
            SetError(txbpass, "Password is required.", errorpass)
            ok = False
        ElseIf txbpass.Text.Length < 8 Then
            SetError(txbpass, "Password must be at least 8 characters.", errorpass)
            ok = False
        ElseIf Not Regex.IsMatch(txbpass.Text, "[A-Z]") Then
            SetError(txbpass, "Password must contain 1 uppercase letter.", errorpass)
            ok = False
        ElseIf Not Regex.IsMatch(txbpass.Text, "[0-9]") Then
            SetError(txbpass, "Password must contain 1 number.", errorpass)
            ok = False
        End If

        ' CONFIRM PASSWORD — ErrorProvider + red border
        If cnpass.Text = "" Then
            SetError(cnpass, "Please confirm your password.", errorcnpass)
            ok = False
        ElseIf cnpass.Text <> txbpass.Text Then
            SetError(cnpass, "Passwords do not match.", errorcnpass)
            ok = False
        End If

    End Sub


    ' ===================== ADD =====================
    Private Sub addbtn_Click(sender As Object, e As EventArgs) Handles addbtn.Click
        Dim ok As Boolean = False
        Valid(ok)
        If Not ok Then Return

        Dim role As String = rolecb.Text.ToLower()
        Dim newId As String = ""

        Try
            Using c As New SqlCommand("sp_AutoNumber", sqlconn)
                c.CommandType = CommandType.StoredProcedure
                c.Parameters.AddWithValue("@username", txbusername.Text.Trim())
                c.Parameters.AddWithValue("@password_hash", txbpass.Text)
                c.Parameters.AddWithValue("@first_name", txbfirstname.Text.Trim())
                c.Parameters.AddWithValue("@middle_name", txbmiddlename.Text.Trim())
                c.Parameters.AddWithValue("@last_name", txblastname.Text.Trim())
                c.Parameters.AddWithValue("@gender", gendercmb.Text.Trim())
                c.Parameters.AddWithValue("@role", role)

                If role = "cashier" Then
                    c.Parameters.AddWithValue("@shift", shiftcmb.Text.Trim())
                Else
                    c.Parameters.AddWithValue("@shift", DBNull.Value)
                End If

                Using r = c.ExecuteReader()
                    If r.Read() Then newId = r("role_id").ToString()
                End Using
            End Using

        Catch ex As SqlException
            If ex.Number = 2627 Then
                SetError(txbusername, "Username already exists.", Errorfn)
            Else
                MsgBox("DB Error: " & ex.Message, MsgBoxStyle.Critical, "Error")
            End If
            Return
        End Try

        txbcode.Text = newId
        readdata()
        MsgBox("User added. Role ID: " & newId, MsgBoxStyle.Information, "Success")
        clear()
    End Sub


    ' ===================== UPDATE =====================
    Private Sub updatebtn_Click(sender As Object, e As EventArgs) Handles updatebtn.Click

        If selectedUser = "" Then
            MsgBox("Click a user first.", MsgBoxStyle.Exclamation, "Error")
            Return
        End If

        Dim ok As Boolean = False
        Valid(ok)
        If Not ok Then Return

        Dim role As String = rolecb.Text.ToLower()
        Dim uid As Integer = 0

        Using c As New SqlCommand("SELECT user_id FROM user_accounts WHERE username=@u", sqlconn)
            c.Parameters.AddWithValue("@u", selectedUser)
            Dim r = c.ExecuteScalar()
            If r Is Nothing Then
                MsgBox("User not found.", MsgBoxStyle.Exclamation, "Error")
                Return
            End If
            uid = Convert.ToInt32(r)
        End Using

        Try
            Using c As New SqlCommand(
                "UPDATE user_accounts SET username=@u,password_hash=@p,first_name=@f,middle_name=@m," &
                "last_name=@l,gender=@g,role=@r WHERE user_id=@id", sqlconn)
                c.Parameters.AddWithValue("@u", txbusername.Text.Trim())
                c.Parameters.AddWithValue("@p", txbpass.Text)
                c.Parameters.AddWithValue("@f", txbfirstname.Text.Trim())
                c.Parameters.AddWithValue("@m", txbmiddlename.Text.Trim())
                c.Parameters.AddWithValue("@l", txblastname.Text.Trim())
                c.Parameters.AddWithValue("@g", gendercmb.Text.Trim())
                c.Parameters.AddWithValue("@r", role)
                c.Parameters.AddWithValue("@id", uid)
                c.ExecuteNonQuery()
            End Using

            If role = "cashier" Then
                Using c As New SqlCommand("UPDATE cashier_accounts SET shift=@s WHERE user_id=@id", sqlconn)
                    c.Parameters.AddWithValue("@s", shiftcmb.Text.Trim())
                    c.Parameters.AddWithValue("@id", uid)
                    c.ExecuteNonQuery()
                End Using
            End If

        Catch ex As SqlException
            If ex.Number = 2627 Then
                SetError(txbusername, "Username already taken.", Errorfn)
            Else
                MsgBox("DB Error: " & ex.Message, MsgBoxStyle.Critical, "Error")
            End If
            Return
        End Try

        readdata()
        MsgBox("User updated.", MsgBoxStyle.Information, "Success")
        clear()
    End Sub


    ' ===================== DELETE =====================
    Private Sub deletebtn_Click(sender As Object, e As EventArgs) Handles deletebtn.Click

        If selectedUser = "" Then
            MsgBox("Click a user first.", MsgBoxStyle.Exclamation, "Error")
            Return
        End If

        If MsgBox("Delete '" & selectedUser & "'?", MsgBoxStyle.Question + MsgBoxStyle.YesNo, "Confirm") = MsgBoxResult.No Then
            Return
        End If

        Dim uid As Integer = 0
        Dim role As String = ""

        Using c As New SqlCommand("SELECT user_id,role FROM user_accounts WHERE username=@u", sqlconn)
            c.Parameters.AddWithValue("@u", selectedUser)
            Using r = c.ExecuteReader()
                If r.Read() Then
                    uid = Convert.ToInt32(r("user_id"))
                    role = r("role").ToString().ToLower()
                Else
                    MsgBox("User not found.", MsgBoxStyle.Exclamation, "Error")
                    Return
                End If
            End Using
        End Using

        Dim tbl As String = ""
        If role = "owner" Then tbl = "owner_accounts"
        If role = "cashier" Then tbl = "cashier_accounts"
        If role = "admin" Then tbl = "admin_accounts"

        Try
            If tbl <> "" Then
                Using c As New SqlCommand("DELETE FROM " & tbl & " WHERE user_id=@id", sqlconn)
                    c.Parameters.AddWithValue("@id", uid)
                    c.ExecuteNonQuery()
                End Using
            End If

            Using c As New SqlCommand("DELETE FROM user_accounts WHERE user_id=@id", sqlconn)
                c.Parameters.AddWithValue("@id", uid)
                c.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            MsgBox("DB Error: " & ex.Message, MsgBoxStyle.Critical, "Error")
            Return
        End Try

        readdata()
        MsgBox("User deleted.", MsgBoxStyle.Information, "Success")
        clear()
    End Sub


    ' ===================== CLEAR BUTTON =====================
    Private Sub clearbtn_Click(sender As Object, e As EventArgs) Handles clearbtn.Click
        clear()
        searchtxb.Clear()
        filtercbx.SelectedIndex = 0
    End Sub

    Private Sub DataGridView1_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellContentClick

    End Sub
End Class