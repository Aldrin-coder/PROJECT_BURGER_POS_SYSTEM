Imports System.Data.SqlClient
Imports System.IO
Imports System.Text.RegularExpressions

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

    Sub clearfields()
        usertxb.Clear()
        passtxb.Clear()
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

        Dim username As String = usertxb.Text

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
            "SELECT userID, first_name, middle_name, last_name, extension_name, userimage, " &
            "gender, role, user_code, shift " &
            "FROM user_accounts " &
            "WHERE username = '" & usertxb.Text & "' AND password = '" & passtxb.Text & "'"

        cmd = New SqlClient.SqlCommand(query, sqlconn)
        dr = cmd.ExecuteReader()

        While dr.Read()
            matcheddata = True

            CurrentUser.user_id = Convert.ToInt32(dr("userID"))
            CurrentUser.firstname = dr("first_name")
            CurrentUser.lastname = dr("last_name")
            CurrentUser.gender = dr("gender")
            CurrentUser.role = dr("role")

            If IsDBNull(dr("middle_name")) Then
                CurrentUser.middle_name = ""
            Else
                CurrentUser.middle_name = dr("middle_name")
            End If

            If IsDBNull(dr("extension_name")) Then
                CurrentUser.extension_name = ""
            Else
                CurrentUser.extension_name = dr("extension_name")
            End If

            CurrentUser.user_code = dr("user_code")

            If IsDBNull(dr("shift")) Then
                CurrentUser.shift = ""
            Else
                CurrentUser.shift = dr("shift")
            End If



            If CurrentUser.extension_name <> "" Then
                CurrentUser.extension_name = " " & CurrentUser.extension_name
            End If

            CurrentUser.fullname = CurrentUser.firstname & " " & CurrentUser.middle_name & " " & CurrentUser.lastname & CurrentUser.extension_name

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
            MsgBox("Welcome! " & CurrentUser.fullname &
                   " is now logged in as (" & CurrentUser.role & ").",
                   MsgBoxStyle.Information, "Login Successful!")

            Mainformd.Show()
            Me.Hide()

            matcheddata = False
            clearfields()
            failedAttempts = 0
            Mainformd.logintimer11.Text = "0"

        Else
            MsgBox("Invalid Credentials", MsgBoxStyle.Exclamation, "Error")
            clearfields()
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