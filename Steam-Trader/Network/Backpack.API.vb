Imports System.Collections.Concurrent
Imports System.IO
Imports System.Net
Imports Newtonsoft.Json
Imports SteamTrader.MainWindow
Imports U

Namespace Network.Backpack.API
    Public Module API

        Sub GetEssentialPrices()
            Dim PricePath As String = IO.Path.Combine(Home, MainWindow.Settings.Container.CurrentGameName, "Backpack.tf Community Prices.xml")
            Dim Backpack_Prices As New Collection

            If IO.File.Exists(PricePath) Then
                Backpack_Prices = XML.Read(Of Collection)(PricePath)

                MainWindow.Settings.Container.Backpack_Price_Key_InMetal = Backpack_Prices.KeyPriceInMetal
                MainWindow.Settings.Container.Backpack_Price_Refined = CDec(Backpack_Prices.Raw_USD_Value)
                SteamTrader.Settings.Generator.Exchange_Calculate()
            Else
                MainWindow.Network.GetAndHandle_Backpack()
            End If
        End Sub

        Public Sub GetAssetPrices()
            Dim Uniques As New ConcurrentBag(Of GUI.Item)(Binding_Lv_Favorite.DistinctBy(Function(F) F.MarketHashID))

            Parallel.ForEach(Uniques, Sub(Unique)
                                          GetAssetPrices_Singleton(Unique)

                                          Dim Item = Binding_Lv_Favorite.FirstOrDefault(Function(F) F.MarketHashID.Equals(Unique.MarketHashID))

                                          If (Item IsNot Nothing) Then
                                              With Item.Price
                                                  .CurrentMarketPrices.Backpack.Buy = Unique.Price.CurrentMarketPrices.Backpack.Buy
                                                  .CurrentMarketPrices.Backpack.Sell = Unique.Price.CurrentMarketPrices.Backpack.Sell
                                                  .CurrentMarketPrices.Backpack.Success = Unique.Price.CurrentMarketPrices.Backpack.Success
                                                  .Backpack = Unique.Price.Backpack
                                                  .Backpack_Average = Unique.Price.Backpack_Average
                                              End With
                                          End If
                                      End Sub)

            Status("Backpack API Prices Successfully Downloaded!")
        End Sub

        Public Sub GetAssetPrices_Singleton(ByRef Item As GUI.Item)
            Dim Download_String As String = Nothing
            Dim ExistingDownload As Archive.Backpack = Nothing
            Archive_Backpack.TryGetValue(Item.MarketHashID, ExistingDownload)

            If (ExistingDownload IsNot Nothing) Then
                With Item.Price
                    .CurrentMarketPrices.Backpack.Buy = New ObservableCollection(Of Backpack.API.Listing)(ExistingDownload.Buy)
                    .CurrentMarketPrices.Backpack.Sell = New ObservableCollection(Of Backpack.API.Listing)(ExistingDownload.Sell)
                    .Backpack = ExistingDownload.Refined
                    .Backpack_Average = ExistingDownload.Average
                    .CurrentMarketPrices.Backpack.Success = 1
                End With
            Else
                Using Client As New WebClient
                    Try
                        Dim Download_Url = New Uri(String.Format("http://backpack.tf/api/classifieds/search/v1?key={0}&item={1}&quality={2}&fold={3}&item_names={4}&page_size={5}&killstreak_tier={6}&australium={7}&tradable={8}&craftable={9}",
                                                     MainWindow.Settings.Container.Backpack_APIKey,
                                                     RemoveQualityName(Item.MarketHashID, True),
                                                     Item.Quality.ID,
                                                     1, 1, 5, 0, -1, Item.Tradable, Item.Craftable))
                        Download_String = Client.DownloadString(Download_Url)
                    Catch ex As WebException
                        If (New StreamReader(ex.Response.GetResponseStream()).ReadToEnd()).Contains("This web API requires a Premium subscription.") Then
                            Status("FAILED : Backpack API Prices : Premium Subscription on Backpack.tf required : " + Item.MarketHashID)
                        Else
                            Status("FAILED : Backpack API Prices : " + Item.MarketHashID)
                        End If
                    End Try
                End Using

                If (Not String.IsNullOrWhiteSpace(Download_String)) Then
                    Dim Output = JSON.Deserialize(Of Output)(Download_String)

                    If String.IsNullOrWhiteSpace(Output.message) Then
                        With Item.Price.CurrentMarketPrices.Backpack
                            Dim Final_Buy = ToGUI_Listing(Output.buy, Item)
                            Dim Final_Sell = ToGUI_Listing(Output.sell, Item)

                            'Kept crashing
                            'If (Final_Buy.Count > 1) Then
                            '    Final_Buy.Sort(Function(f1, f2) f2.Price.Their_InCash.CompareTo(f1.Price.Their_InCash))
                            'End If
                            'If (Final_Sell.Count > 1) Then
                            '    Final_Sell.Sort(Function(f1, f2) f1.Price.Their_InCash.CompareTo(f2.Price.Their_InCash))
                            'End If

                            'Doesn't actually order correctly
                            Final_Buy.OrderBy(Function(F) F.Price.Their_InCash)
                            Final_Sell.OrderBy(Function(F) F.Price.Their_InCash)

                            .Sell = New ObservableCollection(Of Backpack.API.Listing)(Final_Buy)
                            .Buy = New ObservableCollection(Of Backpack.API.Listing)(Final_Sell)

                            If .Buy.Any Then
                                Item.Price.Backpack = CDec(.Buy.FirstOrDefault?.Price?.Their_InCash)
                                Item.Price.Backpack_Average = .Buy.Sum(Function(F) CDec(F.Price?.Their_InCash)) / .Buy.Count
                            End If

                            .Success = 1
                        End With

                        Dim ToArchive As New Archive.Backpack
                        With ToArchive
                            .Buy = Item.Price.CurrentMarketPrices.Backpack.Buy.ToList
                            .Sell = Item.Price.CurrentMarketPrices.Backpack.Sell.ToList
                            .Money = Decimal.Round(Item.Price.Backpack * MainWindow.Settings.Container.CustomExchangeRate, 2)
                            .Refined = Decimal.Round(Item.Price.Backpack / (MainWindow.Settings.Container.Backpack_Price_Refined * MainWindow.Settings.Container.CustomExchangeRate), 2)
                            .Average = Item.Price.Backpack_Average
                        End With

                        If (Not Archive_Backpack.ContainsKey(Item.MarketHashID)) Then
                            Archive_Backpack.Add(Item.MarketHashID, ToArchive)
                        End If
                    Else
                        Item.Price.CurrentMarketPrices.Backpack.Success = -1
                    End If
                Else
                    Item.Price.CurrentMarketPrices.Backpack.Success = -1
                End If
            End If
        End Sub

        Private Function ToGUI_Listing(Input As SteamTrader.Network.Backpack.API.Output.Details, Unique As GUI.Item) As List(Of Backpack.API.Listing)
            Dim Output As New List(Of Backpack.API.Listing)

            Parallel.ForEach(Input.listings, Sub(i)
                                                 Dim Listing As New Backpack.API.Listing(Unique)
                                                 With Listing
                                                     .Partner.ID = i.steamid
                                                     .Partner.TradeID = ConvertToSteamID32(i.steamid).ToString
                                                     Steam.API.API.GetPlayerSummaries(.Partner)
                                                     .Amount = i.count
                                                     .Description = i.details
                                                     .ID = i.Item.MarketHashID 'ID

                                                     Dim DateValue As Date = ConvertUnixToDate(i.created)
                                                     Dim DateToday As Date = Date.Today
                                                     Dim Span As TimeSpan = (DateValue - DateToday)
                                                     .Listed = Span.ToString("dd\:hh\:mm")

                                                     If (i.automatic > 0) Then
                                                         If (i.buyout > 0) Then
                                                             .TradeOption = 3
                                                         ElseIf (i.offers > 0) Then
                                                             .TradeOption = 2
                                                         End If
                                                     ElseIf (i.offers = 1) Then
                                                         If (i.buyout > 0) Then
                                                             .TradeOption = 1
                                                         ElseIf (i.offers > 0) Then
                                                             .TradeOption = 0
                                                         End If
                                                     ElseIf (i.offers = 2) Then
                                                         .TradeOption = -2
                                                         If (i.buyout > 0) Then
                                                             .TradeOption = -1
                                                         End If
                                                     End If

                                                     Dim MetalPrice As New Decimal
                                                     For Each Currency In i.currencies
                                                         Select Case Currency.Key
                                                             Case "metal"
                                                                 MetalPrice += CDec(Currency.Value)
                                                                 .Price.Their += " " + Currency.Value + " ref"
                                                             Case "keys"
                                                                 MetalPrice += CDec(Currency.Value) * MainWindow.Settings.Container.Backpack_Price_Key_InMetal
                                                                 .Price.Their += " " + Currency.Value + " key"
                                                             Case "hat"
                                                                 MetalPrice += CDec(Currency.Value) * CDec("1.33")
                                                                 .Price.Their += " " + Currency.Value + " hat"
                                                         End Select
                                                     Next

                                                     .Price.Their_InCash = Decimal.Round(MetalPrice * MainWindow.Settings.Container.Exchange_ToMoney_Refined, 2).ToString
                                                     .Price.Their_InRef = Decimal.Round(MetalPrice, 2)
                                                     .Price.Cost = FromRefined(.Price.Their_InRef)
                                                     .Price.Backpack = CDec(.Price.Their_InCash)
                                                 End With

                                                 Output.Add(Listing)
                                             End Sub)

            Return Output
        End Function

        Class Input

            <JsonProperty>
            Property key As String

            ''' <summary>
            ''' If set, adds a "name" property to each Item object.
            ''' </summary>
            <JsonProperty>
            Property Item_names As Integer = 1

            ''' <summary>
            ''' Valid options: sell, buy, dual
            ''' Default: dual
            ''' </summary>
            <JsonProperty>
            Property intent As String

            ''' <summary>
            ''' Modify the page size used to paginate. Must be inbetween 1 and 30.
            ''' Default: 10
            ''' </summary>
            ''' <returns></returns>
            <JsonProperty>
            Property page_size As Integer

            ''' <summary>
            ''' If set to 0, disables listing folding.
            ''' </summary>
            <JsonProperty>
            Property fold As Integer = 1

            ''' <summary>
            ''' Item name to search for.
            ''' </summary>
            <JsonProperty>
            Property Item As String

            ''' <summary>
            ''' Only show listings created by the user whose Steam ID Is passed.
            ''' </summary>
            <JsonProperty>
            Property steamid As String

            <JsonProperty>
            Property quality As Integer

        End Class

        Class Output

            ''' <summary>
            ''' Failure message
            ''' </summary>
            <JsonProperty>
            Property message As String

            <JsonProperty>
            Property total As Integer

            <JsonProperty>
            Property skip As Integer

            <JsonProperty>
            Property page_size As Integer

            <JsonProperty>
            Property buy() As Details

            <JsonProperty>
            Property sell() As Details

            Class Details

                ''' <summary>
                ''' Amount of listings matched by the query for this intent.
                ''' </summary>
                <JsonProperty>
                Property total As Integer

                <JsonProperty>
                Property listings As List(Of Listing)

                ''' <summary>
                ''' Whether any folded listings were present in this selection.
                ''' </summary>
                <JsonProperty>
                Property fold As Boolean

                Class Listing

                    ''' <summary>
                    ''' The listing's internal id. Guaranteed to be unique.
                    ''' </summary>
                    <JsonProperty>
                    Property id As String

                    <JsonProperty>
                    Property steamid As String

                    <JsonProperty>
                    Property Item As SteamTrader.Network.Backpack.API.Item

                    <JsonProperty>
                    Property appid As Integer

                    <JsonProperty>
                    Property currencies As Dictionary(Of String, String)

                    <JsonProperty>
                    Property offers As Integer

                    <JsonProperty>
                    Property buyout As Integer

                    <JsonProperty>
                    Property details As String

                    <JsonProperty>
                    Property created As String

                    <JsonProperty>
                    Property bump As String

                    ''' <summary>
                    ''' Either 0 (Buy) Or 1 (Sell).
                    ''' </summary>
                    <JsonProperty>
                    Property intent As Integer

                    <JsonProperty>
                    Property automatic As Integer

                    ''' <summary>
                    ''' If the listing would be folded, refers to how many Items this listing stacks.
                    ''' </summary>
                    <JsonProperty>
                    Property count As Integer

                    <JsonProperty>
                    Property promoted As Integer

                End Class

            End Class

        End Class

    End Module

End Namespace
