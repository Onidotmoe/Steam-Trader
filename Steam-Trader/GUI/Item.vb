Imports System.ComponentModel
Imports System.Xml.Serialization
Imports SteamTrader.GUI.Steam
Imports SteamTrader.Network.Steam.API
Imports SteamTrader.TeamFortress2

Namespace GUI

    <Serializable>
    Public Class Item
        Inherits NotifyPropertyChanged

        Public Function Clone() As Item
            Return DirectCast(MemberwiseClone(), Item)
        End Function

        <XmlAttribute>
        <DefaultValue("")>
        Property AssetID As String

        <XmlAttribute>
        <DefaultValue("")>
        Property ClassID As String

        <XmlIgnore>
        Property ContextID As Integer

        <XmlAttribute>
        <DefaultValue("0")>
        Property InstanceID As String

        Private _MarketHashID As String

        <XmlAttribute>
        <DefaultValue("")>
        Property MarketHashID As String
            Get
                Return _MarketHashID
            End Get
            Set
                NotifyPropertyChanged(_MarketHashID, Value)
            End Set
        End Property

        <XmlIgnore>
        Property Crate As Crate

        <XmlIgnore>
        Property ImageName As String

        Private _Name As String

        <XmlAttribute>
        <DefaultValue("")>
        Property Name As String
            Get
                Return _Name
            End Get
            Set
                NotifyPropertyChanged(_Name, Value)
            End Set
        End Property

        <XmlIgnore>
        Property Description As List(Of U.Pair(Of String, String))

        Private _Level As String

        <XmlAttribute>
        <DefaultValue("0")>
        Property Level As String
            Get
                Return _Level
            End Get
            Set
                Value = RemoveExtraText(Value)
                NotifyPropertyChanged(_Level, Value)
            End Set
        End Property

        Private _Index As Integer

        <XmlAttribute>
        <DefaultValue(0)>
        Property Index As Integer
            Get
                Return _Index
            End Get
            Set
                NotifyPropertyChanged(_Index, Value)
            End Set
        End Property

        Private _Custom_Name As String

        <XmlIgnore>
        Property Custom_Name As String
            Get
                Return _Custom_Name
            End Get
            Set
                NotifyPropertyChanged(_Custom_Name, Value)
            End Set
        End Property

        Private _Custom_Description As String

        <XmlIgnore>
        Property Custom_Description As String
            Get
                Return _Custom_Description
            End Get
            Set
                NotifyPropertyChanged(_Custom_Description, Value)
            End Set
        End Property

        Private _SteamObtained As Integer = -1

        ''' <summary>
        ''' -1 Untouched, 0 Ready to download, 1 Successful, 2 Failed.
        ''' </summary>
        <XmlIgnore>
        Property SteamObtained As Integer
            Get
                Return _SteamObtained
            End Get
            Set
                NotifyPropertyChanged(_SteamObtained, Value)
            End Set
        End Property

        Private _IsSelected As Boolean

        <XmlIgnore>
        Property IsSelected() As Boolean
            Get
                Return _IsSelected
            End Get
            Set
                If (SteamObtained = 0) AndAlso (Not MainWindow.SelectionInProgress) Then
                    Task.Run(Sub()
                                 Dim SteamPrice As Decimal = GetPrice_Steam_Decimal(MarketHashID)

                                 If (SteamPrice > 0) Then
                                     SteamObtained = 1
                                     Price.Steam = SteamPrice
                                     Price.UpdatePrice()
                                 Else
                                     SteamObtained = 2
                                 End If
                             End Sub)
                End If

                NotifyPropertyChanged(_IsSelected, Value)
            End Set
        End Property

        Private _IsExpanded As Boolean

        <XmlIgnore>
        Property IsExpanded() As Boolean
            Get
                Return _IsExpanded
            End Get
            Set
                NotifyPropertyChanged(_IsExpanded, Value)
            End Set
        End Property

        Private _Type As String

        <XmlIgnore>
        Property Type As String
            Get
                Return _Type
            End Get
            Set
                NotifyPropertyChanged(_Type, Value)
            End Set
        End Property

        Private _DefIndex As Integer

        <XmlAttribute>
        <DefaultValue(0)>
        Property DefIndex As Integer
            Get
                Return _DefIndex
            End Get
            Set
                NotifyPropertyChanged(_DefIndex, Value)
            End Set
        End Property

        <XmlArray>
        <XmlArrayItem(IsNullable:=False)>
        Private _DefIndex_Steam As New List(Of Integer)

        ''' <summary>
        ''' Steam's Trading System uses a different DefIndex on some Items than Backpack.tf or relative to itself.
        ''' This is a work around.
        ''' </summary>
        <XmlArray>
        Property DefIndex_Steam As List(Of Integer)
            Get
                Return _DefIndex_Steam
            End Get
            Set
                NotifyPropertyChanged(_DefIndex_Steam, Value)
            End Set
        End Property

        Private _MarkedForSell As Boolean

        <XmlIgnore>
        Property MarkedForSell As Boolean
            Get
                Return _MarkedForSell
            End Get
            Set
                NotifyPropertyChanged(_MarkedForSell, Value)
            End Set
        End Property

        Private _Marketable As Boolean

        <XmlIgnore>
        Property Marketable As Boolean
            Get
                Return _Marketable
            End Get
            Set
                NotifyPropertyChanged(_Marketable, Value)
            End Set
        End Property

        Private _Tradable As Boolean = True

        <XmlIgnore>
        Property Tradable As Boolean
            Get
                Return _Tradable
            End Get
            Set
                NotifyPropertyChanged(_Tradable, Value)
            End Set
        End Property

        Private _Achievement As Boolean

        <XmlIgnore>
        Property Achievement As Boolean
            Get
                Return _Achievement
            End Get
            Set
                NotifyPropertyChanged(_Achievement, Value)
            End Set
        End Property

        Private _Craftable As Boolean = True

        <XmlIgnore>
        Property Craftable As Boolean
            Get
                Return _Craftable
            End Get
            Set
                NotifyPropertyChanged(_Craftable, Value)
            End Set
        End Property

        Private _UsableInCrafting As Boolean = True

        <XmlIgnore>
        Property UsableInCrafting As Boolean
            Get
                Return _UsableInCrafting
            End Get
            Set
                NotifyPropertyChanged(_UsableInCrafting, Value)
            End Set
        End Property

        Private _Counter_Type As String

        <XmlAttribute>
        <DefaultValue("")>
        Property Counter_Type As String
            Get
                Return _Counter_Type
            End Get
            Set
                NotifyPropertyChanged(_Counter_Type, Value)
            End Set
        End Property

        Private _Counter As Integer

        <XmlAttribute>
        <DefaultValue(0)>
        Property Counter As Integer
            Get
                Return _Counter
            End Get
            Set
                NotifyPropertyChanged(_Counter, Value)
            End Set
        End Property

        Private _Locked As Boolean

        <XmlAttribute>
        <DefaultValue(False)>
        Property Locked As Boolean
            Get
                Return _Locked
            End Get
            Set
                NotifyPropertyChanged(_Locked, Value)
            End Set
        End Property

        Private _InTransit As Boolean

        <XmlIgnore>
        Property InTransit As Boolean
            Get
                Return _InTransit
            End Get
            Set
                NotifyPropertyChanged(_InTransit, Value)
            End Set
        End Property

        Private _AppID As Integer = MainWindow.Settings.Container.CurrentAppID

        <XmlIgnore>
        Property AppID As Integer
            Get
                Return _AppID
            End Get
            Set
                NotifyPropertyChanged(_AppID, Value)
            End Set
        End Property

        Private _AppName As String

        <XmlIgnore>
        Property AppName As String
            Get
                Return _AppName
            End Get
            Set
                NotifyPropertyChanged(_AppName, Value)
            End Set
        End Property

        Private _WikiURL As Uri

        <XmlIgnore>
        Property WikiURL As Uri
            Get
                Return _WikiURL
            End Get
            Set
                NotifyPropertyChanged(_WikiURL, Value)
            End Set
        End Property

        Private _ImageSource As Uri

        <XmlIgnore>
        Property ImageSource As Uri
            Get
                Return _ImageSource
            End Get
            Set
                NotifyPropertyChanged(_ImageSource, Value)
            End Set
        End Property
        Private _ImageMemory As BitmapSource
        <XmlIgnore>
        Property ImageMemory As BitmapSource
            Get
                Return _ImageMemory
            End Get
            Set
                NotifyPropertyChanged(_ImageMemory, Value)
            End Set
        End Property
        Private _HaveAmount As Integer

        <XmlAttribute>
        <DefaultValue(0)>
        Property HaveAmount As Integer
            Get
                Return _HaveAmount
            End Get
            Set
                NotifyPropertyChanged(_HaveAmount, Value)
            End Set
        End Property

        Private _WantAmount As Integer

        <XmlAttribute>
        <DefaultValue(0)>
        Property WantAmount As Integer
            Get
                Return _WantAmount
            End Get
            Set
                NotifyPropertyChanged(_WantAmount, Value)
            End Set
        End Property

        Private _GiftTo As String

        <XmlAttribute>
        <DefaultValue("")>
        Property GiftTo As String
            Get
                Return _GiftTo
            End Get
            Set
                NotifyPropertyChanged(_GiftTo, Value)
            End Set
        End Property

        <XmlElement>
        Property Price As New Price

        Private _Quality As New Quality

        <XmlElement>
        Property Quality As Quality
            Get
                Return _Quality
            End Get
            Set
                NotifyPropertyChanged(_Quality, Value)
            End Set
        End Property
        ''' <summary>
        ''' -2 Cancel Failed, -1 Cancel Successful
        ''' </summary>
        Private _Status As Integer
        <XmlIgnore>
        Property Status As Integer
            Get
                Return _Status
            End Get
            Set
                NotifyPropertyChanged(_Status, Value)
            End Set
        End Property
    End Class

End Namespace
