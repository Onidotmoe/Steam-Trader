Imports System.Collections.Concurrent
Imports System.IO
Imports System.Net
Imports SteamTrader.GUI
Imports SteamTrader.MainWindow
Imports SteamTrader.Network.Steam.API.Schema
Imports SteamTrader.TeamFortress2
Imports U

Namespace Network.Steam.API
    Public Module API

        Sub UpdateSchema()
            Dim Path = IO.Path.Combine(Home, MainWindow.Settings.Container.CurrentGameName)

            If IO.Directory.Exists(Path) Then
                Dim SchemaList As New List(Of String) From {"Schema.xml"}
                Dim Files As IO.FileInfo() = New IO.DirectoryInfo(Path).GetFiles()

                For Each File In SchemaList
                    Dim Info = Files.FirstOrDefault(Function(F) F.Name.Equals(File))
                    Info?.Delete()
                Next
            End If

            CreateSchemaList()
        End Sub

        Sub CreateSchemaList()
            Dim Schema_Path As String = IO.Path.Combine(Home, MainWindow.Settings.Container.CurrentGameName, "Schema.xml")

            If (Not IO.File.Exists(Schema_Path)) Then
                If (Not MainWindow.Network.IsLoggedIn) Then
                    Exit Sub
                End If

                DownloadSchema()
                CreateSchema()
            End If

            If IO.File.Exists(Schema_Path) Then
                Dim Schema = XML.Read(Of Schema.Schema)(Schema_Path)
                Schema.Qualities.AsParallel.ForAll(Sub(F) F.Name = Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(F.Name))

                Qualities = Schema.Qualities
                ParticleEffects = Schema.Particles
                Counters = Schema.Counters
                Paints = Schema.Paints

                MainWindow.MainWindow.SelectQualities = Schema.Qualities.Select(Function(F) F.Name).ToList
                MainWindow.MainWindow.SelectParticleEffects = Schema.Particles.Select(Function(F) F.Name).ToList
                MainWindow.MainWindow.SelectPaints = Schema.Paints.Select(Function(F) F.Name).ToList

                Status("Schema Successfully Updated")
            Else
                Status("Schema FAILED to Update")
            End If
        End Sub

        Public Sub DownloadSchema()
            Dim SaveDirectory As String = Path.Combine(MainWindow.Home, MainWindow.Settings.Container.CurrentGameName)

            IO.Directory.CreateDirectory(SaveDirectory)

            Dim Schema_Path_Items As String = Path.Combine(SaveDirectory, "Schema_Raw_Items_{0}.txt")
            Dim Schema_Path_Overview As String = Path.Combine(SaveDirectory, "Schema_Raw_Overview.txt")

            Dim SchemaItems_String As String = String.Empty
            Dim Start As New Integer

            Using Client As New WebClient
                While True
                    SchemaItems_String = Client.DownloadString(String.Format("https://api.steampowered.com/IEconItems_{0}/GetSchemaItems/v1/?key={1}&start={2}", MainWindow.Settings.Container.CurrentAppID, MainWindow.Settings.Container.Steam_APIKey, Start))

                    Dim SchemaItems = JSON.Deserialize(Of Items.SchemaItems)(SchemaItems_String)
                    File.WriteAllText(String.Format(Schema_Path_Items, Start), SchemaItems_String)

                    Status(String.Format("Downloaded Schema Items : {0} - {1}", Start, SchemaItems.result.Next))

                    If (Start <> SchemaItems.result.Next) AndAlso (SchemaItems.result.Next > 0) Then
                        Start = SchemaItems.result.Next
                    Else
                        Exit While
                    End If
                End While

                Client.DownloadFile(String.Format("https://api.steampowered.com/IEconItems_{0}/GetSchemaOverview/v1/?key={1}", MainWindow.Settings.Container.CurrentAppID, MainWindow.Settings.Container.Steam_APIKey), Schema_Path_Overview)

                Status("Schema Download Complete")
            End Using
        End Sub

        Sub CreateSchema()
            Dim Path As String = IO.Path.Combine(MainWindow.Home, MainWindow.Settings.Container.CurrentGameName)
            Dim Schema_Raw_Items As String = IO.Path.Combine(Path, "Schema_Raw_Items_{0}.txt")
            Dim Schema_Raw_Overview As String = IO.Path.Combine(Path, "Schema_Raw_Overview.txt")
            Dim Start As New Integer

            If (IO.File.Exists(String.Format(Schema_Raw_Items, Start)) AndAlso IO.File.Exists(Schema_Raw_Overview)) Then
                Dim Schema_Items As New Items.SchemaItems
                Schema_Items.result = New Items.Result
                Schema_Items.result.Items = New List(Of DownloadManager.Item_Url)

                While True
                    Dim Schema_Raw_Path = String.Format(Schema_Raw_Items, Start)

                    If IO.File.Exists(Schema_Raw_Path) Then
                        Dim Schema_Items_Temp = JSON.Read(Of Items.SchemaItems)(Schema_Raw_Path)
                        Schema_Items.result.Items.AddRange(Schema_Items_Temp.result.Items)

                        If (Start <> Schema_Items_Temp.result.Next) AndAlso (Schema_Items_Temp.result.Next > 0) Then
                            Start = Schema_Items_Temp.result.Next
                        Else
                            Exit While
                        End If
                    Else
                        Status("Schema File Doesn't Exist : " & Schema_Raw_Path)
                        Exit While
                    End If
                End While

                Status("Schema Items Merged")

                Dim Schema_Path As String = IO.Path.Combine(Path, "Schema.xml")
                Dim Schema_Overview = JSON.Read(Of Overview.SchemaOverview)(Schema_Raw_Overview)

                If ((Schema_Items IsNot Nothing) AndAlso (Schema_Overview IsNot Nothing)) Then
                    Dim Schema As New Schema.Schema

                    Schema.Items = Schema_Items.result.Items
                    Schema.Counters = Schema_Overview.result.CounterData
                    Schema.Origins = Schema_Overview.result.OriginNames
                    Schema.Particles = Schema_Overview.result.ParticleEffects

                    For Each Quality In Schema_Overview.result.qualities
                        Schema.Qualities.Add(New IDName With {.ID = Quality.Value, .Name = Quality.Key})
                    Next

                    For Each Item In Schema.Items
                        If Item.DefName.StartsWith("Paint Can") Then
                            Schema.Paints.Add(New Paint With {.Name = Item.Name, .DefIndex = Item.Defindex, .Color = Item.Attributes.Where(Function(F) F.ClassName.StartsWith("set_Item_tint_rgb")).Select(Function(F) F.Value).Where(Function(F) CInt(F) > -1).ToList})
                        End If
                    Next

                    XML.Write(Schema_Path, Schema)
                End If
            End If
        End Sub

        Public Function GetPlayerSummaries(Partner As Partner) As Partner
            If (MainWindow.Settings.Container.Steam_APIKey.Length < 32) Then
                Partner.ProfileVisibility = -1
                Return Partner
            End If

            Dim SteamID As String = ConvertToSteamID64(Partner.ID).ToString
            Dim PlayerURL As Uri = New Uri(String.Format("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&format=xml&steamids={1}", MainWindow.Settings.Container.Steam_APIKey, SteamID))
            Dim Download_String As String = Nothing

            Using Client As New WebClient
                Try
                    Download_String = Client.DownloadString(PlayerURL)
                Catch ex As WebException
                    Return Partner
                End Try
            End Using

            Dim Downloaded_PlayerSummaries = XML.Deserialize(Of PlayerSummaries)(Download_String)

            Dim Player As Player = Downloaded_PlayerSummaries.Players.FirstOrDefault
            If (Player Is Nothing) Then
                Return Partner
            End If

            With Partner
                .Link = New Uri(Player.profileurl)
                .OnlineLast = U.ConvertUnixToDate(Player.lastlogoff).ToString
                .Name = Player.personaname
                .ImageSource = New Uri(Path.Combine(MainWindow.Home, "Traders", .ID + ".png"))
                .ImageUrl = New Uri(Player.avatarfull)
                .Online = If(Player.personastate > 0, True, False)
                .ProfileVisibility = Player.communityvisibilitystate
            End With

            Return Partner
        End Function

        Public Function GetPlayerSummaries_WebRequest(Partner As Partner) As Partner
            Dim SteamID As String = ConvertToSteamID64(Partner.ID).ToString
            Dim Download_Url As Uri = New Uri(String.Format("http://steamcommunity.com/profiles/{0}", Partner.ID))
            Dim Download_String As String = Nothing

            Using Client As New WebClient
                Download_String = Client.DownloadString(Download_Url)
            End Using

            Dim Player As New Player
            With Partner
                FindString(Download_String, "<span class=""actual_persona_name"">", .Name, "</span>")
                FindString(Download_String, "<div class=""profile_in_game_name"">", .OnlineLast, "</span>")
                .Link = Download_Url

                Dim AvatarUrl As String = Nothing
                FindString(Download_String, "<div class=""playerAvatarAutoSizeInner""><img src=""", AvatarUrl, ">")
                .ImageUrl = New Uri(AvatarUrl)
            End With

            Return Partner
        End Function

        Public Function GetPrice_Steam(Item As Item) As Item
            Dim Download_Url = String.Format("http://steamcommunity.com/market/priceoverview/?currency={0}&appid={1}&market_hash_name={2}", 3, MainWindow.Settings.Container.CurrentAppID, Web.HttpUtility.UrlEncode(Item.MarketHashID))
            Dim Download_String As String = Nothing

            Using Client As New WebClient
                Try
                    Download_String = Client.DownloadString(Download_Url)
                Catch ex As WebException
                    Item.SteamObtained = 2
                    Return Item
                End Try
            End Using

            Dim SteamPrice As SteamPriceOverview = JSON.Deserialize(Of SteamPriceOverview)(Download_String)

            If (Not SteamPrice.Success) Then
                Item.SteamObtained = 2
                Return Item
            End If

            Item.Price.Steam = CDec(RemoveExtraText(SteamPrice.Lowest_price))
            Item.Price.Steam_Average = CDec(RemoveExtraText(SteamPrice.Median_price))
            Item.Price.Steam_Listings = CInt(RemoveExtraText(SteamPrice.Volume, False))

            Return Item
        End Function

        Public Function GetPrice_Steam_Decimal(MarketHashID As String) As Decimal
            Dim MarketUrl As String = String.Format("http://steamcommunity.com/market/priceoverview/?currency={0}&appid={1}&market_hash_name={2}", 3, MainWindow.Settings.Container.CurrentAppID, Web.HttpUtility.UrlEncode(MarketHashID))
            Dim Download_String As String = Nothing

            Using Client As New WebClient
                Try
                    Download_String = Client.DownloadString(MarketUrl)
                Catch ex As WebException
                    Return Nothing
                End Try
            End Using

            Dim SteamPrice As SteamPriceOverview = JSON.Deserialize(Of SteamPriceOverview)(Download_String)

            If (Not SteamPrice.Success) Then
                Return Nothing
            End If

            Return CDec(RemoveExtraText(SteamPrice.Lowest_price))
        End Function

        Public Sub GetAssetPrices_Steam()
            Dim ClassIDs As New ConcurrentBag(Of String)

            Parallel.ForEach(BindingMasterList.SelectMany(Function(F) F),
                     Sub(Item)
                         If ClassIDs.Where(Function(F) Equals(F, Item.ClassID)).None Then
                             ClassIDs.Add(Item.ClassID)
                         End If
                     End Sub)

            Dim Lists As New List(Of List(Of String))
            Dim List_ClassID As New List(Of String)(ClassIDs)

            While (List_ClassID.Count > 80)
                Lists.Add(List_ClassID.GetRange(0, 80))
                List_ClassID.RemoveRange(0, 80)
            End While

            Dim AssetPricesBag As New ConcurrentBag(Of GetAssetPrices_Class)

            Parallel.ForEach(Lists,
                     Sub(List)
                         Dim Strings As New ConcurrentBag(Of String)
                         Parallel.For(0, List.Count, Sub(i) Strings.Add(String.Format("&classid{0}={1}", i, List(i))))
                         Dim JsonString As String = String.Join(String.Empty, Strings)

                         Dim Download_Url As String = String.Format("http://api.steampowered.com/ISteamEconomy/GetAssetPrices/v0001/?key={0}&appid={1}&currency={2}&class_count={3}{4}",
                                                                    MainWindow.Settings.Container.Steam_APIKey, MainWindow.Settings.Container.CurrentAppID, 3, List.Count, JsonString)
                         Dim Download_String As String = Nothing

                         Using Client As New WebClient
                             Try
                                 Download_String = Client.DownloadString(Download_Url)
                             Catch ex As WebException
                                 Status("FAILED : Getting Steam API Prices : " & ex.Message)
                                 Exit Sub
                             End Try
                         End Using

                         AssetPricesBag.Add(JSON.Deserialize(Of GetAssetPrices_Class)(Download_String))
                     End Sub)

            Dim AssetPrices As New List(Of GetAssetPrices_Class.Asset)
            For Each List In AssetPricesBag
                AssetPrices.AddRange(List.result.assets.SelectAll)
            Next

            Parallel.ForEach(BindingMasterList.SelectMany(Function(F) F),
                             Sub(Item)
                                 If (Item.ClassID IsNot Nothing) Then
                                     Dim Asset = AssetPrices.FirstOrDefault(Function(F) F.classid = Item.ClassID)

                                     If (Asset IsNot Nothing) Then
                                         Item.Price.Steam = (CDec(Asset.prices.FirstOrDefault(Function(F) F.Key.Equals("EUR")).Value) / 100)
                                         Item.SteamObtained = 1
                                     End If
                                 End If
                             End Sub)

            Status("Steam API Prices Successfully Downloaded!")
        End Sub

        Public Function GetTradeOffers() As List(Of AdvancedItem)
            Dim List As New ConcurrentBag(Of AdvancedItem)
            Dim Download_String As String = Nothing

            Using Client As New WebClient
                Dim Download_Url As String = String.Format("https://api.steampowered.com/IEconService/GetTradeOffers/v1/?key={0}&format=xml&get_sent_offers=1&active_only=1", MainWindow.Settings.Container.Steam_APIKey)
                Try
                    Download_String = Client.DownloadString(Download_Url)
                Catch ex As WebException
                    Status("FAILED : " + ex.Message)
                    Return Nothing
                End Try
            End Using

            Dim TradeOffers As ActiveTradeOffers = XML.Deserialize(Of ActiveTradeOffers)(Download_String)

            Dim StateDictionary As New Dictionary(Of Integer, String) From {
                {1, "Invalid"},
                {2, "Awaiting Response"},
                {3, "Accepted"},
                {4, "Counter Offer Received"},
                {5, "Expired"},
                {6, "Cancelled"},
                {7, "Declined"},
                {8, "Items Missing"},
                {9, "Authentication Required"},
                {10, "Cancelled by Authentication"},
                {11, "Awaiting Escow Expiration"}}

            Parallel.ForEach(TradeOffers.Offers, Sub(Trade)
                                                     Dim NewTrade As New AdvancedItem

                                                     With NewTrade
                                                         .ID = Trade.ID
                                                         .IsOurOffer = Trade.IsOurOffer
                                                         .StateID = Trade.State
                                                         .Partner.ID = Trade.PartnerID
                                                         .Updated = ConvertUnixToDate(Trade.TimeUpdated).ToString
                                                         .Initiated = ConvertUnixToDate(Trade.TimeCreated).ToString
                                                         .Expiration = ConvertUnixToDate(Trade.ExpirationTime).ToString
                                                         .EscrowEnd = ConvertUnixToDate(Trade.EscrowEndDate).ToString
                                                     End With

                                                     If (NewTrade.StateID > -1) Then
                                                         StateDictionary.TryGetValue(NewTrade.StateID, NewTrade.State)
                                                     End If

                                                     Dim List_Incoming_Basic As New ConcurrentBag(Of BasicItem)
                                                     Dim List_Outgoing_Basic As New ConcurrentBag(Of BasicItem)

                                                     Task.WaitAll(
                                             Task.Run(Sub()
                                                          Parallel.ForEach(Trade.Incoming, Sub(Item)
                                                                                               Dim newTradeItem As New BasicItem
                                                                                               With newTradeItem
                                                                                                   .Amount = Item.Amount
                                                                                                   .AssetID = Item.AssetID
                                                                                                   .ClassID = Item.ClassID
                                                                                                   .ContextID = Item.ContextID
                                                                                                   .InstanceID = Item.InstanceID
                                                                                                   .AppID = Item.AppID

                                                                                                   If (NewTrade.StateID = 11) Then
                                                                                                       .IsMissing = False
                                                                                                   Else
                                                                                                       .IsMissing = Item.IsMissing
                                                                                                   End If
                                                                                               End With

                                                                                               List_Incoming_Basic.Add(newTradeItem)
                                                                                           End Sub)
                                                      End Sub),
                                             Task.Run(Sub()
                                                          Parallel.ForEach(Trade.Outgoing, Sub(Item)
                                                                                               Dim newTradeItem As New BasicItem
                                                                                               With newTradeItem
                                                                                                   .Amount = Item.Amount
                                                                                                   .AssetID = Item.AssetID
                                                                                                   .ClassID = Item.ClassID
                                                                                                   .ContextID = Item.ContextID
                                                                                                   .InstanceID = Item.InstanceID
                                                                                                   .AppID = Item.AppID

                                                                                                   If (NewTrade.StateID = 11) Then
                                                                                                       .IsMissing = False
                                                                                                   Else
                                                                                                       .IsMissing = Item.IsMissing
                                                                                                   End If
                                                                                               End With

                                                                                               List_Outgoing_Basic.Add(newTradeItem)
                                                                                           End Sub)
                                                      End Sub))

                                                     NewTrade.Incoming.AddRange(List_Incoming_Basic)
                                                     NewTrade.Outgoing.AddRange(List_Outgoing_Basic)

                                                     List.Add(NewTrade)
                                                 End Sub)
            Return List.ToList
        End Function

    End Module
End Namespace
