Imports SteamTrader.GUI
Imports SteamTrader.MainWindow
Imports SteamTrader.TeamFortress2

Namespace Network.Backpack.API

    Public Class Listing
        Inherits NotifyPropertyChanged

        Public Parent As GUI.Item

        Sub New(Optional Parent As GUI.Item = Nothing)
            If (Parent IsNot Nothing) Then
                Me.Parent = Parent
            End If
        End Sub

        Private _IsEnabled As Boolean = True

        Public Property IsEnabled As Boolean
            Get
                Return _IsEnabled
            End Get
            Set
                NotifyPropertyChanged(_IsEnabled, Value)
            End Set
        End Property

        Private _OfferState As Integer

        ''' <summary>
        ''' -2 Insufficient Funds, -1 Failed, 0 Haven't tried, 1 Success
        ''' </summary>
        Public Property OfferState As Integer
            Get
                Return _OfferState
            End Get
            Set
                NotifyPropertyChanged(_OfferState, Value)
            End Set
        End Property

        Private _InInventory As Integer

        ''' <summary>
        ''' -3 Partner doesn't have enough funds for 1 Item, -2 Failed to acquire inventory, -1 Doesn't exist, 0 Haven't tried, 1 Exist, 2 Exist with warning, 3 Multiple exist, 4 Multiple exist with warnings(renames, stranges with kills, etc.)
        ''' </summary>
        Public Property InInventory As Integer
            Get
                Return _InInventory
            End Get
            Set
                NotifyPropertyChanged(_InInventory, Value)
            End Set
        End Property

        Private _ItemsForSale As New List(Of GUI.Item)

        Public Property ItemsForSale As List(Of GUI.Item)
            Get
                Return _ItemsForSale
            End Get
            Set
                NotifyPropertyChanged(_ItemsForSale, Value)
            End Set
        End Property

        Private _ItemsWithWarnings As New List(Of GUI.Item)

        Public Property ItemsWithWarnings As List(Of GUI.Item)
            Get
                Return _ItemsWithWarnings
            End Get
            Set
                NotifyPropertyChanged(_ItemsWithWarnings, Value)
            End Set
        End Property

        Private _DontBuyWarnings As Boolean

        Public Property DontBuyWarnings As Boolean
            Get
                Return _DontBuyWarnings
            End Get
            Set
                If (_AmountBuy > (_Amount - _AmountWithWarnings)) Then
                    Task.Run(Sub() AmountBuy = (_Amount - _AmountWithWarnings))
                End If

                NotifyPropertyChanged(_DontBuyWarnings, Value)
            End Set
        End Property

        Private _BuyOrSell As New Boolean

        ''' <summary>
        ''' False = Buy, True = Sell
        ''' </summary>
        Public Property BuyOrSell As Boolean
            Get
                Return _BuyOrSell
            End Get
            Set
                NotifyPropertyChanged(_BuyOrSell, Value)
            End Set
        End Property

        Private _IsSelected As Boolean

        Public Property IsSelected As Boolean
            Get
                Return _IsSelected
            End Get
            Set
                NotifyPropertyChanged(_IsSelected, Value)
                If _IsSelected Then
                    If (_Partner.TokenStatus = 0) Then
                        Task.Run(Sub() Scraper.GetTradeToken(_Partner))
                    End If

                    If ((_Escrow_Allow = 0) AndAlso (_Partner.EscrowState = 0) AndAlso MainWindow.Network.IsLoggedIn) Then
                        Task.Run(New Action(Async Sub()
                                                Dim EscrowBrokenState As Integer = Await (MainWindow.Network.CheckIfEscrowIfBroken(_Partner.TradeUrl))

                                                Select Case EscrowBrokenState
                                                    Case 0
                                                        _Partner.EscrowState = 1
                                                    Case 1
                                                        _IsSelected = False
                                                        _Escrow = True
                                                        _Escrow_Allow = 1
                                                        _Partner.EscrowState = 2
                                                    Case 2
                                                        _IsSelected = False
                                                        _IsEnabled = False
                                                End Select
                                            End Sub))

                    ElseIf (_Escrow_Allow = 1) Then
                        _Escrow_Allow = 2
                    End If

                    If (((Not _Escrow) OrElse (_Escrow_Allow = 2)) AndAlso (_InInventory = 0) AndAlso (_Partner.ProfileVisibility = 3)) Then
                        If (Not BuyOrSell) Then 'Buy Item
                            Task.Run(New System.Action(
                                 Async Sub()
                                     Dim CollectedInventory As ObservableCollection(Of GUI.Item) = (Await SteamTrader.Network.Inventory.Get(SteamID:=ConvertToSteamID64(Partner.ID)))

                                     If (CollectedInventory Is Nothing) Then
                                         _InInventory = -2
                                     Else
                                         _Inventory = CollectedInventory.ToList

                                         For Each Item In _Inventory
                                             If ((Equals(Parent.DefIndex, Item.DefIndex) OrElse Parent.DefIndex_Steam.Any(Function(F) F.Equals(Item.DefIndex))) AndAlso Equals(Parent.Quality.ID, Item.Quality.ID)) AndAlso Item.Tradable Then
                                                 _ItemsForSale.Add(Item)

                                                 If ((Not String.IsNullOrWhiteSpace(Item.Custom_Name)) OrElse (Not String.IsNullOrWhiteSpace(Item.Custom_Description)) OrElse ((Not String.IsNullOrWhiteSpace(Item.Name)) AndAlso (Not Equals(Item.Name, Parent.MarketHashID)))) Then
                                                     _ItemsWithWarnings.Add(Item)
                                                 End If
                                             End If
                                         Next

                                         _Amount = _ItemsForSale.Count
                                         _AmountWithWarnings = _ItemsWithWarnings.Count

                                         If (_Amount > 0) Then
                                             _InInventory = 1

                                             If MainWindow.Settings.Container.Always_Buy_Max_Amount Then
                                                 _AmountBuy = _Amount
                                             End If

                                             If (_AmountWithWarnings = 1) Then
                                                 _InInventory = 2
                                             End If

                                             If (_Amount > 1) Then
                                                 _InInventory = 3
                                             End If

                                             If (_AmountWithWarnings > 1) Then
                                                 _InInventory = 4
                                             End If

                                         ElseIf (Not (_InInventory = -2)) Then
                                             _InInventory = -1
                                         End If
                                     End If

                                     If (_InInventory < 0) Then
                                         _IsSelected = False
                                         _IsEnabled = False
                                     End If

                                     Update()
                                 End Sub))
                        Else 'Sell Item
                            Task.Run(New System.Action(
                                     Async Sub()
                                         If (_HasAmount = 0) Then
                                             Dim Listing As Listing = Parent.Price.CurrentMarketPrices.Backpack.Sell.FirstOrDefault

                                             If (Listing IsNot Nothing) AndAlso (Listing.HasAmount = 0) Then
                                                 For Each Item In Binding_Lv_Inventory
                                                     If (Item.DefIndex.Equals(Parent.DefIndex) AndAlso Item.Quality.ID.Equals(Parent.Quality.ID) AndAlso (Not Item.Locked) AndAlso (Not Item.InTransit) AndAlso Item.Tradable) Then
                                                         _HasAmount += 1
                                                     End If
                                                 Next

                                                 If (_HasAmount > 0) Then
                                                     For Each Sell As Listing In Parent.Price.CurrentMarketPrices.Backpack.Sell
                                                         Sell.HasAmount = _HasAmount
                                                     Next
                                                 Else
                                                     For Each Sell As Listing In Parent.Price.CurrentMarketPrices.Backpack.Sell
                                                         Sell.HasAmount = _HasAmount
                                                         Sell.IsSelected = False
                                                         Sell.IsEnabled = False
                                                     Next
                                                 End If
                                             End If
                                         End If

                                         If (_HasAmount > 0) Then
                                             Dim CollectedInventory = (Await SteamTrader.Network.Inventory.Get(SteamID:=ConvertToSteamID64(Partner.ID)))

                                             If (CollectedInventory Is Nothing) Then
                                                 _InInventory = -2
                                             Else
                                                 _Inventory = CollectedInventory.ToList

                                                 For Each Item In _Inventory
                                                     Select Case Item.DefIndex
                                                         Case DefIndex.Key
                                                             Price.Has.Key += 1

                                                         Case DefIndex.Refined
                                                             Price.Has.Refined += 1

                                                         Case DefIndex.Reclaimed
                                                             Price.Has.Reclaimed += 1

                                                         Case DefIndex.Scrap
                                                             Price.Has.Scrap += 1

                                                         Case Else
                                                             If ((Item.Type IsNot Nothing) AndAlso (Item.Type.Contains("weapon")) AndAlso (Item.Quality.ID = 6) AndAlso Item.UsableInCrafting AndAlso Item.Tradable) Then
                                                                 Price.Has.Weapon += 1
                                                             End If
                                                     End Select
                                                 Next

                                                 If (Price.Cost.Key > Price.Has.Key) OrElse (Price.Cost.Refined > Price.Has.Refined) OrElse (Price.Cost.Reclaimed > Price.Has.Reclaimed) OrElse (Price.Cost.Scrap > Price.Has.Scrap) Then
                                                     _InInventory = -3
                                                 End If
                                             End If
                                         End If

                                         If (_InInventory < 0) OrElse (_HasAmount = 0) Then
                                             _IsSelected = False
                                             _IsEnabled = False
                                         End If

                                         Update()
                                     End Sub))
                        End If
                    End If
                End If
            End Set
        End Property

        Private _Partner As New Partner

        Public Property Partner As Partner
            Get
                Return _Partner
            End Get
            Set
                NotifyPropertyChanged(_Partner, Value)
            End Set
        End Property

        Private _TradeOption As Integer

        ''' <summary>
        ''' -2 Added, -1 Wants to be added Buyout only, 0 Offer allowed, 1 Buyout only, 2 Automated, 3 Automated Buyout only
        ''' </summary>
        Public Property TradeOption As Integer
            Get
                Return _TradeOption
            End Get
            Set
                NotifyPropertyChanged(_TradeOption, Value)
            End Set
        End Property

        Public Property Price As New Price
        Public Property Paint As New Paint
        Public Property Description As String
        Public Property Listed As String
        Public Property Level As String
        Public Property Origin As String
        Public Property ID As String
        Private _HasAmount As Integer

        Public Property HasAmount As Integer
            Get
                Return _HasAmount
            End Get
            Set
                NotifyPropertyChanged(_HasAmount, Value)
            End Set
        End Property

        Private _Amount As Integer

        Public Property Amount As Integer
            Get
                Return _Amount
            End Get
            Set
                NotifyPropertyChanged(_Amount, Value)
            End Set
        End Property

        Private _AmountBuy As Integer = 1

        Public Property AmountBuy As Integer
            Get
                Return _AmountBuy
            End Get
            Set
                If (Not _BuyOrSell) Then
                    Value = If(Value > _Amount, _Amount, Value)
                    Value = If(Value < 1, 1, Value)
                    Value = If(((_DontBuyWarnings = True) AndAlso (Value > (_Amount - _AmountWithWarnings))), _Amount - _AmountWithWarnings, Value)
                Else
                    Value = If(Value < 1, 1, Value)
                    Value = If(Value > _HasAmount, _HasAmount, Value)
                    Value = If((Parent.Price.Cost * Value).Total > Parent.Price.Has.Total, CInt((Parent.Price.Cost.Total / Value) - (Parent.Price.Cost.Total Mod Value)), Value)
                End If

                NotifyPropertyChanged(_AmountBuy, Value)
            End Set
        End Property

        Private _AmountWithWarnings As Integer

        Public Property AmountWithWarnings As Integer
            Get
                Return _AmountWithWarnings
            End Get
            Set
                NotifyPropertyChanged(_AmountWithWarnings, Value)
            End Set
        End Property

        Public Property OrderAmount As Integer
        Public Property OrderPrice As Decimal
        Private _Escrow As Boolean

        Public Property Escrow As Boolean
            Get
                Return _Escrow
            End Get
            Set
                NotifyPropertyChanged(_Escrow, Value)
            End Set
        End Property

        Private _Escrow_Allow As Integer

        ''' <summary>
        '''  0 haven't checked, 1 has escrow, 2 user wants to trade anyways
        ''' </summary>
        Public Property Escrow_Allow As Integer
            Get
                Return _Escrow_Allow
            End Get
            Set
                NotifyPropertyChanged(_Escrow_Allow, Value)
            End Set
        End Property

        Private _Inventory As List(Of GUI.Item)

        Public Property Inventory As List(Of GUI.Item)
            Get
                Return _Inventory
            End Get
            Set
                NotifyPropertyChanged(_Inventory, Value)
            End Set
        End Property

    End Class

End Namespace
