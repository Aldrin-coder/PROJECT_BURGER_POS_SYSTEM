<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class loginform
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.Loginformd = New Guna.UI2.WinForms.Guna2BorderlessForm(Me.components)
        Me.Guna2Panel1 = New Guna.UI2.WinForms.Guna2Panel()
        Me.rolecb = New Guna.UI2.WinForms.Guna2ComboBox()
        Me.Guna2HtmlLabel9 = New Guna.UI2.WinForms.Guna2HtmlLabel()
        Me.showpasschb = New Guna.UI2.WinForms.Guna2ImageCheckBox()
        Me.exitbtn = New Guna.UI2.WinForms.Guna2Button()
        Me.loginbtn = New Guna.UI2.WinForms.Guna2Button()
        Me.passtxb = New Guna.UI2.WinForms.Guna2TextBox()
        Me.usertxb = New Guna.UI2.WinForms.Guna2TextBox()
        Me.Guna2HtmlLabel6 = New Guna.UI2.WinForms.Guna2HtmlLabel()
        Me.Guna2Shapes2 = New Guna.UI2.WinForms.Guna2Shapes()
        Me.Guna2Shapes1 = New Guna.UI2.WinForms.Guna2Shapes()
        Me.Guna2HtmlLabel5 = New Guna.UI2.WinForms.Guna2HtmlLabel()
        Me.Guna2HtmlLabel4 = New Guna.UI2.WinForms.Guna2HtmlLabel()
        Me.Guna2HtmlLabel1 = New Guna.UI2.WinForms.Guna2HtmlLabel()
        Me.Guna2HtmlLabel2 = New Guna.UI2.WinForms.Guna2HtmlLabel()
        Me.Guna2HtmlLabel3 = New Guna.UI2.WinForms.Guna2HtmlLabel()
        Me.Guna2Shapes3 = New Guna.UI2.WinForms.Guna2Shapes()
        Me.Guna2Shapes4 = New Guna.UI2.WinForms.Guna2Shapes()
        Me.Guna2HtmlLabel7 = New Guna.UI2.WinForms.Guna2HtmlLabel()
        Me.Guna2HtmlLabel8 = New Guna.UI2.WinForms.Guna2HtmlLabel()
        Me.Guna2PictureBox1 = New Guna.UI2.WinForms.Guna2PictureBox()
        Me.Guna2CirclePictureBox1 = New Guna.UI2.WinForms.Guna2CirclePictureBox()
        Me.minimize1 = New Guna.UI2.WinForms.Guna2ControlBox()
        Me.timerattempt = New System.Windows.Forms.Timer(Me.components)
        Me.attempt1 = New Guna.UI2.WinForms.Guna2HtmlLabel()
        Me.Guna2Panel1.SuspendLayout()
        CType(Me.Guna2PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Guna2CirclePictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Loginformd
        '
        Me.Loginformd.AnimateWindow = True
        Me.Loginformd.AnimationInterval = 600
        Me.Loginformd.AnimationType = Guna.UI2.WinForms.Guna2BorderlessForm.AnimateWindowType.AW_CENTER
        Me.Loginformd.BorderRadius = 40
        Me.Loginformd.ContainerControl = Me
        Me.Loginformd.DockIndicatorTransparencyValue = 0.6R
        Me.Loginformd.HasFormShadow = False
        Me.Loginformd.ResizeForm = False
        Me.Loginformd.ShadowColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        '
        'Guna2Panel1
        '
        Me.Guna2Panel1.BackColor = System.Drawing.Color.Transparent
        Me.Guna2Panel1.BorderColor = System.Drawing.SystemColors.GradientActiveCaption
        Me.Guna2Panel1.BorderRadius = 25
        Me.Guna2Panel1.BorderThickness = 2
        Me.Guna2Panel1.Controls.Add(Me.attempt1)
        Me.Guna2Panel1.Controls.Add(Me.rolecb)
        Me.Guna2Panel1.Controls.Add(Me.Guna2HtmlLabel9)
        Me.Guna2Panel1.Controls.Add(Me.showpasschb)
        Me.Guna2Panel1.Controls.Add(Me.exitbtn)
        Me.Guna2Panel1.Controls.Add(Me.loginbtn)
        Me.Guna2Panel1.Controls.Add(Me.passtxb)
        Me.Guna2Panel1.Controls.Add(Me.usertxb)
        Me.Guna2Panel1.Controls.Add(Me.Guna2HtmlLabel6)
        Me.Guna2Panel1.Controls.Add(Me.Guna2Shapes2)
        Me.Guna2Panel1.Controls.Add(Me.Guna2Shapes1)
        Me.Guna2Panel1.Controls.Add(Me.Guna2HtmlLabel5)
        Me.Guna2Panel1.Controls.Add(Me.Guna2HtmlLabel4)
        Me.Guna2Panel1.FillColor = System.Drawing.Color.Firebrick
        Me.Guna2Panel1.Location = New System.Drawing.Point(19, 226)
        Me.Guna2Panel1.Name = "Guna2Panel1"
        Me.Guna2Panel1.Padding = New System.Windows.Forms.Padding(10)
        Me.Guna2Panel1.Size = New System.Drawing.Size(349, 449)
        Me.Guna2Panel1.TabIndex = 0
        '
        'rolecb
        '
        Me.rolecb.AutoRoundedCorners = True
        Me.rolecb.BackColor = System.Drawing.Color.Transparent
        Me.rolecb.BorderColor = System.Drawing.Color.Black
        Me.rolecb.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed
        Me.rolecb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.rolecb.FocusedColor = System.Drawing.Color.FromArgb(CType(CType(94, Byte), Integer), CType(CType(148, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.rolecb.FocusedState.BorderColor = System.Drawing.Color.FromArgb(CType(CType(94, Byte), Integer), CType(CType(148, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.rolecb.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rolecb.ForeColor = System.Drawing.Color.Black
        Me.rolecb.ItemHeight = 30
        Me.rolecb.ItemsAppearance.BackColor = System.Drawing.Color.White
        Me.rolecb.ItemsAppearance.SelectedBackColor = System.Drawing.Color.LightGreen
        Me.rolecb.ItemsAppearance.SelectedFont = New System.Drawing.Font("Microsoft YaHei UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rolecb.ItemsAppearance.SelectedForeColor = System.Drawing.Color.Red
        Me.rolecb.Location = New System.Drawing.Point(28, 308)
        Me.rolecb.Name = "rolecb"
        Me.rolecb.Size = New System.Drawing.Size(290, 36)
        Me.rolecb.TabIndex = 16
        Me.rolecb.TextOffset = New System.Drawing.Point(8, 0)
        Me.rolecb.TextTransform = Guna.UI2.WinForms.Enums.TextTransform.UpperCase
        '
        'Guna2HtmlLabel9
        '
        Me.Guna2HtmlLabel9.BackColor = System.Drawing.Color.Transparent
        Me.Guna2HtmlLabel9.Font = New System.Drawing.Font("Microsoft YaHei UI", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Guna2HtmlLabel9.ForeColor = System.Drawing.Color.White
        Me.Guna2HtmlLabel9.Location = New System.Drawing.Point(28, 274)
        Me.Guna2HtmlLabel9.Name = "Guna2HtmlLabel9"
        Me.Guna2HtmlLabel9.Size = New System.Drawing.Size(53, 28)
        Me.Guna2HtmlLabel9.TabIndex = 15
        Me.Guna2HtmlLabel9.Text = "ROLE"
        '
        'showpasschb
        '
        Me.showpasschb.CheckedState.Image = Global.BURGER_SHOP_POS_SYSTEm.My.Resources.Resources.hide
        Me.showpasschb.CheckedState.ImageSize = New System.Drawing.Size(22, 22)
        Me.showpasschb.Image = Global.BURGER_SHOP_POS_SYSTEm.My.Resources.Resources.eye
        Me.showpasschb.ImageOffset = New System.Drawing.Point(0, 0)
        Me.showpasschb.ImageRotate = 0!
        Me.showpasschb.ImageSize = New System.Drawing.Size(22, 22)
        Me.showpasschb.Location = New System.Drawing.Point(266, 230)
        Me.showpasschb.Name = "showpasschb"
        Me.showpasschb.Size = New System.Drawing.Size(40, 17)
        Me.showpasschb.TabIndex = 14
        Me.showpasschb.UseTransparentBackground = True
        '
        'exitbtn
        '
        Me.exitbtn.Animated = True
        Me.exitbtn.AutoRoundedCorners = True
        Me.exitbtn.DisabledState.BorderColor = System.Drawing.Color.DarkGray
        Me.exitbtn.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray
        Me.exitbtn.DisabledState.FillColor = System.Drawing.Color.FromArgb(CType(CType(169, Byte), Integer), CType(CType(169, Byte), Integer), CType(CType(169, Byte), Integer))
        Me.exitbtn.DisabledState.ForeColor = System.Drawing.Color.FromArgb(CType(CType(141, Byte), Integer), CType(CType(141, Byte), Integer), CType(CType(141, Byte), Integer))
        Me.exitbtn.FillColor = System.Drawing.Color.Salmon
        Me.exitbtn.Font = New System.Drawing.Font("Microsoft YaHei UI", 12.0!, System.Drawing.FontStyle.Bold)
        Me.exitbtn.ForeColor = System.Drawing.Color.Black
        Me.exitbtn.IndicateFocus = True
        Me.exitbtn.Location = New System.Drawing.Point(181, 384)
        Me.exitbtn.Name = "exitbtn"
        Me.exitbtn.Size = New System.Drawing.Size(153, 45)
        Me.exitbtn.TabIndex = 13
        Me.exitbtn.Text = "EXIT"
        '
        'loginbtn
        '
        Me.loginbtn.Animated = True
        Me.loginbtn.AutoRoundedCorners = True
        Me.loginbtn.DisabledState.BorderColor = System.Drawing.Color.DarkGray
        Me.loginbtn.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray
        Me.loginbtn.DisabledState.FillColor = System.Drawing.Color.FromArgb(CType(CType(169, Byte), Integer), CType(CType(169, Byte), Integer), CType(CType(169, Byte), Integer))
        Me.loginbtn.DisabledState.ForeColor = System.Drawing.Color.FromArgb(CType(CType(141, Byte), Integer), CType(CType(141, Byte), Integer), CType(CType(141, Byte), Integer))
        Me.loginbtn.FillColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.loginbtn.Font = New System.Drawing.Font("Microsoft YaHei UI", 12.0!, System.Drawing.FontStyle.Bold)
        Me.loginbtn.ForeColor = System.Drawing.Color.Black
        Me.loginbtn.IndicateFocus = True
        Me.loginbtn.Location = New System.Drawing.Point(13, 384)
        Me.loginbtn.Name = "loginbtn"
        Me.loginbtn.Size = New System.Drawing.Size(153, 45)
        Me.loginbtn.TabIndex = 12
        Me.loginbtn.Text = "LOGIN"
        '
        'passtxb
        '
        Me.passtxb.Animated = True
        Me.passtxb.AutoRoundedCorners = True
        Me.passtxb.BorderColor = System.Drawing.Color.Black
        Me.passtxb.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.passtxb.DefaultText = ""
        Me.passtxb.DisabledState.BorderColor = System.Drawing.Color.FromArgb(CType(CType(208, Byte), Integer), CType(CType(208, Byte), Integer), CType(CType(208, Byte), Integer))
        Me.passtxb.DisabledState.FillColor = System.Drawing.Color.FromArgb(CType(CType(226, Byte), Integer), CType(CType(226, Byte), Integer), CType(CType(226, Byte), Integer))
        Me.passtxb.DisabledState.ForeColor = System.Drawing.Color.FromArgb(CType(CType(138, Byte), Integer), CType(CType(138, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.passtxb.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(CType(CType(138, Byte), Integer), CType(CType(138, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.passtxb.FocusedState.BorderColor = System.Drawing.Color.FromArgb(CType(CType(94, Byte), Integer), CType(CType(148, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.passtxb.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.passtxb.ForeColor = System.Drawing.Color.Black
        Me.passtxb.HoverState.BorderColor = System.Drawing.Color.FromArgb(CType(CType(94, Byte), Integer), CType(CType(148, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.passtxb.IconLeft = Global.BURGER_SHOP_POS_SYSTEm.My.Resources.Resources.padlock
        Me.passtxb.IconLeftOffset = New System.Drawing.Point(5, 0)
        Me.passtxb.IconLeftSize = New System.Drawing.Size(25, 25)
        Me.passtxb.Location = New System.Drawing.Point(28, 218)
        Me.passtxb.Margin = New System.Windows.Forms.Padding(4)
        Me.passtxb.Name = "passtxb"
        Me.passtxb.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.passtxb.PlaceholderForeColor = System.Drawing.Color.Gray
        Me.passtxb.PlaceholderText = "ENTER YOUR PASSWORD"
        Me.passtxb.SelectedText = ""
        Me.passtxb.Size = New System.Drawing.Size(290, 40)
        Me.passtxb.TabIndex = 11
        Me.passtxb.TextOffset = New System.Drawing.Point(2, 0)
        '
        'usertxb
        '
        Me.usertxb.Animated = True
        Me.usertxb.AutoRoundedCorners = True
        Me.usertxb.BorderColor = System.Drawing.Color.Black
        Me.usertxb.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.usertxb.DefaultText = ""
        Me.usertxb.DisabledState.BorderColor = System.Drawing.Color.FromArgb(CType(CType(208, Byte), Integer), CType(CType(208, Byte), Integer), CType(CType(208, Byte), Integer))
        Me.usertxb.DisabledState.FillColor = System.Drawing.Color.FromArgb(CType(CType(226, Byte), Integer), CType(CType(226, Byte), Integer), CType(CType(226, Byte), Integer))
        Me.usertxb.DisabledState.ForeColor = System.Drawing.Color.FromArgb(CType(CType(138, Byte), Integer), CType(CType(138, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.usertxb.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(CType(CType(138, Byte), Integer), CType(CType(138, Byte), Integer), CType(CType(138, Byte), Integer))
        Me.usertxb.FocusedState.BorderColor = System.Drawing.Color.FromArgb(CType(CType(94, Byte), Integer), CType(CType(148, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.usertxb.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.usertxb.ForeColor = System.Drawing.Color.Black
        Me.usertxb.HoverState.BorderColor = System.Drawing.Color.FromArgb(CType(CType(94, Byte), Integer), CType(CType(148, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.usertxb.IconLeft = Global.BURGER_SHOP_POS_SYSTEm.My.Resources.Resources.user
        Me.usertxb.IconLeftOffset = New System.Drawing.Point(10, 0)
        Me.usertxb.IconLeftSize = New System.Drawing.Size(25, 25)
        Me.usertxb.Location = New System.Drawing.Point(28, 122)
        Me.usertxb.Margin = New System.Windows.Forms.Padding(4)
        Me.usertxb.Name = "usertxb"
        Me.usertxb.PlaceholderForeColor = System.Drawing.Color.Gray
        Me.usertxb.PlaceholderText = "ENTER YOUR USERNAME"
        Me.usertxb.SelectedText = ""
        Me.usertxb.Size = New System.Drawing.Size(290, 40)
        Me.usertxb.TabIndex = 10
        Me.usertxb.TextOffset = New System.Drawing.Point(2, 0)
        '
        'Guna2HtmlLabel6
        '
        Me.Guna2HtmlLabel6.BackColor = System.Drawing.Color.Transparent
        Me.Guna2HtmlLabel6.Font = New System.Drawing.Font("Microsoft YaHei UI", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Guna2HtmlLabel6.ForeColor = System.Drawing.Color.White
        Me.Guna2HtmlLabel6.Location = New System.Drawing.Point(28, 183)
        Me.Guna2HtmlLabel6.Name = "Guna2HtmlLabel6"
        Me.Guna2HtmlLabel6.Size = New System.Drawing.Size(115, 28)
        Me.Guna2HtmlLabel6.TabIndex = 9
        Me.Guna2HtmlLabel6.Text = "PASSWORD"
        '
        'Guna2Shapes2
        '
        Me.Guna2Shapes2.BorderColor = System.Drawing.Color.Transparent
        Me.Guna2Shapes2.BorderThickness = 1
        Me.Guna2Shapes2.FillColor = System.Drawing.Color.Orange
        Me.Guna2Shapes2.LineThickness = 3
        Me.Guna2Shapes2.Location = New System.Drawing.Point(5, 24)
        Me.Guna2Shapes2.Name = "Guna2Shapes2"
        Me.Guna2Shapes2.PolygonSkip = 1
        Me.Guna2Shapes2.Rotate = 0!
        Me.Guna2Shapes2.Shape = Guna.UI2.WinForms.Enums.ShapeType.Line
        Me.Guna2Shapes2.Size = New System.Drawing.Size(141, 34)
        Me.Guna2Shapes2.TabIndex = 8
        Me.Guna2Shapes2.Text = "Guna2Shapes2"
        Me.Guna2Shapes2.UseTransparentBackground = True
        Me.Guna2Shapes2.Zoom = 80
        '
        'Guna2Shapes1
        '
        Me.Guna2Shapes1.BorderColor = System.Drawing.Color.Transparent
        Me.Guna2Shapes1.BorderThickness = 1
        Me.Guna2Shapes1.FillColor = System.Drawing.Color.Orange
        Me.Guna2Shapes1.LineThickness = 3
        Me.Guna2Shapes1.Location = New System.Drawing.Point(199, 24)
        Me.Guna2Shapes1.Name = "Guna2Shapes1"
        Me.Guna2Shapes1.PolygonSkip = 1
        Me.Guna2Shapes1.Rotate = 0!
        Me.Guna2Shapes1.Shape = Guna.UI2.WinForms.Enums.ShapeType.Line
        Me.Guna2Shapes1.Size = New System.Drawing.Size(150, 34)
        Me.Guna2Shapes1.TabIndex = 7
        Me.Guna2Shapes1.Text = "Guna2Shapes1"
        Me.Guna2Shapes1.UseTransparentBackground = True
        Me.Guna2Shapes1.Zoom = 80
        '
        'Guna2HtmlLabel5
        '
        Me.Guna2HtmlLabel5.BackColor = System.Drawing.Color.Transparent
        Me.Guna2HtmlLabel5.Font = New System.Drawing.Font("Microsoft PhagsPa", 17.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Guna2HtmlLabel5.ForeColor = System.Drawing.Color.Orange
        Me.Guna2HtmlLabel5.Location = New System.Drawing.Point(138, 24)
        Me.Guna2HtmlLabel5.Name = "Guna2HtmlLabel5"
        Me.Guna2HtmlLabel5.Size = New System.Drawing.Size(73, 33)
        Me.Guna2HtmlLabel5.TabIndex = 6
        Me.Guna2HtmlLabel5.Text = "LOGIN"
        Me.Guna2HtmlLabel5.TextAlignment = System.Drawing.ContentAlignment.TopCenter
        '
        'Guna2HtmlLabel4
        '
        Me.Guna2HtmlLabel4.BackColor = System.Drawing.Color.Transparent
        Me.Guna2HtmlLabel4.Font = New System.Drawing.Font("Microsoft YaHei UI", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Guna2HtmlLabel4.ForeColor = System.Drawing.Color.White
        Me.Guna2HtmlLabel4.Location = New System.Drawing.Point(32, 87)
        Me.Guna2HtmlLabel4.Name = "Guna2HtmlLabel4"
        Me.Guna2HtmlLabel4.Size = New System.Drawing.Size(114, 28)
        Me.Guna2HtmlLabel4.TabIndex = 5
        Me.Guna2HtmlLabel4.Text = "USERNAME"
        '
        'Guna2HtmlLabel1
        '
        Me.Guna2HtmlLabel1.BackColor = System.Drawing.Color.Transparent
        Me.Guna2HtmlLabel1.Font = New System.Drawing.Font("Microsoft PhagsPa", 26.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Guna2HtmlLabel1.ForeColor = System.Drawing.Color.DarkOrange
        Me.Guna2HtmlLabel1.Location = New System.Drawing.Point(157, 72)
        Me.Guna2HtmlLabel1.Name = "Guna2HtmlLabel1"
        Me.Guna2HtmlLabel1.Size = New System.Drawing.Size(196, 48)
        Me.Guna2HtmlLabel1.TabIndex = 1
        Me.Guna2HtmlLabel1.Text = "DS BURGER"
        '
        'Guna2HtmlLabel2
        '
        Me.Guna2HtmlLabel2.BackColor = System.Drawing.Color.Transparent
        Me.Guna2HtmlLabel2.Font = New System.Drawing.Font("Microsoft PhagsPa", 26.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Guna2HtmlLabel2.ForeColor = System.Drawing.Color.SeaShell
        Me.Guna2HtmlLabel2.Location = New System.Drawing.Point(157, 117)
        Me.Guna2HtmlLabel2.Name = "Guna2HtmlLabel2"
        Me.Guna2HtmlLabel2.Size = New System.Drawing.Size(99, 48)
        Me.Guna2HtmlLabel2.TabIndex = 2
        Me.Guna2HtmlLabel2.Text = "SHOP"
        '
        'Guna2HtmlLabel3
        '
        Me.Guna2HtmlLabel3.BackColor = System.Drawing.Color.Transparent
        Me.Guna2HtmlLabel3.Font = New System.Drawing.Font("Microsoft PhagsPa", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Guna2HtmlLabel3.ForeColor = System.Drawing.Color.SeaShell
        Me.Guna2HtmlLabel3.Location = New System.Drawing.Point(121, 166)
        Me.Guna2HtmlLabel3.Name = "Guna2HtmlLabel3"
        Me.Guna2HtmlLabel3.Size = New System.Drawing.Size(147, 34)
        Me.Guna2HtmlLabel3.TabIndex = 3
        Me.Guna2HtmlLabel3.Text = "POS SYSTEM"
        '
        'Guna2Shapes3
        '
        Me.Guna2Shapes3.BackColor = System.Drawing.Color.Transparent
        Me.Guna2Shapes3.BorderColor = System.Drawing.Color.Transparent
        Me.Guna2Shapes3.BorderThickness = 1
        Me.Guna2Shapes3.FillColor = System.Drawing.Color.Orange
        Me.Guna2Shapes3.LineThickness = 3
        Me.Guna2Shapes3.Location = New System.Drawing.Point(1, 709)
        Me.Guna2Shapes3.Name = "Guna2Shapes3"
        Me.Guna2Shapes3.PolygonSkip = 1
        Me.Guna2Shapes3.Rotate = 0!
        Me.Guna2Shapes3.Shape = Guna.UI2.WinForms.Enums.ShapeType.Line
        Me.Guna2Shapes3.Size = New System.Drawing.Size(164, 34)
        Me.Guna2Shapes3.TabIndex = 14
        Me.Guna2Shapes3.Text = "Guna2Shapes3"
        Me.Guna2Shapes3.UseTransparentBackground = True
        Me.Guna2Shapes3.Zoom = 80
        '
        'Guna2Shapes4
        '
        Me.Guna2Shapes4.BackColor = System.Drawing.Color.Transparent
        Me.Guna2Shapes4.BorderColor = System.Drawing.Color.Transparent
        Me.Guna2Shapes4.BorderThickness = 1
        Me.Guna2Shapes4.FillColor = System.Drawing.Color.Orange
        Me.Guna2Shapes4.LineThickness = 3
        Me.Guna2Shapes4.Location = New System.Drawing.Point(211, 709)
        Me.Guna2Shapes4.Name = "Guna2Shapes4"
        Me.Guna2Shapes4.PolygonSkip = 1
        Me.Guna2Shapes4.Rotate = 0!
        Me.Guna2Shapes4.Shape = Guna.UI2.WinForms.Enums.ShapeType.Line
        Me.Guna2Shapes4.Size = New System.Drawing.Size(175, 34)
        Me.Guna2Shapes4.TabIndex = 17
        Me.Guna2Shapes4.Text = "Guna2Shapes4"
        Me.Guna2Shapes4.UseTransparentBackground = True
        Me.Guna2Shapes4.Zoom = 80
        '
        'Guna2HtmlLabel7
        '
        Me.Guna2HtmlLabel7.BackColor = System.Drawing.Color.Transparent
        Me.Guna2HtmlLabel7.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Guna2HtmlLabel7.ForeColor = System.Drawing.Color.White
        Me.Guna2HtmlLabel7.Location = New System.Drawing.Point(93, 761)
        Me.Guna2HtmlLabel7.Name = "Guna2HtmlLabel7"
        Me.Guna2HtmlLabel7.Size = New System.Drawing.Size(187, 21)
        Me.Guna2HtmlLabel7.TabIndex = 14
        Me.Guna2HtmlLabel7.Text = "Fresh Burgers, Great Taste !"
        '
        'Guna2HtmlLabel8
        '
        Me.Guna2HtmlLabel8.BackColor = System.Drawing.Color.Transparent
        Me.Guna2HtmlLabel8.Font = New System.Drawing.Font("Microsoft YaHei UI", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Guna2HtmlLabel8.ForeColor = System.Drawing.Color.White
        Me.Guna2HtmlLabel8.Location = New System.Drawing.Point(51, 786)
        Me.Guna2HtmlLabel8.Name = "Guna2HtmlLabel8"
        Me.Guna2HtmlLabel8.Size = New System.Drawing.Size(280, 21)
        Me.Guna2HtmlLabel8.TabIndex = 18
        Me.Guna2HtmlLabel8.Text = "© 2026 Burger Shop. All Rights Reserved."
        '
        'Guna2PictureBox1
        '
        Me.Guna2PictureBox1.BackColor = System.Drawing.Color.Transparent
        Me.Guna2PictureBox1.Image = Global.BURGER_SHOP_POS_SYSTEm.My.Resources.Resources.burger
        Me.Guna2PictureBox1.ImageRotate = 0!
        Me.Guna2PictureBox1.Location = New System.Drawing.Point(166, 702)
        Me.Guna2PictureBox1.Name = "Guna2PictureBox1"
        Me.Guna2PictureBox1.Size = New System.Drawing.Size(39, 41)
        Me.Guna2PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.Guna2PictureBox1.TabIndex = 16
        Me.Guna2PictureBox1.TabStop = False
        Me.Guna2PictureBox1.UseTransparentBackground = True
        '
        'Guna2CirclePictureBox1
        '
        Me.Guna2CirclePictureBox1.BackColor = System.Drawing.Color.Transparent
        Me.Guna2CirclePictureBox1.Image = Global.BURGER_SHOP_POS_SYSTEm.My.Resources.Resources.logo__3_
        Me.Guna2CirclePictureBox1.ImageRotate = 0!
        Me.Guna2CirclePictureBox1.Location = New System.Drawing.Point(44, 27)
        Me.Guna2CirclePictureBox1.Name = "Guna2CirclePictureBox1"
        Me.Guna2CirclePictureBox1.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle
        Me.Guna2CirclePictureBox1.Size = New System.Drawing.Size(118, 173)
        Me.Guna2CirclePictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.Guna2CirclePictureBox1.TabIndex = 4
        Me.Guna2CirclePictureBox1.TabStop = False
        Me.Guna2CirclePictureBox1.UseTransparentBackground = True
        '
        'minimize1
        '
        Me.minimize1.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.minimize1.Animated = True
        Me.minimize1.AutoRoundedCorners = True
        Me.minimize1.BackColor = System.Drawing.Color.Transparent
        Me.minimize1.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MinimizeBox
        Me.minimize1.CustomClick = True
        Me.minimize1.FillColor = System.Drawing.Color.Transparent
        Me.minimize1.IconColor = System.Drawing.Color.White
        Me.minimize1.Location = New System.Drawing.Point(330, -1)
        Me.minimize1.Name = "minimize1"
        Me.minimize1.PressedColor = System.Drawing.Color.White
        Me.minimize1.Size = New System.Drawing.Size(56, 37)
        Me.minimize1.TabIndex = 19
        Me.minimize1.UseTransparentBackground = True
        '
        'timerattempt
        '
        Me.timerattempt.Interval = 1000
        '
        'attempt1
        '
        Me.attempt1.BackColor = System.Drawing.Color.Transparent
        Me.attempt1.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.attempt1.ForeColor = System.Drawing.SystemColors.ButtonFace
        Me.attempt1.Location = New System.Drawing.Point(32, 350)
        Me.attempt1.Name = "attempt1"
        Me.attempt1.Size = New System.Drawing.Size(84, 17)
        Me.attempt1.TabIndex = 17
        Me.attempt1.Text = "timerattempt"
        '
        'loginform
        '
        Me.AcceptButton = Me.loginbtn
        Me.AutoScaleDimensions = New System.Drawing.SizeF(10.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.InfoText
        Me.ClientSize = New System.Drawing.Size(390, 836)
        Me.Controls.Add(Me.minimize1)
        Me.Controls.Add(Me.Guna2HtmlLabel8)
        Me.Controls.Add(Me.Guna2HtmlLabel7)
        Me.Controls.Add(Me.Guna2Shapes4)
        Me.Controls.Add(Me.Guna2Shapes3)
        Me.Controls.Add(Me.Guna2PictureBox1)
        Me.Controls.Add(Me.Guna2CirclePictureBox1)
        Me.Controls.Add(Me.Guna2HtmlLabel3)
        Me.Controls.Add(Me.Guna2HtmlLabel2)
        Me.Controls.Add(Me.Guna2HtmlLabel1)
        Me.Controls.Add(Me.Guna2Panel1)
        Me.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Margin = New System.Windows.Forms.Padding(5)
        Me.Name = "loginform"
        Me.Padding = New System.Windows.Forms.Padding(5)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "LOGIN FORM"
        Me.Guna2Panel1.ResumeLayout(False)
        Me.Guna2Panel1.PerformLayout()
        CType(Me.Guna2PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Guna2CirclePictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Loginformd As Guna.UI2.WinForms.Guna2BorderlessForm
    Friend WithEvents Guna2Panel1 As Guna.UI2.WinForms.Guna2Panel
    Friend WithEvents Guna2HtmlLabel1 As Guna.UI2.WinForms.Guna2HtmlLabel
    Friend WithEvents Guna2HtmlLabel2 As Guna.UI2.WinForms.Guna2HtmlLabel
    Friend WithEvents Guna2HtmlLabel3 As Guna.UI2.WinForms.Guna2HtmlLabel
    Friend WithEvents Guna2CirclePictureBox1 As Guna.UI2.WinForms.Guna2CirclePictureBox
    Friend WithEvents Guna2HtmlLabel5 As Guna.UI2.WinForms.Guna2HtmlLabel
    Friend WithEvents Guna2Shapes1 As Guna.UI2.WinForms.Guna2Shapes
    Friend WithEvents Guna2Shapes2 As Guna.UI2.WinForms.Guna2Shapes
    Friend WithEvents Guna2HtmlLabel6 As Guna.UI2.WinForms.Guna2HtmlLabel
    Friend WithEvents usertxb As Guna.UI2.WinForms.Guna2TextBox
    Friend WithEvents passtxb As Guna.UI2.WinForms.Guna2TextBox
    Friend WithEvents loginbtn As Guna.UI2.WinForms.Guna2Button
    Friend WithEvents exitbtn As Guna.UI2.WinForms.Guna2Button
    Friend WithEvents Guna2HtmlLabel7 As Guna.UI2.WinForms.Guna2HtmlLabel
    Friend WithEvents Guna2Shapes4 As Guna.UI2.WinForms.Guna2Shapes
    Friend WithEvents Guna2Shapes3 As Guna.UI2.WinForms.Guna2Shapes
    Friend WithEvents Guna2PictureBox1 As Guna.UI2.WinForms.Guna2PictureBox
    Friend WithEvents Guna2HtmlLabel8 As Guna.UI2.WinForms.Guna2HtmlLabel
    Friend WithEvents showpasschb As Guna.UI2.WinForms.Guna2ImageCheckBox
    Friend WithEvents minimize1 As Guna.UI2.WinForms.Guna2ControlBox
    Friend WithEvents Guna2HtmlLabel9 As Guna.UI2.WinForms.Guna2HtmlLabel
    Friend WithEvents rolecb As Guna.UI2.WinForms.Guna2ComboBox
    Friend WithEvents Guna2HtmlLabel4 As Guna.UI2.WinForms.Guna2HtmlLabel
    Friend WithEvents timerattempt As Timer
    Friend WithEvents attempt1 As Guna.UI2.WinForms.Guna2HtmlLabel
End Class
