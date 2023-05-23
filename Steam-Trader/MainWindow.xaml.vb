Imports System.Collections.Concurrent
Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Windows.Threading
Imports SteamTrader
Imports SteamTrader.GUI
Imports SteamTrader.Modify.Multiple
Imports SteamTrader.Network
Imports SteamTrader.Network.Steam.API
Imports SteamTrader.Network.Steam.API.Schema
Imports SteamTrader.Settings
Imports SteamTrader.TeamFortress2
Imports U
Imports U.PropertyManipulation
Imports U.StringManipulation

Public Class MainWindow
    Implements INotifyPropertyChanged
    Implements IDisposable

    Public Shared MainWindow As MainWindow
    Public Shared Property Settings As New Settings.Settings
    Public Shared Property Subroutines As New Settings.Subroutines
    Public Shared TwoFactorAuth As String = Nothing
    Public Shared CaptchaCode As String = Nothing
    Private AuthAppWindow As ConfirmAuthWindow
    Public Shared BindingMasterList As New List(Of ObservableCollection(Of Item))
    Public Shared Home As String = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
    Public Shared DownloadHandler As DownloadManager
    Public Shared SelectionInProgress As New Boolean
    Private TabControl_Main_PreviousIndex As Integer
    Public Shared Property Network As New Network.Network
    Public Shared Property Binding_Lv_Prices As New ObservableCollection(Of Item)
    Public Shared Property Binding_Lv_Cart As New ObservableCollection(Of Item)
    Public Shared Property Binding_Lv_Backpack As New ObservableCollection(Of Backpack.Price)
    Public Shared Property Binding_Lv_Favorite As New ObservableCollection(Of Item)
    Public Shared Property Binding_Lv_Keys As New ObservableCollection(Of Item)
    Public Shared Property Binding_Lv_Inventory As New ObservableCollection(Of Item)
    Public Shared Property Binding_Lv_Listings As New ObservableCollection(Of Item)
    Public Shared Property Binding_Lv_Orders As New ObservableCollection(Of Item)
    Public Shared Property Binding_Lv_UpForTrade As New ObservableCollection(Of AdvancedItem)
    Public Shared Property Binding_Lv_WishList As New ObservableCollection(Of Item)
    Public Shared Property Binding_Lv_GiftList As New ObservableCollection(Of Item)
    Public Shared Archive_Steam As New Dictionary(Of String, List(Of Archive.Steam))
    Public Shared Archive_Backpack As New Dictionary(Of String, Archive.Backpack)

    Public Shared Binding_Cbx_GameList As New List(Of String)
    Public Property GameList() As ObservableCollection(Of Game)

    Public Property SelectQualities As New List(Of String)
    Public Property SelectParticleEffects As New List(Of String)
    Public Property SelectPaints As New List(Of String)
    Public Shared Qualities As New List(Of IDName)
    Public Shared ParticleEffects As New List(Of IDName)
    Public Shared Paints As New List(Of Paint)
    Public Shared Counters As New List(Of Counter)

    Public Enum LogInState
        LogIn = 0
        Cancel = 1
        LogOff = 2
        LogOut = 3
        ReLogIn = 4
        ChangeUser = 5
    End Enum

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        FrameworkElement.StyleProperty.OverrideMetadata(GetType(Window), New FrameworkPropertyMetadata() With {.DefaultValue = FindResource(GetType(Window))})
        Dim Culture As CultureInfo = CultureInfo.CreateSpecificCulture("en-US")
        Thread.CurrentThread.CurrentCulture = Culture
        Thread.CurrentThread.CurrentUICulture = Culture
        MainWindow = DirectCast(Windows.Application.Current.MainWindow, MainWindow)
        DataContext = Me

        ' TODO : Fix this(requires alot of work :d)
        ' If (Network.IsLoggedIn) Then
        DownloadHandler = New DownloadManager
        ' End If

        Create_GameList()
        CreateSchemaList()
        Set_SettingsDefaultValues()
        Backpack.API.API.GetEssentialPrices()

        Binding_AllDataContext()
        SetDataGridFilter()
        Create_ConfirmAuthWindow()

        AddTemp()

        If (My.Settings.LastUserID <> Nothing) Then
            Settings.Container.CurrentUser.SteamID = My.Settings.LastUserID
            Settings.Container.CurrentUser.AccountName = My.Settings.LastUserName
            Status("You're offline : " + Settings.Container.CurrentUser.AccountName)
            Load()

            If My.Settings.AutoLogon Then
                Progress(10)
                Settings.Container.LoginButtonSetting = LogInState.Cancel

                Network.ConnectionWorker.WorkerSupportsCancellation = True
                Network.ConnectionWorker.RunWorkerAsync()
            Else
                Settings.Container.ReLoginPossible = True
                Settings.Container.LoginButtonSetting = LogInState.ReLogIn
            End If
        End If

    End Sub

    Sub Create_GameList()
        Binding_Cbx_GameList.Add("Team Fortress 2")
        Binding_Cbx_GameList.Add("Trading Cards")

        MainWindow.MainWindow.Cbx_GameList.SelectedIndex() = 0
    End Sub
    ' When you don't wanna type in stuff all the time...
    Public Sub AddTemp()
        My.Settings.LastUserID = 0
        My.Settings.LastUserName = ""
        My.Settings.AutoLogon = False
        SteamTrader.Network.Network.Username = ""
        SteamTrader.Network.Network.Password = ""

        RenderOptions.ProcessRenderMode = Interop.RenderMode.SoftwareOnly
    End Sub

    Private Sub Window_KeyDown(sender As Object, e As KeyEventArgs)
        If (e.Key = Key.F11) Then
            If (WindowStyle <> WindowStyle.SingleBorderWindow) Then
                ResizeMode = ResizeMode.CanResize
                WindowStyle = WindowStyle.SingleBorderWindow
                WindowState = WindowState.Normal
                Topmost = False
            Else
                ResizeMode = ResizeMode.NoResize
                WindowStyle = WindowStyle.ToolWindow
                WindowState = WindowState.Maximized
                Topmost = True
            End If
        End If
    End Sub

    Private Sub Button_LogIn_Cbx_Item_Click(sender As Object, e As RoutedEventArgs)
        Dim Item As MenuItem = CType(sender, MenuItem)
        Dim LoginState = DirectCast(Item.Tag, LogInState)

        If (LoginState < 4) Then
            Settings.Container.LoginButtonSetting = LoginState
        Else
            Dim ChangeUser As New ChangeUserWindow With {.Owner = Me}
            ChangeUser.ShowDialog()

            If (ChangeUser.DialogResult = True) Then
                Dim SelectedUser As User = ChangeUser.Users.Where(Function(F) F.IsSelected).FirstOrDefault
                Network.SetDefaults()
                Settings.Container.CurrentUser = SelectedUser
                Settings.Container.LoginButtonSetting = LogInState.ReLogIn
                SteamTrader.Network.Network.Username = SelectedUser.AccountName
                My.Settings.LastUserName = SelectedUser.AccountName
                My.Settings.LastUserID = SelectedUser.SteamID
                Network.Btn_Login_Click(Nothing, Nothing)
            End If
        End If
    End Sub

    Private Sub Button_LogIn_Cbx_Click(sender As Object, e As RoutedEventArgs)
        Dim Button As Button = CType(sender, Button)
        With Button.ContextMenu
            .IsEnabled = True
            .PlacementTarget = Button
            .Placement = Primitives.PlacementMode.Bottom
            .IsOpen = True
        End With
    End Sub

    Private Sub Btn_Search_Click(sender As Object, e As RoutedEventArgs)
        Txb_URL.IsEnabled = False
        CheckURL()
    End Sub

    Public Function GetSelectedDataGrid(Optional Tabcontrol As TabControl = Nothing) As DataGrid
        If (Tabcontrol Is Nothing) Then
            Tabcontrol = TabControl_Main
        End If

        Dim TabItem As TabItem = TryCast(Tabcontrol.Items(Tabcontrol.SelectedIndex), TabItem)
        Dim SelectedDataGrid As DataGrid = TryCast(TabItem.Content, DataGrid)
        Return SelectedDataGrid
    End Function

    Public Function GetSelectedDataGridSource(Optional Tabcontrol As TabControl = Nothing) As ICollection
        If (Tabcontrol Is Nothing) Then
            Tabcontrol = TabControl_Main
        End If

        Dim TabItem As TabItem = CType(Tabcontrol.SelectedItem, TabItem)
        Dim SelectedDataGrid As DataGrid = TryCast(TabItem.Content, DataGrid)
        Dim HostDataContext = SelectedDataGrid.DataContext
        Return DirectCast(HostDataContext, ICollection)
    End Function

    Private Async Sub CheckURL()
        Dim URLstring As String = Txb_URL.Text

        If (Not String.IsNullOrWhiteSpace(URLstring)) Then
            Dim Index As New Integer
            Dim Backpack As String = "backpack.tf/stats/"
            Dim SteamCommunity As String = "steamcommunity.com"
            Dim TF2Wiki As String = "wiki.teamfortress.com/wiki/"

            'Gets Item Info
            'Gets Quality and Item name
            If URLstring.Contains(Backpack) Then
                Index = 0
                Dim Quality As String = Nothing
                Dim Name As String = Nothing
                Dim Tradable As String = Nothing
                Dim Craftable As String = Nothing
                FindString(URLstring, "stats/", Quality, "/"c)
                FindString(URLstring, Quality + "/", Name, "/"c)
                FindString(URLstring, Name + "/", Tradable, "/"c)
                FindString(URLstring, Tradable + "/", Craftable, Nothing)

                Await Task.Run(
                    Sub()
                        Dim RetrievalUrl As String = AssignUrl_Backpack(Name, Quality, CBool(Tradable), CBool(Craftable)).OriginalString
                        Dim Download_String As String = Nothing

                        Using Client As New WebClient()
                            Download_String = Client.DownloadString(RetrievalUrl)
                        End Using

                        Dim Node_Start As String = "<a class=""price-box"" href="""
                        Dim Node_End As String = """ data-tip=""top"" target=""_blank"" title=""Steam Community Market"">"
                        Dim Line = Download_String.Split(CType(Environment.NewLine, Char())).FirstOrDefault(Function(F) (F.Contains(Node_Start) AndAlso F.Contains(Node_End)))

                        If (Line IsNot Nothing) Then
                            FindString(Line, Node_Start, URLstring, Node_End, True)
                        End If

                        FindString(URLstring, "/market/listings/" + Settings.Container.CurrentAppID.ToString + "/", URLstring, Nothing, True)
                    End Sub)

                'Gets name from listings
            ElseIf URLstring.Contains("/market/listings/") Then
                Index = 0
                If (URLstring.Last = "/"c) Then
                    FindString(URLstring, "/market/listings/", URLstring, "/"c, True)
                Else
                    FindString(URLstring, "/market/listings/", URLstring, Nothing, True)
                End If

                URLstring = Web.HttpUtility.UrlDecode(URLstring)

            ElseIf URLstring.Contains(TF2Wiki) Then
                Index = 0
                FindString(URLstring, TF2Wiki, URLstring, "/"c, True)
                URLstring = Net.WebUtility.UrlDecode(URLstring).Replace("_", " ")

                'Gets SteamUser inventory for current game
                'Gets SteamCommunity name then gets the ID
            ElseIf URLstring.Contains(SteamCommunity) Then
                Index = 1
                If URLstring.Contains("/id/") Then
                    FindString(URLstring, (SteamCommunity + "/id/"), URLstring, "/"c, True)

                    Await Task.Run(Sub()
                                       Dim RetrievalUrl As String = String.Format("http://steamcommunity.com/id/{0}/?xml=1", URLstring)

                                       Using Client As New WebClient()
                                           Dim ProfileInfo As String = Client.DownloadString(RetrievalUrl)
                                           FindString(ProfileInfo, "<steamID64>", URLstring, "</steamID64>", True)
                                       End Using
                                   End Sub)

                    'Gets ID from profiles
                ElseIf URLstring.Contains("/profiles/") Then
                    If (URLstring.Last = "/"c) Then
                        FindString(URLstring, "/profiles/", URLstring, "/"c, True)
                    Else
                        FindString(URLstring, "/profiles/", URLstring, Nothing, True)
                    End If

                End If

                'Allows 64bit SteamID to get inventory
            ElseIf ULong.TryParse(URLstring, Nothing) Then
                Index = 1
            End If

            If (Not String.IsNullOrWhiteSpace(URLstring)) Then
                If (Index = 0) Then
                    Await Task.Run(Sub() Scraper.Search(URLstring))

                ElseIf (Index = 1) Then
                    Dim SteamID As ULong = Nothing

                    If (ULong.TryParse(RemoveExtraText(URLstring), SteamID)) Then
                        Dim UserInventory As New ObservableCollection(Of Item)
                        Await Task.Run(New Action(Async Sub()
                                                      UserInventory = Await Inventory.Get(SteamID:=SteamID, ForUser:=True)

                                                      If (UserInventory IsNot Nothing) Then
                                                          DownloadHandler.Start(UserInventory)
                                                      End If
                                                  End Sub))

                        If (UserInventory IsNot Nothing) Then
                            Binding_Lv_Prices.ReplaceAll(UserInventory)
                        End If
                    Else
                        Status("SteamID invalid.")
                    End If
                End If
            End If
        Else
            Status("Please supply an Item link or name.")
        End If

        Txb_URL.Clear()
        Txb_URL.IsEnabled = True
    End Sub

    Private Sub Btn_OpenAuthApp_Click(sender As Object, e As RoutedEventArgs)
        If (Not MainWindow.Settings.Container.AuthAppOpen) Then
            MainWindow.Settings.Container.AuthAppOpen = True
            AuthAppWindow.Dispatcher.BeginInvoke(Sub()
                                                     AddHandler AuthAppWindow.AffirmClosure, AddressOf EnableAuthReOpening
                                                     AuthAppWindow.Show()
                                                 End Sub)
        Else
            AuthAppWindow.Dispatcher.BeginInvoke(Sub() AuthAppWindow.Close())
        End If
    End Sub

    Private Sub Create_ConfirmAuthWindow()
        Dim Thread As New Thread(Sub()
                                     AuthAppWindow = New ConfirmAuthWindow()
                                     Dispatcher.Run()
                                 End Sub)
        Thread.SetApartmentState(ApartmentState.STA)
        Thread.IsBackground = True
        Thread.Start()
    End Sub

    Public Sub EnableAuthReOpening()
        MainWindow.Settings.Container.AuthAppOpen = False
    End Sub

    Private Sub Txb_InputButtonClicked_Click(sender As Object, e As RoutedEventArgs)
        Dim HostDataContext As SettingsItem = DirectCast(DirectCast(sender, Button).DataContext, SettingsItem)
        CallByName(Subroutines, HostDataContext.Subroutine, CallType.Method)
    End Sub

    Private Sub Binding_AllDataContext()
        Lv_Prices.DataContext = Binding_Lv_Prices
        Lv_Prices.ItemsSource = Binding_Lv_Prices

        Lv_Cart.DataContext = Binding_Lv_Cart
        Lv_Cart.ItemsSource = Binding_Lv_Cart

        Lv_Backpack.DataContext = Binding_Lv_Backpack
        Lv_Backpack.ItemsSource = Binding_Lv_Backpack

        Lv_Favorite.DataContext = Binding_Lv_Favorite
        Lv_Favorite.ItemsSource = Binding_Lv_Favorite

        Lv_Keys.DataContext = Binding_Lv_Keys
        Lv_Keys.ItemsSource = Binding_Lv_Keys

        Lv_Inventory.DataContext = Binding_Lv_Inventory
        Dim View_Inventory As New ListCollectionView(Binding_Lv_Inventory)
        View_Inventory.GroupDescriptions.Add(New PropertyGroupDescription("DefIndex"))
        Lv_Inventory.ItemsSource = View_Inventory

        Lv_Listings.DataContext = Binding_Lv_Listings
        Dim View_Listings As New ListCollectionView(Binding_Lv_Listings)
        View_Listings.GroupDescriptions.Add(New PropertyGroupDescription("Name"))
        Lv_Listings.ItemsSource = View_Listings

        Lv_Orders.DataContext = Binding_Lv_Orders
        Lv_Orders.ItemsSource = Binding_Lv_Orders

        Lv_UpForTrade.DataContext = Binding_Lv_UpForTrade
        Lv_UpForTrade.ItemsSource = Binding_Lv_UpForTrade

        Lv_WishList.DataContext = Binding_Lv_WishList
        Lv_WishList.ItemsSource = Binding_Lv_WishList

        Lv_GiftList.DataContext = Binding_Lv_GiftList
        Lv_GiftList.ItemsSource = Binding_Lv_GiftList

        Lv_Settings.DataContext = Settings.Collection
        Lv_Settings.ItemsSource = Settings.Collection

        Cbx_GameList.DataContext = Binding_Cbx_GameList
        Cbx_GameList.ItemsSource = Binding_Cbx_GameList

        With BindingMasterList
            .Add(Binding_Lv_Prices)
            .Add(Binding_Lv_Cart)
            .Add(Binding_Lv_Keys)
            .Add(Binding_Lv_Favorite)
            .Add(Binding_Lv_Inventory)
            .Add(Binding_Lv_Listings)
            .Add(Binding_Lv_Orders)
            .Add(Binding_Lv_GiftList)
            .Add(Binding_Lv_WishList)
        End With
    End Sub

    Private Async Sub Btn_Save_Click(sender As Object, e As RoutedEventArgs) Handles Btn_Save.Click
        If (Not Settings.Container.IsRunning.Saving) Then
            Settings.Container.IsRunning.Saving = True

            Await Task.Run(Sub()
                               Dim Document As New UserDocument With
                                             {
                                                .Favorites = Binding_Lv_Favorite.ToList,
                                                .GiftList = Binding_Lv_GiftList.ToList,
                                                .Inventory = Binding_Lv_Inventory.ToList,
                                                .WishList = Binding_Lv_WishList.ToList,
                                                .Settings = Settings.Collection.ToList
                                            }

                               Dim Path As String = IO.Path.Combine(Home, Settings.Container.CurrentGameName, SteamTrader.Network.Network.Username + ".xml")

                               If Document.Save(Path) Then
                                   UpdateSaveStamp(IO.File.GetLastWriteTime(Path).ToString)
                                   Status("Save Successful!")
                               Else
                                   Status("Save FAILED.")
                               End If
                           End Sub)

            Settings.Container.IsRunning.Saving = False
        End If
    End Sub

    Public Async Sub Load()
        If (Not Settings.Container.IsRunning.Load) Then
            Settings.Container.IsRunning.Load = True
            Dim Path As String = IO.Path.Combine(Home, Settings.Container.CurrentGameName, SteamTrader.Network.Network.Username + ".xml")

            If (IO.File.Exists(Path) AndAlso TestLoad(Path)) Then
                Dim Document As New UserDocument
                Await Task.Run(Sub() Document = XML.Read(Of UserDocument)(Path))

                If (Not Equals(Settings.Container.CurrentUser.SteamID, Nothing)) Then
                    Dim UserPath As String = IO.Path.Combine(Home, "Users", Settings.Container.CurrentUser.SteamID.ToString, Settings.Container.CurrentUser.SteamID.ToString + ".xml")

                    If IO.File.Exists(UserPath) Then
                        Settings.Container.CurrentUser = XML.Read(Of User)(UserPath)
                        Settings.Container.CurrentUser.ImageSource = New Uri(IO.Path.Combine(Home, "Users", Settings.Container.CurrentUser.SteamID.ToString, Settings.Container.CurrentUser.SteamID.ToString + ".jpg"))
                        Settings.Container.CurrentUser.Link = New Uri("https://steamcommunity.com/profiles/" + Settings.Container.CurrentUser.SteamID.ToString)
                    End If
                End If

                Await Dispatcher.BeginInvoke(Sub()
                                                 With Document
                                                     If (DownloadHandler IsNot Nothing) Then
                                                         DownloadHandler.Start(New List(Of Item)({ .Favorites, .GiftList, .Inventory, .WishList}.SelectMany(Function(F) F.SelectAll)))
                                                     End If
                                                     Binding_Lv_Favorite.ReplaceAll(.Favorites)
                                                     Binding_Lv_GiftList.ReplaceAll(.GiftList)
                                                     Binding_Lv_Inventory.ReplaceAll(.Inventory)
                                                     Binding_Lv_WishList.ReplaceAll(.WishList)
                                                     Import(Settings.Collection, .Settings)
                                                     UpdateSaveStamp(IO.File.GetLastWriteTime(Path).ToString)
                                                 End With

                                                 If (Settings.Container.AutoGetBackpack AndAlso Network.IsLoggedIn) Then
                                                     Task.Run(Sub()
                                                                  Network.GetAndHandle_Backpack()
                                                                  Network.GetAndHandle_Keys()
                                                                  UpdateMetalCount()
                                                                  AssignUrls_MasterList()
                                                                  AssignHave()
                                                              End Sub)
                                                 End If
                                             End Sub)
            Else
                Status("Userdocument is Unreadable.")
            End If

            Settings.Container.IsRunning.Load = False
        End If
    End Sub

    Public Sub AssignHave()
        Dim Uniques As List(Of Item) = Binding_Lv_Inventory.DistinctBy(Function(F) F.MarketHashID).ToList

        Parallel.ForEach(Uniques, Sub(Unique) Unique.HaveAmount = 0)
        Parallel.ForEach(Uniques, Sub(Unique)
                                      Unique.HaveAmount += Binding_Lv_Inventory.AsEnumerable.Count(Function(F) F.MarketHashID = Unique.MarketHashID)

                                      Parallel.ForEach(BindingMasterList, Sub(List)
                                                                              Parallel.ForEach(List, Sub(Item)
                                                                                                         If Equals(Unique.MarketHashID, Item.MarketHashID) Then
                                                                                                             Item.HaveAmount = Unique.HaveAmount
                                                                                                         End If
                                                                                                     End Sub)
                                                                          End Sub)
                                  End Sub)
    End Sub

    Private Sub Txb_URL_TextChanged(sender As Object, e As TextChangedEventArgs)
        Timer_InputUrl.Stop()
        Timer_InputUrl.Start()
    End Sub

    Private WithEvents Timer_InputUrl As New Timers.Timer With {.Interval = 400}

    Private Sub Timer_InputUrl_Tick(sender As Object, e As EventArgs) Handles Timer_InputUrl.Elapsed
        Timer_InputUrl.Stop()

        Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, Sub()
                                                                       Dim SelectedDataGrid As DataGrid = GetSelectedDataGrid()
                                                                       Dim DataView As ICollectionView = CollectionViewSource.GetDefaultView(SelectedDataGrid.ItemsSource)
                                                                       DataView.Refresh()
                                                                   End Sub)
    End Sub

    Private Sub SetDataGridFilter()
        Dim List As New List(Of DataGrid)
        With List
            .Add(Lv_Prices)
            .Add(Lv_Backpack)
            .Add(Lv_Favorite)
            .Add(Lv_Keys)
            .Add(Lv_UpForTrade)
            .Add(Lv_Listings)
            .Add(Lv_WishList)
            .Add(Lv_GiftList)
        End With

        For Each SubList As DataGrid In List
            CollectionViewSource.GetDefaultView(SubList.ItemsSource).Filter = (New Predicate(Of Object)(AddressOf UserFilter))
        Next
    End Sub

    Public Function UserFilter(FilterItem As Object) As Boolean
        Dim Search As String = Txb_URL.Text
        If String.IsNullOrEmpty(Search) Then
            Return True
        End If

        Dim SortProperty As String = "Name"
        Dim List_AdditionalProperties As New List(Of String)

        'not fully done
        If Search.Contains("&&") Then
            List_AdditionalProperties.AddRange(Search.Split(CType("&&", Char())))
        End If

        If Search.Contains("::") Then
            FindString(Search, Nothing, SortProperty, "::", True)
            FindString(Search, "::", Search, Nothing, True)

            Dim Column = GetSelectedDataGrid.Columns.FirstOrDefault(Function(F) F.SortMemberPath.IndexOf(SortProperty, StringComparison.InvariantCultureIgnoreCase) > -1)

            If Column IsNot Nothing Then
                SortProperty = Column.SortMemberPath
            End If
        End If

        If (String.IsNullOrEmpty(Search) OrElse String.IsNullOrWhiteSpace(SortProperty)) Then
            Return True
        End If

        If SortProperty.Contains("."c) Then
            Return String.Equals(GetNestedProperty(SortProperty, FilterItem).ToString, Search, StringComparison.InvariantCultureIgnoreCase)
        Else
            If (FilterItem.GetType.GetProperty(SortProperty, BindingFlags.Public Or BindingFlags.Instance Or BindingFlags.IgnoreCase) Is Nothing) Then
                Return True
            Else
                Return (FilterItem.GetType.GetProperty(SortProperty, BindingFlags.Public Or BindingFlags.Instance Or BindingFlags.IgnoreCase).GetValue(FilterItem).ToString.IndexOf(Search, 0, StringComparison.CurrentCultureIgnoreCase) > -1)
            End If
        End If

        Return True
    End Function

    Private Sub TabControl_Main_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If (TypeOf e.OriginalSource Is TabControl) Then
            Dim TabControl As TabControl = DirectCast(sender, TabControl)
            Dim TabItem As TabItem = DirectCast(TabControl.SelectedItem, TabItem)
            PropertyBrushOFF()

            If Windows.Application.Current.MainWindow.IsLoaded Then
                If (Settings.Container.Independent_Search_Query) Then
                    If (TabControl_Main_PreviousIndex <> TabControl.SelectedIndex) Then
                        DirectCast(TabControl.Items.GetItemAt(TabControl_Main_PreviousIndex), TabItem).Tag = Txb_URL.Text
                        Txb_URL.Text = TabItem.Tag?.ToString
                    End If

                    TabControl_Main_PreviousIndex = TabControl.SelectedIndex
                End If
            End If

            Btn_SelectedRemoveFavorite.IsEnabled = True
            Btn_SelectedRemoveGiftlist.IsEnabled = True
            Btn_SelectedRemoveWishlist.IsEnabled = True
            Btn_SelectedBuy.IsEnabled = True
            Btn_SelectedSell.IsEnabled = True
            Btn_SelectedQuickBuy.IsEnabled = True
            Btn_SelectedFavorite.IsEnabled = True
            Btn_SelectedGiftlist.IsEnabled = True
            Btn_SelectedWishlist.IsEnabled = True
            Btn_SelectedMarkedForSell.IsEnabled = False
            Btn_SelectedUnMarkedForSell.IsEnabled = False
            Btn_SelectedLock.IsEnabled = False
            Btn_SelectedUnlock.IsEnabled = False
            Btn_SelectedRefresh.IsEnabled = False
            Btn_SelectedExpandCollapse.IsEnabled = False
            Btn_SelectedCancel.IsEnabled = False
            Btn_SelectedRelist.IsEnabled = False

            If Equals(TabItem, Tab_Prices) OrElse Equals(TabItem, Tab_Keys) Then
                Btn_SelectedRemoveFavorite.IsEnabled = False
                Btn_SelectedRemoveGiftlist.IsEnabled = False
                Btn_SelectedRemoveWishlist.IsEnabled = False
                Btn_SelectedRefresh.IsEnabled = True

                Btn_SelectedExpandCollapse.IsEnabled = True

            ElseIf Equals(TabItem, Tab_Backpack) Then
                Btn_SelectedRefresh.IsEnabled = True
                Btn_SelectedRemoveFavorite.IsEnabled = False
                Btn_SelectedRemoveGiftlist.IsEnabled = False
                Btn_SelectedRemoveWishlist.IsEnabled = False
                Btn_SelectedSelectAll.IsEnabled = False

            ElseIf Equals(TabItem, Tab_Inventory) Then
                Btn_SelectedRemoveFavorite.IsEnabled = False
                Btn_SelectedRemoveGiftlist.IsEnabled = False
                Btn_SelectedRemoveWishlist.IsEnabled = False
                Btn_SelectedMarkedForSell.IsEnabled = True
                Btn_SelectedUnMarkedForSell.IsEnabled = True
                Btn_SelectedLock.IsEnabled = True
                Btn_SelectedUnlock.IsEnabled = True

                Btn_SelectedRefresh.IsEnabled = True

            ElseIf Equals(TabItem, Tab_Favorites) Then
                Btn_SelectedRemoveGiftlist.IsEnabled = False
                Btn_SelectedRemoveWishlist.IsEnabled = False

                Btn_SelectedFavorite.IsEnabled = False
                Btn_SelectedExpandCollapse.IsEnabled = True

            ElseIf Equals(TabItem, Tab_Listings) OrElse Equals(TabItem, Tab_Orders) Then
                Btn_SelectedRemoveFavorite.IsEnabled = False
                Btn_SelectedRemoveGiftlist.IsEnabled = False
                Btn_SelectedRemoveWishlist.IsEnabled = False

                Btn_SelectedFavorite.IsEnabled = False
                Btn_SelectedGiftlist.IsEnabled = False
                Btn_SelectedWishlist.IsEnabled = False

                Btn_SelectedBuy.IsEnabled = False
                Btn_SelectedSell.IsEnabled = False
                Btn_SelectedQuickBuy.IsEnabled = False

                Btn_SelectedRefresh.IsEnabled = True
                Btn_SelectedCancel.IsEnabled = True
                Btn_SelectedRelist.IsEnabled = True

            ElseIf Equals(TabItem, Tab_UpForTrade) Then
                Btn_SelectedRemoveFavorite.IsEnabled = False
                Btn_SelectedRemoveGiftlist.IsEnabled = False
                Btn_SelectedRemoveWishlist.IsEnabled = False

                Btn_SelectedFavorite.IsEnabled = False
                Btn_SelectedGiftlist.IsEnabled = False
                Btn_SelectedWishlist.IsEnabled = False

                Btn_SelectedBuy.IsEnabled = False
                Btn_SelectedSell.IsEnabled = False
                Btn_SelectedQuickBuy.IsEnabled = False

                Btn_SelectedRefresh.IsEnabled = True
                Btn_SelectedExpandCollapse.IsEnabled = True
                Btn_SelectedCancel.IsEnabled = True

            ElseIf Equals(TabItem, Tab_Wishlist) Then
                Btn_SelectedRemoveFavorite.IsEnabled = False
                Btn_SelectedRemoveGiftlist.IsEnabled = False

                Btn_SelectedWishlist.IsEnabled = False

            ElseIf Equals(TabItem, Tab_Giftlist) Then
                Btn_SelectedRemoveFavorite.IsEnabled = False
                Btn_SelectedRemoveWishlist.IsEnabled = False

                Btn_SelectedGiftlist.IsEnabled = False

            ElseIf Equals(TabItem, Tab_Settings) Then
                Btn_SelectedRemoveFavorite.IsEnabled = False
                Btn_SelectedRemoveGiftlist.IsEnabled = False
                Btn_SelectedRemoveWishlist.IsEnabled = False

                Btn_SelectedFavorite.IsEnabled = False
                Btn_SelectedGiftlist.IsEnabled = False
                Btn_SelectedWishlist.IsEnabled = False

                Btn_SelectedBuy.IsEnabled = False
                Btn_SelectedSell.IsEnabled = False
                Btn_SelectedQuickBuy.IsEnabled = False
            End If
        End If
    End Sub

    Private Sub Cbx_GameList_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim sComboBox As ComboBox = DirectCast(sender, ComboBox)

        If (sComboBox.SelectedIndex <> Settings.Container.PreviousSelectedGame) Then
            If MsgBox("Changing games will flush all current data, if you haven't saved everything will be lost. Are you sure about this?", MsgBoxStyle.OkCancel, "Confirm Action") = MsgBoxResult.Ok Then
                Select Case sComboBox.SelectedIndex
                    Case 0
                        Settings.Container.CurrentGameName = "TeamFortress 2"
                    Case 1
                        Settings.Container.CurrentGameName = "Trading Cards"
                End Select
            End If
        End If
    End Sub

    Private Sub Cbx_GameList_DropDownOpened(sender As Object, e As EventArgs)
        Settings.Container.PreviousSelectedGame = DirectCast(sender, ComboBox).SelectedIndex
    End Sub

    Private Async Sub Btn_SelectedSell_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedSell.Click
        Dim SelectedDataGrid As DataGrid = GetSelectedDataGrid()

        Await Task.Run(Sub()
                           If Equals(SelectedDataGrid, Lv_Inventory) Then
                               Dim ItemCount As New Integer
                               Dim Total As New Decimal
                               Dim TotalAfterSteamTax As New Decimal

                               For Each Item As Item In SelectedDataGrid.ItemsSource
                                   If (Item.IsSelected AndAlso (Not Item.Locked) AndAlso Item.Marketable AndAlso (Not Item.InTransit) AndAlso (Not Item.Price.SellHasError)) Then
                                       ItemCount += 1
                                       TotalAfterSteamTax += Item.Price.SellAfterSteamTax
                                       Total += Item.Price.Sell
                                   End If
                               Next

                               If (ItemCount > 0) Then
                                   If MsgBox(
                                   Box(String.Format("Are you sure you want to sell {0} Item(s)?", ItemCount),
                                          "You Receive: " + TotalAfterSteamTax.ToString,
                                          "Total: " + Total.ToString),
                                          MsgBoxStyle.OkCancel, "Confirm Action") = MsgBoxResult.Ok Then

                                       For Each Item In SelectedDataGrid.ItemsSource.Cast(Of Item).ToList
                                           If (Item.IsSelected AndAlso (Not Item.Locked) AndAlso Item.Marketable AndAlso (Not Item.InTransit) AndAlso (Not Item.Price.SellHasError)) Then
                                               Dim Result As String = Nothing
                                               Network.SellItem(Item.AppID, Item.AssetID, Item.Price.SellAfterSteamTax, Result:=Result)

                                               If Result.Contains("""success"":true") Then
                                                   Item.Price.SellSuccess = 1
                                               Else
                                                   Item.Price.SellSuccess = -1
                                               End If
                                           End If
                                       Next
                                   End If
                               Else
                                   Status("Nothing selected or Restricted Items.")
                               End If

                           ElseIf Equals(SelectedDataGrid, Lv_Keys) OrElse Equals(SelectedDataGrid, Lv_Favorite) OrElse Equals(SelectedDataGrid, Lv_WishList) OrElse Equals(SelectedDataGrid, Lv_GiftList) Then
                               Dim Listings As New ConcurrentBag(Of Backpack.API.Listing)
                               Dim ToReceive As New Decimal
                               Dim ItemCount As New Integer
                               Dim Missing As New ConcurrentBag(Of String)

                               Parallel.ForEach(SelectedDataGrid.ItemsSource.Cast(Of Item).ToList,
                                     Sub(Level_1)
                                         Parallel.ForEach(Level_1.Price.CurrentMarketPrices.Backpack.Sell,
                                                          Sub(Level_2)
                                                              If (Level_2.IsSelected = True) Then
                                                                  Dim Amount As Integer = Level_2.AmountBuy
                                                                  ToReceive += Level_2.Price.Cost.Total * Level_2.AmountBuy
                                                                  Listings.Add(Level_2)

                                                                  For Each Level_3 In Binding_Lv_Inventory
                                                                      If (Level_3.DefIndex.Equals(Level_2.Parent.DefIndex) AndAlso Level_3.Quality.ID.Equals(Level_2.Parent.Quality.ID) AndAlso (Not Level_3.Locked) AndAlso (Not Level_3.InTransit) AndAlso Level_3.Tradable) Then
                                                                          Amount -= 1

                                                                          If (Amount = 0) Then
                                                                              Exit For
                                                                          End If
                                                                      End If
                                                                  Next

                                                                  If (Amount > 0) Then
                                                                      Interlocked.Add(ItemCount, Amount)
                                                                      Missing.Add(vbNewLine & Level_2.Parent.MarketHashID)
                                                                  End If
                                                              End If
                                                          End Sub)
                                     End Sub)

                               If (ItemCount = 0) Then
                                   If MsgBox(Box("Backpack.tf Listings: " + Listings.Count.ToString, "Refined to Receive: " + ToReceive.ToString, "Do you want to sell these Item(s)?"), MsgBoxStyle.OkCancel, "Confirm Action") = MsgBoxResult.Ok Then
                                       For Each Listing In Listings
                                           SteamTrader.Network.Backpack.API.Trade.Sell(Listing)
                                       Next
                                   End If
                               Else
                                   MsgBox(String.Format("Missing: {0}", String.Join(String.Empty, Missing)), MsgBoxStyle.Exclamation, "Insufficient funds")
                               End If
                           Else
                               Status("Nothing selected or Restricted Items.")
                           End If
                       End Sub)
    End Sub

    Private Async Sub Btn_SelectedBuy_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedBuy.Click
        Dim SelectedDataGrid As DataGrid = GetSelectedDataGrid()

        Await Task.Run(Sub()
                           If Equals(SelectedDataGrid, Lv_Inventory) Then
                               Dim Listings As New List(Of Archive.Steam)
                               Dim TotalCost As New Decimal

                               For Each Item As Item In SelectedDataGrid.ItemsSource
                                   For Each Listing_Steam In Item.Price.CurrentMarketPrices.Steam.Buy
                                       If (Listing_Steam.IsSelected = True) Then
                                           TotalCost += Listing_Steam.Price
                                           Listings.Add(Listing_Steam)
                                       End If
                                   Next
                               Next

                               If (Listings.Count > 0) Then
                                   If (Settings.Container.Balance >= TotalCost) Then
                                       If MsgBox(String.Format("Are you sure you want to buy {0} Item(s) for a total of {1}?", Listings.Count, TotalCost), MsgBoxStyle.OkCancel, "Confirm Action") = MsgBoxResult.Ok Then
                                           For Each Listing In Listings
                                               Dim Fee As Decimal = ((Listing.Price * Settings.Container.Exchange_SteamTax) / 100)
                                               Network.BuyItem_Steam(Listing.ID, Listing.Price - Fee, Fee, Listing.Price)
                                           Next
                                       End If
                                   Else
                                       MsgBox(Box("Balance: " + Settings.Container.Balance.ToString, "Need: " + TotalCost.ToString, "Missing: " + Math.Abs(CDec(Settings.Container.CurrentUser.WalletBalance) - TotalCost).ToString), MsgBoxStyle.Exclamation, "Insufficient funds")
                                   End If
                               Else
                                   Status("Nothing selected or Restricted Items.")
                               End If

                           ElseIf Equals(SelectedDataGrid, Lv_Prices) OrElse Equals(SelectedDataGrid, Lv_Favorite) OrElse Equals(SelectedDataGrid, Lv_WishList) OrElse Equals(SelectedDataGrid, Lv_GiftList) Then
                               Dim Listings_Steam As New ConcurrentBag(Of Archive.Steam)
                               Dim Listings_Backpack As New ConcurrentBag(Of Backpack.API.Listing)
                               Dim TotalCost_Steam As New Decimal
                               Dim TotalCost_Backpack As New Metal
                               Dim AmountBuy As New Integer
                               Dim Cancelled As New Boolean
                               Dim Cancelled_Listing As New Backpack.API.Listing
                               Dim Cancelled_Metal As New Metal

                               Parallel.ForEach(DirectCast(SelectedDataGrid.ItemsSource, ObservableCollection(Of Item)),
                                                         Sub(Item As Item, LoopState As ParallelLoopState)
                                                             For Each Listing_Steam In Item.Price.CurrentMarketPrices.Steam.Buy
                                                                 If (Listing_Steam.IsSelected = True) Then
                                                                     TotalCost_Steam += Listing_Steam.Price
                                                                     Listings_Steam.Add(Listing_Steam)
                                                                 End If
                                                             Next

                                                             For Each Listing_Backpack In Item.Price.CurrentMarketPrices.Backpack.Buy
                                                                 If (Listing_Backpack.IsSelected = True) Then
                                                                     Dim Metal As Metal = (Listing_Backpack.Price.Cost * Listing_Backpack.AmountBuy).Realize

                                                                     If Metal.HasNegative Then
                                                                         Dispatcher.BeginInvoke(Sub()
                                                                                                    Cancelled = True
                                                                                                    Cancelled_Metal = Metal
                                                                                                    Cancelled_Listing = Listing_Backpack
                                                                                                End Sub)
                                                                         LoopState.Stop()
                                                                     End If

                                                                     TotalCost_Backpack += Metal
                                                                     AmountBuy += Listing_Backpack.AmountBuy
                                                                     Listings_Backpack.Add(Listing_Backpack)
                                                                 End If
                                                             Next
                                                         End Sub)

                               If Cancelled Then
                                   MsgBox(Box("Troubled Listing: " + Cancelled_Listing.Parent.MarketHashID,
                                              "Partner: " + Cancelled_Listing.Partner.Name,
                                              "Item: " + Cancelled_Listing.Parent.MarketHashID,
                                              "Total Price: " + Cancelled_Listing.Price.Their_InRef.ToString,
                                              "Amount: " + Cancelled_Listing.OrderAmount.ToString,
                                              "Missing: " + Cancelled_Metal.ToString), MsgBoxStyle.Exclamation, "Insufficient funds")
                               Else
                                   TotalCost_Backpack = Settings.Container.TotalMetal.Equalize(TotalCost_Backpack)

                                   If ((Listings_Steam.Count + Listings_Backpack.Count) > 0) Then
                                       If (Settings.Container.Balance >= TotalCost_Steam) AndAlso (Not TotalCost_Backpack.Weapon = -1) Then
                                           If MsgBox(Box("Steam Listings: " + Listings_Steam.Count.ToString,
                                                "Price in Cash: " + TotalCost_Steam.ToString,
                                                "Backpack.tf Listings: " + AmountBuy.ToString,
                                                "Price in Refined: " + TotalCost_Backpack.ToString), MsgBoxStyle.OkCancel, "Confirm Action") = MsgBoxResult.Ok Then

                                               For Each Listing In Listings_Steam
                                                   Dim Fee As Decimal = ((Listing.Price * Settings.Container.Exchange_SteamTax) / 100)
                                                   Network.BuyItem_Steam(Listing.ID, Listing.Price - Fee, Fee, Listing.Price)
                                               Next
                                               For Each Listing In Listings_Backpack
                                                   SteamTrader.Network.Backpack.API.Buy(Listing)
                                               Next
                                           End If
                                       Else
                                           MsgBox(Box("Steam Listings: " + Listings_Steam.Count.ToString,
                                                "Price in Cash: " + TotalCost_Steam.ToString,
                                                "Backpack.tf Listings: " + AmountBuy.ToString,
                                                "Price in Refined: " + TotalCost_Backpack.ToString,
                                                "Missing: " + (TotalCost_Backpack - Settings.Container.TotalMetal).MinZero.ToString), MsgBoxStyle.Exclamation, "Insufficient funds")
                                       End If
                                   Else
                                       Status("Nothing selected or Restricted Items.")
                                   End If
                               End If
                           End If
                       End Sub)
    End Sub

    Private Function CheckFunds(Cost As Metal) As Metal
        If (Settings.Container.TotalMetal.Key < Cost.Key) Then
            Dim RequiredKey As Integer = (Cost.Key - Settings.Container.TotalMetal.Key)
            Cost.Key -= RequiredKey
            Cost += FromRefined(RequiredKey * Settings.Container.Backpack_Price_Key_InMetal).MinZero
        End If
        If (Settings.Container.TotalMetal.Refined < Cost.Refined) Then
            Dim RequiredMetal As Integer = (Cost.Refined - Settings.Container.TotalMetal.Refined)
            Cost.Refined -= RequiredMetal
            Cost.Reclaimed += (RequiredMetal * 3)
        End If
        If (Settings.Container.TotalMetal.Reclaimed < Cost.Reclaimed) Then
            Dim RequiredMetal As Integer = (Cost.Reclaimed - Settings.Container.TotalMetal.Reclaimed)
            Cost.Reclaimed -= RequiredMetal
            Cost.Scrap += (RequiredMetal * 3)
        End If
        If (Settings.Container.TotalMetal.Scrap < Cost.Scrap) Then
            Dim RequiredMetal As Integer = (Cost.Scrap - Settings.Container.TotalMetal.Scrap)
            Cost.Scrap -= RequiredMetal
            Cost.Weapon += (RequiredMetal * 2)
        End If
        If (Cost.Weapon > 0) AndAlso (Settings.Container.TotalMetal.Weapon < Cost.Weapon) Then
            Cost.Weapon = -1
        End If

        Return Cost
    End Function

    Private Sub Btn_SelectedQuickBuy_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedQuickBuy.Click
        If MsgBox("Are you sure you want to activate Quickbuy?", MsgBoxStyle.OkCancel, "Confirm Action") = MsgBoxResult.Ok Then
            Debug.WriteLine("Quickbuy activated!")
        End If
    End Sub

    Private Async Sub Btn_SelectedRelist_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedRelist.Click
        Dim SelectedDataGrid As DataGrid = GetSelectedDataGrid()

        Await Task.Run(
               Sub()
                   Dim ItemCount As New Integer

                   For Each Item As Item In SelectedDataGrid.ItemsSource
                       If Item.IsSelected Then
                           ItemCount += 1
                       End If
                   Next

                   If (ItemCount > 0) Then
                       If Equals(SelectedDataGrid, Lv_Listings) Then
                           If MsgBox(String.Format("Are you sure you want to relist {0} listing(s)?", ItemCount), MsgBoxStyle.OkCancel, "Confirm Action") = MsgBoxResult.Ok Then
                               Task.Run(Sub() Network.SellItem_Relist(SelectedDataGrid.ItemsSource.Cast(Of Item).ToList))
                           End If

                       ElseIf Equals(SelectedDataGrid, Lv_Orders) Then
                           If MsgBox(String.Format("Are you sure you want to relist {0} order(s)?", ItemCount), MsgBoxStyle.OkCancel, "Confirm Action") = MsgBoxResult.Ok Then
                               Parallel.ForEach(DirectCast(SelectedDataGrid.ItemsSource, ObservableCollection(Of Item)),
                                 Sub(Item)
                                     If (Item.IsSelected AndAlso (Not Item.Price.RelistHasError)) Then
                                         Network.CancelBuyOrder(Item.Price.Listing_ID_Econ)
                                         Network.CreateBuyOrder(Item.AppID, Item.MarketHashID, Item.Price.RelistOrderTotalCost, Item.Price.RelistAmount)
                                     End If
                                 End Sub)
                           End If
                       End If
                   Else
                       Status("Nothing selected.")
                   End If
               End Sub)
    End Sub

    Public Shared Function ConvertToSteamNumber(Input As Object) As String
        Dim sInput As String = Input.ToString.Replace(","c, "."c)
        Dim Data As Decimal = CDec(CDec(sInput).ToString("F"))
        Dim Output As String = Data.ToString.Replace("."c, "")
        Return Output
    End Function

    Private Async Sub Btn_SelectedCancel_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedCancel.Click
        Dim SelectedDataGrid As DataGrid = GetSelectedDataGrid()

        Await Task.Run(Sub()
                           Dim ItemCount As New Integer

                           If Equals(SelectedDataGrid, Lv_UpForTrade) Then
                               For Each iItem As AdvancedItem In SelectedDataGrid.ItemsSource
                                   If (iItem.IsSelected = True) Then
                                       ItemCount += 1
                                   End If
                               Next

                               If (ItemCount > 0) Then
                                   If MsgBox(String.Format("Are you sure you want to cancel {0} offer(s)?", ItemCount), MsgBoxStyle.OkCancel, "Confirm Action") = MsgBoxResult.Ok Then
                                       Dim CancelUrl As String = String.Format("http://api.steampowered.com/IEconService/CancelTradeOffer/v1/?key={0}&format=json", Settings.Container.Steam_APIKey)

                                       Parallel.ForEach(DirectCast(SelectedDataGrid.ItemsSource, ObservableCollection(Of AdvancedItem)),
                                                                   Sub(Item)
                                                                       If Item.IsSelected Then
                                                                           Dim Data As New Dictionary(Of String, String) From {{"tradeofferid", Item.ID}}

                                                                           Using Client As New HttpClient
                                                                               Dim Response = Client.PostAsync(CancelUrl, New FormUrlEncodedContent(Data)).Result.IsSuccessStatusCode

                                                                               If Response Then
                                                                                   Item.Status = -1
                                                                               Else
                                                                                   Item.Status = -2
                                                                               End If
                                                                           End Using
                                                                       End If
                                                                   End Sub)
                                   End If
                               Else
                                   Status("Nothing selected.")
                               End If

                           ElseIf Equals(SelectedDataGrid, Lv_Listings) OrElse Equals(SelectedDataGrid, Lv_Orders) Then
                               For Each Item As Item In SelectedDataGrid.ItemsSource
                                   If Item.IsSelected Then
                                       ItemCount += 1
                                   End If
                               Next

                               If (ItemCount > 0) Then
                                   If Equals(SelectedDataGrid, Lv_Listings) Then
                                       If MsgBox(String.Format("Are you sure you want to cancel {0} listing(s)?", ItemCount), MsgBoxStyle.OkCancel, "Confirm Action") = MsgBoxResult.Ok Then
                                           Parallel.ForEach(SelectedDataGrid.ItemsSource.Cast(Of Item).ToList,
                                                                   Sub(Item)
                                                                       If Item.IsSelected Then
                                                                           Network.RemoveListing(Item.Price.Listing_ID_Econ)
                                                                       End If
                                                                   End Sub)
                                       End If

                                   ElseIf Equals(SelectedDataGrid, Lv_Orders) Then
                                       If MsgBox(String.Format("Are you sure you want to cancel {0} order(s)?", ItemCount), MsgBoxStyle.OkCancel, "Confirm Action") = MsgBoxResult.Ok Then
                                           Parallel.ForEach(DirectCast(SelectedDataGrid.ItemsSource, ObservableCollection(Of Item)),
                                                                   Sub(Item)
                                                                       If Item.IsSelected Then
                                                                           Network.CancelBuyOrder(Item.Price.Listing_ID_Econ)
                                                                       End If
                                                                   End Sub)
                                       End If
                                   End If
                               Else
                                   Status("Nothing selected.")
                               End If
                           End If
                       End Sub)
    End Sub

#Region "Mod Collection Properties"

    Private Async Sub Btn_SelectedSelectAll_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedSelectAll.Click
        Await Task.Run(Sub() SelectDeselectAllAsync(TabControl_Main, True))
    End Sub

    Private Async Sub Btn_SelectedDeselectAll_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedDeselectAll.Click
        Await Task.Run(Sub() SelectDeselectAllAsync(TabControl_Main, False))
    End Sub

    Private Async Sub SelectDeselectAllAsync(sender As TabControl, ToggleState As Boolean)
        SelectionInProgress = True
        Dim Source As ICollection = Nothing
        Await Dispatcher.BeginInvoke(New Action(Sub() Source = GetSelectedDataGridSource()))

        Select Case Source(0).GetType()
            Case GetType(Item)
                Dim HostCollection As New ObservableCollection(Of Item)(DirectCast(Source, ObservableCollection(Of Item)))
                HostCollection.SetSilentlyAll("IsSelected", ToggleState)

                If (Not ToggleState) Then
                    Await Dispatcher.BeginInvoke(Sub()
                                                     For Each Item In HostCollection
                                                         Item.Price.CurrentMarketPrices.Backpack.Buy.SetSilentlyAll("IsSelected", False)
                                                         Item.Price.CurrentMarketPrices.Backpack.Sell.SetSilentlyAll("IsSelected", False)
                                                         Item.Price.CurrentMarketPrices.Steam.Buy.SetSilentlyAll("IsSelected", False)
                                                     Next
                                                 End Sub)
                End If

            Case GetType(SettingsItem)
                Dim HostCollection As New ObservableCollection(Of SettingsItem)(DirectCast(Source, ObservableCollection(Of SettingsItem)))
                HostCollection.SetSilentlyAll("IsSelected", ToggleState)

            Case GetType(AdvancedItem)
                Dim HostCollection As New ObservableCollection(Of AdvancedItem)(DirectCast(Source, ObservableCollection(Of AdvancedItem)))
                HostCollection.SetSilentlyAll("IsSelected", ToggleState)

            Case GetType(Backpack.Price)
                Dim HostCollection As New ObservableCollection(Of Backpack.Price)(DirectCast(Source, ObservableCollection(Of Backpack.Price)))

                Parallel.ForEach(HostCollection, Sub(Level_1)
                                                     Parallel.ForEach(Qualities, Sub(Q)
                                                                                     Dim Level_2 = Level_1.GetType.GetProperty(Q.Name)

                                                                                     If (Level_2 IsNot Nothing) Then
                                                                                         Dim Level_3 As Backpack.Quality = DirectCast(Level_2.GetValue(Level_1), Backpack.Quality)

                                                                                         If (Level_3 IsNot Nothing) Then
                                                                                             Level_3.IsSelected = False
                                                                                         End If
                                                                                     End If
                                                                                 End Sub)
                                                 End Sub)
        End Select

        SelectionInProgress = False
    End Sub

    Private Sub Btn_SelectedFavorite_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedFavorite.Click
        AddToList(Binding_Lv_Favorite)
    End Sub

    Private Sub Btn_SelectedRemoveFavorite_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedRemoveFavorite.Click
        RemoveFromList(Binding_Lv_Favorite)
    End Sub

    Private Sub Btn_SelectedWishlist_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedWishlist.Click
        AddToList(Binding_Lv_WishList)
    End Sub

    Private Sub Btn_SelectedRemoveWishlist_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedRemoveWishlist.Click
        RemoveFromList(Binding_Lv_WishList)
    End Sub

    Private Sub Btn_SelectedGiftlist_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedGiftlist.Click
        AddToList(Binding_Lv_GiftList)
    End Sub

    Private Sub Btn_SelectedRemoveGiftlist_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedRemoveGiftlist.Click
        RemoveFromList(Binding_Lv_GiftList)
    End Sub

    Private Sub Btn_SelectedLock_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedLock.Click
        LockUnlock(True)
    End Sub

    Private Sub Btn_SelectedUnlock_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedUnlock.Click
        LockUnlock(False)
    End Sub

    Private Sub Btn_SelectedMarkedForSell_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedMarkedForSell.Click
        MarkUnMark(True)
    End Sub

    Private Sub Btn_SelectedUnMarkedForSell_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedUnMarkedForSell.Click
        MarkUnMark(False)
    End Sub

#End Region

    Private Sub CloseButton_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub

    Private Sub MaximizeButton_Click(sender As Object, e As EventArgs)
        If (Me.WindowState <> WindowState.Maximized) Then
            Me.WindowState = WindowState.Maximized
        Else
            Me.WindowState = WindowState.Normal
        End If
    End Sub

    Private Sub MinimizeButton_Click(sender As Object, e As EventArgs)
        Me.WindowState = WindowState.Minimized
    End Sub

    Private Sub Window_Closed(sender As Object, e As EventArgs)
        If (Settings.Container.OnShutDown.ClearImageCache = True) Then
            If IO.Directory.Exists((AppDomain.CurrentDomain.BaseDirectory + "/Team Fortress 2/Cache")) Then
                For Each File As IO.FileInfo In New IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "/Team Fortress 2/Cache").GetFiles()
                    Try
                        File.Delete()
                    Catch ex As IOException
                    End Try
                Next
            End If
        End If
    End Sub

    Private Sub Window_Closing(sender As Object, e As CancelEventArgs)
        If (MsgBox("Are you sure you want to close the Trader? Any unsaved data will be lost.", MsgBoxStyle.OkCancel, "Confirm Action") = MsgBoxResult.Cancel) Then
            e.Cancel = True
        Else
            AuthAppWindow.Dispatcher.BeginInvoke(Sub()
                                                     AuthAppWindow.IsActualClosing = True
                                                     AuthAppWindow.Close()
                                                 End Sub)
        End If
    End Sub

    Private Async Sub Btn_SelectedRefresh_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedRefresh.Click
        If (Not String.IsNullOrWhiteSpace(SteamTrader.Network.Network.Username)) Then
            If (Not Settings.Container.IsRunning.Refresh) Then
                Settings.Container.IsRunning.Refresh = True
                Dim sTabItem As TabItem = CType(TabControl_Main.SelectedItem, TabItem)

                Await Task.Run(Sub()
                                   If Equals(sTabItem, Tab_Keys) Then
                                       Network.GetAndHandle_Keys()

                                   ElseIf Equals(sTabItem, Tab_Inventory) Then
                                       Inventory.Refresh()
                                       CheckInTransit()

                                   ElseIf Equals(sTabItem, Tab_Listings) OrElse Equals(sTabItem, Tab_Orders) Then
                                       Network.GetAndHandle_SteamListings()

                                   ElseIf Equals(sTabItem, Tab_UpForTrade) Then
                                       Network.GetAndHandle_UpForTrade()
                                       Dispatcher.BeginInvoke(Sub() CheckInTransit())

                                   ElseIf Equals(sTabItem, Tab_Backpack) Then
                                       Network.GetAndHandle_Backpack()
                                   End If
                               End Sub)

                Await Dispatcher.BeginInvoke(Sub() UpdateMetalCount(), DispatcherPriority.ContextIdle, Nothing)
                Settings.Container.IsRunning.Refresh = False
            Else
                Status("Refresh in progress, please wait...")
            End If
        Else
            Status("No user specified, please login.")
        End If
    End Sub

    Private Sub CheckInTransit()
        Dim IDs = {2, 4, 9, 11}

        Parallel.ForEach(Binding_Lv_UpForTrade,
                         Sub(Trade)
                             If (IDs.Contains(Trade.StateID)) Then
                                 Parallel.ForEach(Trade.Outgoing, Sub(Level_1)
                                                                      Dim Level_2 = Binding_Lv_Inventory.FirstOrDefault(Function(F) Level_1.DefIndex.Equals(F.DefIndex) AndAlso Level_1.ClassID.Equals(F.ClassID) AndAlso Level_1.AssetID.Equals(F.AssetID) AndAlso Level_1.InstanceID.Equals(F.InstanceID))

                                                                      If (Level_2 IsNot Nothing) Then
                                                                          Level_2.InTransit = True
                                                                      End If
                                                                  End Sub)
                             End If
                         End Sub)
    End Sub

    Private Sub Btn_SelectedRefreshPrices_All_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedRefreshPrices_All.Click
        If (Not Settings.Container.IsRunning.Refresh_Prices) Then
            Settings.Container.IsRunning.Refresh_Prices = True

            Task.Run(Sub()
                         GetAssetPrices_Steam()
                         Backpack.API.GetAssetPrices()
                         Settings.Container.IsRunning.Refresh_Prices = False
                         Status("Steam & Backpack.tf Prices updated on All possible Items!")
                     End Sub)
        End If
    End Sub

    Private Sub Btn_SelectedExpandCollapse_Click(sender As Object, e As RoutedEventArgs) Handles Btn_SelectedExpandCollapse.Click
        Dim SelectedDataGrid As DataGrid = GetSelectedDataGrid()

        If (SelectedDataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Visible) Then
            SelectedDataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed
        Else
            SelectedDataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Visible
        End If
    End Sub

    Public Async Sub UpdateMetalCount()
        With Settings.Container.TotalMetal
            .Refined = Binding_Lv_Inventory.Where(Function(F) F.DefIndex.Equals(DefIndex.Refined)).Count
            .Reclaimed = Binding_Lv_Inventory.Where(Function(F) F.DefIndex.Equals(DefIndex.Reclaimed)).Count
            .Scrap = Binding_Lv_Inventory.Where(Function(F) F.DefIndex.Equals(DefIndex.Scrap)).Count
            .Weapon = Binding_Lv_Inventory.Where(Function(F) (F.MarkedForSell AndAlso ((Not F.InTransit) AndAlso (Not F.Locked)))).Count
        End With

        Dim InTransit As New Metal With
        {
            .Refined = Binding_Lv_Inventory.Where(Function(F) (F.DefIndex.Equals(DefIndex.Refined) AndAlso (F.InTransit OrElse F.Locked))).Count,
            .Reclaimed = Binding_Lv_Inventory.Where(Function(F) (F.DefIndex.Equals(DefIndex.Reclaimed) AndAlso (F.InTransit OrElse F.Locked))).Count,
            .Scrap = Binding_Lv_Inventory.Where(Function(F) (F.DefIndex.Equals(DefIndex.Scrap) AndAlso (F.InTransit OrElse F.Locked))).Count,
            .Weapon = Binding_Lv_Inventory.Where(Function(F) (F.MarkedForSell AndAlso F.InTransit OrElse F.Locked)).Count
        }

        Dim Available As Metal = (Settings.Container.TotalMetal - InTransit).MinZero
        Settings.Container.BalanceMetal = String.Format("{0} ({9})(Refined: {1}({5}), Reclaimed: {2}({6}), Scrap: {3}({7}), Weapon: {4}({8}))", Available.Total(True), Available.Refined, Available.Reclaimed, Available.Scrap, Available.Weapon, InTransit.Refined, InTransit.Reclaimed, InTransit.Scrap, InTransit.Weapon, InTransit.Total)
        Dim Local_Keys = Binding_Lv_Inventory.Where(Function(F) Regex.IsMatch(F.Name, "\bkey\b", RegexOptions.IgnoreCase))
        Settings.Container.TotalMetal.Key = Local_Keys.Where(Function(F) F.Tradable AndAlso (Not F.InTransit) AndAlso (Not F.Locked)).Count
        Await Dispatcher.BeginInvoke(Sub() L_Keys.Content = String.Format("{0} ({1})", Settings.Container.TotalMetal.Key, Local_Keys.Count - Settings.Container.TotalMetal.Key))
        Status("Metal Balance Updated!")
    End Sub

    Public Function ToRefined(Optional Key As Integer = 0, Optional Refined As Integer = 0, Optional Reclaimed As Integer = 0, Optional Scrap As Integer = 0, Optional Weapon As Integer = 0) As Decimal
        Dim Scrap_From_Reclaimed As Decimal = Reclaimed * 3
        Dim Mod_Weapon As Decimal = Weapon Mod 2
        Dim Scrap_From_Weapon As Decimal = (Weapon - Mod_Weapon) / 2
        Dim Total_Scrap As Decimal = Scrap_From_Reclaimed + Scrap_From_Weapon + Scrap
        Dim Mod_Total_Scrap As Decimal = Total_Scrap Mod 9
        Dim Whole_Scrap As Decimal = (Total_Scrap - Mod_Total_Scrap) / 9
        Return CDec(Refined + Whole_Scrap + (Mod_Weapon * 0.05) + (Mod_Total_Scrap * 0.11) + (Key * Settings.Container.Backpack_Price_Key_InMetal))
    End Function

    Private Sub Settings_ChBx_Checked(sender As Object, e As RoutedEventArgs)
        Dim Checkbox As CheckBox = CType(sender, CheckBox)
        Dim HostDataContext As SettingsItem = DirectCast(Checkbox.DataContext, SettingsItem)

        If (Not String.IsNullOrWhiteSpace(HostDataContext.InputProperty)) Then
            SetPropertyValueByName(Me, HostDataContext.InputProperty, Checkbox.IsChecked)
        End If
    End Sub

    Private Async Sub L_Balance_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        If MainWindow.Network.IsLoggedIn Then
            Await Task.Run(Sub() Network.Update_Balance())
            Status("Balance Updated!")
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        DirectCast(MainWindow, IDisposable).Dispose()
    End Sub

    'Clone from Resource Directory.vb
    Public Sub Hyperlink_RequestNavigate(sender As Object, e As RequestNavigateEventArgs)
        Dim Url As String = e.Uri.AbsoluteUri.ToString

        If Url.Contains("#") Then
            Url = Url.Replace("#", "%23")
        End If

        Process.Start(New ProcessStartInfo(Url))
        e.Handled = True
    End Sub

#Region "PropertyBrush"

    Private Sub Btn_SelectedFavorite_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        TogglePropertyBrush(sender)
    End Sub

    Private Sub Btn_SelectedRemoveFavorite_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        TogglePropertyBrush(sender)
    End Sub

    Private Sub Btn_SelectedWishlist_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        TogglePropertyBrush(sender)
    End Sub

    Private Sub Btn_SelectedRemoveWishlist_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        TogglePropertyBrush(sender)
    End Sub

    Private Sub Btn_SelectedGiftlist_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        TogglePropertyBrush(sender)
    End Sub

    Private Sub Btn_SelectedRemoveGiftlist_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        TogglePropertyBrush(sender)
    End Sub

    Private Sub Btn_SelectedMarkedForSell_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        TogglePropertyBrush(sender)
    End Sub

    Private Sub Btn_SelectedUnMarkedForSell_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        TogglePropertyBrush(sender)
    End Sub

    Private Sub Btn_SelectedLock_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        TogglePropertyBrush(sender)
    End Sub

    Private Sub Btn_SelectedUnlock_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        TogglePropertyBrush(sender)
    End Sub

    Private Sub TogglePropertyBrush(Sender As Object)
        If (Settings.Container.IsRunning.PropertyBrush = 0) Then
            Settings.Container.IsRunning.PropertyBrush = 1

            Dim Button As Button = DirectCast(Sender, Button)

            If Button.Foreground.ToString.Equals(Colors.Black.ToString) Then
                Hint.Foreground = New SolidColorBrush With {.Color = Colors.White}
            Else
                Hint.Foreground = New SolidColorBrush With {.Color = Colors.Black}
            End If
            Hint.Background = Button.Foreground
            Hint.Content = Button.ToolTip
            Hint.Tag = Sender
            Window_MouseMove(Nothing, Nothing)

            Mouse.OverrideCursor = Cursors.Pen
            PopUp_PropertyBrush.IsOpen = True
        Else
            PropertyBrushOFF()
        End If
    End Sub

    Private Sub Window_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If (Settings.Container.IsRunning.PropertyBrush > 1) Then
            Dim Position As Windows.Point = Mouse.GetPosition(Me)
            PopUp_PropertyBrush.HorizontalOffset = Position.X + 20
            PopUp_PropertyBrush.VerticalOffset = Position.Y
        End If
    End Sub

    Private Sub Window_MouseRightButtonDown(sender As Object, e As MouseButtonEventArgs)
        If (Settings.Container.IsRunning.PropertyBrush > 0) Then
            If (Settings.Container.IsRunning.PropertyBrush = 2) Then
                PropertyBrushOFF()
            Else
                Settings.Container.IsRunning.PropertyBrush = 2
            End If
        End If
    End Sub

    Private Sub PropertyBrushOFF()
        PopUp_PropertyBrush.IsOpen = False
        Mouse.OverrideCursor = Nothing
        Settings.Container.IsRunning.PropertyBrush = 0
    End Sub

#End Region

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Sub NotifyPropertyChanged(Of T)(ByRef field As T, value As T, <CallerMemberName> Optional name As String = "")
        If Not EqualityComparer(Of T).[Default].Equals(field, value) Then
            field = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
        End If
    End Sub

    Public Sub Update()
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(String.Empty))
    End Sub

    Public Sub Btn_Login_Click(sender As Object, e As RoutedEventArgs)
        Network.Btn_Login_Click(sender, e)
    End Sub

    Class Game

    End Class

End Class
