Namespace Network.Steam.API

    Public Module GetAssetClassInfo

        Public Class ClassInfo_Result
            Public Property result As Dictionary(Of String, ClassInfo)
            Public Property success As Boolean
        End Class

        Public Class ClassInfo
            Public Property icon_url_large As String
            Public Property name As String
            Public Property market_hash_name As String
            Public Property type As String
            Public Property tradable As String
            Public Property marketable As String
            Public Property Item As String
            Public Property market_tradable_restriction As Integer
            Public Property market_marketable_restriction As Integer
            Public Property app_data As New app_data
            Public Property classid As String
            Public Property instanceid As String
        End Class

        Public Class app_data
            Public Property def_index As Integer
            Public Property quality As Integer
            Public Property slot As String
        End Class

    End Module
End Namespace
