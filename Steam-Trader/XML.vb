Imports System.Collections.Concurrent
Imports System.Globalization
Imports System.IO
Imports SteamTrader.GUI
Imports SteamTrader.Network.Backpack.API
Imports SteamTrader.Network.Backpack.Response
Imports SteamTrader.Network.Steam.API
Imports SteamTrader.Settings
Imports U

Partial Class MainWindow

    Private Sub AssignUrls_MasterList()
        Parallel.ForEach(BindingMasterList,
                         Sub(List)
                             Parallel.ForEach(List,
                                              Sub(Item)
                                                  Item.Price.Url_Backpack = AssignUrl_Backpack(If(Item.MarketHashID, Item.Name), Item.Quality.Name, Item.Tradable, Item.Craftable)
                                                  Item.Price.Url_Steam = AssignUrl_Steam(If(Item.MarketHashID, Item.Name))
                                              End Sub)
                         End Sub)
    End Sub

    Private Sub AssignUrls(List As ObservableCollection(Of GUI.Item))
        Parallel.ForEach(List,
                         Sub(Item)
                             Item.Price.Url_Backpack = AssignUrl_Backpack(If(Item.MarketHashID, Item.Name), Item.Quality.Name, Item.Tradable, Item.Craftable)
                             Item.Price.Url_Steam = AssignUrl_Steam(If(Item.MarketHashID, Item.Name))
                         End Sub)
    End Sub

    Public Shared Function AssignUrl_Steam(Name As String) As Uri
        Return New Uri(String.Format("https://steamcommunity.com/market/listings/{0}/{1}/", MainWindow.Settings.Container.CurrentAppID, Name))
    End Function

    Public Shared Function AssignUrl_Backpack(Name As String, Optional Quality As String = "Unique", Optional Tradable As Boolean = True, Optional Craftable As Boolean = True) As Uri
        Return New Uri(String.Format("https://backpack.tf/stats/{0}/{1}/{2}/{3}", Quality, RemoveQualityName(Name, True), If(Tradable, 1, 0), If(Craftable, 1, 0)))
    End Function

    Public Shared Function AssignUrl_Wiki(Name As String) As Uri
        Return New Uri(String.Format("https://wiki.teamfortress.com/wiki/{0}", RemoveQualityName(Name)))
    End Function

    Public Function To_XML_Craftable(List_Craftable As SortedList(Of String, Value)) As List(Of Entry)
        Dim Craftable As New List(Of Entry)

        For Each ListValue In List_Craftable
            Dim NewValue As New Entry
            Dim Key As String = ListValue.Key

            With NewValue
                If Key.Contains("-"c) Then
                    Dim TempString As String() = Key.Split("-"c)
                    Dim Recipe As New Recipe With
                    {
                        .Output_ID = CInt(TempString(0)),
                        .Output_Quality = CInt(TempString(1))
                    }
                    .Recipe = Recipe

                ElseIf (CInt(Key) > 0) Then
                    Dim Particle As New IDName With {.ID = CInt(Key)}
                    Dim ParticleName = ParticleEffects.Where(Function(F) F.ID.Equals(Key)).FirstOrDefault
                    Particle.Name = If(ParticleName IsNot Nothing, ParticleName.Name, "")
                    .ParticleEffect = Particle
                End If

                .Currency = ListValue.Value.currency
                .Difference = ListValue.Value.difference
                .Last_update = ListValue.Value.last_update
                .Value = ListValue.Value.value.ToString
                .Value_high = ListValue.Value.value_high
            End With

            Craftable.Add(NewValue)
        Next

        Return Craftable
    End Function

    Public Sub AssignPrices()
        Dim Price_Path As String = Path.Combine(Home, MainWindow.Settings.Container.CurrentGameName, "Backpack.tf Community Prices.xml")
        Dim Price_Collection = U.XML.Read(Of Collection)(Price_Path)

        Exchange_Calculate()

        Parallel.ForEach(BindingMasterList,
                         Sub(List)
                             Parallel.ForEach(List,
                                              Sub(Item)
                                                  Dim ItemValue As New Entry
                                                  Dim Price = Price_Collection.Items.FirstOrDefault(Function(F) (Item.DefIndex.Equals(F.DefIndex) OrElse Item.DefIndex_Steam.Any(Function(F2) F2.Equals(F.DefIndex))))

                                                  If (Price IsNot Nothing) Then
                                                      Dim Quality = Price.Prices.FirstOrDefault(Function(F) Item.Quality.ID.Equals(F.ID))

                                                      If (Quality IsNot Nothing) Then
                                                          If Item.Tradable Then
                                                              If (Item.Craftable = True) Then
                                                                  ItemValue = Find_Craftable(Quality.Tradable.Craftable, Item)
                                                              Else
                                                                  ItemValue = Find_Craftable(Quality.Tradable.NonCraftable, Item)
                                                              End If

                                                              'Else
                                                              'Need a reliable way to get if an Item was obtained by purchase or not
                                                              'If (iItem.Craftable = True) Then
                                                              '    ItemValue = Find_Craftable(Quality.NonTradable.Craftable, iItem)
                                                              'Else
                                                              '    ItemValue = Find_Craftable(Quality.NonTradable.NonCraftable, iItem)
                                                              'End If
                                                          End If

                                                          Item.Price.Url_Backpack = AssignUrl_Backpack(Price.MarketHashID, Item.Quality.Name, Item.Tradable, Item.Craftable)
                                                      End If

                                                  End If

                                                  If ((ItemValue IsNot Nothing) AndAlso (ItemValue.Currency IsNot Nothing)) Then
                                                      With Item.Price
                                                          Select Case ItemValue.Currency.ToLower
                                                              Case "metal"
                                                                  .Backpack = Decimal.Round((Decimal.Parse(ItemValue.Value, CultureInfo.InvariantCulture) * MainWindow.Settings.Container.Exchange_ToMoney_Refined), 3)
                                                              Case "keys"
                                                                  .Backpack = Decimal.Round((Decimal.Parse(ItemValue.Value, CultureInfo.InvariantCulture) * MainWindow.Settings.Container.Exchange_ToMoney_Keys), 3)
                                                              Case "usd"
                                                                  .Backpack = (Decimal.Parse(ItemValue.Value, CultureInfo.InvariantCulture) * Settings.Container.CustomExchangeRate)
                                                              Case "hat"
                                                                  .Backpack = Decimal.Round((Decimal.Parse(ItemValue.Value, CultureInfo.InvariantCulture) * (CDec("1.33") * MainWindow.Settings.Container.Exchange_ToMoney_Refined)), 3)
                                                          End Select

                                                          Dim Difference As Decimal = Decimal.Round(Decimal.Parse(ItemValue.Difference, NumberStyles.Float, CultureInfo.InvariantCulture), 5)
                                                          .Backpack_Trend = Decimal.Round((Difference * MainWindow.Settings.Container.Exchange_ToMoney_Refined), 3)
                                                      End With
                                                  End If
                                              End Sub)
                         End Sub)
    End Sub

    Public Function Find_Craftable(List_Craftable As List(Of Entry), Item As GUI.Item) As Entry
        If (Item.Quality.ID = 5) Then
            For Each ListValue In List_Craftable
                If ((ListValue.ParticleEffect IsNot Nothing) AndAlso Equals(ListValue.ParticleEffect.ID, Item.Quality.ParticleEffect.ID)) Then
                    Return ListValue
                End If
            Next

            Return Nothing
        Else
            Return List_Craftable.FirstOrDefault
        End If
    End Function

    Public Function GenerateGUIItemList_Backpack() As ObservableCollection(Of GUI.Backpack.Price)
        Dim List As New ConcurrentBag(Of GUI.Backpack.Price)

        Dim Price_Collection As New Collection
        Dim Price_Path As String = Path.Combine(Home, MainWindow.Settings.Container.CurrentGameName, "Backpack.tf Community Prices.xml")

        Price_Collection = XML.Read(Of Collection)(Price_Path)

        MainWindow.Settings.Container.Backpack_Price_Key_InMetal = Price_Collection.KeyPriceInMetal
        MainWindow.Settings.Container.Backpack_Price_Refined = Decimal.Parse(Price_Collection.Raw_USD_Value, CultureInfo.InvariantCulture)

        Dim QualityList As New List(Of String)({NameOf(Backpack.Price.Unique), NameOf(Backpack.Price.Vintage), NameOf(Backpack.Price.Genuine), NameOf(Backpack.Price.Strange), NameOf(Backpack.Price.Haunted), NameOf(Backpack.Price.Collectors)})

        Parallel.ForEach(Price_Collection.Items, Sub(Item)
                                                     Dim NewItem As New Backpack.Price With
                                                     {
                                                         .DefIndex = Item.DefIndex,
                                                         .Name = Item.MarketHashID
                                                     }

                                                     Dim Item_TC As Backpack.Price = NewItem.Clone()

                                                     Dim Item_TNC As Backpack.Price = NewItem.Clone()
                                                     Item_TNC.Craftable = False
                                                     Item_TNC.UsableInCrafting = False

                                                     Dim Item_NTC As Backpack.Price = NewItem.Clone()
                                                     Item_NTC.Tradable = False

                                                     Dim Item_NTNC As Backpack.Price = NewItem.Clone()
                                                     Item_NTNC.Tradable = False
                                                     Item_NTNC.Craftable = False
                                                     Item_NTNC.UsableInCrafting = False

                                                     Parallel.ForEach(Item.Prices,
                                                                      Sub(Quality)
                                                                          If (Not Equals(Quality.ID, 5)) Then
                                                                              If (Quality.Tradable IsNot Nothing) Then
                                                                                  If (Quality.Tradable.Craftable IsNot Nothing) Then
                                                                                      Item_TC = SetQualityValues(Item_TC, Quality, Quality.Tradable.Craftable.FirstOrDefault, True, True)
                                                                                  End If

                                                                                  If (Quality.Tradable.NonCraftable IsNot Nothing) Then
                                                                                      Item_TNC = SetQualityValues(Item_TNC, Quality, Quality.Tradable.NonCraftable.FirstOrDefault, True, False)
                                                                                  End If
                                                                              End If

                                                                              If (Quality.NonTradable IsNot Nothing) Then
                                                                                  If (Quality.NonTradable.Craftable IsNot Nothing) Then
                                                                                      Item_NTC = SetQualityValues(Item_NTC, Quality, Quality.NonTradable.Craftable.FirstOrDefault, False, True)
                                                                                  End If

                                                                                  If (Quality.NonTradable.NonCraftable IsNot Nothing) Then
                                                                                      Item_NTNC = SetQualityValues(Item_NTNC, Quality, Quality.NonTradable.NonCraftable.FirstOrDefault, False, False)
                                                                                  End If
                                                                              End If
                                                                          End If
                                                                      End Sub)

                                                     For Each Price In New List(Of Backpack.Price)({Item_TC, Item_TNC, Item_NTC, Item_NTNC})
                                                         If (QualityList.FirstOrDefault(Function(F) GetPropertyValueByName(Price, F) IsNot Nothing) IsNot Nothing) Then
                                                             List.Add(Price)
                                                         End If
                                                     Next
                                                 End Sub)

        Return SetTypes(New ObservableCollection(Of Backpack.Price)(List))
    End Function

    Private Function SetQualityValues(NewItem As Backpack.Price, Quality As Network.Backpack.API.Quality, Item As Entry, Tradable As Boolean, Craftable As Boolean) As Backpack.Price
        If (Item Is Nothing) Then
            Return NewItem
        End If

        Dim NewQuality As New Backpack.Quality(NewItem)

        With NewQuality
            .ID = Quality.ID
            .Name = If(Not String.IsNullOrWhiteSpace(Quality.Name), Quality.Name, GetQualityName(.ID))
            .Original = Item.Value

            If (Item.Currency Is Nothing) Then
                .Original = Nothing
                .Money = Nothing
                .Refined = Nothing
            Else
                Select Case Item.Currency.ToLower
                    Case "metal"
                        .Original &= " ref"
                        .Money = Decimal.Round((Decimal.Parse(Item.Value, CultureInfo.InvariantCulture) * MainWindow.Settings.Container.Exchange_ToMoney_Refined), 2)
                        .Refined = Decimal.Round(Decimal.Parse(Item.Value, CultureInfo.InvariantCulture), 2)
                    Case "keys"
                        .Original &= " 🔑"
                        .Money = Decimal.Round((Decimal.Parse(Item.Value, CultureInfo.InvariantCulture) * MainWindow.Settings.Container.Exchange_ToMoney_Keys), 2)
                        .Refined = Decimal.Round(Decimal.Parse(Item.Value, CultureInfo.InvariantCulture) * MainWindow.Settings.Container.Backpack_Price_Key_InMetal, 2)
                    Case "usd"
                        .Original = "$" + .Original
                        .Money = ((Decimal.Parse(Item.Value, CultureInfo.InvariantCulture)) * MainWindow.Settings.Container.CustomExchangeRate)
                        .Refined = Decimal.Round(Decimal.Parse(Item.Value, CultureInfo.InvariantCulture) / MainWindow.Settings.Container.Exchange_ToMoney_Refined, 2)
                    Case "hat"
                        .Original &= " 🎩"
                        .Money = Nothing
                        .Refined = Nothing
                End Select
            End If

            .Trend = Decimal.Round(CDec(Item.Difference), 3)
            .Steam = NewQuality.Money
            .SteamTax = Decimal.Round(((.Money * (MainWindow.Settings.Container.Exchange_SteamTax / 100)) + NewQuality.Money), 2)

            If (.Trend > 0) Then
                .Direction = 1
            ElseIf (.Trend < 0) Then
                .Direction = -1
            Else
                .Direction = 0
            End If

            If (.ID <> 6) Then
                .Url_Steam = AssignUrl_Steam(.Name + " " + NewItem.Name)
            Else
                .Url_Steam = AssignUrl_Steam(NewItem.Name)
            End If

            .Url_Backpack = AssignUrl_Backpack(NewItem.Name, .Name, Tradable, Craftable)
        End With

        Select Case NewQuality.ID
            Case 6
                NewItem.Unique = NewQuality
            Case 3
                NewItem.Vintage = NewQuality
            Case 1
                NewItem.Genuine = NewQuality
            Case 11
                NewItem.Strange = NewQuality
            Case 13
                NewItem.Haunted = NewQuality
            Case 14
                NewItem.Collectors = NewQuality
        End Select

        Return NewItem
    End Function

    Private Function SetTypes(List As ObservableCollection(Of Backpack.Price)) As ObservableCollection(Of Backpack.Price)
        Dim SlaveBag As New ConcurrentBag(Of DownloadManager.Item_Url)(Schema.Schema.Load().Items)

        Parallel.ForEach(List, Sub(Item)
                                   Dim ItemUrl As DownloadManager.Item_Url = SlaveBag.FirstOrDefault(Function(F) F.Defindex.Equals(Item.DefIndex))

                                   If (ItemUrl IsNot Nothing) Then
                                       Item.Type = ItemUrl.Type
                                   End If
                               End Sub)
        Return List
    End Function

End Class
