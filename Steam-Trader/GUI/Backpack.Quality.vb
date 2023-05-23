Imports SteamTrader.Network.Backpack.API.API
Imports SteamTrader.Network.Steam.API

Namespace GUI.Backpack

    Public Class Quality
        Inherits NotifyPropertyChanged

        Private Parent As Price

        Sub New(Parent As Price)
            Me.Parent = Parent
        End Sub

        Private _IsSelected As Boolean

        Public Property IsSelected As Boolean
            Get
                Return _IsSelected
            End Get
            Set
                If ((Not SteamSuccess) AndAlso (Not SteamFailed) AndAlso (Not MainWindow.SelectionInProgress)) Then
                    Task.Run(Sub()
                                 Dim SteamPrice As New Decimal
                                 Dim Temp_Name As String = Nothing

                                 If (Not (ID = 6)) Then
                                     Temp_Name = (Name + " " + Parent.Name)
                                 Else
                                     Temp_Name = Parent.Name
                                 End If

                                 SteamPrice = GetPrice_Steam_Decimal(Temp_Name)

                                 If (SteamPrice > 0) Then
                                     SteamSuccess = True
                                     SteamTax = SteamPrice
                                     Steam = Decimal.Round(((SteamPrice / (MainWindow.Settings.Container.Exchange_SteamTax + 100)) * 100), 2)
                                 Else
                                     SteamFailed = True
                                 End If

                                 If (Status_Backpack = 0) Then
                                     Dim Fetched As New Item
                                     With Fetched
                                         .MarketHashID = Temp_Name
                                         .Quality.ID = ID
                                         .Tradable = Parent.Tradable
                                         .Craftable = Parent.Craftable
                                     End With

                                     GetAssetPrices_Singleton(Fetched)
                                     Status_Backpack = Fetched.Price.CurrentMarketPrices.Backpack.Success

                                     If (Status_Backpack = 1) Then
                                         Money = Decimal.Round(Fetched.Price.Backpack / MainWindow.Settings.Container.CustomExchangeRate, 2)
                                         Refined = Decimal.Round(Fetched.Price.Backpack / (MainWindow.Settings.Container.Backpack_Price_Refined * MainWindow.Settings.Container.CustomExchangeRate), 2)
                                         Average = Fetched.Price.Backpack_Average
                                     End If
                                 End If
                             End Sub)
                End If

                NotifyPropertyChanged(_IsSelected, Value)
            End Set
        End Property

        Public Property ID As Integer
        Public Property ClassID As Integer
        Private _Name As String
        Public Property Name As String
            Get
                Return _Name
            End Get
            Set
                DisplayName = GetQualityDisplayName(Value)
                NotifyPropertyChanged(_Name, Value)
            End Set
        End Property
        Private _DisplayName As String
        Public Property DisplayName As String
            Get
                Return _DisplayName
            End Get
            Set
                NotifyPropertyChanged(_DisplayName, Value)
            End Set
        End Property
        Public Property Trend As Decimal
        Public Property Direction As Integer
        Public Property Original As String
        Private _Refined As Decimal

        Public Property Refined As Decimal
            Get
                Return _Refined
            End Get
            Set
                NotifyPropertyChanged(_Refined, Value)

                Task.Run(Sub() Money = Decimal.Round((Value * MainWindow.Settings.Container.Exchange_ToMoney_Refined), 2))
            End Set
        End Property
        Private _Average As Decimal

        Public Property Average As Decimal
            Get
                Return _Average
            End Get
            Set
                NotifyPropertyChanged(_Average, Value)
            End Set
        End Property

        Private _Money As Decimal

        Public Property Money As Decimal
            Get
                Return _Money
            End Get
            Set
                NotifyPropertyChanged(_Money, Value)
            End Set
        End Property

        Private _Steam As Decimal

        Public Property Steam As Decimal
            Get
                Return _Steam
            End Get
            Set
                NotifyPropertyChanged(_Steam, Value)
            End Set
        End Property

        Private _SteamTax As Decimal

        Public Property SteamTax As Decimal
            Get
                Return _SteamTax
            End Get
            Set
                NotifyPropertyChanged(_SteamTax, Value)
            End Set
        End Property

        Private _SteamSuccess As Boolean

        Public Property SteamSuccess As Boolean
            Get
                Return _SteamSuccess
            End Get
            Set
                NotifyPropertyChanged(_SteamSuccess, Value)
            End Set
        End Property

        Private _SteamFailed As Boolean

        Public Property SteamFailed As Boolean
            Get
                Return _SteamFailed
            End Get
            Set
                NotifyPropertyChanged(_SteamFailed, Value)
            End Set
        End Property
        Private _Status_Backpack As Integer
        ''' <summary>
        ''' -1 Failed, 0 Haven't Tried, 1 Success
        ''' </summary>
        Public Property Status_Backpack As Integer
            Get
                Return _Status_Backpack
            End Get
            Set
                NotifyPropertyChanged(_Status_Backpack, Value)
            End Set
        End Property
        Public Property Url_Steam As Uri
        Public Property Url_Backpack As Uri

    End Class

End Namespace
