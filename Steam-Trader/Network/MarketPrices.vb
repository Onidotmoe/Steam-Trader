Imports System.Xml.Serialization

Namespace Network.MarketPrices

    Public Class MarketPrices
        Inherits NotifyPropertyChanged
        Private _Steam As New Steam

        <XmlIgnore>
        Property Steam As Steam
            Get
                Return _Steam
            End Get
            Set
                NotifyPropertyChanged(_Steam, Value)
            End Set
        End Property

        Private _Backpack As New Backpack

        <XmlIgnore>
        Property Backpack As Backpack
            Get
                Return _Backpack
            End Get
            Set
                NotifyPropertyChanged(_Backpack, Value)
            End Set
        End Property

    End Class

    Public Class Steam
        Inherits NotifyPropertyChanged

        Private _Success As Integer

        ''' <summary>
        ''' -1 Failed, 0 Haven't tried, 1 Success, 2 Busy
        ''' </summary>
        <XmlIgnore>
        Property Success As Integer
            Get
                Return _Success
            End Get
            Set
                NotifyPropertyChanged(_Success, Value)
            End Set
        End Property

        <XmlIgnore>
        Property Buy As New ObservableCollection(Of Archive.Steam)

    End Class

    Public Class Backpack
        Inherits NotifyPropertyChanged

        Private _Success As Integer

        ''' <summary>
        ''' -1 Failed, 0 Haven't tried, 1 Success, 2 Busy
        ''' </summary>
        <XmlIgnore>
        Property Success As Integer
            Get
                Return _Success
            End Get
            Set
                NotifyPropertyChanged(_Success, Value)
            End Set
        End Property

        <XmlIgnore>
        Property Buy As New ObservableCollection(Of SteamTrader.Network.Backpack.API.Listing)

        <XmlIgnore>
        Property Sell As New ObservableCollection(Of SteamTrader.Network.Backpack.API.Listing)

    End Class

End Namespace
