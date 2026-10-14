Imports Guna.Charts.WinForms

Public Class ucdashboard4admin
    Private Sub ucdashboard4admin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ChartLowStock1.Update()
        Menucomposition.Update()
        AddTableColumns()
        LoadDummyTable()
    End Sub

    Private Sub AddTableColumns()
        dgvstock.Columns.Clear()

        dgvstock.Columns.Add("Item", "Item Name")
        dgvstock.Columns.Add("Price", "Price")
        dgvstock.Columns.Add("Stock", "Stock")
        dgvstock.Columns.Add("Status", "Status")
        dgvstock.Columns.Add("Category", "Category")

        dgvstock.Columns("Item").Width = 250
        dgvstock.Columns("Price").Width = 80
        dgvstock.Columns("Stock").Width = 80
        dgvstock.Columns("Status").Width = 120
        dgvstock.Columns("Category").Width = 120
    End Sub

    Private Sub LoadDummyTable()
        dgvstock.Rows.Clear()

        ' === BURGERS (2) ===
        dgvstock.Rows.Add("Papa Beef Burger", "₱40", 5, "Critical", "Burgers")
        dgvstock.Rows.Add("Papa Cheesy Burger", "₱50", 6, "Low", "Burgers")

        ' === HOTDOGS (3) ===
        dgvstock.Rows.Add("Papa Cheesy Hotdog", "₱47", 15, "Refill Soon", "Hotdogs")
        dgvstock.Rows.Add("Papa Cheesy Footlong", "₱56", 12, "Low", "Hotdogs")
        dgvstock.Rows.Add("Papa Cheesy Hungarian", "₱70", 3, "Critical", "Hotdogs")

        ' === SANDWICH (1) ===
        dgvstock.Rows.Add("Papa Egg Sandwich", "₱25", 10, "Low", "Sandwiches")

        ' === FRIES (1) ===
        dgvstock.Rows.Add("Papa Fries", "₱40", 12, "Low", "Fries")

        ' === ADD-ONS (4) ===
        dgvstock.Rows.Add("Papa Sauce", "₱5", 3, "Critical", "Add-ons")
        dgvstock.Rows.Add("Papa Cheese", "₱6", 8, "Low", "Add-ons")
        dgvstock.Rows.Add("Papa Patty", "₱10", 2, "Critical", "Add-ons")
        dgvstock.Rows.Add("Papa Egg", "₱15", 4, "Critical", "Add-ons")

        ' === DRINKS (5) ===
        dgvstock.Rows.Add("Coke Royal", "₱25", 30, "OK", "Drinks")
        dgvstock.Rows.Add("Sprite", "₱25", 28, "OK", "Drinks")
        dgvstock.Rows.Add("C2 Sting", "₱32", 22, "OK", "Drinks")
        dgvstock.Rows.Add("Vitamilk", "₱32", 25, "OK", "Drinks")
        dgvstock.Rows.Add("Bottled Water", "₱15", 18, "Refill Soon", "Drinks")

        ' Color-code rows by status
        For Each row As DataGridViewRow In dgvstock.Rows
            Dim status As String = row.Cells("Status").Value.ToString()
            Select Case status
                Case "Critical"
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220)
                Case "Low"
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 220)
                Case "Refill Soon"
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 220)
                Case "OK"
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220)
            End Select
        Next
    End Sub


End Class