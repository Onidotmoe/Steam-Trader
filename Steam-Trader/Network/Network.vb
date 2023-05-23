Imports System.Collections.Concurrent
Imports System.ComponentModel
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Newtonsoft.Json
Imports SteamTrader.GUI
Imports SteamTrader.MainWindow
Imports SteamTrader.Network.Backpack.Response
Imports SteamTrader.Network.Security
Imports SteamTrader.Network.Steam.API
Imports SteamTrader.Network.Steam.Login
Imports SteamTrader.Scraper
Imports SteamTrader.TeamFortress2
Imports U

Namespace Network

    Public Class Network
        Inherits NotifyPropertyChanged
        Public Shared Property Username As String
        Public Shared Property Password As String
        Public WithEvents ConnectionWorker As New BackgroundWorker
        Public WithEvents AuthorizationWorker As New BackgroundWorker
        Private WaitForAuthThreadToEnd As New AutoResetEvent(False)
        Private WaitForCaptchaThreadToEnd As New AutoResetEvent(False)
        Public Shared Cookie As New CookieContainer
        Private _IsLoggedIn As Boolean = False

        Public Property IsLoggedIn As Boolean
            Get
                Return _IsLoggedIn
            End Get
            Set
                NotifyPropertyChanged(_IsLoggedIn, Value)
            End Set
        End Property

        Public Sub Btn_Login_Click(sender As Object, e As RoutedEventArgs)
            Select Case MainWindow.Settings.Container.LoginButtonSetting
                Case LogInState.LogIn
                    SetDefaults()
                    Dim LoginScreen As New LoginWindowForm With {.Owner = MainWindow.MainWindow}
                    LoginScreen.ShowDialog()
                    Progress(0)

                    If (LoginScreen.DialogResult = True) Then
                        If (ConnectionWorker.IsBusy = True) Then
                            Status("Canceling current worker thread...")
                            ConnectionWorker.CancelAsync()
                            ConnectionWorker = New BackgroundWorker
                        End If

                        Progress(10)

                        ConnectionWorker.WorkerSupportsCancellation = True
                        ConnectionWorker.RunWorkerAsync()
                        MainWindow.Settings.Container.ReLoginPossible = False
                        MainWindow.Settings.Container.LoginButtonSetting = LogInState.Cancel

                        My.Settings.LastUserName = MainWindow.Settings.Container.CurrentUser.AccountName
                        My.Settings.LastUserID = MainWindow.Settings.Container.CurrentUser.SteamID
                        My.Settings.Save()
                        MainWindow.MainWindow.Load()
                    Else
                        MainWindow.Settings.Container.LoginButtonSetting = LogInState.LogIn
                        Status("Logon Canceled")
                    End If
                Case LogInState.Cancel
                    ConnectionWorker.CancelAsync()
                    Progress(0)
                    MainWindow.Settings.Container.LoginButtonSetting = LogInState.LogIn
                    MainWindow.Settings.Container.ReLoginPossible = False
                    Status("Logon Canceled")

                Case LogInState.LogOff
                    SetDefaults()
                    MainWindow.Network.IsLoggedIn = False
                    MainWindow.Settings.Container.CurrentUser = New User
                    Progress(0)
                    MainWindow.Settings.Container.ReLoginPossible = False
                    MainWindow.Settings.Container.LoginButtonSetting = LogInState.LogIn
                    Status("LoggedOff from Steam Trader")

                Case LogInState.LogOut
                    SetDefaults()
                    Dim Secret As New Secret
                    Secret.Clear()

                    MainWindow.Network.IsLoggedIn = False
                    MainWindow.Settings.Container.ReLoginPossible = False
                    MainWindow.Settings.Container.LoginButtonSetting = LogInState.LogIn
                    Status("LoggedOut from Steam")

                'TODO:
                'Do LogOut
                'Send logout post request etc
                'LogOutSteam

                Case LogInState.ReLogIn
                    Progress(10)
                    MainWindow.Settings.Container.LoginButtonSetting = LogInState.Cancel
                    MainWindow.Settings.Container.ReLoginPossible = False

                    ConnectionWorker.WorkerSupportsCancellation = True
                    ConnectionWorker.RunWorkerAsync()
            End Select
        End Sub

        Public Sub SetDefaults()
            For Each List In BindingMasterList
                List.Clear()
            Next

            Binding_Lv_UpForTrade.Clear()
            My.Settings.Reset()

            For Each Setting In MainWindow.Settings.Collection
                If (Setting.InputProperty IsNot Nothing) Then
                    Dim Prop = If(My.Settings.GetType.GetProperty(Setting.InputProperty), Nothing)
                    Dim Value As Object = If(Prop.GetValue(My.Settings), Nothing)
                    SetPropertyValueByName(Setting, Setting.InputProperty, Value)
                Else
                    Setting.Input = Nothing
                End If
            Next

            With MainWindow.Settings.Container
                .Steam_APIKey = Nothing
                .Backpack_APIKey = Nothing
                .Balance = Nothing
                .BalanceMetal = Nothing
                .TotalMetal = New Metal
                .TotalForSale = Nothing
            End With
        End Sub

        Private Async Sub ConnectionWorker_DoWork(sender As Object, e As DoWorkEventArgs) Handles ConnectionWorker.DoWork
            If String.IsNullOrWhiteSpace(Username) Then
                Status("Username is missing.")
                Progress(0)
                Exit Sub
            End If

            Cookie = New CookieContainer
            InsertSteamCookie()
            Dim Secret As New Secret
            If (Equals(Username, My.Settings.LastUserName) AndAlso (My.Settings.LastUserID <> Nothing) AndAlso Secret.Check) Then
                Secret.Load()
                Secret.Reload()

                Dim Node_Start As String = "<span class=""pulldown global_action_link"" id=""account_pulldown"" onclick=""ShowMenu( this, 'account_dropdown', 'right', 'bottom', true );"">"
                Dim Node_End As String = "</span>"
                Dim Download_String As String = Nothing

                Using Client As New HttpClient(New HttpClientHandler() With {.CookieContainer = Cookie})
                    Download_String = Client.GetStringAsync("https://steamcommunity.com/").Result
                End Using

                Dim AccountName As String = Nothing
                FindString(Download_String, Node_Start, AccountName, Node_End)

                If ((Not String.IsNullOrWhiteSpace(AccountName)) AndAlso Equals(Username, AccountName)) Then
                    Status("Relogin successful!")
                    Progress(100)
                    Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() MainWindow.Settings.Container.LoginButtonSetting = LogInState.LogOff)

                    MainWindow.Network.IsLoggedIn = True
                    DownloadHandler = New DownloadManager
                    SteamTrader.Network.Steam.API.API.CreateSchemaList()
                    MainWindow.Settings.Container.IsRunning.Login = False
                    GetAndHandle_LoginProfile()
                    Return
                End If
            End If

            If String.IsNullOrWhiteSpace(Password) Then
                Status("Password is missing.")
                Progress(0)
                Return
            End If

            Status("Connecting to Steam...")
            Progress(20)

            Cookie = New CookieContainer
            InsertSteamCookie()

            Using Client As New HttpClient(New HttpClientHandler() With {.CookieContainer = Cookie})
                Dim Download_Main As HttpResponseMessage = Client.GetAsync("https://steamcommunity.com/").Result
            End Using

            Dim Login As Login_Response = Await LoginSteam()
            Progress(40)

            MainWindow.Settings.Container.IsRunning.Login = True
            Dim StartTime As Date = Date.Now
            Dim Attempts As Integer = 0

            While MainWindow.Settings.Container.IsRunning.Login
                If ((Date.Now - StartTime) >= TimeSpan.FromSeconds(300)) OrElse (Attempts >= 25) Then
                    Status("Login timedout.")
                    Progress(0)
                    Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() MainWindow.Settings.Container.LoginButtonSetting = LogInState.LogIn)
                    ConnectionWorker.CancelAsync()
                Else
                    Attempts += 1
                End If

                If ConnectionWorker.CancellationPending Then
                    MainWindow.Settings.Container.IsRunning.Login = False
                    Exit While
                End If

                With Login
                    If (Not String.IsNullOrWhiteSpace(.Message)) Then
                        If .Message.Contains("Incorrect") Then
                            Status("Incorrect username or password.")
                        ElseIf .Message.Contains("login failures from your network in a short time period") Then
                            Status("Login cooldown encountered, try again later.")
                        End If
                        Progress(0)
                        MainWindow.Settings.Container.IsRunning.Login = False
                        Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() MainWindow.Settings.Container.LoginButtonSetting = LogInState.LogIn)

                    ElseIf .Captcha_Needed Then
                        Dim DialogValue As Boolean = Nothing

                        Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub()
                                                                               Dim CaptchaUrl As New Uri("http://steamcommunity.com/public/captcha.php?gid=" + .Captcha_ID, UriKind.Absolute)
                                                                               Dim CaptchaImage As New BitmapImage
                                                                               CaptchaImage.BeginInit()
                                                                               CaptchaImage.UriSource = CaptchaUrl
                                                                               CaptchaImage.EndInit()

                                                                               Dim CaptchaWindow As New CaptchaWindow
                                                                               CaptchaWindow.Img_Captcha.Source = CaptchaImage
                                                                               CaptchaWindow.Owner = Windows.Application.Current.MainWindow
                                                                               CaptchaWindow.ShowDialog()
                                                                               DialogValue = CBool(CaptchaWindow.DialogResult)
                                                                               WaitForCaptchaThreadToEnd.Set()
                                                                           End Sub)
                        WaitForCaptchaThreadToEnd.WaitOne()

                        If (Not DialogValue) Then
                            Status("Logon Cancelled")
                            Progress(0)
                            MainWindow.Settings.Container.IsRunning.Login = False
                            Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() MainWindow.Settings.Container.LoginButtonSetting = LogInState.LogIn)
                        Else
                            Login = Await LoginSteam(CaptchaID:= .Captcha_ID, CaptchaText:=CaptchaCode)
                        End If

                    ElseIf (.RequiresTwoFactor OrElse .Email_Needed) Then
                        Dim DialogValue As Boolean = Nothing

                        Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub()
                                                                               Dim AuthWindow As New AuthCodeWindow With {.Owner = Windows.Application.Current.MainWindow}
                                                                               AuthWindow.ShowDialog()
                                                                               DialogValue = CBool(AuthWindow.DialogResult)
                                                                               WaitForAuthThreadToEnd.Set()
                                                                           End Sub)
                        WaitForAuthThreadToEnd.WaitOne()

                        If (Not DialogValue) Then
                            Status("Logon Cancelled")
                            Progress(0)
                            Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() MainWindow.Settings.Container.LoginButtonSetting = LogInState.LogIn)
                            MainWindow.Settings.Container.IsRunning.Login = False
                        Else
                            Progress(60)

                            If .Email_Needed Then
                                Login = Await LoginSteam(Email:=TwoFactorAuth)

                            ElseIf .RequiresTwoFactor Then
                                Login = Await LoginSteam(TwoFactor:=TwoFactorAuth)
                            End If
                        End If

                    ElseIf (.Success AndAlso .LoginComplete) Then
                        Status("Login successful!")
                        Progress(100)
                        Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() MainWindow.Settings.Container.LoginButtonSetting = LogInState.LogOff)

                        Dim NewUser As New User
                        If (MainWindow.Settings.Container.CurrentUser.ImageSource IsNot Nothing) Then
                            NewUser.ImageSource = MainWindow.Settings.Container.CurrentUser.ImageSource
                        End If

                        Secret.Refresh()
                        Secret.Save()
                        MainWindow.Settings.Container.CurrentUser = NewUser
                        MainWindow.Settings.Container.CurrentUser.SteamID = .Transfer_Parameters.SteamID
                        MainWindow.Network.IsLoggedIn = True
                        DownloadHandler = New DownloadManager
                        SteamTrader.Network.Steam.API.API.CreateSchemaList()
                        MainWindow.Settings.Container.IsRunning.Login = False
                        GetAndHandle_LoginProfile()
                    Else
                        Status("Login Failed. Retrying...")
                        Progress(40)

                        If (Attempts <= 25) Then
                            Thread.Sleep(100)
                        Else
                            Thread.Sleep(1000)
                        End If
                    End If
                End With
            End While

            If MainWindow.Network.IsLoggedIn Then
                MainWindow.Settings.Container.LoginButtonSetting = LogInState.LogOff
            Else
                MainWindow.Settings.Container.LoginButtonSetting = LogInState.LogIn
            End If
        End Sub

        Private Async Function LoginSteam(Optional TwoFactor As String = Nothing, Optional Email As String = Nothing, Optional CaptchaID As String = Nothing, Optional CaptchaText As String = Nothing) As Task(Of Login_Response)
            Using Client As New HttpClient(New HttpClientHandler With {.CookieContainer = Cookie})
                Client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.116 Safari/537.36")
                Client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/javascript, text/html, application/xml, text/xml, */*")
                Client.DefaultRequestHeaders.ExpectContinue = False

                Dim CurrentTime = Convert.ToInt64((Date.UtcNow - New DateTime(1970, 1, 1)).TotalMilliseconds).ToString
                Dim RSAKeyUrl As String = "https://steamcommunity.com/login/getrsakey/"
                Dim LoginUrl As String = "https://steamcommunity.com/login/dologin/"

                Dim Data_Username As New Dictionary(Of String, String) From {{"username", Username}, {"donotcache", CurrentTime}}
                Dim Download_RSA As HttpResponseMessage = Await Client.PostAsync(RSAKeyUrl, New FormUrlEncodedContent(Data_Username))
                Dim RSA_ResponseString As String = Await Download_RSA.Content.ReadAsStringAsync()
                Dim RSA_Response_Deserialized = JsonConvert.DeserializeObject(Of RSA_Response)(RSA_ResponseString)

                Dim Crypto_Parameters As New RSAParameters With
                {
                    .Modulus = HexToByte(RSA_Response_Deserialized.Key_Modulus),
                    .Exponent = HexToByte(RSA_Response_Deserialized.Key_Exponent)
                }
                Dim Crypto As New RSACryptoServiceProvider
                Crypto.ImportParameters(Crypto_Parameters)

                Dim Password_Bytes = Encoding.ASCII.GetBytes(Password)
                Dim Password_Encrypted = Crypto.Encrypt(Password_Bytes, False)
                Dim Password_Base64 = Convert.ToBase64String(Password_Encrypted)
                Crypto.Dispose()

                Dim Data As New Dictionary(Of String, String)
                With Data
                    .Add("username", Username)
                    .Add("twofactorcode", TwoFactor)
                    .Add("emailauth", Email)
                    .Add("loginfriendlyname", String.Empty)
                    .Add("captchagid", CaptchaID)
                    .Add("captcha_text", CaptchaText)
                    .Add("emailsteamid", String.Empty)
                    .Add("rsatimestamp", RSA_Response_Deserialized.TimeStamp)
                    .Add("remember_login", "true")
                    .Add("donotcache", CurrentTime)
                    .Add("password", Password_Base64)
                End With

                Dim Login_Response = Await Client.PostAsync(LoginUrl, New FormUrlEncodedContent(Data))
                Dim Login_ResponseString As String = Await Login_Response.Content.ReadAsStringAsync()
                Dim Login_Response_Deserialized = JSON.Deserialize(Of Login_Response)(Login_ResponseString)

                Return Login_Response_Deserialized
            End Using
        End Function

        Private Async Sub GetAndHandle_LoginProfile()
            Dim User As New Partner With {.ID = MainWindow.Settings.Container.CurrentUser.SteamID.ToString}

            If (Not String.IsNullOrWhiteSpace(MainWindow.Settings.Container.Steam_APIKey)) Then
                User = Steam.API.API.GetPlayerSummaries(User)
            Else
                User = GetPlayerSummaries_WebRequest(User)
            End If

            With MainWindow.Settings.Container.CurrentUser
                .AccountName = Username
                .PersonaName = User.Name
                .ImageUrl = User.ImageUrl
                .Link = User.Link
                .SteamID = CULng(User.ID)
            End With

            Status("Welcome to Steam " + MainWindow.Settings.Container.CurrentUser.PersonaName + "!")
            Await Task.Run(Sub() Inventory.Refresh())

            Dim FilePath As String = Path.Combine(Home, "Users", MainWindow.Settings.Container.CurrentUser.SteamID.ToString)
            If (Not IO.File.Exists(Path.Combine(FilePath, MainWindow.Settings.Container.CurrentUser.SteamID.ToString + ".jpg"))) Then
                IO.Directory.CreateDirectory(FilePath)

                Using Client As New WebClient
                    Client.DownloadFile(MainWindow.Settings.Container.CurrentUser.ImageUrl, Path.Combine(FilePath, MainWindow.Settings.Container.CurrentUser.SteamID.ToString + ".jpg"))
                End Using
            End If
            MainWindow.Settings.Container.CurrentUser.ImageSource = New Uri(Path.Combine(FilePath, MainWindow.Settings.Container.CurrentUser.SteamID.ToString + ".jpg"))
            Update_Balance()
            MainWindow.MainWindow.UpdateMetalCount()

            Dim Secret As New Secret
            Secret.Refresh()
            Secret.Save()
        End Sub

        Public Async Sub Update_Balance()
            Dim Node_WalletBalance As String = "<span id=""marketWalletBalanceAmount"">"
            Dim Node_End As String = "</span>"
            Dim Source As String = Nothing

            Using Client As New HttpClient(New HttpClientHandler() With {.CookieContainer = Cookie})
                Source = Await Client.GetStringAsync("http://steamcommunity.com/market/")
            End Using

            FindString(Source.ToString, Node_WalletBalance, MainWindow.Settings.Container.CurrentUser.WalletBalance, Node_End, True)
            MainWindow.Settings.Container.Balance = CDec(RemoveExtraText(MainWindow.Settings.Container.CurrentUser.WalletBalance))
        End Sub

        Public Async Sub GetAndHandle_SteamListings()
            Dim Scrap = Await GetUserMarketListings()

            Dim Listings As New ObservableCollection(Of GUI.Item)(Scrap.Item1)
            Dim Orders As New ObservableCollection(Of GUI.Item)(Scrap.Item2)

            DownloadHandler.Start(New ObservableCollection(Of GUI.Item)(Listings.Concat(Orders)))

            Dim Validator As New Validator
            Validator.Specified(Listings)

            Dim TotalForSale As Decimal = Nothing
            For Each Item In Listings
                TotalForSale += CDec(Item.Price.My)
            Next

            Dim Uniques As New List(Of GUI.Item)
            For Each Level_1 In Listings
                Dim Level_2 As Integer = Uniques.FindIndex(Function(F) Level_1.DefIndex.Equals(F.DefIndex) AndAlso Level_1.Quality.ID.Equals(F.Quality.ID))

                If (Level_2 >= 0) Then
                    Uniques(Level_2).Price.Steam_Listings_My += 1
                Else
                    Level_1.Price.Steam_Listings_My += 1
                    Uniques.Add(Level_1)
                End If
            Next

            Parallel.ForEach(BindingMasterList,
                         Sub(Level_2)
                             Parallel.ForEach(Level_2,
                                                  Sub(Level_3)
                                                      Level_3.Price.Steam_Listings_My = 0
                                                  End Sub)
                         End Sub)

            Parallel.ForEach(Uniques,
                         Sub(Level_1)
                             If (Level_1.Price.Steam_Listings_My > 0) Then
                                 Parallel.ForEach(BindingMasterList,
                                              Sub(Level_2)
                                                  Parallel.ForEach(Level_2,
                                                  Sub(Level_3)
                                                      If ((Level_3.DefIndex = Level_1.DefIndex) AndAlso (Level_3.Quality.ID = Level_1.Quality.ID)) Then
                                                          Level_3.Price.Steam_Listings_My = Level_1.Price.Steam_Listings_My
                                                      End If
                                                  End Sub)
                                              End Sub)
                             End If
                         End Sub)

            Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub()
                                                                   MainWindow.MainWindow.L_TotalForSale.Content = TotalForSale
                                                                   Binding_Lv_Listings.ReplaceAll(Listings)
                                                                   Binding_Lv_Orders.ReplaceAll(Orders)
                                                               End Sub)
        End Sub

        Public Async Sub GetAndHandle_Keys()
            Dim Keys As New ConcurrentBag(Of GUI.Item)

            Parallel.ForEach(DownloadHandler.SlaveBag,
                         Sub(Slave)
                             If ((Slave.Name IsNot Nothing) AndAlso Regex.IsMatch(Slave.Name, "\bkey\b", RegexOptions.IgnoreCase)) Then
                                 Dim Key As New GUI.Item With {
                                                              .Name = Slave.Name,
                                                              .MarketHashID = Slave.Name,
                                                              .DefIndex = Slave.Defindex,
                                                              .ImageSource = New Uri(Slave.UrlLarge),
                                                              .Quality = New GUI.Steam.Quality With {.ID = 6, .Name = "Unique"},
                                                              .AppID = MainWindow.Settings.Container.CurrentAppID,
                                                              .WikiURL = AssignUrl_Wiki(Slave.Name),
                                                              .Price = New Price() With {.Url_Steam = AssignUrl_Steam(Slave.Name), .Url_Backpack = AssignUrl_Backpack(Slave.Name, Craftable:=False)}
                                                          }

                                 Keys.Add(Key)
                             End If
                         End Sub)

            Dim Unique As List(Of String) = Keys.Select(Function(F) F.Name).Distinct.ToList
            Dim Output As New List(Of GUI.Item)

            For i As Integer = (Unique.Count - 1) To 0 Step -1
                Dim Name = Unique(i)
                Dim Item = Keys.FirstOrDefault(Function(F) F.Name = Name)

                If (Item IsNot Nothing) Then
                    Unique.RemoveAt(i)
                    Output.Add(Item)
                End If
            Next

            Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() Binding_Lv_Keys.ReplaceAll(Output))
        End Sub

        Public Async Sub GetAndHandle_UpForTrade()
            Dim List As List(Of AdvancedItem) = GenerateGUIItemList_ActiveTrades(GetTradeOffers())
            Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() Binding_Lv_UpForTrade.ReplaceAll(List))
            Status("Updated Active Trades!")
        End Sub

        Public Async Sub GetAndHandle_Backpack()
            If (Not String.IsNullOrWhiteSpace(MainWindow.Settings.Container.Backpack_APIKey)) Then
                Dim Backpack_Path As String = Path.Combine(MainWindow.Home, MainWindow.Settings.Container.CurrentGameName, "Backpack.tf Community Prices.xml")

                If File.Exists(Backpack_Path) Then
                    Dim LastUpdate As Date = IO.File.GetLastWriteTime(Backpack_Path)

                    If (Date.Compare(Date.Today.AddDays(-2), LastUpdate) <= 0) Then
                        Dim Backpack_GUI = MainWindow.MainWindow.GenerateGUIItemList_Backpack()
                        DownloadHandler.Start(Backpack_GUI)

                        Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() Binding_Lv_Backpack.ReplaceAll(Backpack_GUI))
                        Status("Current Backpack.tf Community Prices are Not 2 days old yet. Old prices will be used instead.")
                        MainWindow.MainWindow.AssignPrices()
                        Exit Sub
                    End If
                End If

                If GetPriceList_Backpack() Then
                    Dim Backpack_GUI = MainWindow.MainWindow.GenerateGUIItemList_Backpack()
                    DownloadHandler.Start(Backpack_GUI)

                    Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() Binding_Lv_Backpack.ReplaceAll(Backpack_GUI))
                    Status("Backpack.tf prices have been downloaded!")
                    MainWindow.MainWindow.AssignPrices()
                Else
                    Status("Failed to download community prices from Backpack.tf")
                End If
            Else
                Status("Backpack.tf API Key missing. No New prices will be downloaded.")
            End If
        End Sub

        Public Function GenerateGUIItemList_ActiveTrades(List As List(Of AdvancedItem)) As List(Of AdvancedItem)
            If (List.Count > 0) Then
                Parallel.ForEach(List, Sub(Level_1)
                                           With Level_1
                                               .Incoming = ApplyItemCount(GetAssetClassInfo(.Incoming))
                                               .Outgoing = ApplyItemCount(GetAssetClassInfo(.Outgoing))
                                               .Partner = Steam.API.API.GetPlayerSummaries(.Partner)
                                               DownloadHandler.Start(Partner:= .Partner)
                                               DownloadHandler.Start(New ObservableCollection(Of BasicItem)(.Incoming.Concat(.Outgoing)))
                                           End With
                                       End Sub)
            End If

            Return List
        End Function

        Public Function ApplyItemCount(List As List(Of BasicItem)) As List(Of BasicItem)
            Parallel.ForEach(List, Sub(Item)
                                       Dim First As Boolean = True

                                       For Each Item_2 In List
                                           If ((Item.AppID = Item_2.AppID) AndAlso (Item.DefIndex = Item_2.DefIndex) AndAlso (Item.Quality.ID = Item_2.Quality.ID)) Then
                                               If (Not First) Then
                                                   Item.Amount += 1
                                                   Item_2.IsVisible = False
                                               Else
                                                   First = False
                                                   Item.IsVisible = True
                                               End If
                                           End If
                                       Next
                                   End Sub)

            Return List
        End Function

        Public Function GetAssetClassInfo(List As List(Of BasicItem)) As List(Of BasicItem)
            Dim List_AppID As New List(Of Integer)
            Dim ClassInfoBag As New ConcurrentBag(Of Tuple(Of Integer, ClassInfo_Result))

            For Each Item In List
                If (Not List_AppID.Exists(Function(F) F.Equals(Item.AppID))) Then
                    List_AppID.Add(Item.AppID)
                End If
            Next

            Parallel.ForEach(List_AppID,
                       Async Sub(AppID)
                           Dim Download_List As List(Of BasicItem) = List.FindAll(Function(F) F.AppID = AppID)
                           Dim Strings As New ConcurrentBag(Of String)
                           Dim JsonString As String = Nothing

                           Parallel.For(0, Download_List.Count, Sub(i)
                                                                    Dim ClassID As String = String.Format("&classid{0}={1}", i, Download_List(i).ClassID)
                                                                    Dim InstanceID As String = String.Format("&instanceid{0}={1}", i, Download_List(i).InstanceID)
                                                                    Strings.Add(ClassID & InstanceID)
                                                                End Sub)

                           JsonString = String.Join(String.Empty, Strings)

                           Dim ClassURL As Uri = New Uri(String.Format("https://api.steampowered.com/ISteamEconomy/GetAssetClassInfo/v1/?key={0}&format=json&class_count={1}&appid={2}{3}", MainWindow.Settings.Container.Steam_APIKey, Download_List.Count, AppID, JsonString))
                           Dim Download_String As String = Nothing

                           Using Client As New HttpClient
                               Try
                                   Download_String = Await Client.GetAsync(ClassURL).Result.Content.ReadAsStringAsync
                               Catch ex As WebException
                                   Status("FAILED : Fetching ClassInfo")
                                   Exit Sub
                               End Try
                           End Using

                           Download_String = Download_String.Remove(Download_String.Length - 1, 1)

                           Dim i2 As Integer = Download_String.LastIndexOf(","c, Download_String.Length - 1)

                           If ((i2 < Download_String.Length) AndAlso (i2 > -1)) Then
                               Download_String = Download_String.Remove(i2, 1)
                           End If

                           Download_String = Download_String.Replace("""success", "},""success")

                           Dim ClassInfo As ClassInfo_Result = JSON.Deserialize(Of ClassInfo_Result)(Download_String)
                           ClassInfoBag.Add(New Tuple(Of Integer, ClassInfo_Result)(AppID, ClassInfo))
                       End Sub)

            Parallel.ForEach(List, Sub(Level_1)
                                       Dim Level_2 = ClassInfoBag.FirstOrDefault(Function(F) F.Item1 = Level_1.AppID)

                                       If (Level_2 IsNot Nothing) Then
                                           Dim Level_3 = Level_2.Item2.result.FirstOrDefault(Function(F) ((Level_1.ClassID = F.Value.classid) AndAlso (Level_1.InstanceID = F.Value.instanceid)))

                                           If (Not Equals(Level_3, Nothing)) Then
                                               With Level_1
                                                   .MarketHashID = Level_3.Value.market_hash_name
                                                   .ImageDownloadUrl = Level_3.Value.icon_url_large
                                                   .Name = Level_3.Value.name
                                                   .DefIndex = Level_3.Value.app_data.def_index
                                                   .Quality.ID = Level_3.Value.app_data.quality
                                                   .Type = Level_3.Value.app_data.slot

                                                   Dim MightBeNumeric As String = Level_3.Value.type.Split().Last
                                                   If IsNumeric(MightBeNumeric) AndAlso (CInt(MightBeNumeric) > 0) Then
                                                       .HasKills = True
                                                   End If
                                               End With
                                           End If
                                       End If
                                   End Sub)

            Return List
        End Function

        ''' <summary>
        ''' 0 for no Escrow, 1 has Escrow, 2 TradeUrl is no longer valid
        ''' </summary>
        ''' <param name="TradeUrl"></param>
        ''' <returns></returns>
        Public Async Function CheckIfEscrowIfBroken(TradeUrl As String) As Task(Of Integer)
            Dim Download_Url As String = String.Format("https://steamcommunity.com/tradeoffer/new/{0}", TradeUrl)
            Dim Download_String As String = Nothing

            Using Client As New HttpClient(New HttpClientHandler() With {.CookieContainer = Cookie})
                Download_String = Await Client.GetStringAsync(Download_Url)
            End Using

            Dim Search As String = "var g_daysTheirEscrow = "
            Dim EndChar As String = ";"
            Dim Result As String = Nothing
            Dim Invalid As String = "This Trade URL is no longer valid for sending a trade offer to"
            Dim Validity As Boolean = (Download_String.IndexOf(Invalid) > 0)

            FindString(Download_String, Search, Result, EndChar)

            If (Validity = True) Then
                Return 2
            End If

            If (CInt(Result) > 0) Then
                Return 1
            End If

            Return 0
        End Function

        Public Function GetPriceList_Backpack() As Boolean
            Dim SaveDirectory As String = Path.Combine(Home, MainWindow.Settings.Container.CurrentGameName)
            Dim Download_String_BackPack_Response As String = Nothing

            Using Client As New WebClient
                Try
                    Download_String_BackPack_Response = Client.DownloadString("https://backpack.tf/api/IGetPrices/v4?key=" + MainWindow.Settings.Container.Backpack_APIKey)
                Catch ex As WebException
                    Status(ex.Message)
                    Return False
                End Try
            End Using

            Dim BackPack_Response = JSON.Deserialize(Of Response)(Download_String_BackPack_Response.Replace(":null", ":""0"""))

            If ((BackPack_Response.Response.success) = "1"c) Then
                Dim BackPack_Collection As New Backpack.API.Collection

                IO.Directory.CreateDirectory(SaveDirectory)

                Dim XmlFilePath = Path.Combine(SaveDirectory, "Backpack.tf Community Prices.xml")

                With BackPack_Collection
                    .Success = BackPack_Response.Response.success
                    .Current_time = BackPack_Response.Response.current_time
                    .Raw_USD_Value = BackPack_Response.Response.raw_usd_value
                    .Usd_Currency = BackPack_Response.Response.usd_currency
                    .Usd_Currency_Index = BackPack_Response.Response.usd_currency_index

                    MainWindow.Settings.Container.Backpack_Price_Refined = CDec(.Raw_USD_Value)
                End With

                Dim Done As New Boolean

                Dim ResponseItem = BackPack_Response.Response.Items.FirstOrDefault(Function(F) F.Value.defindex.ToList.Contains(DefIndex.Key.ToString))
                If (Not Equals(ResponseItem, Nothing)) Then
                    Dim KeyPrice As Decimal = CDec(ResponseItem.Value.prices.First.Value.Tradable.Craftable.First.Value.value)
                    MainWindow.Settings.Container.Backpack_Price_Key_InMetal = KeyPrice
                    BackPack_Collection.KeyPriceInMetal = KeyPrice
                End If

                Dim Backpack_Collection_Items As New ConcurrentBag(Of Backpack.API.Item)

                Parallel.ForEach(BackPack_Response.Response.Items,
                                 Sub(Item)
                                     Dim Backpack_Item As New Backpack.API.Item
                                     Dim Import_Prices As New ConcurrentBag(Of Backpack.API.Quality)

                                     Parallel.ForEach(Item.Value.prices,
                                                      Sub(Price)
                                                          Dim Quality As New Backpack.API.Quality With {.ID = Price.Key}
                                                          Quality.Name = GetQualityName(Quality.ID)

                                                          With Quality
                                                              If (Price.Value.Tradable IsNot Nothing) Then
                                                                  If (Price.Value.Tradable.Craftable IsNot Nothing) Then
                                                                      .Tradable.Craftable = MainWindow.MainWindow.To_XML_Craftable(Price.Value.Tradable.Craftable)
                                                                  Else
                                                                      .Tradable.Craftable = Nothing
                                                                  End If

                                                                  If (Price.Value.Tradable.NonCraftable IsNot Nothing) Then
                                                                      .Tradable.NonCraftable = MainWindow.MainWindow.To_XML_Craftable(Price.Value.Tradable.NonCraftable)
                                                                  Else
                                                                      .Tradable.NonCraftable = Nothing
                                                                  End If
                                                              Else
                                                                  .Tradable = Nothing
                                                              End If

                                                              If (Price.Value.NonTradable IsNot Nothing) Then
                                                                  If (Price.Value.NonTradable.Craftable IsNot Nothing) Then
                                                                      .NonTradable.Craftable = MainWindow.MainWindow.To_XML_Craftable(Price.Value.NonTradable.Craftable)
                                                                  Else
                                                                      .NonTradable.Craftable = Nothing
                                                                  End If

                                                                  If (Price.Value.NonTradable.NonCraftable IsNot Nothing) Then
                                                                      .NonTradable.NonCraftable = MainWindow.MainWindow.To_XML_Craftable(Price.Value.NonTradable.NonCraftable)
                                                                  Else
                                                                      .NonTradable.Craftable = Nothing
                                                                  End If
                                                              Else
                                                                  .NonTradable = Nothing
                                                              End If
                                                          End With

                                                          Import_Prices.Add(Quality)
                                                      End Sub)

                                     Backpack_Item.DefIndex = CInt(Item.Value.defindex.FirstOrDefault)
                                     Backpack_Item.MarketHashID = Item.Key
                                     Backpack_Item.Prices = Import_Prices.ToList
                                     Backpack_Collection_Items.Add(Backpack_Item)
                                 End Sub)

                BackPack_Collection.Items = Backpack_Collection_Items.ToList

                XML.Serialize(Path.Combine(SaveDirectory, "Backpack.tf Community Prices.xml"), BackPack_Collection)

                Return True
            Else
                Return False
            End If
        End Function

        Public Sub SendPostRequest(Data As String, Url As String, Optional Secure As Boolean = False, Optional SendOfferReferer As String = Nothing, Optional ByRef Result As String = Nothing)
            Dim Request_Data = Encoding.UTF8.GetBytes(Data)
            Dim Request = DirectCast(WebRequest.Create(Url), HttpWebRequest)

            Request.ContentLength = Request_Data.Length
            Request.CookieContainer = Cookie
            Request.Method = "POST"
            Request.Proxy = Nothing
            Request.Timeout = 30000

            Request.AutomaticDecompression = DecompressionMethods.GZip
            Request.Host = "steamcommunity.com"
            Request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36"
            Request.Accept = "text/javascript, text/html, application/xml, text/xml, application/json, */*"
            Request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8"
            Request.Headers.Add("Accept-Encoding", "gzip, deflate")
            Request.Headers.Add("Accept-Language", "en-US, en;q=0.8, en-US;q=0.5")

            If String.IsNullOrWhiteSpace(SendOfferReferer) Then
                If (Not Secure) Then
                    Request.Referer = "http://steamcommunity.com/my/inventory/"
                Else
                    Request.Referer = "https://steamcommunity.com/my/inventory/"
                End If
            Else
                Request.Referer = SendOfferReferer
            End If

            Status("Sending Post Request...")
            Using Stream = Request.GetRequestStream()
                Stream.Write(Request_Data, 0, Request_Data.Length)
            End Using

            Try
                Using Response As HttpWebResponse = DirectCast(Request.GetResponse(), HttpWebResponse)
                    Using Stream = New StreamReader(Response.GetResponseStream())
                        Result = Stream.ReadToEnd()
                    End Using
                End Using
            Catch e As WebException
                Result = e.Message
                Status("FAILED : " + e.Message)
            Finally
                Status("Post Request Completed")
            End Try
        End Sub

        Public Shared Function GetSessionID(CookieContainer As CookieContainer) As String
            For Each Cookie As Cookie In CookieContainer.GetCookies(New Uri("http://steamcommunity.com/market/"))
                If (Cookie.Name = "sessionid") Then
                    Return Cookie.Value
                End If
            Next

            Return Nothing
        End Function

        Public Shared Sub InsertSteamCookie()
            Cookie.Add(New Cookie("ActListPageSize", "100", "/"c, "steamcommunity.com"))
        End Sub

        Public Sub ActivateFailSafe(FailSafe As ObservableCollection(Of GUI.Item))
            For Each Level_1 In FailSafe
                Dim Item = FailSafe.FirstOrDefault(Function(F) (F.AssetID = Level_1.AssetID) AndAlso (F.AppID = Level_1.AppID))

                If (Item IsNot Nothing) Then
                    Item.InTransit = False
                End If
            Next
        End Sub

        'Add Items manually from your's and partner's inventory
        Public Sub ManualTrading()

        End Sub

        Public Sub BuyItem_Steam(ListingID As String, Subtotal As Decimal, Fee As Decimal, Total As Decimal, Optional Amount As Integer = 1)
            Dim Url As String = String.Format("https://steamcommunity.com/market/buylisting/{0}/", ListingID)
            Dim Data As String = String.Format("sessionid={0}&currency={1}&subtotal={2}&fee={3}&total={4}&quantity={5}", GetSessionID(Cookie), 3, ConvertToSteamNumber(Subtotal), ConvertToSteamNumber(Fee), ConvertToSteamNumber(Total), Amount)
            SendPostRequest(Data, Url)
        End Sub

        Public Async Sub SellItem_Relist(List As List(Of GUI.Item))
            Dim List_AppID As New List(Of Integer)
            Dim List_Old As New List(Of GUI.Item)
            Dim List_New As New List(Of GUI.Item)
            Dim List_ToSell As New List(Of GUI.Item)

            For Each Item As GUI.Item In List
                If (Item.IsSelected AndAlso (Not Item.Price.RelistHasError)) Then
                    List_ToSell.Add(Item)
                    List_AppID.Add(Item.AppID)
                End If
            Next

            List_AppID = List_AppID.Distinct.ToList

            For Each AppID In List_AppID
                List_Old.AddRange(Await Inventory.Get(AppID, ForSelf:=True))
            Next
            For Each Item In List_ToSell
                RemoveListing(Item.Price.Listing_ID_Econ)
            Next
            For Each AppID In List_AppID
                List_New.AddRange(Await Inventory.Get(AppID, ForSelf:=True))
            Next

            For i As Integer = (List_New.Count - 1) To 0 Step -1
                Dim NewItem = List_New(i)
                Dim Item = List_Old.FirstOrDefault(Function(F) (F.AssetID = NewItem.AssetID) AndAlso (F.AppID = NewItem.AppID))

                If (Item IsNot Nothing) Then
                    List_New.RemoveAt(i)
                End If
            Next

            For iSell As Integer = (List_ToSell.Count - 1) To 0 Step -1
                For iNew As Integer = (List_New.Count - 1) To 0 Step -1
                    If (Equals(List_ToSell(iSell).MarketHashID, List_New(iNew).MarketHashID) AndAlso Equals(List_ToSell(iSell).AppID, List_New(iNew).AppID)) Then
                        List_ToSell(iSell).AssetID = List_New(iNew).AssetID
                        List_New.RemoveAt(iNew)
                        Exit For
                    End If
                Next
            Next

            For Each Item In List_ToSell
                SellItem(Item.AppID, Item.AssetID, Item.Price.RelistListingAt)
            Next
        End Sub

        Public Sub SellItem(AppID As Integer, AssetID As String, SellPrice As Decimal, Optional Quantity As Integer = 1, Optional ContextID As String = "2", Optional ByRef Result As String = Nothing)
            Dim Url As String = "https://steamcommunity.com/market/sellitem/"
            Dim Data As String = String.Format("sessionid={0}&appid={1}&contextid={2}&assetid={3}&amount={4}&price={5}", GetSessionID(Cookie), AppID, ContextID, AssetID, Quantity, ConvertToSteamNumber(SellPrice))
            SendPostRequest(Data, Url, Result:=Result)
        End Sub

        Public Sub RemoveListing(ID As String)
            Dim Url As String = String.Format("http://steamcommunity.com/market/removelisting/{0}/", ID)
            Dim Data As String = String.Format("sessionid={0}", GetSessionID(Cookie))
            SendPostRequest(Data, Url)
        End Sub

        Public Sub CreateBuyOrder(AppID As Integer, MarketHashID As String, TotalCost As Decimal, Quantity As Integer)
            Dim Url As String = "https://steamcommunity.com/market/createbuyorder/"
            Dim Data As String = String.Format("sessionid={0}&currency={1}&appid={2}&market_hash_name={3}&price_total={4}&quantity={5}", GetSessionID(Cookie), 3, AppID, MarketHashID, ConvertToSteamNumber(TotalCost), Quantity)
            SendPostRequest(Data, Url, True)
        End Sub

        Public Sub CancelBuyOrder(ID As String)
            Dim Url As String = "http://steamcommunity.com/market/cancelbuyorder/"
            Dim Data As String = String.Format("sessionid={0}&buy_orderid={1}", GetSessionID(Cookie), ID)
            SendPostRequest(Data, Url)
        End Sub

        Public Sub Crafting_Metal(Metal As Metal)

        End Sub

    End Class

End Namespace
