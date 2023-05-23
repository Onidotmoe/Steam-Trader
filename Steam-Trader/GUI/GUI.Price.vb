Imports System.ComponentModel
Imports System.Xml.Serialization
Imports SteamTrader.TeamFortress2

Namespace GUI

    <Serializable>
    Public Class Price
        Inherits NotifyPropertyChanged

        Private _IsSelected As Boolean

        <XmlIgnore>
        Public Property IsSelected() As Boolean
            Get
                Return _IsSelected
            End Get
            Set
                NotifyPropertyChanged(_IsSelected, Value)
            End Set
        End Property

        Private _Steam As Decimal

        <XmlAttribute>
        <DefaultValue(0)>
        Public Property Steam() As Decimal
            Get
                Return _Steam
            End Get
            Set
                Value = CDec(Value.ToString("F"))
                NotifyPropertyChanged(_Steam, Value)
            End Set
        End Property

        Private _Steam_Average As Decimal

        <XmlAttribute>
        <DefaultValue(0)>
        Public Property Steam_Average() As Decimal
            Get
                Return _Steam_Average
            End Get
            Set
                Value = CDec(Value.ToString("F"))
                NotifyPropertyChanged(_Steam_Average, Value)
            End Set
        End Property

        Private _Steam_Listings As Integer

        <XmlAttribute>
        <DefaultValue(0)>
        Public Property Steam_Listings() As Integer
            Get
                Return _Steam_Listings
            End Get
            Set
                NotifyPropertyChanged(_Steam_Listings, Value)
            End Set
        End Property

        Private _Steam_Listings_My As Integer

        <XmlAttribute>
        <DefaultValue(0)>
        Public Property Steam_Listings_My() As Integer
            Get
                Return _Steam_Listings_My
            End Get
            Set
                NotifyPropertyChanged(_Steam_Listings_My, Value)
            End Set
        End Property

        <XmlAttribute>
        <DefaultValue(0)>
        Public Property Steam_Frequency As Decimal

        Private _Sell As Decimal

        <XmlIgnore>
        Public Property Sell As Decimal
            Get
                Return _Sell
            End Get
            Set
                Dim New_SellAfterSteamTax As Decimal = Math.Round(((Value / (MainWindow.Settings.Container.Exchange_SteamTax + 100)) * 100), 2)

                If (Not New_SellAfterSteamTax.Equals(SellAfterSteamTax)) Then
                    Task.Run(Sub()
                                 SellAfterSteamTax = New_SellAfterSteamTax
                                 UpdatePrice()
                             End Sub)
                End If

                If (Value < CDec("0.01")) Then
                    SellHasError = True
                ElseIf SellHasError Then
                    SellHasError = False
                End If

                Value = CDec(Value.ToString("0.00"))
                NotifyPropertyChanged(_Sell, Value)
            End Set
        End Property

        Private _SellAfterSteamTax As Decimal

        <XmlIgnore>
        Public Property SellAfterSteamTax As Decimal
            Get
                Return _SellAfterSteamTax
            End Get
            Set
                Dim New_Sell As Decimal = Math.Round(((Value * (MainWindow.Settings.Container.Exchange_SteamTax / 100)) + Value), 2)

                If (Not New_Sell.Equals(Sell)) Then
                    Task.Run(Sub()
                                 Sell = New_Sell
                                 UpdatePrice()
                             End Sub)
                End If

                Value = CDec(Value.ToString("0.00"))
                NotifyPropertyChanged(_SellAfterSteamTax, Value)
            End Set
        End Property

        Private _SellSuccess As Integer

        <XmlIgnore>
        Public Property SellSuccess() As Integer
            Get
                Return _SellSuccess
            End Get
            Set
                NotifyPropertyChanged(_SellSuccess, Value)
            End Set
        End Property

        Private _SellHasError As Boolean

        <XmlIgnore>
        Public Property SellHasError() As Boolean
            Get
                Return _SellHasError
            End Get
            Set
                NotifyPropertyChanged(_SellHasError, Value)
            End Set
        End Property

        Private _Profit As Decimal

        <XmlIgnore>
        Public Property Profit() As Decimal
            Get
                Return _Profit
            End Get
            Set
                NotifyPropertyChanged(_Profit, Value)
            End Set
        End Property

        Private _ProfitPercentage As Decimal

        <XmlIgnore>
        Public Property ProfitPercentage() As Decimal
            Get
                Return _ProfitPercentage
            End Get
            Set
                Value = CDec(Value.ToString("0.00"))
                NotifyPropertyChanged(_ProfitPercentage, Value)
            End Set
        End Property

        Private _ProfitEffectiveness As Decimal

        Public Property ProfitEffectiveness() As Decimal
            Get
                Return _ProfitEffectiveness
            End Get
            Set
                Value = CDec(Value.ToString("0.00"))
                NotifyPropertyChanged(_ProfitEffectiveness, Value)
            End Set
        End Property

        <XmlIgnore>
        Public Property My As String

        <XmlIgnore>
        Public Property Their As String

        <XmlIgnore>
        Public Property Their_InCash As String

        Private _Their_InRef As Decimal

        <XmlIgnore>
        Public Property Their_InRef As Decimal
            Get
                Return _Their_InRef
            End Get
            Set
                Value = CDec(Value.ToString("0.00"))
                Task.Run(Sub()
                             _Cost = FromRefined(Value)
                             Dim Price = Decimal.Round((Value * MainWindow.Settings.Container.Exchange_ToMoney_Refined), 2)
                             Profit = (SellAfterSteamTax - Price)
                             ProfitEffectiveness = If(Price = 0, 0, If(Profit = 0, 0, Decimal.Round((Profit / Price), 2)))
                         End Sub)

                NotifyPropertyChanged(_Their_InRef, Value)
            End Set
        End Property

        Private _Cost As New Metal

        <XmlIgnore>
        Public Property Cost As Metal
            Get
                Return _Cost
            End Get
            Set
                NotifyPropertyChanged(_Cost, Value)
            End Set
        End Property

        <XmlIgnore>
        Public Property Has As New Metal

        <XmlIgnore>
        Public Property Store As Decimal

        Private _RelistHasError As Boolean

        <XmlIgnore>
        Public Property RelistHasError() As Boolean
            Get
                Return _RelistHasError
            End Get
            Set
                NotifyPropertyChanged(_RelistHasError, Value)
            End Set
        End Property

        Private _RelistListingAt As Decimal

        <XmlIgnore>
        Public Property RelistListingAt As Decimal
            Get
                Return _RelistListingAt
            End Get
            Set
                Value = CDec(Value.ToString("F"))
                NotifyPropertyChanged(_RelistListingAt, Value)

                Dim SteamTax_If As Decimal = Decimal.Round(((Value * (MainWindow.Settings.Container.Exchange_SteamTax / 100)) + Value), 2)
                If (Not Equals(RelistListingAtAfterSteamTax, SteamTax_If)) Then
                    Task.Run(Sub()
                                 RelistListingAtAfterSteamTax = SteamTax_If
                                 RelistListingTotalCost = RelistAmount * SteamTax_If
                             End Sub)
                End If
            End Set
        End Property

        Private _RelistListingAtAfterSteamTax As Decimal

        <XmlIgnore>
        Public Property RelistListingAtAfterSteamTax As Decimal
            Get
                Return _RelistListingAtAfterSteamTax
            End Get
            Set
                Value = CDec(Value.ToString("F"))
                NotifyPropertyChanged(_RelistListingAtAfterSteamTax, Value)

                Dim Sell_If As Decimal = Decimal.Round(((Value / (MainWindow.Settings.Container.Exchange_SteamTax + 100)) * 100), 2)
                If (Not Equals(RelistListingAt, Sell_If)) Then
                    Task.Run(Sub()
                                 RelistListingAt = Sell_If
                                 RelistListingTotalCost = RelistAmount * Sell_If
                             End Sub)
                End If
            End Set
        End Property

        Private _RelistListingTotalCost As Decimal

        <XmlIgnore>
        Public Property RelistListingTotalCost As Decimal
            Get
                Return _RelistListingTotalCost
            End Get
            Set
                Value = CDec(Value.ToString("F"))
                NotifyPropertyChanged(_RelistListingTotalCost, Value)

                If (RelistListingTotalCost < CDec("0.03")) Then
                    Task.Run(Sub() RelistHasError = True)

                ElseIf (RelistHasError = True) Then
                    Task.Run(Sub() RelistHasError = False)
                End If
            End Set
        End Property

        Private _RelistOrderAt As Decimal

        <XmlIgnore>
        Public Property RelistOrderAt As Decimal
            Get
                Return _RelistOrderAt
            End Get
            Set
                Value = CDec(Value.ToString("F"))
                NotifyPropertyChanged(_RelistOrderAt, Value)

                Dim SteamTax_If As Decimal = Decimal.Round(((Value * (MainWindow.Settings.Container.Exchange_SteamTax / 100)) + Value), 2)
                If (Not Equals(RelistOrderAtAfterSteamTax, SteamTax_If)) Then
                    Task.Run(Sub()
                                 RelistOrderAtAfterSteamTax = SteamTax_If
                                 RelistOrderTotalCost = RelistAmount * SteamTax_If
                             End Sub)
                End If
            End Set
        End Property

        Private _RelistOrderAtAfterSteamTax As Decimal

        <XmlIgnore>
        Public Property RelistOrderAtAfterSteamTax As Decimal
            Get
                Return _RelistOrderAtAfterSteamTax
            End Get
            Set
                Value = CDec(Value.ToString("F"))
                NotifyPropertyChanged(_RelistOrderAtAfterSteamTax, Value)

                Dim Sell_If As Decimal = Decimal.Round(((Value / (MainWindow.Settings.Container.Exchange_SteamTax + 100)) * 100), 2)
                If (Not Equals(RelistOrderAt, Sell_If)) Then
                    Task.Run(Sub()
                                 RelistOrderAt = Sell_If
                                 RelistOrderTotalCost = RelistAmount * RelistOrderAtAfterSteamTax
                             End Sub)
                End If
            End Set
        End Property

        Private _RelistAmount As Integer = 1

        <XmlIgnore>
        Public Property RelistAmount As Integer
            Get
                Return _RelistAmount
            End Get
            Set
                If (Value = 0) Then
                    Value = 1
                End If

                NotifyPropertyChanged(_RelistAmount, Value)
                Task.Run(Sub() RelistOrderTotalCost = RelistAmount * RelistOrderAt)
            End Set
        End Property

        Private _RelistOrderTotalCost As Decimal

        <XmlIgnore>
        Public Property RelistOrderTotalCost As Decimal
            Get
                Return _RelistOrderTotalCost
            End Get
            Set
                Value = CDec(Value.ToString("F"))
                NotifyPropertyChanged(_RelistOrderTotalCost, Value)

                If ((RelistOrderTotalCost <= CDec("0.03")) AndAlso (RelistAmount > 1)) OrElse ((RelistOrderAt < CDec("0.03")) AndAlso
                    (RelistAmount = 1)) OrElse (RelistOrderTotalCost > CDec(MainWindow.Settings.Container.Balance)) OrElse ((RelistOrderTotalCost / RelistAmount) < CDec("0.03")) Then
                    Task.Run(Sub() RelistHasError = True)

                ElseIf (RelistHasError = True) Then
                    Task.Run(Sub() RelistHasError = False)
                End If
            End Set
        End Property

        Private _DealValuation As Integer

        <XmlIgnore>
        Public Property DealValuation As Integer '-1 = Bad deal, 0 = Neutral deal, 1 = Good deal, 2 = Great deal, 3 = Fantastic deal
            Get
                Return _DealValuation
            End Get
            Set
                NotifyPropertyChanged(_DealValuation, Value)
            End Set
        End Property

        <XmlAttribute>
        <DefaultValue(0)>
        Public Property ScrapTF As Decimal

        Private _Backpack As Decimal

        <XmlAttribute>
        <DefaultValue(0)>
        Public Property Backpack As Decimal
            Get
                Return _Backpack
            End Get
            Set
                Value = CDec(Value.ToString("F"))
                NotifyPropertyChanged(_Backpack, Value)
                Task.Run(Sub() UpdatePrice())
            End Set
        End Property

        Private _Backpack_Average As Decimal

        <XmlIgnore>
        Public Property Backpack_Average As Decimal
            Get
                Return _Backpack_Average
            End Get
            Set
                NotifyPropertyChanged(_Backpack_Average, Value)
            End Set
        End Property

        Private _Backpack_Trend As Decimal

        <XmlIgnore>
        Public Property Backpack_Trend As Decimal
            Get
                Return _Backpack_Trend
            End Get
            Set
                NotifyPropertyChanged(_Backpack_Trend, Value)
            End Set
        End Property

        Private _Order As String

        <XmlIgnore>
        Public Property Order As String
            Get
                Return _Order
            End Get
            Set
                NotifyPropertyChanged(_Order, Value)
                If (_Order IsNot Nothing) AndAlso (_OrderQuantity IsNot Nothing) Then
                    Task.Run(Sub() _OrderTotalCost = Decimal.Parse(RemoveExtraText(Value)) * CInt(_OrderQuantity))
                End If
            End Set
        End Property

        Private _OrderQuantity As String

        <XmlIgnore>
        Public Property OrderQuantity As String
            Get
                Return _OrderQuantity
            End Get
            Set
                NotifyPropertyChanged(_OrderQuantity, Value)
                If (_Order IsNot Nothing) AndAlso (_OrderQuantity IsNot Nothing) Then
                    Task.Run(Sub() _OrderTotalCost = Decimal.Parse(RemoveExtraText(_Order)) * CInt(Value))
                End If
            End Set
        End Property

        Private _OrderTotalCost As Decimal

        <XmlIgnore>
        Public Property OrderTotalCost As Decimal
            Get
                Return _OrderTotalCost
            End Get
            Set
                NotifyPropertyChanged(_OrderTotalCost, Value)
            End Set
        End Property

        Private _Listing_ID_Econ As String

        <XmlIgnore>
        Public Property Listing_ID_Econ As String
            Get
                Return _Listing_ID_Econ
            End Get
            Set
                NotifyPropertyChanged(_Listing_ID_Econ, Value)
            End Set
        End Property

        Private _Listing_ID_MarketListing As String

        <XmlIgnore>
        Public Property Listing_ID_MarketListing As String
            Get
                Return _Listing_ID_MarketListing
            End Get
            Set
                NotifyPropertyChanged(_Listing_ID_MarketListing, Value)
            End Set
        End Property

        Private _Listing_Date As String

        <XmlIgnore>
        Public Property Listing_Date As String
            Get
                Return _Listing_Date
            End Get
            Set
                NotifyPropertyChanged(_Listing_Date, Value)
            End Set
        End Property

        Private _Listing_Remove As String

        <XmlIgnore>
        Public Property Listing_Remove As String
            Get
                Return _Listing_Remove
            End Get
            Set
                NotifyPropertyChanged(_Listing_Remove, Value)
            End Set
        End Property

        <XmlIgnore>
        Public Property UnlockDate As String

        <XmlIgnore>
        Public Property Hold_Trade As Integer

        <XmlIgnore>
        Public Property Hold_Market As Integer

        Private _Url_Steam As Uri

        <XmlIgnore>
        Public Property Url_Steam As Uri
            Get
                Return _Url_Steam
            End Get
            Set
                NotifyPropertyChanged(_Url_Steam, Value)
            End Set
        End Property

        Private _Url_Backpack As Uri

        <XmlIgnore>
        Public Property Url_Backpack As Uri
            Get
                Return _Url_Backpack
            End Get
            Set
                NotifyPropertyChanged(_Url_Backpack, Value)
            End Set
        End Property

        Private _Url_TradeOffer As Uri

        <XmlIgnore>
        Public Property Url_TradeOffer As Uri
            Get
                Return _Url_TradeOffer
            End Get
            Set
                NotifyPropertyChanged(_Url_TradeOffer, Value)
            End Set
        End Property

        Private _CurrentMarketPrices As New Network.MarketPrices.MarketPrices

        <XmlIgnore>
        Public Property CurrentMarketPrices As Network.MarketPrices.MarketPrices
            Get
                Return _CurrentMarketPrices
            End Get
            Set
                NotifyPropertyChanged(_CurrentMarketPrices, Value)
                UpdatePrice()
            End Set
        End Property

        Public Sub UpdatePrice()
            If ((Backpack > 0) AndAlso (Steam > 0)) Then
                If (Sell = 0) Then
                    Sell = Steam
                End If

                Dim Price As Decimal = If(SellAfterSteamTax = 0, Steam, SellAfterSteamTax)
                Profit = If(SellAfterSteamTax = 0, (Price - Backpack), SellAfterSteamTax - Backpack)
                ProfitPercentage = CDec(((Profit * 100) / Price).ToString("F"))
                ProfitEffectiveness = If(Profit = 0, 0, Decimal.Round((Profit / Backpack), 2))

                Select Case ProfitPercentage
                    Case >= MainWindow.Settings.Container.FantasticDealProcentage
                        DealValuation = 3
                    Case >= MainWindow.Settings.Container.GreatDealProcentage
                        DealValuation = 2
                    Case >= MainWindow.Settings.Container.GoodDealProcentage
                        DealValuation = 1
                    Case >= 0
                        DealValuation = 0
                    Case <= MainWindow.Settings.Container.BadDealProcentage
                        DealValuation = -1
                End Select

            End If

            If CurrentMarketPrices.Backpack.Buy.Any Then
                For Each Listing In CurrentMarketPrices.Backpack.Buy
                    Dim Price = Decimal.Round((Listing.Price.Their_InRef * MainWindow.Settings.Container.Exchange_ToMoney_Refined), 2)
                    Listing.Price.SellAfterSteamTax = SellAfterSteamTax
                    Listing.Price.Profit = (Listing.Price.SellAfterSteamTax - Price)
                    Listing.Price.ProfitEffectiveness = If(Price = 0, 0, If(Listing.Price.Profit = 0, 0, Decimal.Round((Listing.Price.Profit / Price), 2)))
                Next
            End If
        End Sub

    End Class

End Namespace
