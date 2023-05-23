Namespace Network.Steam.API

    Public Module GetAssetPrices

        Public Class GetAssetPrices_Class
            Property result As Result_Class

            Class Result_Class
                Property success As Boolean
                Property assets As List(Of Asset)
                Property tags As Dictionary(Of String, String)
                Property tag_ids As Dictionary(Of String, String)
            End Class

            Class Asset
                Property prices As Dictionary(Of String, String)
                Property name As String
                Property _date As String
                Property _class As ClassInfo
                Property classid As String
                Property tags As List(Of String)
                Property tag_ids As List(Of Long)
                Property original_prices As Dictionary(Of String, String)
            End Class

            Public Class ClassInfo
                Property name As Integer
                Property value As Integer
            End Class

        End Class

    End Module
End Namespace
