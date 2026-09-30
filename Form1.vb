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
        CurrentUser.Clear()


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

        CurrentUser.Clear()


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
            uservalm.Text = "⚠️ Usernames requires a minimum of 6 characters."
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
            Passvalm.Text = "⚠️ Password requires a minimum of 8 characters.."
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
    "SELECT u.user_id, u.first_name, u.middle_name, u.last_name, u.gender, u.role, o.owner_code, a.admin_code, c.cashier_code,c.shift " &
    "FROM user_accounts u " &
    "LEFT JOIN owner_accounts   o ON u.user_id = o.user_id " &
    "LEFT JOIN admin_accounts   a ON u.user_id = a.user_id " &
    "LEFT JOIN cashier_accounts c ON u.user_id = c.user_id " &
          "WHERE u.username COLLATE SQL_Latin1_General_CP1_CS_AS = '" & usertxb.Text & "'
           AND u.password_hash COLLATE SQL_Latin1_General_CP1_CS_AS = '" & passtxb.Text & "'"
        cmd = New SqlClient.SqlCommand(query, sqlconn)

        dr = cmd.ExecuteReader()

        If dr.HasRows Then
            dr.Read()
            matcheddata = True

            midname = dr("middle_name").ToString()

            Dim middlePart As String = ""
            If Not String.IsNullOrWhiteSpace(midname) Then
                Dim m As String = midname.Trim().TrimEnd("."c).ToUpper()
                middlePart = " " & m & If(m.Length = 1, ".", "")
            End If



            CurrentUser.firstname = dr("first_name").ToString()
            CurrentUser.lastname = dr("last_name").ToString()
            CurrentUser.midname = midname
            CurrentUser.gender = dr("gender").ToString()
            CurrentUser.role = dr("role").ToString()
            CurrentUser.shift = dr("shift").ToString()

            CurrentUser.fullname =
                StrConv(CurrentUser.firstname.Trim(), vbProperCase) &
                middlePart & " " &
                StrConv(CurrentUser.lastname.Trim(), vbProperCase)

            If role.ToLower = "owner" Then
                CurrentUser.code = dr("owner_code").ToString()

            ElseIf role.ToLower = "admin" Then
                CurrentUser.code = dr("admin_code").ToString()
            ElseIf role.ToLower = "cashier" Then
                CurrentUser.code = dr("cashier_code").ToString()
            End If
        End If


        dr.Close()
        cmd.Dispose()



        If matcheddata Then

            MsgBox("Welcome! " & CurrentUser.fullname & " is now logged in as" & " (" & StrConv(CurrentUser.role, vbProperCase) & ")", MsgBoxStyle.Information, "Login Successfully!")

            If role = "owner" Then

                Dim f As New Mainformd()
                f.Show()
                Me.Hide()

            ElseIf role = "admin" Then
                Dim f As New Mainformd()
                f.Show()
                Me.Hide()

            ElseIf role = "cashier" Then
                Dim f As New Mainformd()
                f.Show()
                Me.Hide()

            End If


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

            timerattempt.Start()
            MsgBox("You have login failed for 3 times continuously. Please try after " & secondsleft & " seconds.", MsgBoxStyle.Critical, "Attempt")

        End If


    End Sub

    Private Sub timerattempt_Tick(sender As Object, e As EventArgs) Handles timerattempt.Tick

        secondsleft -= 1


        attempt.Text = "⚠️ Account locked ! Please try again in " & secondsleft & " seconds.".ToString
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
            MsgBox("You can try again now.", MsgBoxStyle.Information, "Unlocked")
            attempt.Visible = False
            usertxb.Enabled = True
            passtxb.Enabled = True

            loginbtn.Enabled = True
            exitbtn.Enabled = True
            Return
        End If
    End Sub

    Private Sub minimize_Click(sender As Object, e As EventArgs) Handles minimize.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub


End Class
