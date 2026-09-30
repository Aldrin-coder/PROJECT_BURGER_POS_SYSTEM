Imports System.Text


Public Class Mainformd
    Private Sub Mainformd_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        datentimer.Start()
        ApplyUserInfo()
        CurrentUser.Clear()


    End Sub
    Private Sub ApplyUserInfo()

        dashboardbtn.Visible = False
        oebtn.Visible = False
        mmbtn.Visible = False
        muabtn.Visible = False
        trbtn.Visible = False
        srbtn.Visible = False
        lblusershift.Visible = True


        If CurrentUser.role.ToLower = "owner" Then

            dashboardbtn.Visible = True
            oebtn.Visible = True
            mmbtn.Visible = True
            muabtn.Visible = True
            trbtn.Visible = True
            srbtn.Visible = True
            lbluserinfo.Text = "OWNER INFORMATION"
            lbluserid.Text = "OWNER ID      : " & CurrentUser.code
            lbluserfn.Text = "FULLNAME           : " & CurrentUser.fullname
            lbluserg.Text = "GENDER           : " & CurrentUser.gender
            lblusershift.Visible = False
            lbluserid.Margin = New Padding(20, 40, 3, 5)
            FlowLayoutPanel3.Margin = New Padding(25, 40, 3, 20)

        ElseIf CurrentUser.role.ToLower = "admin" Then
            mmbtn.Visible = True
            muabtn.Visible = True

            lbluserinfo.Text = "ADMIN INFORMATION"
            lbluserid.Text = "ADMIN ID      : " & CurrentUser.code
            lbluserfn.Text = "FULLNAME           : " & CurrentUser.fullname
            lbluserg.Text = "GENDER           : " & CurrentUser.gender
            lblusershift.Visible = False
            lbluserid.Margin = New Padding(20, 40, 3, 5)
            FlowLayoutPanel3.Margin = New Padding(25, 260, 3, 20)
        ElseIf CurrentUser.role.ToLower = "cashier" Then
            trbtn.Visible = True
            oebtn.Visible = True
            dashboardbtn.Visible = True

            lbluserinfo.Text = "CASHIER INFORMATION"
            lbluserid.Text = "CASHIER ID      : " & CurrentUser.code
            lbluserfn.Text = "FULLNAME           : " & CurrentUser.fullname
            lbluserg.Text = "GENDER           : " & CurrentUser.gender
            lblusershift.Text = "SHIFT           : " & CurrentUser.shift

            FlowLayoutPanel3.Margin = New Padding(25, 200, 3, 20)
        End If

    End Sub





    Private Sub logoutbtn_Click(sender As Object, e As EventArgs) Handles logoutbtn.Click
        Dim result As DialogResult = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If result = DialogResult.Yes Then

            Me.Close()
            Dim f As New loginform()
            f.Show()

        End If
    End Sub
    Private Sub datentimer_Tick(sender As Object, e As EventArgs) Handles datentimer.Tick
        datelabel.Text = Date.Now.ToString("MMM dd, yyyy")
        timerlabel.Text = Date.Now.ToString("hh:mm:s tt")
    End Sub



    Private Sub Guna2ControlBox2_Click(sender As Object, e As EventArgs) Handles Guna2ControlBox2.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub exit2_Click(sender As Object, e As EventArgs) Handles exit2.Click
        Application.Exit()
    End Sub

    Private Sub FlowLayoutPanel3_Paint(sender As Object, e As PaintEventArgs) Handles FlowLayoutPanel3.Paint
        FlowLayoutPanel3.BackColor = Color.FromArgb(128, 0, 0, 0)

    End Sub


End Class