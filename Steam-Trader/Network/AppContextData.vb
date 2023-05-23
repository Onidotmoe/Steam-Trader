Imports Newtonsoft.Json

Namespace Network.Steam

    Public Module AppContextData

        Public Class AppContextData

            <JsonProperty("g_rgAppContextData")>
            Public Property Apps As New Dictionary(Of Integer, AppDetails)

        End Class

        Public Class AppDetails

            <JsonProperty("appid")>
            Public Property appid As Integer

            <JsonProperty("name")>
            Public Property name As String

            <JsonProperty("icon")>
            Public Property icon As String

            <JsonProperty("link")>
            Public Property link As String

        End Class

    End Module
End Namespace
