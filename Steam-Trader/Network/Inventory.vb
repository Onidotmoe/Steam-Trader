Imports System.Collections.Concurrent
Imports System.ComponentModel
Imports System.Net
Imports System.Net.Http
Imports System.Text.RegularExpressions
Imports SteamTrader.MainWindow
Imports SteamTrader.Network.Steam.API.Schema
Imports U

Namespace Network.Inventory
    Public Module Inventory

        Public Async Function [Get](Optional AppID As Integer = Nothing, Optional SteamID As ULong = Nothing, Optional Context As Integer = 2, Optional ForUser As Boolean = False, Optional ForSelf As Boolean = False) As Task(Of ObservableCollection(Of GUI.Item))
            If (AppID = Nothing) Then
                AppID = MainWindow.Settings.Container.CurrentAppID
            End If

            If (ForUser OrElse ForSelf) Then
                If ForSelf Then
                    If (MainWindow.Settings.Container.CurrentUser.SteamID > Nothing) Then
                        SteamID = MainWindow.Settings.Container.CurrentUser.SteamID
                    Else
                        Return Nothing
                    End If
                End If

                Dim Response = Await WebRequest(AppID, SteamID)

                If (Response Is Nothing) Then
                    Return Nothing
                Else
                    Return GenerateGUI(Response)
                End If

            ElseIf ((Not String.IsNullOrWhiteSpace(MainWindow.Settings.Container.Steam_APIKey)) AndAlso (SteamID > Nothing)) Then
                Dim Result = API(AppID, SteamID)

                If (Result IsNot Nothing) Then
                    Return Result
                Else
                    Return Await [Get](AppID, SteamID:=SteamID, ForUser:=True)
                End If
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' Sends a Webrequest out for the specified user's game inventory. Returns deserialized JSON file.
        ''' </summary>
        ''' <param name="AppID">ID of the game you wish to get inventory of. For non-Valve games use "Game_GetInventory".</param>
        ''' <param name="SteamID">Optional: The users SteamID as UInt64, otherwise will use the current logged-in user's ID.</param>
        ''' <param name="Context">Optional: Specify the context of the request, defaults to "2".</param>
        Private Async Function WebRequest(AppID As Integer, SteamID As ULong, Optional Context As Integer = 2) As Task(Of Response.Response)
            Dim Download_Url As String = String.Format("https://steamcommunity.com/profiles/{0}/inventory/json/{1}/{2}", SteamID.ToString, AppID, Context)
            Dim Download_String As String = Nothing

            If (Not MainWindow.Network.IsLoggedIn) Then
                Try
                    Using Client As New WebClient
                        Download_String = Client.DownloadString(Download_Url)
                    End Using
                Catch ex As WebException
                    Status("FAILED GetInventory : " + ex.Message)
                    Return Nothing
                End Try
            Else
                Try
                    Using Client As New HttpClient(New HttpClientHandler() With {.CookieContainer = Network.Cookie})
                        Download_String = Await Client.GetStringAsync(Download_Url)
                    End Using
                Catch ex As HttpRequestException
                    Status("FAILED GetInventory : " + ex.Message)
                    Return Nothing
                End Try
            End If

            Return U.JSON.Deserialize(Of Response.Response)(Download_String)
        End Function

        Private Function API(AppID As Integer, SteamID As ULong) As ObservableCollection(Of GUI.Item)
            Dim Download_Url As String = String.Format("http://api.steampowered.com/IEconItems_{0}/GetPlayerItems/v0001/?key={1}&steamid={2}&format=xml", AppID, MainWindow.Settings.Container.Steam_APIKey, SteamID)
            Dim Download_String As String = Nothing

            Using Client As New WebClient
                Try
                    Download_String = Client.DownloadString(Download_Url)
                Catch ex As WebException
                    Status("FAILED : Inventory API request")
                    Return Nothing
                End Try
            End Using

            Dim Inventory_Result As Result.Result = U.XML.Deserialize(Of Result.Result)(Download_String)
            Dim Inventory As New ConcurrentBag(Of GUI.Item)

            If (Inventory_Result.Status <> 1) Then
                Status("FAILED : Inventory API request, Status Code : " + Inventory_Result.Status.ToString)
                Return Nothing
            End If

            Parallel.ForEach(Inventory_Result.Items,
                         Sub(Input)
                             Dim Output As New GUI.Item

                             With Output
                                 .Tradable = If(Input.NonTradable, False, True)
                                 .UsableInCrafting = If(Input.NonCraftable, False, True)
                                 .AssetID = Input.ID
                                 .DefIndex = Input.DefIndex
                                 .Level = Input.Level
                                 .Quality.ID = Input.Quality
                                 .Quality.Name = GetQualityName(Input.Quality)
                                 .Custom_Name = Input.Custom_Name
                                 .Custom_Description = Input.Custom_Description
                             End With

                             Inventory.Add(Output)
                         End Sub)

            Return New ObservableCollection(Of GUI.Item)(Inventory)
        End Function

        Public Async Sub Refresh()
            Dim My As ObservableCollection(Of GUI.Item) = Await [Get](ForSelf:=True)
            Dim Final As New ConcurrentBag(Of GUI.Item)(ImportDownloadToSave(My, Binding_Lv_Inventory))

            Parallel.ForEach(Final, Sub(Level_1)
                                        Dim Level_2 = Binding_Lv_Inventory.FirstOrDefault(Function(F) F.MarketHashID.Equals(Level_1.MarketHashID))

                                        If (Level_2 IsNot Nothing) Then
                                            With Level_1
                                                .Price.Steam = Level_2.Price.Steam
                                                .Price.Backpack = Level_2.Price.Backpack
                                                .Price.Steam_Average = Level_2.Price.Steam_Average
                                                .Price.Steam_Listings = Level_2.Price.Steam_Listings
                                            End With
                                        End If
                                    End Sub)

            If (DownloadHandler IsNot Nothing) Then
                DownloadHandler.Start(Final)
            End If

            Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub()
                                                                   Binding_Lv_Inventory.ReplaceAll(Final)
                                                                   MainWindow.MainWindow.Lv_Inventory.Columns(0).SortDirection = ListSortDirection.Ascending
                                                               End Sub)

            MainWindow.MainWindow.AssignHave()
            Status("Updated Inventory!")
        End Sub

        Private Function ImportDownloadToSave([New] As IEnumerable(Of GUI.Item), Old As ObservableCollection(Of GUI.Item)) As IEnumerable(Of GUI.Item)
            Parallel.ForEach([New], Sub(iNew)
                                        Dim iOld = Old.FirstOrDefault(Function(F) (Equals(F.ClassID, iNew.ClassID) AndAlso (Equals(F.InstanceID, iNew.InstanceID) OrElse (Equals(F.Name, iNew.Name) AndAlso (F.Counter <= iNew.Counter)))))

                                        If (iOld IsNot Nothing) Then
                                            With iNew
                                                .Index = iOld.Index
                                                .Locked = iOld.Locked
                                                .MarkedForSell = iOld.MarkedForSell
                                                .WantAmount = iOld.WantAmount
                                                .GiftTo = iOld.GiftTo
                                            End With
                                        End If
                                    End Sub)
            Return [New]
        End Function

        Private Function GenerateGUI(Inventory As Response.Response) As ObservableCollection(Of GUI.Item)
            If (Not Inventory.Success) Then
                Status("FAILED : " + Inventory.Inventory_Error)
                Return Nothing
            End If

            Dim List As New ConcurrentBag(Of GUI.Item)

            Parallel.ForEach(Inventory.ItemsInventory,
                             Sub(Level_1)
                                 For Each Level_2 In Inventory.ItemsDescriptions
                                     If Equals(Level_1.Value.classid, Level_2.Value.classid) AndAlso Equals(Level_1.Value.instanceid, Level_2.Value.instanceid) Then
                                         Dim NewItem As New GUI.Item

                                         With NewItem
                                             .AssetID = Level_1.Value.Id
                                             .ClassID = Level_1.Value.classid
                                             .InstanceID = Level_1.Value.instanceid
                                             .DefIndex = Level_2.Value.AppData.DefIndex

                                             .Quality.ID = CInt(Level_2.Value.AppData.Quality)
                                             .Index = CInt(Level_1.Value.pos)
                                             .Name = Level_2.Value.name
                                             .Marketable = CType(Level_2.Value.marketable, Boolean)
                                             .Tradable = CType(Level_2.Value.tradable, Boolean)
                                             .MarketHashID = Level_2.Value.market_hash_name
                                             .Price.Hold_Trade = CInt(Level_2.Value.market_tradable_restriction)
                                             .Price.Hold_Market = CInt(Level_2.Value.market_tradable_restriction)

                                             If (Not String.IsNullOrWhiteSpace(Level_2.Value.cache_expiration)) AndAlso (Not Equals(Date.MinValue, Level_2.Value.cache_expiration)) Then
                                                 .Price.UnlockDate = (Date.ParseExact(Level_2.Value.cache_expiration, "yyyy-MM-ddTHH:mm:ssZ", Globalization.CultureInfo.InvariantCulture)).ToString
                                             End If

                                             For Each lAction In Level_2.Value.Actions
                                                 If (lAction.name = "Item Wiki Page...") Then
                                                     .WikiURL = New Uri(lAction.link)
                                                     Exit For
                                                 End If
                                             Next

                                             Dim DoneBoolean As Integer = 0

                                             For Each iTag In Level_2.Value.Tags
                                                 Select Case iTag.category_name
                                                     Case "Quality"
                                                         .Quality.Name = iTag.name
                                                         DoneBoolean += 1

                                                     Case "Type"
                                                         .Type = iTag.name
                                                         DoneBoolean += 1
                                                 End Select

                                                 If (DoneBoolean >= 2) Then
                                                     Exit For
                                                 End If
                                             Next

                                             If (Level_2.Value.type.IndexOf("Level") > -1) Then
                                                 .Level = Regex.Match(Level_2.Value.type, "\d+").Value
                                             End If

                                             For Each Counter As Counter In Counters
                                                 If (Level_2.Value.type.IndexOf(Counter.Name, StringComparison.InvariantCultureIgnoreCase) > -1) Then
                                                     Dim Count As String = Nothing
                                                     FindString(Level_2.Value.type, Counter.Name, Count, Nothing)
                                                     Integer.TryParse(RemoveExtraText(Count, False), .Counter)
                                                     .Counter_Type = Counter.Name
                                                     Exit For
                                                 End If
                                             Next

                                             If (Level_2.Value.descriptions IsNot Nothing) Then
                                                 Dim Description As New List(Of Pair(Of String, String))

                                                 For Each Level_3 In Level_2.Value.descriptions
                                                     If (Not String.IsNullOrWhiteSpace(Level_3.value)) Then
                                                         If (Level_3.value.IndexOf("Usable in Crafting", StringComparison.CurrentCultureIgnoreCase) > -1) Then
                                                             .UsableInCrafting = False
                                                         End If
                                                         If (Level_3.value.IndexOf("Achievement", StringComparison.CurrentCultureIgnoreCase) > -1) Then
                                                             .Achievement = True
                                                             .UsableInCrafting = False
                                                             .Tradable = False
                                                         End If

                                                         Dim Line As New Pair(Of String, String)(Level_3.value, Nothing)
                                                         If (Not String.IsNullOrWhiteSpace(Level_3.color)) Then
                                                             Line.Second = "#"c + Level_3.color
                                                         End If

                                                         Description.Add(Line)
                                                     End If
                                                 Next

                                                 .Description = Description
                                             End If

                                             If (Not String.IsNullOrWhiteSpace(Level_2.Value.market_hash_name)) Then
                                                 .Price.Url_Backpack = AssignUrl_Backpack(.Name, .Quality.Name, .Tradable, .Craftable)

                                                 If CType(Level_2.Value.marketable, Boolean) Then
                                                     .Price.Url_Steam = AssignUrl_Steam(Level_2.Value.market_hash_name)
                                                 End If
                                             End If

                                         End With

                                         List.Add(NewItem)
                                         Exit For
                                     End If
                                 Next
                             End Sub)

            Return New ObservableCollection(Of GUI.Item)(List)
        End Function

    End Module
End Namespace
