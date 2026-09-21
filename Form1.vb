Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ApplicationServices
Public Class loginform
    Dim matcheddata As Boolean = False
    Dim failedAttempts As Integer = 0
    Dim maxtries As Integer = 3
    Dim secondsleft As Integer = 60
    Dim username1 As String = ""

    Private Sub loginform_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()


        attempt.Visible = False
        uservalm.Visible = False
        Passvalm.Visible = False '



    End Sub


    Private Sub exitbtn_Click(sender As Object, e As EventArgs) Handles exitbtn.Click
        Me.Close()
    End Sub



    Private Sub minimize1_Click(sender As Object, e As EventArgs)
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub showpasschb_CheckedChanged(sender As Object, e As EventArgs) Handles showpasschb.CheckedChanged
        If showpasschb.Checked Then
            passtxb.PasswordChar = ""
        Else
            passtxb.PasswordChar = "*"
        End If
    End Sub

    Private Sub loginbtn_Click(sender As Object, e As EventArgs) Handles loginbtn.Click
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

        Dim pass As String = passtxb.Text.Trim()

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
            usertxb.BorderColor = Color.Transparent
        End If





        query = "SELECT * FROM User_Accounts WHERE USERNAME = '" & usertxb.Text & "' AND PASSWORD = '" & passtxb.Text & "'"
        cmd = New SqlClient.SqlCommand(query, sqlconn)

        dr = cmd.ExecuteReader()

        If dr.HasRows Then
            dr.Read()
            matcheddata = True

            username1 = dr("USERNAME").ToString()
        End If
        dr.Close()
        cmd.Dispose()



        If matcheddata Then
            MsgBox("Login Successful! Welcome " & username1, MsgBoxStyle.Information, "Success")
            Me.Hide()
            matcheddata = False
            mainform.Show()
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



End Class
