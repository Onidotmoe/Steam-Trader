Imports System.Collections.Concurrent
Imports System.Globalization
Imports System.Net
Imports System.Net.Http
Imports System.Web
Imports AngleSharp.Html.Dom
Imports AngleSharp.Html.Parser
Imports Newtonsoft.Json
Imports SteamTrader.GUI
Imports SteamTrader.MainWindow
Imports SteamTrader.Network.Inventory.Response
Imports U

Namespace Scraper

    Public Module Steam

        Public Async Function GetUserMarketListings() As Task(Of Tuple(Of List(Of GUI.Item), List(Of GUI.Item)))
            Dim Download_Url As Uri = New Uri("https://steamcommunity.com/market/mylistings?count=100&start=0")
            Dim Download_String As String = Nothing
            Dim MyListings As New MyListings
            Dim HTMLDocs As New ConcurrentBag(Of IHtmlDocument)

            Using Client As New HttpClient(New HttpClientHandler() With {.CookieContainer = MainWindow.Network.Cookie})
                Download_String = Await Client.GetStringAsync(Download_Url)
                MyListings = JSON.Deserialize(Of MyListings)(Download_String)
                HTMLDocs.Add((New HtmlParser).ParseDocument(HttpUtility.HtmlDecode(MyListings.results_html)))

                If MyListings.success Then
                    Dim Bag As New ConcurrentBag(Of MyListings)

                    If (MyListings.total_count > 100) Then
                        Dim LoopAmount As Integer = CInt(Math.Ceiling(MyListings.total_count / 100))

                        Parallel.For(1, LoopAmount, Sub(i)
                                                        Dim Local_Download_Url = New Uri("https://steamcommunity.com/market/mylistings?count=100&start=" + (i * 100).ToString)
                                                        Dim Local_Download_String = Client.GetStringAsync(Local_Download_Url).Result
                                                        Dim Local_MyListings = JSON.Deserialize(Of MyListings)(Local_Download_String)
                                                        Bag.Add(Local_MyListings)
                                                        HTMLDocs.Add((New HtmlParser).ParseDocument(HttpUtility.HtmlDecode(Local_MyListings.results_html)))
                                                    End Sub)
                    End If

                    Parallel.ForEach(Bag,
                                     Sub(Level_0)
                                         Parallel.ForEach(Level_0.assets,
                                                          Sub(App_A)
                                                              Parallel.ForEach(MyListings.assets,
                                                                Sub(App_B, State)
                                                                    If (App_A.Key = App_B.Key) Then
                                                                        Parallel.ForEach(App_A.Value,
                                                                             Sub(ContextA)
                                                                                 For Each Context_B In App_B.Value
                                                                                     If (ContextA.Key = Context_B.Key) Then
                                                                                         For Each Asset In ContextA.Value
                                                                                             Context_B.Value.Add(Asset.Key, Asset.Value)
                                                                                         Next
                                                                                         Exit For
                                                                                     End If
                                                                                 Next
                                                                             End Sub)
                                                                        State.Break()
                                                                    End If
                                                                End Sub)
                                                          End Sub)
                                     End Sub)
                Else
                    Status("FAILED : To acquire Steam listings and Orders.")
                    Return Nothing
                End If
            End Using

            Dim Orders As New ConcurrentBag(Of GUI.Item)
            Dim Listings As New ConcurrentBag(Of GUI.Item)

            Await Task.WhenAll(
            Task.Run(Sub()
                         Dim RAW_Orders = HTMLDocs(0).GetElementsByClassName("my_listing_section market_content_block market_home_listing_table")(1)
                         Parallel.ForEach(RAW_Orders.GetElementsByClassName("market_listing_row market_recent_listing_row"),
                                         Sub(Node)
                                             Dim Order As New GUI.Item

                                             With Order
                                                 If (Node.GetElementsByTagName("img").Count > 0) Then
                                                     Dim Image_Url = Node.GetElementsByTagName("img")(0).GetAttribute("src")
                                                     FindString(Image_Url, "https://steamcommunity-a.akamaihd.net/economy/image/", .ImageName, "/"c, True)
                                                 End If

                                                 Dim Price_1 = RemoveExtraText(Node.GetElementsByClassName("market_listing_right_cell market_listing_my_price market_listing_buyorder_qty")(0).GetElementsByClassName("market_listing_price")(0).TextContent)
                                                 Dim Price_2 = RemoveExtraText(Node.GetElementsByClassName("market_listing_right_cell market_listing_my_price")(0).GetElementsByClassName("market_listing_price")(0).TextContent)
                                                 .Price.OrderQuantity = Price_1
                                                 .Price.Order = ReplaceFirst(Price_2, Price_1, "")

                                                 Dim Name_Node = Node.GetElementsByClassName("market_listing_item_name_link")(0)
                                                 .MarketHashID = Name_Node.TextContent
                                                 .Name = .MarketHashID
                                                 .Price.Url_Steam = New Uri(Name_Node.GetAttribute("href"))
                                                 .WikiURL = .Price.Url_Steam
                                                 .AppName = Node.GetElementsByClassName("market_listing_item_name_block")(0).GetElementsByClassName("market_listing_game_name")(0).TextContent

                                                 Dim AppID As String = Nothing
                                                 FindString(.Price.Url_Steam.OriginalString, "steamcommunity.com/market/listings/", AppID, "/"c)
                                                 .AppID = CInt(AppID)

                                                 .Price.Listing_ID_Econ = RemoveExtraText(Node.GetElementsByClassName("item_market_action_button item_market_action_button_edit nodisable")(0).GetAttribute("href"))
                                             End With

                                             Orders.Add(Order)
                                         End Sub)
                     End Sub),
                    Task.Run(Sub()
                                 Parallel.ForEach(HTMLDocs, Sub(HTML)
                                                                Dim RAW_Listings = HTML.GetElementById("tabContentsMyActiveMarketListingsRows")
                                                                Parallel.ForEach(RAW_Listings.Children,
                                                                        Sub(Node)
                                                                            Dim Listing As New GUI.Item

                                                                            With Listing
                                                                                Dim Price_Node = Node.GetElementsByClassName("market_listing_right_cell market_listing_my_price")(0).GetElementsByClassName("market_listing_price")(0)
                                                                                Dim Price_Nodes = Price_Node.GetElementsByTagName("span")(0).GetElementsByTagName("span")
                                                                                .Price.My = RemoveExtraText(Price_Nodes(0).TextContent)
                                                                                Decimal.TryParse(RemoveExtraText(Price_Nodes(1).TextContent), .Price.SellAfterSteamTax)

                                                                                Dim TempDate As Date = Nothing
                                                                                Date.TryParseExact(Node.GetElementsByClassName("market_listing_right_cell market_listing_listed_date can_combine")(0).TextContent, "dMMM", CultureInfo.InvariantCulture, DateTimeStyles.None, TempDate)
                                                                                .Price.Listing_Date = TempDate.ToString

                                                                                Dim Name_Node = Node.GetElementsByClassName("market_listing_item_name_link")(0)
                                                                                .MarketHashID = Name_Node.TextContent
                                                                                .Name = .MarketHashID
                                                                                .Price.Url_Steam = New Uri(Node.GetElementsByClassName("market_listing_item_name_link")(0).GetAttribute("href"))
                                                                                .WikiURL = .Price.Url_Steam
                                                                                .AppName = Node.GetElementsByClassName("market_listing_item_name_block")(0).GetElementsByClassName("market_listing_game_name")(0).TextContent

                                                                                Dim ID_Data = Node.GetElementsByClassName("item_market_action_button item_market_action_button_edit nodisable")(0).GetAttribute("href")
                                                                                Dim Data As String() = ID_Data.Split(","c)
                                                                                .Price.Listing_ID_Econ = RemoveExtraText(Data(1), False)
                                                                                .AppID = CInt(RemoveExtraText(Data(2), False))
                                                                                .ContextID = CInt(RemoveExtraText(Data(3), False))
                                                                                .Price.Listing_ID_MarketListing = RemoveExtraText(Data(4), False)
                                                                            End With

                                                                            Listings.Add(Listing)
                                                                        End Sub)
                                                            End Sub)

                                 Parallel.ForEach(MyListings.assets,
                                                                 Sub(App)
                                                                     Dim Context = App.Value.FirstOrDefault(Function(F) F.Key = 2)

                                                                     If (Not Equals(Context, Nothing)) Then
                                                                         Parallel.ForEach(Context.Value,
                                                                                          Sub(Asset)
                                                                                              Dim Listing = Listings.FirstOrDefault(Function(F) (F.Price.Listing_ID_MarketListing = Asset.Key))

                                                                                              If (Listing IsNot Nothing) Then
                                                                                                  Dim Import As Asset_Details = Asset.Value

                                                                                                  With Listing
                                                                                                      .AssetID = Import.id
                                                                                                      .InstanceID = Import.instanceid
                                                                                                      .Tradable = CBool(Import.tradable)
                                                                                                      .Type = Import.type
                                                                                                      .ClassID = Import.classid
                                                                                                      .Price.Hold_Market = Import.market_marketable_restriction
                                                                                                      .Price.Hold_Trade = Import.market_tradable_restriction
                                                                                                      .MarketHashID = Import.market_hash_name
                                                                                                      .Name = Import.name
                                                                                                      .ImageName = Import.icon_url_large
                                                                                                      .Quality.Name = GetQualityName(Name:= .MarketHashID)
                                                                                                      .Quality.ID = CInt(GetQualityName(Name:= .Quality.Name, GiveIDInstead:=True))
                                                                                                  End With
                                                                                              End If
                                                                                          End Sub)
                                                                     End If
                                                                 End Sub)
                             End Sub))

            Return New Tuple(Of List(Of GUI.Item), List(Of GUI.Item))(Listings.ToList, Orders.ToList)
        End Function

        Public Async Function GetActualMarketListings_Steam(Item As GUI.Item) As Task(Of ObservableCollection(Of Archive.Steam))
            Dim MarketHashID As String = Item.MarketHashID
            Dim ExistingDownload = Archive_Steam.Where(Function(F) F.Key.Equals(MarketHashID)).FirstOrDefault

            If (ExistingDownload.Value IsNot Nothing) Then
                Return New ObservableCollection(Of Archive.Steam)(ExistingDownload.Value)
            End If

            Dim Download_Url As Uri = AssignUrl_Steam(MarketHashID)
            Dim Download_String As String = Nothing

            If MainWindow.Network.IsLoggedIn Then
                Using Client As New HttpClient(New HttpClientHandler() With {.CookieContainer = MainWindow.Network.Cookie})
                    Download_String = Await Client.GetStringAsync(Download_Url)
                End Using
            Else
                Using Client As New WebClient
                    Download_String = Client.DownloadString(Download_Url)
                End Using
            End If

            Dim Listings As New List(Of Archive.Steam)

            If (Not Download_String.Contains("""Item"":1")) Then
                Dim Parser As New HtmlParser
                Dim HTML As IHtmlDocument = Parser.ParseDocument(Download_String)
                Dim Prices As AngleSharp.Dom.IHtmlCollection(Of AngleSharp.Dom.IElement) = HTML.GetElementsByClassName("market_listing_price market_listing_price_with_fee")
                Dim IDs As AngleSharp.Dom.IHtmlCollection(Of AngleSharp.Dom.IElement) = HTML.GetElementsByClassName("market_listing_row market_recent_listing_row")

                For i As Integer = 0 To Prices.Count - 1
                    Dim New_Price As New Decimal
                    Decimal.TryParse(RemoveExtraText(Prices(i).TextContent), New_Price)
                    Listings.Add(New Archive.Steam With {.Price = New_Price, .ID = IDs(i).Id.Remove(0, "listing_".Length)})
                Next

                If (Binding_Lv_Listings.Count > 0) Then
                    Parallel.ForEach(Listings, Sub(Listing)
                                                   Dim iItem = Binding_Lv_Listings.FirstOrDefault(Function(F) (Listing.ID = F.Price.Listing_ID_Econ))

                                                   If (iItem IsNot Nothing) Then
                                                       Listing.My = True
                                                   End If
                                               End Sub)
                End If
            Else
                Download_Url = New Uri(String.Format("http://steamcommunity.com/market/priceoverview/?currency={0}&appid={1}&market_hash_name={2}", 3, MainWindow.Settings.Container.CurrentAppID, Web.HttpUtility.UrlEncode(Item.MarketHashID)))
                Download_String = Nothing

                Using Client As New WebClient
                    Try
                        Download_String = Client.DownloadString(Download_Url)
                    Catch ex As WebException
                        Download_String = Nothing
                    End Try
                End Using

                If (Download_String IsNot Nothing) Then
                    Dim SteamPrice As Network.Steam.Steam.SteamPriceOverview = JSON.Deserialize(Of Network.Steam.Steam.SteamPriceOverview)(Download_String)
                    Listings.Add(New Archive.Steam With {.Price = CDec(RemoveExtraText(SteamPrice.Lowest_price)), .Quantity = CInt(RemoveExtraText(SteamPrice.Volume, False))})
                End If
            End If

            ExistingDownload = Archive_Steam.Where(Function(F) F.Key.Equals(MarketHashID)).FirstOrDefault
            If (ExistingDownload.Key Is Nothing) Then
                Archive_Steam.Add(MarketHashID, Listings)
            End If

            Return New ObservableCollection(Of Archive.Steam)(Listings)
        End Function

        Private Class MyListings

            <JsonProperty("success")>
            Public Property success As Boolean

            <JsonProperty("pagesize")>
            Public Property pagesize As Integer

            <JsonProperty("total_count")>
            Public Property total_count As Integer

            ''' <summary>
            ''' Apps(of AppID, Dictionary(Of ContextID, Dictionary(Of AssetID, Asset_Details)))
            ''' </summary>
            <JsonProperty("assets")>
            Public Property assets As New Dictionary(Of Integer, Dictionary(Of Integer, Dictionary(Of String, Asset_Details)))

            <JsonProperty("start")>
            Public Property start As Integer

            <JsonProperty("num_active_listings")>
            Public Property num_active_listings As Integer

            <JsonProperty("hovers")>
            Public Property hovers As String

            <JsonProperty("results_html")>
            Public Property results_html As String

        End Class

        Public Async Sub Search(Input As String)
            Dim Download_Url As String = String.Format("http://steamcommunity.com/market/search/?l=english&cc=eu&country=dk&start=0&count=20&currency=3&appid={0}&q={1}", MainWindow.Settings.Container.CurrentAppID, Web.HttpUtility.UrlEncode(Input))
            Dim Download_String As String = Nothing

            If (MainWindow.Network.IsLoggedIn = True) Then
                Using Client As New HttpClient(New HttpClientHandler() With {.CookieContainer = MainWindow.Network.Cookie})
                    Download_String = Await Client.GetStringAsync(Download_Url)
                End Using
            Else
                Using Client As New WebClient
                    Download_String = Client.DownloadString(Download_Url)
                End Using
            End If

            Dim HTML As IHtmlDocument = (New HtmlParser).ParseDocument(Download_String)
            Dim SearchResults = HTML.GetElementsByClassName("market_listing_row market_recent_listing_row market_listing_searchresult")
            Dim Bag As New ConcurrentBag(Of GUI.Item)

            If SearchResults.None Then
                Status("FAILED : There were no Items matching your search. Try again with different keywords.")
                Exit Sub
            End If

            Parallel.ForEach(SearchResults, Sub(Result)
                                                Dim Item As New GUI.Item

                                                With Item
                                                    .Price.Steam_Listings = CInt(Result.GetElementsByClassName("market_listing_num_listings_qty")(0).TextContent)
                                                    .Price.Steam = CDec(RemoveExtraText(Result.GetElementsByClassName("market_table_value normal_price")(0).GetElementsByClassName("normal_price")(0).TextContent))
                                                    .MarketHashID = Result.GetElementsByClassName("market_listing_Item_name")(0).TextContent
                                                    .Name = .MarketHashID
                                                    Dim QualityName As String = GetQualityName(Name:= .MarketHashID)
                                                    .Quality.Name = If(.MarketHashID.Equals(QualityName), "Unique", QualityName)
                                                    .Quality.ID = CInt(GetQualityName(Name:= .Quality.Name, GiveIDInstead:=True))
                                                    .Price.Url_Backpack = AssignUrl_Backpack(.MarketHashID, .Quality.Name)
                                                    .Price.Url_Steam = AssignUrl_Steam(.MarketHashID)

                                                    Dim MarketID As String = RemoveQualityName(.MarketHashID)
                                                    Dim Slave = DownloadHandler.SlaveBag.FirstOrDefault(Function(F) F.Name.EndsWith(MarketID))

                                                    If (Slave IsNot Nothing) Then
                                                        .DefIndex = Slave.Defindex
                                                    End If
                                                End With

                                                Bag.Add(Item)
                                            End Sub)

            DownloadHandler.Start(New ObservableCollection(Of GUI.Item)(Bag))
            Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() Binding_Lv_Prices.ReplaceAll(Bag))
        End Sub

    End Module

    Public Module Backpack

        Public Function GetTradeToken(Partner As Partner) As Partner
            If String.IsNullOrWhiteSpace(Partner.TradeToken) Then
                Dim Download_Url = New Uri("https://backpack.tf/u/" + Partner.ID)
                Dim Download_String As String = Nothing

                Using Client As New WebClient
                    Try
                        Download_String = Client.DownloadString(Download_Url)
                    Catch ex As WebException
                        Status("FAILED : Could not download Trade Token, Partner : " + Partner.Name)
                        Return Nothing
                    End Try
                End Using

                Dim HTML As IHtmlDocument = (New HtmlParser).ParseDocument(Download_String)
                Dim User = HTML.GetElementsByClassName("title")(0).GetElementsByTagName("a")(0)

                If Equals(Partner.ID, User.GetAttribute("data-id")) Then
                    Partner.TradeUrl = User.GetAttribute("data-offers-params")
                    FindString(Partner.TradeUrl, "?Partner=", Partner.TradeID, "&"c)
                    FindString(Partner.TradeUrl, "&token=", Partner.TradeToken, Nothing)
                    Partner.TokenStatus = 1
                Else
                    Partner.TokenStatus = -1
                End If
            End If

            Return Partner
        End Function

#Region "Steam Market Listings Assets"

        Public Class Asset_App

            ''' <summary>
            ''' Apps(of AppID, Dictionary(Of ContextID, Dictionary(Of AssetID, Asset_Details)))
            ''' </summary>
            <JsonProperty("g_rgAssets")>
            Public Property Apps As New Dictionary(Of Integer, Dictionary(Of Integer, Dictionary(Of String, Asset_Details)))

        End Class

        Public Class Asset_Details

            <JsonProperty("currency")>
            Public Property currency As String

            <JsonProperty("appid")>
            Public Property appid As Integer

            <JsonProperty("contextid")>
            Public Property contextid As Integer

            <JsonProperty("id")>
            Public Property id As String

            <JsonProperty("classid")>
            Public Property classid As String

            <JsonProperty("instanceid")>
            Public Property instanceid As String

            <JsonProperty("amount")>
            Public Property amount As Integer

            <JsonProperty("status")>
            Public Property status As Integer

            <JsonProperty("original_amount")>
            Public Property original_amount As Integer

            <JsonProperty("background_color")>
            Public Property background_color As String

            <JsonProperty("icon_url")>
            Public Property icon_url As String

            <JsonProperty("icon_url_large")>
            Public Property icon_url_large As String

            <JsonProperty("descriptions")>
            Public Property descriptions As List(Of Description)

            <JsonProperty("actions")>
            Public Property Actions As List(Of Action)

            <JsonProperty("market_actions")>
            Public Property MarketActions As List(Of MarketAction)

            <JsonProperty("tradable")>
            Public Property tradable As Integer

            <JsonProperty("name")>
            Public Property name As String

            <JsonProperty("name_color")>
            Public Property name_color As String

            <JsonProperty("type")>
            Public Property type As String

            <JsonProperty("market_name")>
            Public Property market_name As String

            <JsonProperty("market_hash_name")>
            Public Property market_hash_name As String

            <JsonProperty("Item")>
            Public Property Item As Integer

            <JsonProperty("market_tradable_restriction")>
            Public Property market_tradable_restriction As Integer

            <JsonProperty("market_marketable_restriction")>
            Public Property market_marketable_restriction As Integer

            <JsonProperty("app_icon")>
            Public Property app_icon As String

            <JsonProperty("owner")>
            Public Property owner As Integer

        End Class

#End Region

    End Module

End Namespace
