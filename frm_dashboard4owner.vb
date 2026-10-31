Public Class frm_dashboard4owner
    Private Sub frm_dashboard4owner_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Chartms.Update()
        charttsi.Update()
        AddTableColumns()
        LoadDummyTable()

    End Sub

    Private Sub AddTableColumns()
        dgvTransactions.Columns.Clear()

        dgvTransactions.Columns.Add("TransactionID", "Transaction ID")
        dgvTransactions.Columns.Add("Date", "Date")
        dgvTransactions.Columns.Add("Time", "Time")
        dgvTransactions.Columns.Add("Cashier", "Cashier")
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
        dgvTransactions.Columns("Cashier").Width = 100
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

        ' === 20 STORE-WIDE TRANSACTIONS (All Cashiers) ===

        ' --- MARIA (5 transactions) ---
        dgvTransactions.Rows.Add("TXN-001", "10/13/26", "08:00", "Maria", "Cheesy Hungarian", "₱70", 1, "Cash", "₱70.00", "₱1.40", "₱71.40", "₱28.60")
        dgvTransactions.Rows.Add("TXN-002", "10/13/26", "08:15", "Maria", "Papa Cheesy Burger", "₱50", 2, "GCash", "₱100.00", "₱2.00", "₱102.00", "₱0.00")
        dgvTransactions.Rows.Add("TXN-003", "10/13/26", "08:30", "Maria", "Coke Royal", "₱25", 3, "Maya", "₱75.00", "₱1.50", "₱76.50", "₱23.50")
        dgvTransactions.Rows.Add("TXN-004", "10/13/26", "09:00", "Maria", "Papa Beef Burger", "₱40", 2, "Cash", "₱80.00", "₱1.60", "₱81.60", "₱18.40")
        dgvTransactions.Rows.Add("TXN-005", "10/13/26", "09:15", "Maria", "Papa Fries", "₱40", 1, "GCash", "₱40.00", "₱0.80", "₱40.80", "₱0.00")

        ' --- PEDRO (5 transactions) ---
        dgvTransactions.Rows.Add("TXN-006", "10/13/26", "08:05", "Pedro", "Cheesy Footlong", "₱56", 1, "Cash", "₱56.00", "₱1.12", "₱57.12", "₱42.88")
        dgvTransactions.Rows.Add("TXN-007", "10/13/26", "08:45", "Pedro", "Papa Egg Sandwich", "₱25", 2, "GCash", "₱50.00", "₱1.00", "₱51.00", "₱0.00")
        dgvTransactions.Rows.Add("TXN-008", "10/13/26", "09:10", "Pedro", "Sprite", "₱25", 1, "Maya", "₱25.00", "₱0.50", "₱25.50", "₱0.00")
        dgvTransactions.Rows.Add("TXN-009", "10/13/26", "09:30", "Pedro", "Cheesy Hotdog", "₱47", 1, "Cash", "₱47.00", "₱0.94", "₱47.94", "₱52.06")
        dgvTransactions.Rows.Add("TXN-010", "10/13/26", "09:45", "Pedro", "Bottled Water", "₱15", 4, "GCash", "₱60.00", "₱1.20", "₱61.20", "₱0.00")

        ' --- ANA (5 transactions) ---
        dgvTransactions.Rows.Add("TXN-011", "10/13/26", "08:10", "Ana", "Cheesy Hungarian", "₱70", 1, "Maya", "₱70.00", "₱1.40", "₱71.40", "₱0.00")
        dgvTransactions.Rows.Add("TXN-012", "10/13/26", "09:20", "Ana", "Papa Cheesy Burger", "₱50", 1, "Cash", "₱50.00", "₱1.00", "₱51.00", "₱49.00")
        dgvTransactions.Rows.Add("TXN-013", "10/13/26", "09:50", "Ana", "Coke Royal", "₱25", 2, "GCash", "₱50.00", "₱1.00", "₱51.00", "₱0.00")
        dgvTransactions.Rows.Add("TXN-014", "10/13/26", "10:15", "Ana", "Cheesy Footlong", "₱56", 1, "Cash", "₱56.00", "₱1.12", "₱57.12", "₱42.88")
        dgvTransactions.Rows.Add("TXN-015", "10/13/26", "10:30", "Ana", "Papa Beef Burger", "₱40", 1, "Maya", "₱40.00", "₱0.80", "₱40.80", "₱0.00")

        ' --- JUAN (5 transactions) ---
        dgvTransactions.Rows.Add("TXN-016", "10/13/26", "08:20", "Juan", "Papa Fries", "₱40", 2, "Cash", "₱80.00", "₱1.60", "₱81.60", "₱18.40")
        dgvTransactions.Rows.Add("TXN-017", "10/13/26", "10:00", "Juan", "Papa Egg Sandwich", "₱25", 1, "GCash", "₱25.00", "₱0.50", "₱25.50", "₱0.00")
        dgvTransactions.Rows.Add("TXN-018", "10/13/26", "10:45", "Juan", "Sprite", "₱25", 2, "Maya", "₱50.00", "₱1.00", "₱51.00", "₱0.00")
        dgvTransactions.Rows.Add("TXN-019", "10/13/26", "11:00", "Juan", "Cheesy Hotdog", "₱47", 1, "Cash", "₱47.00", "₱0.94", "₱47.94", "₱52.06")
        dgvTransactions.Rows.Add("TXN-020", "10/13/26", "11:15", "Juan", "Bottled Water", "₱15", 2, "GCash", "₱30.00", "₱0.60", "₱30.60", "₱0.00")

        ' Color-code rows by cashier
        For Each row As DataGridViewRow In dgvTransactions.Rows
            Dim cashier As String = row.Cells("Cashier").Value.ToString()
            Select Case cashier
                Case "Maria"
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 220)   ' Light Yellow
                Case "Pedro"
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 240, 255)   ' Light Blue
                Case "Ana"
                    row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230)   ' Light Green
                Case "Juan"
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230)   ' Light Pink
            End Select
        Next
    End Sub


End Class