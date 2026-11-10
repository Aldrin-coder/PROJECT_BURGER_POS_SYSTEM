<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frm_receipt
    Inherits System.Windows.Forms.Form

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.pnlHeader = New Guna.UI2.WinForms.Guna2Panel()
        Me.exitbtn = New Guna.UI2.WinForms.Guna2ControlBox()
        Me.lblTitle = New Guna.UI2.WinForms.Guna2HtmlLabel()
        Me.pnlFooter = New Guna.UI2.WinForms.Guna2Panel()
        Me.btnPrint = New Guna.UI2.WinForms.Guna2Button()
        Me.btnSavePDF = New Guna.UI2.WinForms.Guna2Button()
        Me.pnlReceiptHolder = New Guna.UI2.WinForms.Guna2Panel()
        Me.picReceipt = New Guna.UI2.WinForms.Guna2PictureBox()
        Me.PrintDocument1 = New System.Drawing.Printing.PrintDocument()
        Me.PrintDialog1 = New System.Windows.Forms.PrintDialog()
        Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
        Me.pnlHeader.SuspendLayout()
        Me.pnlFooter.SuspendLayout()
        Me.pnlReceiptHolder.SuspendLayout()
        CType(Me.picReceipt, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'pnlHeader
        '
        Me.pnlHeader.Controls.Add(Me.exitbtn)
        Me.pnlHeader.Controls.Add(Me.lblTitle)
        Me.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlHeader.FillColor = System.Drawing.Color.FromArgb(CType(CType(34, Byte), Integer), CType(CType(68, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.pnlHeader.Location = New System.Drawing.Point(0, 0)
        Me.pnlHeader.Name = "pnlHeader"
        Me.pnlHeader.Size = New System.Drawing.Size(460, 60)
        Me.pnlHeader.TabIndex = 0
        '
        'exitbtn
        '
        Me.exitbtn.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.exitbtn.FillColor = System.Drawing.Color.FromArgb(CType(CType(139, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(166, Byte), Integer))
        Me.exitbtn.HoverState.FillColor = System.Drawing.Color.Red
        Me.exitbtn.IconColor = System.Drawing.Color.White
        Me.exitbtn.Location = New System.Drawing.Point(421, 0)
        Me.exitbtn.Name = "exitbtn"
        Me.exitbtn.Size = New System.Drawing.Size(39, 23)
        Me.exitbtn.TabIndex = 1
        '
        'lblTitle
        '
        Me.lblTitle.BackColor = System.Drawing.Color.Transparent
        Me.lblTitle.Font = New System.Drawing.Font("Microsoft YaHei UI", 15.0!, System.Drawing.FontStyle.Bold)
        Me.lblTitle.ForeColor = System.Drawing.Color.White
        Me.lblTitle.Location = New System.Drawing.Point(20, 14)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(183, 29)
        Me.lblTitle.TabIndex = 0
        Me.lblTitle.Text = "RECEIPT PREVIEW"
        '
        'pnlFooter
        '
        Me.pnlFooter.BackColor = System.Drawing.Color.FromArgb(CType(CType(245, Byte), Integer), CType(CType(245, Byte), Integer), CType(CType(245, Byte), Integer))
        Me.pnlFooter.Controls.Add(Me.btnPrint)
        Me.pnlFooter.Controls.Add(Me.btnSavePDF)
        Me.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlFooter.Location = New System.Drawing.Point(0, 650)
        Me.pnlFooter.Name = "pnlFooter"
        Me.pnlFooter.Size = New System.Drawing.Size(460, 80)
        Me.pnlFooter.TabIndex = 1
        '
        'btnPrint
        '
        Me.btnPrint.AutoRoundedCorners = True
        Me.btnPrint.BorderRadius = 21
        Me.btnPrint.FillColor = System.Drawing.Color.ForestGreen
        Me.btnPrint.Font = New System.Drawing.Font("Microsoft YaHei UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnPrint.ForeColor = System.Drawing.Color.White
        Me.btnPrint.HoverState.FillColor = System.Drawing.Color.LimeGreen
        Me.btnPrint.Location = New System.Drawing.Point(20, 18)
        Me.btnPrint.Name = "btnPrint"
        Me.btnPrint.Size = New System.Drawing.Size(226, 45)
        Me.btnPrint.TabIndex = 0
        Me.btnPrint.Text = "PRINT"
        '
        'btnSavePDF
        '
        Me.btnSavePDF.AutoRoundedCorners = True
        Me.btnSavePDF.BorderRadius = 21
        Me.btnSavePDF.FillColor = System.Drawing.Color.FromArgb(CType(CType(34, Byte), Integer), CType(CType(68, Byte), Integer), CType(CType(125, Byte), Integer))
        Me.btnSavePDF.Font = New System.Drawing.Font("Microsoft YaHei UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.btnSavePDF.ForeColor = System.Drawing.Color.White
        Me.btnSavePDF.HoverState.FillColor = System.Drawing.Color.FromArgb(CType(CType(50, Byte), Integer), CType(CType(90, Byte), Integer), CType(CType(160, Byte), Integer))
        Me.btnSavePDF.Location = New System.Drawing.Point(270, 18)
        Me.btnSavePDF.Name = "btnSavePDF"
        Me.btnSavePDF.Size = New System.Drawing.Size(164, 45)
        Me.btnSavePDF.TabIndex = 1
        Me.btnSavePDF.Text = "SAVE PDF"
        '
        'pnlReceiptHolder
        '
        Me.pnlReceiptHolder.AutoScroll = True
        Me.pnlReceiptHolder.BackColor = System.Drawing.Color.FromArgb(CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(230, Byte), Integer))
        Me.pnlReceiptHolder.Controls.Add(Me.picReceipt)
        Me.pnlReceiptHolder.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlReceiptHolder.Location = New System.Drawing.Point(0, 60)
        Me.pnlReceiptHolder.Name = "pnlReceiptHolder"
        Me.pnlReceiptHolder.Size = New System.Drawing.Size(460, 590)
        Me.pnlReceiptHolder.TabIndex = 2
        '
        'picReceipt
        '
        Me.picReceipt.BackColor = System.Drawing.Color.White
        Me.picReceipt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.picReceipt.ImageRotate = 0!
        Me.picReceipt.Location = New System.Drawing.Point(40, 20)
        Me.picReceipt.Name = "picReceipt"
        Me.picReceipt.Size = New System.Drawing.Size(380, 700)
        Me.picReceipt.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize
        Me.picReceipt.TabIndex = 0
        Me.picReceipt.TabStop = False
        '
        'PrintDocument1
        '
        Me.PrintDocument1.DocumentName = "BurgerShopReceipt"
        '
        'frm_receipt
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(460, 730)
        Me.Controls.Add(Me.pnlReceiptHolder)
        Me.Controls.Add(Me.pnlFooter)
        Me.Controls.Add(Me.pnlHeader)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "frm_receipt"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Receipt Preview"
        Me.pnlHeader.ResumeLayout(False)
        Me.pnlHeader.PerformLayout()
        Me.pnlFooter.ResumeLayout(False)
        Me.pnlReceiptHolder.ResumeLayout(False)
        Me.pnlReceiptHolder.PerformLayout()
        CType(Me.picReceipt, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents pnlHeader As Guna.UI2.WinForms.Guna2Panel
    Friend WithEvents pnlFooter As Guna.UI2.WinForms.Guna2Panel
    Friend WithEvents btnPrint As Guna.UI2.WinForms.Guna2Button
    Friend WithEvents btnSavePDF As Guna.UI2.WinForms.Guna2Button
    Friend WithEvents pnlReceiptHolder As Guna.UI2.WinForms.Guna2Panel
    Friend WithEvents picReceipt As Guna.UI2.WinForms.Guna2PictureBox
    Friend WithEvents PrintDocument1 As System.Drawing.Printing.PrintDocument
    Friend WithEvents PrintDialog1 As PrintDialog
    Friend WithEvents SaveFileDialog1 As SaveFileDialog
    Friend WithEvents lblTitle As Guna.UI2.WinForms.Guna2HtmlLabel
    Friend WithEvents exitbtn As Guna.UI2.WinForms.Guna2ControlBox
End Class