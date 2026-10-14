Imports System.Diagnostics.Eventing.Reader
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


        Dim query As String =
            "SELECT u.user_id, u.first_name, u.middle_name, u.last_name, " &
            "       u.gender, u.role, " &
            "       a.admin_id, o.owner_id, c.cashier_id, " &
            "       c.shift " &
            "FROM user_accounts u " &
            "LEFT JOIN admin_accounts   a ON a.user_id = u.user_id " &
            "LEFT JOIN owner_accounts   o ON o.user_id = u.user_id " &
            "LEFT JOIN cashier_accounts c ON c.user_id = u.user_id " &
            "WHERE u.username = @username AND u.password_hash = @password"

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@username", username)
        cmd.Parameters.AddWithValue("@password", pass)

        dr = cmd.ExecuteReader()

        If dr.HasRows Then
            dr.Read()
            matcheddata = True

            CurrentUser.user_id = Convert.ToInt32(dr("user_id"))
            CurrentUser.firstname = dr("first_name").ToString()
            CurrentUser.lastname = dr("last_name").ToString()
            CurrentUser.gender = dr("gender").ToString()
            CurrentUser.role = dr("role").ToString()


            If Not IsDBNull(dr("admin_id")) Then
                CurrentUser.role_code = dr("admin_id").ToString()
            ElseIf Not IsDBNull(dr("owner_id")) Then
                CurrentUser.role_code = dr("owner_id").ToString()
            ElseIf Not IsDBNull(dr("cashier_id")) Then
                CurrentUser.role_code = dr("cashier_id").ToString()
            Else
                CurrentUser.role_code = ""
            End If

            If Not IsDBNull(dr("shift")) Then
                CurrentUser.shift = dr("shift").ToString()
            Else
                CurrentUser.shift = ""
            End If

            Dim midname As String = ""
            If Not IsDBNull(dr("middle_name")) Then
                midname = dr("middle_name").ToString().Trim()
            End If

            Dim middlePart As String = ""
            If midname <> "" Then
                Dim m As String = midname.Substring(0, 1).ToUpper()
                middlePart = " " & m & "."
            End If

            CurrentUser.fullname =
                StrConv(CurrentUser.firstname.Trim(), vbProperCase) &
                middlePart & " " &
                StrConv(CurrentUser.lastname.Trim(), vbProperCase)
        End If

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

    Private Sub minimize_Click(sender As Object, e As EventArgs) Handles minimize.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub Guna2GradientPanel2_Paint(sender As Object, e As PaintEventArgs) Handles Guna2GradientPanel2.Paint

    End Sub

    Private Sub attempt_Click(sender As Object, e As EventArgs) Handles attempt.Click

    End Sub
End Class