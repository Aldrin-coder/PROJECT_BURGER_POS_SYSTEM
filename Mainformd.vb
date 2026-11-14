Imports System.Data.SqlClient
Imports System.IO








Public Class Mainformd

    Dim timercout As Integer = 0


    Private Sub Mainformd_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        dashboardbtn.Checked = True
        datetimenlogintimer.Start()
        ApplyUserInfo()
        loaddashboardroles()

    End Sub





    Sub loaddashboardroles()
        If CurrentUser.role.ToLower() = "owner" Then
            LoadFormInsidePanel(New frm_dashboard1())
        ElseIf CurrentUser.role.ToLower() = "admin" Then
            LoadFormInsidePanel(New frm_dashboard1())
        ElseIf CurrentUser.role.ToLower() = "cashier" Then
            LoadFormInsidePanel(New frm_dashboard1())
        End If
    End Sub

    Sub LoadFormInsidePanel(frm As Form)
        panelcontainer.Controls.Clear()
        frm.TopLevel = False
        frm.FormBorderStyle = FormBorderStyle.None
        frm.Dock = DockStyle.Fill
        panelcontainer.Controls.Add(frm)
        frm.Show()
    End Sub

    Private Sub buttonswitch_Click(sender As Object, e As EventArgs) Handles _
        dashboardbtn.Click, mmbtn.Click, muabtn.Click, oebtn.Click, srbtn.Click

        Dim btn As Guna.UI2.WinForms.Guna2Button = CType(sender, Guna.UI2.WinForms.Guna2Button)


        For Each b As Guna.UI2.WinForms.Guna2Button In {dashboardbtn, mmbtn, muabtn, srbtn}
            b.Checked = (b Is sender)
        Next


        If btn.Name = "dashboardbtn" Then
            If CurrentUser.role.ToLower() = "owner" Then
                LoadFormInsidePanel(New frm_dashboard1())
            ElseIf CurrentUser.role.ToLower() = "admin" Then
                LoadFormInsidePanel(New frm_dashboard1())
            ElseIf CurrentUser.role.ToLower() = "cashier" Then
                LoadFormInsidePanel(New frm_dashboard1())
            End If
        ElseIf btn.Name = "mmbtn" Then
            If CurrentUser.role.ToLower() = "admin" Then
                LoadFormInsidePanel(New frm_mm4admin())
            End If
            If CurrentUser.role.ToLower() = "owner" Then
                LoadFormInsidePanel(New frm_mm4admin())
            End If
        ElseIf btn.Name = "muabtn" Then
            If CurrentUser.role.ToLower() = "owner" Then
                LoadFormInsidePanel(New frm_mua4admin_owner2())
            ElseIf CurrentUser.role.ToLower() = "admin" Then
                LoadFormInsidePanel(New frm_mua4admin_owner2())
            End If

        ElseIf btn.Name = "oebtn" Then
            If CurrentUser.role.ToLower() = "cashier" Then
                LoadFormInsidePanel(New frm_oe4cashier())
            End If


        ElseIf btn.Name = "srbtn" Then
            If CurrentUser.role.ToLower() = "owner" Then
                LoadFormInsidePanel(New frm_sales_report4owner())


            End If

        End If
    End Sub



    Private Sub ApplyUserInfo()

        dashboardbtn.Visible = False
        oebtn.Visible = False
        mmbtn.Visible = False
        muabtn.Visible = False

        srbtn.Visible = False
        lblusershift1.Visible = False


        If CurrentUser.role.ToLower = "owner" Then

            dashboardbtn.Visible = True
            muabtn.Visible = True
            srbtn.Visible = True
            mmbtn.Visible = True

            lbluserinfo1.Text = "OWNER INFORMATION"
            lbluserid1.Text = "OWNER ID      : " & CurrentUser.user_code
            lbluserfn1.Text = "FULLNAME           : " & CurrentUser.fullname
            lbluserg1.Text = "GENDER           : " & CurrentUser.gender.ToUpper

            lblusershift1.Visible = False
            lbluserinfo1.Margin = New Padding(0, 0, 0, 20)
            logoutbtn.Margin = New Padding(92, 130, 3, 0)




        ElseIf CurrentUser.role.ToLower = "admin" Then
            dashboardbtn.Visible = True
            mmbtn.Visible = True
            muabtn.Visible = True

            lbluserinfo1.Text = "ADMIN INFORMATION"
            lbluserid1.Text = "ADMIN ID      : " & CurrentUser.user_code
            lbluserfn1.Text = "FULLNAME           : " & CurrentUser.fullname
            lbluserg1.Text = "GENDER           : " & CurrentUser.gender.ToUpper
            lblusershift1.Visible = False
            lbluserinfo1.Margin = New Padding(0, 0, 0, 20)
            logoutbtn.Margin = New Padding(92, 180, 3, 0)



        ElseIf CurrentUser.role.ToLower = "cashier" Then
            oebtn.Visible = True

            dashboardbtn.Visible = True
            lblusershift1.Visible = True


            lbluserinfo1.Text = "CASHIER INFORMATION"
            lbluserid1.Text = "CASHIER ID      : " & CurrentUser.user_code
            lbluserfn1.Text = "FULLNAME           : " & CurrentUser.fullname
            lbluserg1.Text = "GENDER           : " & CurrentUser.gender.ToUpper
            lblusershift1.Text = "SHIFT           : " & CurrentUser.shift.ToUpper
            lbluserinfo1.Margin = New Padding(0, 0, 0, 10)
            logoutbtn.Margin = New Padding(92, 230, 3, 0)


        End If

    End Sub




    Private Sub logoutbtn_Click(sender As Object, e As EventArgs) Handles logoutbtn.Click
        Dim result As DialogResult = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If result = DialogResult.Yes Then
            Me.Close()

            loginform.Show()
        Else
            logoutbtn.Checked = False
        End If
    End Sub




    Private Sub datentimer_Tick(sender As Object, e As EventArgs) Handles datetimenlogintimer.Tick
        datelabel1.Text = " DATE: " & Date.Now.ToString(" MMM dd, yyyy").ToUpper
        timerlabel1.Text = "TIME: " & Date.Now.ToString(" hh:mm:ss tt").ToUpper

        timercout += 1
        Dim ts As TimeSpan = TimeSpan.FromSeconds(timercout)
        logintimer11.Text = "TIME IN: " & ts.ToString("hh\:mm\:ss")

    End Sub


    Private Sub minimize_Click(sender As Object, e As EventArgs) Handles minimize.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub exit2_Click(sender As Object, e As EventArgs) Handles exit2.Click
        Application.Exit()
    End Sub

    Private Sub FlowLayoutPanel3_Paint(sender As Object, e As PaintEventArgs) Handles FlowLayoutPanel3.Paint
        FlowLayoutPanel3.BackColor = Color.FromArgb(100, 10, 20, 40)

    End Sub

    Private Sub Guna2Panel1_Paint(sender As Object, e As PaintEventArgs) Handles Guna2Panel1.Paint

    End Sub

    Private Sub panelcontainer_Paint(sender As Object, e As PaintEventArgs) Handles panelcontainer.Paint

    End Sub
End Class