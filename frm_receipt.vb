Imports System.Drawing.Drawing2D
Imports System.Drawing.Printing
Imports System.Drawing.Text

Public Class frm_receipt

    Public Class ReceiptItem
        Public Property Name As String
        Public Property Qty As Integer
        Public Property UnitPrice As Decimal
        Public Property Subtotal As Decimal
    End Class

    Private _txnId As String
    Private _orderId As String
    Private _cashier As String
    Private _method As String
    Private _subtotal As Decimal
    Private _tax As Decimal
    Private _total As Decimal
    Private _paid As Decimal
    Private _change As Decimal
    Private _items As New List(Of ReceiptItem)

    ' --- SINGLE SOURCE OF TRUTH FOR COLUMN GEOMETRY ---
    ' Every money value (item amount, subtotal, tax, TOTAL, paid, change)
    ' is right-aligned to AmtRight. Every quantity is right-aligned to QtyRight.
    Private Const ReceiptWidth As Integer = 400
    Private Const ColLeft As Single = 10
    Private Const ColRight As Single = 390
    Private Const QtyRight As Single = 270      ' right edge of QTY column
    Private Const AmtRight As Single = 386      ' right edge of ALL money values

    Public Sub ShowReceipt(txnId As String, orderId As String, cashier As String,
                           method As String, subtotal As Decimal, tax As Decimal,
                           total As Decimal, paid As Decimal, change As Decimal,
                           items As List(Of ReceiptItem))
        _txnId = txnId
        _orderId = orderId
        _cashier = cashier
        _method = method
        _subtotal = subtotal
        _tax = tax
        _total = total
        _paid = paid
        _change = change
        _items = items

        RenderPreview()
        Me.ShowDialog()
    End Sub

    Private Sub RenderPreview()
        Dim bmpHeight As Integer = 500 + (_items.Count * 25)
        If bmpHeight < 800 Then bmpHeight = 800

        Dim bmp As New Bitmap(ReceiptWidth, bmpHeight)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.White)
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit
            g.SmoothingMode = SmoothingMode.AntiAlias
            DrawReceipt(g, ColLeft, ColRight)
        End Using

        picReceipt.Image = bmp
        picReceipt.Size = bmp.Size
    End Sub

    Private Sub DrawReceipt(g As Graphics, L As Single, R As Single)
        Dim fTitle As New Font("Consolas", 14, FontStyle.Bold)
        Dim fSub As New Font("Consolas", 8, FontStyle.Italic)
        Dim fBold As New Font("Consolas", 8.5, FontStyle.Bold)
        Dim fNorm As New Font("Consolas", 8.5, FontStyle.Regular)
        Dim fSmall As New Font("Consolas", 7, FontStyle.Regular)
        Dim fTotal As New Font("Consolas", 12, FontStyle.Bold)
        Dim black As New SolidBrush(Color.Black)

        Dim CX As Single = (L + R) / 2
        Dim y As Single = 15
        Dim lh As Single = 15
        Dim gap As Single = 10

        ' --- HEADER ---
        DrawCenter(g, "BURGER SHOP", fTitle, black, CX, y) : y += 22
        DrawCenter(g, "Fresh Burger, Great Taste !", fSub, black, CX, y) : y += lh

        DrawRule(g, L, R, y, fSmall, black) : y += gap

        DrawKV(g, "TXN # :", _txnId, fNorm, fBold, black, L, y) : y += lh
        DrawKV(g, "Order :", _orderId, fNorm, fBold, black, L, y) : y += lh
        DrawKV(g, "Date  :", DateTime.Now.ToString("MM/dd/yyyy hh:mm tt"), fNorm, fNorm, black, L, y) : y += lh
        DrawKV(g, "Cashier:", _cashier, fNorm, fNorm, black, L, y) : y += lh
        DrawKV(g, "Payment:", _method, fNorm, fBold, black, L, y) : y += lh

        DrawRule(g, L, R, y, fSmall, black) : y += gap

        ' --- COLUMN HEADERS (right-aligned to match values) ---
        g.DrawString("ITEM", fBold, black, L, y)

        Dim qtyHdrSz As SizeF = g.MeasureString("QTY", fBold)
        g.DrawString("QTY", fBold, black, QtyRight - qtyHdrSz.Width, y)

        Dim amtHdrSz As SizeF = g.MeasureString("AMOUNT", fBold)
        g.DrawString("AMOUNT", fBold, black, AmtRight - amtHdrSz.Width, y)

        y += lh
        DrawRule(g, L, R, y, fSmall, black) : y += gap

        ' --- ITEMS ---
        For Each it As ReceiptItem In _items
            Dim nm As String = it.Name.ToUpper()
            If nm.Length > 26 Then nm = nm.Substring(0, 25) & "~"

            ' Item name (left-aligned)
            g.DrawString(nm, fNorm, black, L, y)

            ' QTY (right-aligned to QtyRight)
            Dim qtyTxt As String = it.Qty.ToString()
            Dim qtySz As SizeF = g.MeasureString(qtyTxt, fNorm)
            g.DrawString(qtyTxt, fNorm, black, QtyRight - qtySz.Width, y)

            ' AMOUNT (right-aligned to AmtRight)
            Dim amtTxt As String = "P" & it.Subtotal.ToString("N2")
            Dim amtSz As SizeF = g.MeasureString(amtTxt, fNorm)
            g.DrawString(amtTxt, fNorm, black, AmtRight - amtSz.Width, y)

            If it.Qty > 1 Then
                y += 11
                g.DrawString("   @ P" & it.UnitPrice.ToString("N2") & " ea", fSmall, black, L, y)
            End If
            y += lh
        Next

        ' --- TOTALS (all right-aligned to AmtRight) ---
        DrawRule(g, L, R, y, fSmall, black) : y += gap
        DrawRight(g, "Subtotal:", "P " & _subtotal.ToString("N2"), fNorm, fNorm, black, y) : y += lh
        DrawRight(g, "Tax (2%):", "P " & _tax.ToString("N2"), fNorm, fNorm, black, y) : y += lh

        DrawRule(g, L, R, y, fSmall, black) : y += gap

        ' TOTAL — right-aligned to the SAME AmtRight edge
        g.DrawString("TOTAL:", fTotal, black, L, y)
        Dim totTxt As String = "P " & _total.ToString("N2")
        Dim totSz As SizeF = g.MeasureString(totTxt, fTotal)
        g.DrawString(totTxt, fTotal, black, AmtRight - totSz.Width, y) : y += 20

        y += 4
        DrawRight(g, "Paid (" & _method & "):", "P " & _paid.ToString("N2"), fNorm, fNorm, black, y) : y += lh
        DrawRight(g, "Change:", "P " & _change.ToString("N2"), fBold, fBold, black, y) : y += lh

        ' --- FOOTER ---
        y += 8
        DrawRule(g, L, R, y, fSmall, black) : y += 14

        DrawCenter(g, "THANK YOU FOR YOUR PURCHASE!", fBold, black, CX, y) : y += 16
        DrawCenter(g, "Please come again!", fNorm, black, CX, y) : y += 14
        y += 4
        DrawCenter(g, "*** THIS IS YOUR RECEIPT ***", fSmall, black, CX, y)

        ' Dispose fonts/brush
        fTitle.Dispose() : fSub.Dispose() : fBold.Dispose()
        fNorm.Dispose() : fSmall.Dispose() : fTotal.Dispose()
        black.Dispose()
    End Sub

    Private Sub DrawCenter(g As Graphics, txt As String, f As Font, br As Brush, cx As Single, y As Single)
        Dim sz As SizeF = g.MeasureString(txt, f)
        g.DrawString(txt, f, br, cx - sz.Width / 2, y)
    End Sub

    Private Sub DrawRule(g As Graphics, L As Single, R As Single, y As Single, f As Font, br As Brush)
        Dim count As Integer = CInt((R - L) / 5.2)
        g.DrawString(New String("-"c, count), f, br, L, y)
    End Sub

    ' DrawRight now ALWAYS right-aligns to AmtRight (the single money edge),
    ' so Subtotal / Tax / Paid / Change align with TOTAL and item amounts.
    Private Sub DrawRight(g As Graphics, label As String, value As String,
                          fLabel As Font, fValue As Font, br As Brush,
                          y As Single)
        g.DrawString(label, fLabel, br, ColLeft, y)
        Dim sz As SizeF = g.MeasureString(value, fValue)
        g.DrawString(value, fValue, br, AmtRight - sz.Width, y)
    End Sub

    Private Sub DrawKV(g As Graphics, label As String, value As String,
                       fLabel As Font, fValue As Font, br As Brush, L As Single, y As Single)
        g.DrawString(label, fLabel, br, L, y)
        g.DrawString(value, fValue, br, L + 80, y)
    End Sub

    Private Sub PrintDocument1_PrintPage(sender As Object, e As PrintPageEventArgs) Handles PrintDocument1.PrintPage
        DrawReceipt(e.Graphics, e.MarginBounds.Left, e.MarginBounds.Right)
        e.HasMorePages = False
    End Sub

    Private Sub btnPrint_Click(sender As Object, e As EventArgs) Handles btnPrint.Click
        Try
            PrintDocument1.DefaultPageSettings.PaperSize = New PaperSize("Receipt80mm", 315, 1200)
            PrintDocument1.DefaultPageSettings.Margins = New Margins(8, 8, 8, 8)
            PrintDialog1.Document = PrintDocument1
            PrintDialog1.UseEXDialog = True

            If PrintDialog1.ShowDialog() = DialogResult.OK Then
                PrintDocument1.Print()
                MessageBox.Show("Receipt sent to printer!", "Print",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show("Print failed: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnSavePDF_Click(sender As Object, e As EventArgs) Handles btnSavePDF.Click
        Try
            SaveFileDialog1.Filter = "PDF Files|*.pdf"
            SaveFileDialog1.FileName = "Receipt_" & _txnId & "_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".pdf"

            If SaveFileDialog1.ShowDialog() = DialogResult.OK Then
                PrintDocument1.DefaultPageSettings.PaperSize = New PaperSize("Receipt80mm", 315, 1200)
                PrintDocument1.DefaultPageSettings.Margins = New Margins(8, 8, 8, 8)
                PrintDocument1.PrinterSettings.PrinterName = "Microsoft Print to PDF"
                PrintDocument1.PrinterSettings.PrintToFile = True
                PrintDocument1.PrinterSettings.PrintFileName = SaveFileDialog1.FileName
                PrintDocument1.Print()
                MessageBox.Show("Receipt saved to:" & vbCrLf & SaveFileDialog1.FileName,
                                "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show("Save failed: " & ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private isDragging As Boolean = False
    Private dragPoint As Point

    Private Sub pnlHeader_MouseDown(sender As Object, e As MouseEventArgs) Handles pnlHeader.MouseDown, lblTitle.MouseDown
        isDragging = True
        dragPoint = e.Location
    End Sub

    Private Sub pnlHeader_MouseMove(sender As Object, e As MouseEventArgs) Handles pnlHeader.MouseMove, lblTitle.MouseMove
        If isDragging Then
            Me.Location = New Point(Me.Location.X + (e.X - dragPoint.X),
                                    Me.Location.Y + (e.Y - dragPoint.Y))
        End If
    End Sub

    Private Sub pnlHeader_MouseUp(sender As Object, e As MouseEventArgs) Handles pnlHeader.MouseUp, lblTitle.MouseUp
        isDragging = False
    End Sub

    Private Sub Guna2ControlBox1_Click(sender As Object, e As EventArgs) Handles exitbtn.Click
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub pnlReceiptHolder_Paint(sender As Object, e As PaintEventArgs) Handles pnlReceiptHolder.Paint

    End Sub
End Class