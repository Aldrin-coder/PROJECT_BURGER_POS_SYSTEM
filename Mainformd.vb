Imports System.Data.Common
Imports System.Drawing.Text
Imports System.Text
Imports System.Web.UI.Design.WebControls
Imports Microsoft.VisualBasic.ApplicationServices
Imports TheArtOfDevHtmlRenderer.Adapters.RGraphicsPath




Public Class Mainformd
    Private Sub Mainformd_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        datentimer.Start()
        ApplyUserInfo()
        dashboardbtn.Checked = True

        loaddashboardroles()


    End Sub

    Private Sub loaddashboardroles()
        If CurrentUser.role.ToLower() = "owner" Then
            LoadUserControl(New ucdashboard4owner())
        ElseIf CurrentUser.role.ToLower() = "admin" Then
            LoadUserControl(New ucdashboard4admin())
        ElseIf CurrentUser.role.ToLower() = "cashier" Then
            LoadUserControl(New ucdashboard4cashier())
        End If
    End Sub

    Sub LoadUserControl(uc As UserControl)
        panelcontainer.Controls.Clear()
        uc.Dock = DockStyle.Fill
        panelcontainer.Controls.Add(uc)

    End Sub

    Private Sub buttonswitch_Click(sender As Object, e As EventArgs) Handles _
        dashboardbtn.Click, mmbtn.Click, muabtn.Click, oebtn.Click,
        trbtn.Click, srbtn.Click

        Dim btn As Guna.UI2.WinForms.Guna2Button = CType(sender, Guna.UI2.WinForms.Guna2Button)


        For Each b As Guna.UI2.WinForms.Guna2Button In {dashboardbtn, mmbtn, muabtn, oebtn, trbtn, srbtn}
            b.Checked = (b Is sender)
        Next


        If btn.Name = "dashboardbtn" Then
            If CurrentUser.role.ToLower() = "owner" Then
                LoadUserControl(New ucdashboard4owner())
            ElseIf CurrentUser.role.ToLower() = "admin" Then
                LoadUserControl(New ucdashboard4admin())
            ElseIf CurrentUser.role.ToLower() = "cashier" Then
                LoadUserControl(New ucdashboard4cashier())
            End If
        ElseIf btn.Name = "mmbtn" Then


            ElseIf btn.Name = "muabtn" Then


        ElseIf btn.Name = "oebtn" Then


            ElseIf btn.Name = "trbtn" Then

            ElseIf btn.Name = "srbtn" Then

            End If
    End Sub



    Private Sub ApplyUserInfo()

        dashboardbtn.Visible = False
        oebtn.Visible = False
        mmbtn.Visible = False
        muabtn.Visible = False
        trbtn.Visible = False
        srbtn.Visible = False
        lblusershift1.Visible = False


        If CurrentUser.role.ToLower = "owner" Then

            dashboardbtn.Visible = True
            oebtn.Visible = True
            mmbtn.Visible = True
            muabtn.Visible = True
            trbtn.Visible = True
            srbtn.Visible = True

            lbluserinfo1.Text = "OWNER INFORMATION"
            lbluserid1.Text = "OWNER ID      : " & CurrentUser.role_code
            lbluserfn1.Text = "FULLNAME           : " & CurrentUser.fullname
            lbluserg1.Text = "GENDER           : " & CurrentUser.gender.ToUpper
            lblusershift1.Visible = False
            lbluserinfo1.Margin = New Padding(0, 0, 0, 15)
            logoutbtn.Margin = New Padding(92, 70, 0, 0)



        ElseIf CurrentUser.role.ToLower = "admin" Then
            dashboardbtn.Visible = True
            mmbtn.Visible = True
            muabtn.Visible = True

            lbluserinfo1.Text = "ADMIN INFORMATION"
            lbluserid1.Text = "ADMIN ID      : " & CurrentUser.role_code
            lbluserfn1.Text = "FULLNAME           : " & CurrentUser.fullname
            lbluserg1.Text = "GENDER           : " & CurrentUser.gender.ToUpper
            lblusershift1.Visible = False
            lbluserinfo1.Margin = New Padding(0, 0, 0, 15)
            logoutbtn.Margin = New Padding(90, 230, 0, 0)



        ElseIf CurrentUser.role.ToLower = "cashier" Then
            oebtn.Visible = True
            trbtn.Visible = True
            dashboardbtn.Visible = True
            lblusershift1.Visible = True
            mmbtn.Visible = True

            lbluserinfo1.Text = "CASHIER INFORMATION"
            lbluserid1.Text = "CASHIER ID      : " & CurrentUser.role_code
            lbluserfn1.Text = "FULLNAME           : " & CurrentUser.fullname
            lbluserg1.Text = "GENDER           : " & CurrentUser.gender.ToUpper
            lblusershift1.Text = "SHIFT           : " & CurrentUser.shift.ToUpper
            lbluserinfo1.Margin = New Padding(0, 0, 0, 10)
            logoutbtn.Margin = New Padding(92, 180, 3, 0)

        End If

    End Sub





    Private Sub logoutbtn_Click(sender As Object, e As EventArgs) Handles logoutbtn.Click
        Dim result As DialogResult = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If result = DialogResult.Yes Then
            Me.Close()
            Dim f As New loginform()
            f.Show()
        Else
            logoutbtn.Checked = False
        End If
    End Sub




    Private Sub datentimer_Tick(sender As Object, e As EventArgs) Handles datentimer.Tick
        datelabel1.Text = " DATE: " & Date.Now.ToString(" MMM dd, yyyy").ToUpper
        timerlabel1.Text = "TIME: " & Date.Now.ToString(" hh:mm:ss tt").ToUpper

    End Sub


    Private Sub Guna2ControlBox2_Click(sender As Object, e As EventArgs) Handles Guna2ControlBox2.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub exit2_Click(sender As Object, e As EventArgs) Handles exit2.Click
        Application.Exit()
    End Sub

    Private Sub FlowLayoutPanel3_Paint(sender As Object, e As PaintEventArgs) Handles FlowLayoutPanel3.Paint
        FlowLayoutPanel3.BackColor = Color.FromArgb(50, 10, 20, 40)

    End Sub

End Class