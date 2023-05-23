Imports SteamTrader.GUI.Steam
Imports SteamTrader.Network.Steam.API

Namespace GUI

    Public Class BasicItem
        Property AppID As Integer
        Property AssetID As String
        Property ClassID As String
        Property InstanceID As String
        Property ContextID As Integer
        Property MarketHashID As String
        Property HasKills As Boolean
        Property Amount As Integer = 1
        Property IsVisible As Boolean = True
        Property IsMissing As Boolean
        Property DefIndex As Integer
        Property Quality As New Quality
        Property Level As String
        Property Name As String
        Property Type As String
        Property Description As String
        Property ImageSource As Uri
        Property ImageDownloadUrl As String
        Property ParticleEffect As IDName
        Property ImageMemory As BitmapSource
    End Class

End Namespace
