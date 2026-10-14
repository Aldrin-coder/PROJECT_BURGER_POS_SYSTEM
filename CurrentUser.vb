Module CurrentUser
    Public user_id As Integer = 0
    Public firstname As String = ""
    Public lastname As String = ""
    Public fullname As String = ""
    Public gender As String = ""
    Public role As String = ""
    Public role_code As String = ""
    Public shift = ""
    Public Sub Clear()
        user_id = 0
        firstname = ""
        lastname = ""
        fullname = ""
        gender = ""
        role = ""
        role_code = ""     '
        shift = ""
    End Sub
End Module
