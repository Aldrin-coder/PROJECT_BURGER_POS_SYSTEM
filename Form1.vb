Imports System.Diagnostics.Eventing.Reader
Imports System.Text

Public Class loginform
    Dim matcheddata As Boolean = False
    Dim failedAttempts As Integer = 0
    Dim maxtries As Integer = 3
    Dim secondsleft As Integer = 60



    Private Sub loginform_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        conn()
        roles()
        attempt1.Visible = False
    End Sub

    Private Sub exitbtn_Click(sender As Object, e As EventArgs) Handles exitbtn.Click
        Me.Close()
    End Sub

    Sub roles()
        rolecb.Items.Clear()
        With rolecb.Items
            .Add("OWNER")
            .Add("ADMIN")
            .Add("CASHIER")

            rolecb.SelectedIndex = -1

        End With
    End Sub
    Private Sub minimize1_Click(sender As Object, e As EventArgs) Handles minimize1.Click
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
        If usertxb.Text = "" And passtxb.Text = "" And rolecb.SelectedIndex = -1 Then
            MsgBox("Please fill up all the fields", MsgBoxStyle.Exclamation, "Error")
            Return
        ElseIf usertxb.Text = "" Then
            MsgBox("Please enter your username", MsgBoxStyle.Exclamation, "Error")
            usertxb.Focus()
            Return
        ElseIf passtxb.Text = "" Then
            MsgBox("Please enter your password", MsgBoxStyle.Exclamation, "Error")
            passtxb.Focus()
            Return
        ElseIf rolecb.SelectedIndex = -1 Then
            MsgBox("Please select a role", MsgBoxStyle.Exclamation, "Error")
            rolecb.Focus()
            Return

        End If


        Dim table As String = ""
        Dim idcolumn As String = ""

        If rolecb.SelectedItem = "OWNER" Then
            table = "owneraccts"
            idcolumn = "ownerid"

        ElseIf rolecb.SelectedItem = "ADMIN" Then
            table = "adminaccts"
            idcolumn = "adminid"

        ElseIf rolecb.SelectedItem = "CASHIER" Then
            table = "cashieraccts"
            idcolumn = "cashierid"


        Else
            MsgBox("Invalid role selected", MsgBoxStyle.Exclamation, "Error")
            Return
        End If

        matcheddata = False


        query = "SELECT * FROM " & table & " WHERE   username = @username AND password = @password"
        cmd = New SqlClient.SqlCommand(query, sqlconn)
        cmd.Parameters.AddWithValue("@username", usertxb.Text)
        cmd.Parameters.AddWithValue("@password", passtxb.Text)

        dr = cmd.ExecuteReader()

        If dr.HasRows Then
            dr.Read()
            matcheddata = True
        End If
        dr.Close()
        cmd.Dispose()


        If matcheddata Then
            MsgBox("Login successful", MsgBoxStyle.Information, "Success")
            Me.Hide()
            matcheddata = False
            mainform.Show()
            usertxb.Clear()
            passtxb.Clear()
            rolecb.SelectedIndex = -1
            failedAttempts = 0


        Else
            MsgBox("Invalid Username or Password for Selected role", MsgBoxStyle.Exclamation, "Error")
            usertxb.Clear()
            passtxb.Clear()
            rolecb.SelectedIndex = -1
            failedAttempts += 1

        End If

        If failedAttempts >= maxtries Then
            usertxb.Enabled = False
            passtxb.Enabled = False
            rolecb.Enabled = False
            loginbtn.Enabled = False
            exitbtn.Enabled = False
            attempt1.Visible = False
            timerattempt.Start()
            MsgBox("You have login failed for 3 times continuously. Please try after " & secondsleft & " seconds.", MsgBoxStyle.Critical, "Locked")


        End If

    End Sub

    Private Sub timerattempt_Tick(sender As Object, e As EventArgs) Handles timerattempt.Tick

        secondsleft -= 1
        attempt1.Text = "Try again in " & secondsleft & " seconds remaining".ToString
        attempt1.Visible = True

        If secondsleft <= 0 Then
            timerattempt.Stop()
            usertxb.Enabled = True
            passtxb.Enabled = True
            rolecb.Enabled = True
            loginbtn.Enabled = True
            exitbtn.Enabled = True


            failedAttempts = 0
            MsgBox("You can try again now.", MsgBoxStyle.Information, "Unlocked")
            attempt1.Visible = False
            usertxb.Enabled = True
            passtxb.Enabled = True
            rolecb.Enabled = True
            loginbtn.Enabled = True
            exitbtn.Enabled = True
            Return
        End If



    End Sub
End Class
