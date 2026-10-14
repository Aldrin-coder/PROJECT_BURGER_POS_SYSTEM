Public Class ucdashboard4cashier
    Private Sub ucdashboard4cashier_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        chartsph.Update()
        chartpaymentm.Update()
        AddTableColumns()
        LoadDummyTable()

    End Sub
    Private Sub AddTableColumns()
        dgvTransactions.Columns.Clear()

        dgvTransactions.Columns.Add("TransactionID", "Transaction ID")
        dgvTransactions.Columns.Add("Date", "Date")
        dgvTransactions.Columns.Add("Time", "Time")
        dgvTransactions.Columns.Add("Product", "Product")
        dgvTransactions.Columns.Add("Price", "Price")
        dgvTransactions.Columns.Add("Qty", "Qty")
        dgvTransactions.Columns.Add("Payment", "Payment")
        dgvTransactions.Columns.Add("Subtotal", "Subtotal")
        dgvTransactions.Columns.Add("Tax", "Tax (2%)")
        dgvTransactions.Columns.Add("Total", "Total")
        dgvTransactions.Columns.Add("Change", "Change")

        dgvTransactions.Columns("TransactionID").Width = 110
        dgvTransactions.Columns("Date").Width = 80
        dgvTransactions.Columns("Time").Width = 60
        dgvTransactions.Columns("Product").Width = 180
        dgvTransactions.Columns("Price").Width = 70
        dgvTransactions.Columns("Qty").Width = 50
        dgvTransactions.Columns("Payment").Width = 80
        dgvTransactions.Columns("Subtotal").Width = 80
        dgvTransactions.Columns("Tax").Width = 70
        dgvTransactions.Columns("Total").Width = 80
        dgvTransactions.Columns("Change").Width = 80
    End Sub

    Private Sub LoadDummyTable()
        dgvTransactions.Rows.Clear()

        ' === 26 ORDERS — 45 ITEMS SOLD — ₱3,000 TOTAL ===

        ' --- MORNING (8AM-10AM) ---
        dgvTransactions.Rows.Add("TXN-001", "10/13/26", "08:00", "Cheesy Hungarian", "₱70", 1, "Cash", "₱70.00", "₱1.40", "₱71.40", "₱0.00")
        dgvTransactions.Rows.Add("TXN-002", "10/13/26", "08:15", "Papa Cheesy Burger", "₱50", 2, "GCash", "₱100.00", "₱2.00", "₱102.00", "₱0.00")
        dgvTransactions.Rows.Add("TXN-003", "10/13/26", "08:30", "Coke Royal", "₱25", 3, "Maya", "₱75.00", "₱1.50", "₱76.50", "₱23.50")
        dgvTransactions.Rows.Add("TXN-004", "10/13/26", "08:45", "Cheesy Footlong", "₱56", 1, "Cash", "₱56.00", "₱1.12", "₱57.12", "₱0.00")
        dgvTransactions.Rows.Add("TXN-005", "10/13/26", "09:00", "Papa Beef Burger", "₱40", 2, "GCash", "₱80.00", "₱1.60", "₱81.60", "₱18.40")
        dgvTransactions.Rows.Add("TXN-006", "10/13/26", "09:15", "Papa Fries", "₱40", 1, "Maya", "₱40.00", "₱0.80", "₱40.80", "₱0.00")
        dgvTransactions.Rows.Add("TXN-007", "10/13/26", "09:30", "Papa Egg Sandwich", "₱25", 2, "Cash", "₱50.00", "₱1.00", "₱51.00", "₱49.00")
        dgvTransactions.Rows.Add("TXN-008", "10/13/26", "09:45", "Sprite", "₱25", 1, "GCash", "₱25.00", "₱0.50", "₱25.50", "₱0.00")

        ' --- MID-MORNING (10AM-12PM) ---
        dgvTransactions.Rows.Add("TXN-009", "10/13/26", "10:00", "Cheesy Hotdog", "₱47", 1, "Maya", "₱47.00", "₱0.94", "₱47.94", "₱0.00")
        dgvTransactions.Rows.Add("TXN-010", "10/13/26", "10:15", "Bottled Water", "₱15", 4, "Cash", "₱60.00", "₱1.20", "₱61.20", "₱38.80")
        dgvTransactions.Rows.Add("TXN-011", "10/13/26", "10:30", "Cheesy Hungarian", "₱70", 1, "GCash", "₱70.00", "₱1.40", "₱71.40", "₱0.00")
        dgvTransactions.Rows.Add("TXN-012", "10/13/26", "10:45", "Papa Cheesy Burger", "₱50", 1, "Maya", "₱50.00", "₱1.00", "₱51.00", "₱0.00")
        dgvTransactions.Rows.Add("TXN-013", "10/13/26", "11:00", "Coke Royal", "₱25", 2, "Cash", "₱50.00", "₱1.00", "₱51.00", "₱49.00")
        dgvTransactions.Rows.Add("TXN-014", "10/13/26", "11:15", "Cheesy Footlong", "₱56", 1, "GCash", "₱56.00", "₱1.12", "₱57.12", "₱0.00")
        dgvTransactions.Rows.Add("TXN-015", "10/13/26", "11:30", "Papa Beef Burger", "₱40", 1, "Maya", "₱40.00", "₱0.80", "₱40.80", "₱0.00")
        dgvTransactions.Rows.Add("TXN-016", "10/13/26", "11:45", "Papa Fries", "₱40", 2, "Cash", "₱80.00", "₱1.60", "₱81.60", "₱18.40")

        ' --- LUNCH (12PM-2PM) ---
        dgvTransactions.Rows.Add("TXN-017", "10/13/26", "12:00", "Papa Egg Sandwich", "₱25", 1, "GCash", "₱25.00", "₱0.50", "₱25.50", "₱0.00")
        dgvTransactions.Rows.Add("TXN-018", "10/13/26", "12:15", "Sprite", "₱25", 2, "Maya", "₱50.00", "₱1.00", "₱51.00", "₱0.00")
        dgvTransactions.Rows.Add("TXN-019", "10/13/26", "12:30", "Cheesy Hotdog", "₱47", 1, "Cash", "₱47.00", "₱0.94", "₱47.94", "₱52.06")
        dgvTransactions.Rows.Add("TXN-020", "10/13/26", "12:45", "Bottled Water", "₱15", 2, "GCash", "₱30.00", "₱0.60", "₱30.60", "₱0.00")
        dgvTransactions.Rows.Add("TXN-021", "10/13/26", "1:00", "Cheesy Hungarian", "₱70", 1, "Maya", "₱70.00", "₱1.40", "₱71.40", "₱0.00")
        dgvTransactions.Rows.Add("TXN-022", "10/13/26", "1:15", "Papa Cheesy Burger", "₱50", 1, "Cash", "₱50.00", "₱1.00", "₱51.00", "₱49.00")
        dgvTransactions.Rows.Add("TXN-023", "10/13/26", "1:30", "Coke Royal", "₱25", 1, "GCash", "₱25.00", "₱0.50", "₱25.50", "₱0.00")
        dgvTransactions.Rows.Add("TXN-024", "10/13/26", "1:45", "Cheesy Footlong", "₱56", 1, "Maya", "₱56.00", "₱1.12", "₱57.12", "₱0.00")
        dgvTransactions.Rows.Add("TXN-025", "10/13/26", "2:00", "Papa Beef Burger", "₱40", 1, "Cash", "₱40.00", "₱0.80", "₱40.80", "₱9.20")
        dgvTransactions.Rows.Add("TXN-026", "10/13/26", "2:15", "Papa Fries", "₱40", 1, "GCash", "₱40.00", "₱0.80", "₱40.80", "₱0.00")


        For Each row As DataGridViewRow In dgvTransactions.Rows
            Dim payment As String = row.Cells("Payment").Value.ToString()
            Select Case payment
                Case "Cash"
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220)   ' Light Green
                Case "GCash"
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 240, 255)   ' Light Blue
                Case "Maya"
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 220)   ' Light Orange
            End Select
        Next
    End Sub

End Class
