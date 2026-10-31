Imports System.Diagnostics.Eventing.Reader
Imports System.IO
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Web.Security
Imports Microsoft.VisualBasic.ApplicationServices
Imports TheArtOfDevHtmlRenderer

Public Class loginform

    Dim matcheddata As Boolean = False
    Dim failedAttempts As Integer = 0
    Dim maxtries As Integer = 3
    Dim secondsleft As Integer = 60

    Private Sub loginform_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()

        attempt.Visible = False
        uservalm.Visible = False
        Passvalm.Visible = False

    End Sub

    Private Sub exitbtn_Click(sender As Object, e As EventArgs) Handles exitbtn.Click
        Application.Exit()
    End Sub

    Private Sub showpasschb_CheckedChanged(sender As Object, e As EventArgs) Handles showpasschb.CheckedChanged
        If showpasschb.Checked Then
            passtxb.PasswordChar = ""
        Else
            passtxb.PasswordChar = "*"
        End If
    End Sub

    Private Sub loginbtn_Click(sender As Object, e As EventArgs) Handles loginbtn.Click

        matcheddata = False
        Dim img() As Byte

        If usertxb.Text = "" And passtxb.Text = "" Then
            MsgBox("Please Fill up the Fields.", MsgBoxStyle.Exclamation, "Error")
            Return
        End If

        Dim username As String = usertxb.Text.Trim()

        If username.Length = 0 Then
            uservalm.Text = "⚠️ Please enter a username."
            uservalm.ForeColor = Color.Tomato
            uservalm.Visible = True
            usertxb.BorderColor = Color.Red
            Return
        End If

        If username.Length < 6 Then
            uservalm.Text = "⚠️ Usernames require a minimum of 6 characters."
            uservalm.ForeColor = Color.Tomato
            uservalm.Visible = True
            usertxb.BorderColor = Color.Red
            Return
        Else
            uservalm.Visible = False
            usertxb.BorderColor = Color.Transparent
        End If

        Dim pass As String = passtxb.Text()

        If pass.Length = 0 Then
            Passvalm.Text = "⚠️ Please enter a Password"
            Passvalm.Visible = True
            Passvalm.ForeColor = Color.Tomato
            passtxb.BorderColor = Color.Red
            Return
        ElseIf pass.Length < 8 Then
            Passvalm.Text = "⚠️ Password requires a minimum of 8 characters."
            Passvalm.ForeColor = Color.Tomato
            passtxb.BorderColor = Color.Red
            Passvalm.Visible = True
            Return
        End If

        Dim hasCapital As Boolean = Regex.IsMatch(pass, "[A-Z]")
        Dim hasNumber As Boolean = Regex.IsMatch(pass, "[0-9]")

        If Not hasCapital AndAlso Not hasNumber Then
            Passvalm.Text = "⚠️ Passwords must contain one uppercase letter and one number."
            Passvalm.ForeColor = Color.Tomato
            passtxb.BorderColor = Color.Red
            Passvalm.Visible = True
            Return
        ElseIf Not hasCapital Then
            Passvalm.Text = "⚠️ Password must contain at least one Capital letter."
            Passvalm.ForeColor = Color.Tomato
            passtxb.BorderColor = Color.Red
            Passvalm.Visible = True
            Return
        ElseIf Not hasNumber Then
            Passvalm.Text = "⚠️ Password must contain at least one Number."
            Passvalm.ForeColor = Color.Tomato
            passtxb.BorderColor = Color.Red
            Passvalm.Visible = True
            Return
        Else
            Passvalm.Visible = False
            passtxb.BorderColor = Color.Transparent
        End If

        query =
            "SELECT u.userID, u.first_name, u.middle_name, u.last_name,u.extension_name,u.userimage," &
            "       u.gender, u.role, " &
            "       a.admin_code, o.owner_code, c.cashier_code, " &
            "       c.shift " &
            "FROM user_accounts u " &
            "LEFT JOIN admin_accounts   a ON a.userID = u.userID " &
            "LEFT JOIN owner_accounts   o ON o.userID = u.userID " &
            "LEFT JOIN cashier_accounts c ON c.userID = u.userID " &
            "WHERE u.username = @username AND u.password = @password"

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@username", username)
        cmd.Parameters.AddWithValue("@password", pass)

        dr = cmd.ExecuteReader()

        While dr.Read()
            matcheddata = True

            CurrentUser.user_id = Convert.ToInt32(dr("userID"))
            CurrentUser.firstname = dr("first_name").ToString()
            CurrentUser.lastname = dr("last_name").ToString()
            CurrentUser.gender = dr("gender").ToString()
            CurrentUser.role = dr("role").ToString()
            CurrentUser.admin_id = dr("admin_code").ToString()
            CurrentUser.owner_id = dr("owner_code").ToString()
            CurrentUser.cashier_id = dr("cashier_code").ToString()
            CurrentUser.middle_name = dr("middle_name").ToString()
            CurrentUser.extension_name = dr("extension_name").ToString()
            CurrentUser.shift = dr("shift").ToString()

            If CurrentUser.admin_id <> "" Then
                CurrentUser.role_code = CurrentUser.admin_id
            ElseIf CurrentUser.owner_id <> "" Then
                CurrentUser.role_code = CurrentUser.owner_id
            ElseIf CurrentUser.cashier_id <> "" Then
                CurrentUser.role_code = CurrentUser.cashier_id
            Else
                CurrentUser.role_code = ""
            End If

            If CurrentUser.shift <> "" Then
                CurrentUser.shift = CurrentUser.shift
            Else
                CurrentUser.shift = ""
            End If

            If CurrentUser.middle_name <> "" Then
                CurrentUser.middle_name = CurrentUser.middle_name
            End If

            If CurrentUser.middle_name <> "" Then
                CurrentUser.middle_name = CurrentUser.middle_name.Substring(0, 1).ToUpper()
                CurrentUser.middle_name = " " & CurrentUser.middle_name & "."
            End If

            CurrentUser.fullname =
                StrConv(CurrentUser.firstname, vbProperCase) &
                CurrentUser.middle_name & " " &
                StrConv(CurrentUser.lastname, vbProperCase) &
                If(CurrentUser.extension_name <> "", " " & StrConv(CurrentUser.extension_name, vbProperCase), "")

            If IsDBNull(dr("userimage")) Then
                Mainformd.picturebox1.Image = Nothing
            Else
                img = CType(dr("userimage"), Byte())
                Dim ms As New MemoryStream(img)
                Mainformd.picturebox1.Image = Image.FromStream(ms)
            End If

        End While

        dr.Close()
        cmd.Dispose()

        If matcheddata Then

            MsgBox("Welcome! " & CurrentUser.fullname & " " &
                   "is now logged in as (" &
                   StrConv(CurrentUser.role, vbProperCase) & ").",
                   MsgBoxStyle.Information, "Login Successful!")

            Mainformd.Show()

            Me.Hide()

            matcheddata = False
            usertxb.Clear()
            passtxb.Clear()
            failedAttempts = 0
            Mainformd.logintimer11.Text = "0"

        Else
            MsgBox("Invalid Credentials", MsgBoxStyle.Exclamation, "Error")
            usertxb.Clear()
            passtxb.Clear()
            failedAttempts += 1
        End If

        If failedAttempts >= maxtries Then
            usertxb.Enabled = False
            passtxb.Enabled = False
            loginbtn.Enabled = False
            exitbtn.Enabled = False
            showpasschb.Enabled = False
            attempt.Visible = False
            secondsleft = 60
            timerattempt.Start()

            MsgBox("You have failed to login 3 times continuously. Please try again after " &
                   secondsleft & " seconds.", MsgBoxStyle.Critical, "Account Locked")
        End If

    End Sub

    Private Sub timerattempt_Tick(sender As Object, e As EventArgs) Handles timerattempt.Tick

        secondsleft -= 1

        attempt.Text = "⚠️ Account locked! Please try again in " & secondsleft & " seconds."
        attempt.ForeColor = Color.Tomato
        attempt.Visible = True

        If secondsleft <= 0 Then
            timerattempt.Stop()

            usertxb.Enabled = True
            passtxb.Enabled = True
            showpasschb.Enabled = True
            loginbtn.Enabled = True
            exitbtn.Enabled = True

            failedAttempts = 0
            attempt.Visible = False

            MsgBox("You can try again now.", MsgBoxStyle.Information, "Unlocked")
        End If
    End Sub

    Private Sub minimize1_Click(sender As Object, e As EventArgs) Handles minimize.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub Guna2GradientPanel2_Paint(sender As Object, e As PaintEventArgs) Handles Guna2GradientPanel2.Paint

    End Sub
End Class