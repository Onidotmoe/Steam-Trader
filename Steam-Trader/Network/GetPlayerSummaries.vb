Imports System.Xml.Serialization

Namespace Network.Steam.API

    Public Module GetPlayerSummaries

        <XmlRoot("response")>
        Public Class PlayerSummaries

            <XmlArray("players"), XmlArrayItem("player")>
            Public Property Players As New List(Of Player)

        End Class

        <XmlRoot("player")>
        Public Class Player
            Public Property steamid As Long
            Public Property communityvisibilitystate As Integer
            Public Property profilestate As Integer
            Public Property personaname As String
            Public Property lastlogoff As String
            Public Property commentpermission As Integer
            Public Property profileurl As String
            Public Property avatar As String
            Public Property avatarmedium As String
            Public Property avatarfull As String
            Public Property personastate As Integer
            Public Property primaryclanid As String
            Public Property timecreated As String
            Public Property personastateflags As Integer
            Public Property gameextrainfo As String
            Public Property gameid As Integer
            Public Property loccountrycode As String
            Public Property locstatecode As String
            Public Property loccityid As Integer
        End Class

    End Module
End Namespace
