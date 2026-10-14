Imports System.Data.SqlClient

Module connect
    Public sqlconn As New SqlConnection
    Public cmd As New SqlCommand
    Public dr As SqlDataReader
    Public query As String



    Sub conn()
        Try
            If sqlconn.State = ConnectionState.Open Then sqlconn.Close()
            sqlconn.ConnectionString = "Server = ALDRIN\SQLEXPRESS; Database = DB_DARKSYSTEM; Trusted_Connection=True; MultipleActiveResultSets = True;"
            sqlconn.Open()

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub
End Module
