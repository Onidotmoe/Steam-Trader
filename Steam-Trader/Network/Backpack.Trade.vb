Imports SteamTrader.MainWindow
Imports SteamTrader.Network.Steam.Steam
Imports SteamTrader.TeamFortress2
Imports U

Namespace Network.Backpack.API
    Public Module Trade

        Sub Buy(Listing As Listing)
            If (Listing.Partner.TokenStatus = -1) Then
                Listing.OfferState = -1
                Exit Sub
            End If

            Dim Outgoing As New List(Of TradeItem)
            Dim Incoming As New List(Of TradeItem)

            Dim Cost As Metal = (Listing.Price.Cost * Listing.AmountBuy).Realize
            Dim FailSafe As New ObservableCollection(Of GUI.Item)

            For Each Item In Binding_Lv_Inventory
                Dim UseItem As New Boolean

                If (Not Item.InTransit) AndAlso (Not Item.Locked) Then
                    If ((Cost.Key > 0) AndAlso Item.DefIndex.Equals(DefIndex.Key) AndAlso Item.Tradable) Then
                        UseItem = True
                        Cost.Key -= 1

                    ElseIf ((Cost.Refined > 0) AndAlso Item.DefIndex.Equals(DefIndex.Refined)) Then
                        UseItem = True
                        Cost.Refined -= 1

                    ElseIf ((Cost.Reclaimed > 0) AndAlso Item.DefIndex.Equals(DefIndex.Reclaimed)) Then
                        UseItem = True
                        Cost.Reclaimed -= 1

                    ElseIf ((Cost.Scrap > 0) AndAlso Item.DefIndex.Equals(DefIndex.Scrap)) Then
                        UseItem = True
                        Cost.Scrap -= 1

                    ElseIf ((Cost.Weapon > 0) AndAlso (Item.MarkedForSell = True)) Then
                        UseItem = True
                        Cost.Weapon -= 1
                    End If
                End If

                If UseItem Then
                    Item.InTransit = True

                    Dim TradeItem As New TradeItem
                    With TradeItem
                        .AppID = Item.AppID
                        .ContextID = 2
                        .AssetID = Item.AssetID
                        .Amount = 1
                    End With

                    FailSafe.Add(Item)
                    Outgoing.Add(TradeItem)
                    UseItem = False

                    If Cost.IsEmpty Then
                        Exit For
                    End If
                End If
            Next

            Dim Amount As Integer = Listing.AmountBuy
            For Each Item In Listing.Inventory
                If Item.DefIndex.Equals(Listing.Parent.DefIndex) AndAlso Item.Quality.ID.Equals(Listing.Parent.Quality.ID) AndAlso Item.Tradable Then
                    Dim TradeItem As New TradeItem
                    With TradeItem
                        .AppID = Item.AppID
                        .ContextID = 2
                        .AssetID = Item.AssetID
                        .Amount = 1
                    End With

                    Incoming.Add(TradeItem)
                    Amount -= 1

                    If (Amount = 0) Then
                        Exit For
                    End If
                End If
            Next

            If (Cost.IsEmpty AndAlso (Amount = 0)) Then
                Dim Trade_Items As New TradeOffer
                Trade_Items.Outgoing.Items = Outgoing
                Trade_Items.Incoming.Items = Incoming
                Dim json_tradeoffer = JSON.Serialize(Trade_Items)
                Dim Url_Referer As String = String.Format("https://steamcommunity.com/tradeoffer/new/?partner={0}&token={1}", Listing.Partner.TradeID, Listing.Partner.TradeToken)
                Dim Data As String = String.Format("sessionid={0}&serverid={1}&partner={2}&tradeoffermessage={3}&json_tradeoffer={4}&trade_offer_create_params={5}", Network.GetSessionID(Network.Cookie), 1, Listing.Partner.ID, "", json_tradeoffer, "{""trade_offer_access_token"":""" + Listing.Partner.TradeToken + """}")
                Dim Result As String = Nothing
                MainWindow.Network.SendPostRequest(Data, "https://steamcommunity.com/tradeoffer/new/send", SendOfferReferer:=Url_Referer, Result:=Result)

                If ((Result IsNot Nothing) AndAlso Result.Contains("tradeofferid")) Then
                    Listing.OfferState = 1
                Else
                    Listing.OfferState = -1
                    Status("FAILED : TradeOffer Item : " + Listing.Parent.MarketHashID + ", Partner : " + Listing.Partner.Name)
                End If
            Else
                Listing.OfferState = -2
            End If

            If (Listing.OfferState < 0) Then
                MainWindow.Network.ActivateFailSafe(FailSafe)
            End If
        End Sub

        Sub Sell(Listing As Listing)
            If (Listing.Partner.TokenStatus = -1) Then
                Listing.OfferState = -1
                Exit Sub
            End If

            Dim Outgoing As New List(Of TradeItem)
            Dim Incoming As New List(Of TradeItem)

            Dim Cost As Metal = (Listing.Price.Cost * Listing.AmountBuy)
            Dim FailSafe As New ObservableCollection(Of GUI.Item)

            For Each Item In Listing.Inventory
                Dim UseItem As New Boolean
                If ((Cost.Key > 0) AndAlso Item.DefIndex.Equals(DefIndex.Key) And Item.Tradable) Then
                    UseItem = True
                    Cost.Key -= 1

                ElseIf ((Cost.Refined > 0) AndAlso Item.DefIndex.Equals(DefIndex.Refined)) Then
                    UseItem = True
                    Cost.Refined -= 1

                ElseIf ((Cost.Reclaimed > 0) AndAlso Item.DefIndex.Equals(DefIndex.Reclaimed)) Then
                    UseItem = True
                    Cost.Reclaimed -= 1

                ElseIf ((Cost.Scrap > 0) AndAlso Item.DefIndex.Equals(DefIndex.Scrap)) Then
                    UseItem = True
                    Cost.Scrap -= 1

                ElseIf ((Cost.Weapon > 0) AndAlso (Item.Type IsNot Nothing) AndAlso (Item.Type.Contains("weapon")) AndAlso (Item.Quality.ID = 6) AndAlso Item.UsableInCrafting AndAlso Item.Tradable) Then
                    UseItem = True
                    Cost.Weapon -= 1
                End If

                If (UseItem = True) Then
                    Dim TradeItem As New TradeItem
                    With TradeItem
                        .AppID = Item.AppID
                        .ContextID = 2
                        .AssetID = Item.AssetID
                        .Amount = 1
                    End With

                    FailSafe.Add(Item)
                    Incoming.Add(TradeItem)
                    UseItem = False

                    If (Cost.IsEmpty = True) Then
                        Exit For
                    End If
                End If
            Next

            Dim Amount As Integer = Listing.AmountBuy

            For Each Item In Binding_Lv_Inventory
                If Item.DefIndex.Equals(Listing.Parent.DefIndex) AndAlso Item.Quality.ID.Equals(Listing.Parent.Quality.ID) AndAlso Item.Tradable AndAlso (Not Item.Locked) AndAlso (Not Item.InTransit) Then
                    Item.InTransit = True

                    Dim TradeItem As New TradeItem
                    With TradeItem
                        .AppID = Item.AppID
                        .ContextID = 2
                        .AssetID = Item.AssetID
                        .Amount = 1
                    End With

                    Outgoing.Add(TradeItem)
                    Amount -= 1

                    If (Amount = 0) Then
                        Exit For
                    End If
                End If
            Next

            If (Cost.IsEmpty AndAlso (Amount = 0)) Then
                Dim Trade_Items As New TradeOffer
                Trade_Items.Outgoing.Items = Outgoing
                Trade_Items.Incoming.Items = Incoming
                Dim json_tradeoffer = JSON.Serialize(Trade_Items)
                Dim Url_Referer As String = String.Format("https://steamcommunity.com/tradeoffer/new/?partner={0}&token={1}", Listing.Partner.TradeID, Listing.Partner.TradeToken)
                Dim Data As String = String.Format("sessionid={0}&serverid={1}&partner={2}&tradeoffermessage={3}&json_tradeoffer={4}&trade_offer_create_params={5}", Network.GetSessionID(Network.Cookie), 1, Listing.Partner.ID, "", json_tradeoffer, "{""trade_offer_access_token"":""" + Listing.Partner.TradeToken + """}")
                Dim Result As String = Nothing
                MainWindow.Network.SendPostRequest(Data, "https://steamcommunity.com/tradeoffer/new/send", SendOfferReferer:=Url_Referer, Result:=Result)

                If ((Result IsNot Nothing) AndAlso Result.Contains("tradeofferid")) Then
                    Listing.OfferState = 1
                Else
                    Listing.OfferState = -1
                End If
            Else
                Listing.OfferState = -1
            End If
            If (Listing.OfferState = -1) Then
                MainWindow.Network.ActivateFailSafe(FailSafe)
            End If
        End Sub

    End Module

End Namespace
