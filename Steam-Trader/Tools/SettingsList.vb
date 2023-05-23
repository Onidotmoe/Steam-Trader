Imports System.IO
Imports System.Xml.Serialization
Imports SteamTrader.GUI
Imports SteamTrader.TeamFortress2
Imports U

Namespace Settings

    Public Class Settings
        Property Collection As New ObservableCollection(Of SettingsItem)
        Property Container As New Class_Container

        Sub New()
            'TODO : Change this shitty system
            Dim Item_ShowValuation As New SettingsItem
            With Item_ShowValuation
                .InputStyle = "Boolean"
                .Name = "Show Valuation"
                .Description = "Toggle valuation colors on market prices."
                .IsEnabled = True
                .InputProperty = NameOf(Container.ShowValuation)
            End With
            Collection.Add(Item_ShowValuation)

            Dim Item_Grouping_Inventory As New SettingsItem
            With Item_Grouping_Inventory
                .InputStyle = "Boolean"
                .Name = "Grouping Inventory"
                .Description = "Toggle grouping on inventory tab."
                .IsEnabled = True
                .InputProperty = NameOf(Container.Grouping_Inventory)
            End With
            Collection.Add(Item_Grouping_Inventory)

            Dim Item_Grouping_Listings As New SettingsItem
            With Item_Grouping_Listings
                .InputStyle = "Boolean"
                .Name = "Grouping Listings"
                .Description = "Toggle grouping on Listings tab."
                .IsEnabled = True
                .InputProperty = NameOf(Container.Grouping_Listings)
            End With
            Collection.Add(Item_Grouping_Listings)

            Dim Item_Alternative_DisplayPrices As New SettingsItem
            With Item_Alternative_DisplayPrices
                .InputStyle = "Boolean"
                .Name = "Alternative Price Display"
                .Description = "Toggle grid details arrangement in Prices and Favorites."
                .IsEnabled = True
                .InputProperty = NameOf(Container.Alternative_DisplayPrices)
            End With
            Collection.Add(Item_Alternative_DisplayPrices)

            Dim Item_Grouping_Min_Inventory As New SettingsItem
            With Item_Grouping_Min_Inventory
                .Name = "Grouping Minimum - Inventory"
                .Description = "Minimum amount before identical Items are grouped. Disable for default 5."
                .IsEnabled = False
                .InputStyle = "String"
                .Input = Container.Grouping_Min_Inventory.ToString
                .InputProperty = NameOf(Container.Grouping_Min_Inventory)
            End With
            Collection.Add(Item_Grouping_Min_Inventory)

            Dim Item_Grouping_Min_Listings As New SettingsItem
            With Item_Grouping_Min_Listings
                .Name = "Grouping Minimum - Listings"
                .Description = "Minimum amount before identical Items are grouped. Disable for default 3."
                .IsEnabled = False
                .InputStyle = "String"
                .Input = Container.Grouping_Min_Listings.ToString
                .InputProperty = NameOf(Container.Grouping_Min_Listings)
            End With
            Collection.Add(Item_Grouping_Min_Listings)

            Dim Item_Independent_Search_Query As New SettingsItem
            With Item_Independent_Search_Query
                .Name = "Independent Search Query"
                .Description = "Search parameters will not transfer or overwrite between tabs."
                .IsEnabled = False
                .InputStyle = "Boolean"
                .InputProperty = NameOf(Container.Independent_Search_Query)
            End With
            Collection.Add(Item_Independent_Search_Query)

            Dim Item_Always_Buy_Max_Amount As New SettingsItem
            With Item_Always_Buy_Max_Amount
                .Name = "Always Buy Max Amount"
                .Description = "Will always set wanted purchase amount to the max of traders stock."
                .IsEnabled = False
                .InputStyle = "Boolean"
                .InputProperty = NameOf(Container.Always_Buy_Max_Amount)
            End With
            Collection.Add(Item_Always_Buy_Max_Amount)

            Dim Item_AutoLogon As New SettingsItem
            With Item_AutoLogon
                .Name = "Auto Logon"
                .Description = "Toggle if logon should start automatically."
                .IsEnabled = False
                .InputStyle = "Boolean"
                .InputProperty = NameOf(Container.AutoLogon)
            End With
            Collection.Add(Item_AutoLogon)

            Dim Item_AutoGetBackpack As New SettingsItem
            With Item_AutoGetBackpack
                .Name = "Auto Get Backpack.tf Prices"
                .Description = "Toggle if on logon should start automatically fetching Backpack.tf Prices."
                .IsEnabled = False
                .InputStyle = "Boolean"
                .InputProperty = NameOf(Container.AutoGetBackpack)
            End With
            Collection.Add(Item_AutoGetBackpack)

            Dim Item_KeyPrice As New SettingsItem
            With Item_KeyPrice
                .Name = "Key Price"
                .Description = "Custom key price here, used to calculate profit. Specify in refined metal. Disable for build-in Backpack.tf pricing."
                .IsEnabled = False
                .InputStyle = "String"
                .Input = Container.KeyPrice.ToString
                .InputProperty = NameOf(Container.KeyPrice)
            End With
            Collection.Add(Item_KeyPrice)

            Dim Item_RefinedMetalPrice As New SettingsItem
            With Item_RefinedMetalPrice
                .Name = "Refined Metal Price"
                .Description = "Custom refined metal price here, used to calculate profit. Specify in cash. Disable for build-in Backpack.tf pricing."
                .IsEnabled = False
                .InputStyle = "String"
                .Input = Container.RefinedMetalPrice.ToString
                .InputProperty = NameOf(Container.RefinedMetalPrice)
            End With
            Collection.Add(Item_RefinedMetalPrice)

            Dim Item_FantasticDealProcentage As New SettingsItem
            With Item_FantasticDealProcentage
                .Name = "Fantastic Deal"
                .Description = "Custom profit procentaged used in deal evaluation. Disable for 50% profit."
                .IsEnabled = False
                .InputStyle = "String"
                .Input = Container.FantasticDealProcentage.ToString
                .InputProperty = NameOf(Container.FantasticDealProcentage)
            End With
            Collection.Add(Item_FantasticDealProcentage)

            Dim Item_GreatDealProcentage As New SettingsItem
            With Item_GreatDealProcentage
                .Name = "Great Deal"
                .Description = "Custom profit procentaged used in deal evaluation. Disable for 25% profit."
                .IsEnabled = False
                .InputStyle = "String"
                .Input = Container.GreatDealProcentage.ToString
                .InputProperty = NameOf(Container.GreatDealProcentage)
            End With
            Collection.Add(Item_GreatDealProcentage)

            Dim Item_GoodDealProcentage As New SettingsItem
            With Item_GoodDealProcentage
                .Name = "Good Deal"
                .Description = "Custom profit procentaged used in deal evaluation. Disable for 10% profit."
                .IsEnabled = False
                .InputStyle = "String"
                .Input = Container.GoodDealProcentage.ToString
                .InputProperty = NameOf(Container.GoodDealProcentage)
            End With
            Collection.Add(Item_GoodDealProcentage)

            Dim Item_BadDealProcentage As New SettingsItem
            With Item_BadDealProcentage
                .Name = "Bad Deal"
                .Description = "Custom profit procentaged used in deal evaluation. Disable for no profit/loss."
                .IsEnabled = False
                .InputStyle = "String"
                .Input = Container.BadDealProcentage.ToString
                .InputProperty = NameOf(Container.BadDealProcentage)
            End With
            Collection.Add(Item_BadDealProcentage)

            Dim Item_AverageCountStart As New SettingsItem
            With Item_AverageCountStart
                .Name = "Average Count Start"
                .Description = "Specify When the average count should start, this Is useful To skip over below average listings. Disable for default 2."
                .IsEnabled = False
                .InputStyle = "String"
                .Input = Container.AverageCountStart.ToString
                .InputProperty = NameOf(Container.AverageCountStart)
            End With
            Collection.Add(Item_AverageCountStart)

            Dim Item_AverageCountRange As New SettingsItem
            With Item_AverageCountRange
                .Name = "Average Count Range"
                .Description = "Specify how far from the start the average count should take into account. Disable for default 3."
                .IsEnabled = False
                .InputStyle = "String"
                .Input = Container.AverageCountRange.ToString
                .InputProperty = NameOf(Container.AverageCountRange)
            End With
            Collection.Add(Item_AverageCountRange)

            Dim Item_CustomExchangeRate As New SettingsItem
            With Item_CustomExchangeRate
                .Name = "Custom Exchange Rate"
                .Description = "How much a USD is worth in other currencies, this is only used to convert Backpack.tf prices. Disable for default Euro: $1 = 0.89€"
                .IsEnabled = False
                .InputStyle = "String"
                .Input = Container.CustomExchangeRate.ToString
                .InputProperty = NameOf(Container.CustomExchangeRate)
            End With
            Collection.Add(Item_CustomExchangeRate)

            Dim Item_Backpack_APIKey As New SettingsItem
            With Item_Backpack_APIKey
                .Name = "Backpack.tf API Key"
                .Description = "Your API Key for Backpack.tf, otherwise you can not get their prices."
                .IsEnabled = True
                .InputStyle = "String_NoCheckBox"
                .Input = Container.Backpack_APIKey
                .InputProperty = NameOf(Container.Backpack_APIKey)
            End With
            Collection.Add(Item_Backpack_APIKey)

            Dim Item_Steam_APIKey As New SettingsItem
            With Item_Steam_APIKey
                .Name = "Steam Web API Key"
                .Description = "Your API Key for Steam's Web API, otherwise you will have limited functionalities."
                .IsEnabled = True
                .InputStyle = "String_NoCheckBox"
                .Input = Container.Steam_APIKey
                .InputProperty = NameOf(Container.Steam_APIKey)
            End With
            Collection.Add(Item_Steam_APIKey)

            Dim Item_ClearImageCache As New SettingsItem
            With Item_ClearImageCache
                .Name = "Clear Image Cache"
                .Description = "Click to delete cache of Item images."
                .IsEnabled = True
                .InputStyle = "Button"
                .Subroutine = NameOf(Subroutines.ClearImageCache)
            End With
            Collection.Add(Item_ClearImageCache)

            Dim Item_UpdateGameSchema As New SettingsItem
            With Item_UpdateGameSchema
                .Name = "Update Game Scheme"
                .Description = "Click to update current game schema, this will fix missing images related to new Items."
                .IsEnabled = True
                .InputStyle = "Button"
                .Subroutine = NameOf(Subroutines.UpdateGameSchema)
            End With
            Collection.Add(Item_UpdateGameSchema)

            Dim Item_OpenHomeFolder As New SettingsItem
            With Item_OpenHomeFolder
                .Name = "Open Home Folder"
                .Description = "Opens the folder where this program resides."
                .IsEnabled = True
                .InputStyle = "Button"
                .Subroutine = NameOf(Subroutines.OpenHomeFolder)
            End With
            Collection.Add(Item_OpenHomeFolder)

            Dim Item_Validation As New SettingsItem
            With Item_Validation
                .Name = "Validation"
                .Description = "Checks all savable lists for missing values."
                .IsEnabled = True
                .InputStyle = "Button"
                .Subroutine = NameOf(Subroutines.Validation)
            End With
            Collection.Add(Item_Validation)

            Dim Item_CreateBackup As New SettingsItem
            With Item_CreateBackup
                .Name = "Create Backup"
                .Description = "Creates a Backup of the current Game's Savable lists, in the users directory."
                .IsEnabled = True
                .InputStyle = "Button"
                .Subroutine = NameOf(Subroutines.CreateBackup)
            End With
            Collection.Add(Item_CreateBackup)

            Dim Item_BrowserPath As New SettingsItem
            With Item_BrowserPath
                .Name = "Browser Path"
                .Description = "The Path to the Browser that should open Urls."
                .IsEnabled = True
                .InputStyle = "String"
                .Input = Container.BrowserPath
            End With
            Collection.Add(Item_BrowserPath)
        End Sub

        Public Class Class_Container
            Inherits NotifyPropertyChanged

            Class Class_OnShutDown
                Property ClearImageCache As New Boolean
            End Class

            Class Class_IsRunning
                Inherits NotifyPropertyChanged

                Property Refresh As New Boolean
                Private _Refresh_Prices As New Boolean

                Property Refresh_Prices As Boolean
                    Get
                        Return _Refresh_Prices
                    End Get
                    Set
                        NotifyPropertyChanged(_Refresh_Prices, Value)
                    End Set
                End Property

                Property Load As New Boolean
                Property Backup As New Boolean
                Property Login As New Boolean
                Private _PropertyBrush As New Integer

                Public Property PropertyBrush As Integer
                    Get
                        Return _PropertyBrush
                    End Get
                    Set
                        NotifyPropertyChanged(_PropertyBrush, Value)
                    End Set
                End Property

                Private _Saving As Boolean

                Public Property Saving As Boolean
                    Get
                        Return _Saving
                    End Get
                    Set
                        NotifyPropertyChanged(_Saving, Value)
                    End Set
                End Property

            End Class

            Property IsRunning As New Class_IsRunning
            Property OnShutDown As New Class_OnShutDown

            Public Property AutoLogon As Boolean
            Private _AutoGetBackpack As Boolean

            Public Property AutoGetBackpack As Boolean
                Get
                    Return _AutoGetBackpack
                End Get
                Set
                    NotifyPropertyChanged(_AutoGetBackpack, Value)
                End Set
            End Property

            Public Property FantasticDealProcentage As Decimal
            Public Property GreatDealProcentage As Decimal
            Public Property GoodDealProcentage As Decimal
            Public Property BadDealProcentage As Decimal
            Public Property AverageCountStart As Decimal
            Public Property AverageCountRange As Decimal
            Public Property CustomExchangeRate As Decimal
            Private _Backpack_APIKey As String

            Public Property Backpack_APIKey As String
                Get
                    Return _Backpack_APIKey
                End Get
                Set
                    NotifyPropertyChanged(_Backpack_APIKey, Value)
                End Set
            End Property

            Private _Steam_APIKey As String

            Public Property Steam_APIKey As String
                Get
                    Return _Steam_APIKey
                End Get
                Set
                    NotifyPropertyChanged(_Steam_APIKey, Value)
                End Set
            End Property

            Private _ShowValuation As Boolean

            Public Property ShowValuation As Boolean
                Get
                    Return _ShowValuation
                End Get
                Set
                    NotifyPropertyChanged(_ShowValuation, Value)
                End Set
            End Property

            Private _Alternative_DisplayPrices As Boolean

            Public Property Alternative_DisplayPrices As Boolean
                Get
                    Return _Alternative_DisplayPrices
                End Get
                Set
                    NotifyPropertyChanged(_Alternative_DisplayPrices, Value)
                End Set
            End Property

            Private _Grouping_Min_Inventory As Integer

            Public Property Grouping_Min_Inventory As Integer
                Get
                    Return _Grouping_Min_Inventory
                End Get
                Set
                    NotifyPropertyChanged(_Grouping_Min_Inventory, Value)
                    UpdateGrouping()
                End Set
            End Property

            Private _Grouping_Min_Listings As Integer

            Public Property Grouping_Min_Listings As Integer
                Get
                    Return _Grouping_Min_Listings
                End Get
                Set
                    NotifyPropertyChanged(_Grouping_Min_Listings, Value)
                    UpdateGrouping()
                End Set
            End Property

            Private _Grouping_Inventory As Boolean

            Public Property Grouping_Inventory As Boolean
                Get
                    Return _Grouping_Inventory
                End Get
                Set
                    NotifyPropertyChanged(_Grouping_Inventory, Value)
                    UpdateGrouping()
                End Set
            End Property

            Private _Independent_Search_Query As Boolean

            Public Property Independent_Search_Query As Boolean
                Get
                    Return _Independent_Search_Query
                End Get
                Set
                    NotifyPropertyChanged(_Independent_Search_Query, Value)
                End Set
            End Property

            Private _Always_Buy_Max_Amount As Boolean

            Public Property Always_Buy_Max_Amount As Boolean
                Get
                    Return _Always_Buy_Max_Amount
                End Get
                Set
                    NotifyPropertyChanged(_Always_Buy_Max_Amount, Value)
                End Set
            End Property

            Private _Grouping_Listings As Boolean

            Public Property Grouping_Listings As Boolean
                Get
                    Return _Grouping_Listings
                End Get
                Set
                    NotifyPropertyChanged(_Grouping_Listings, Value)
                    UpdateGrouping()
                End Set
            End Property

            Private _Backpack_Price_Key_InMetal As Decimal

            Public Property Backpack_Price_Key_InMetal As Decimal
                Get
                    Return _Backpack_Price_Key_InMetal
                End Get
                Set
                    NotifyPropertyChanged(_Backpack_Price_Key_InMetal, Value)
                    Exchange_Calculate()
                End Set
            End Property

            Private _Backpack_Price_Refined As Decimal

            Public Property Backpack_Price_Refined As Decimal
                Get
                    Return _Backpack_Price_Refined
                End Get
                Set
                    NotifyPropertyChanged(_Backpack_Price_Refined, Value)
                    Exchange_Calculate()
                End Set
            End Property

            Private _KeyPrice As Decimal

            Public Property KeyPrice As Decimal
                Get
                    Return _KeyPrice
                End Get
                Set
                    NotifyPropertyChanged(_KeyPrice, Value)
                    Exchange_Calculate()
                End Set
            End Property

            Private _RefinedMetalPrice As Decimal

            Public Property RefinedMetalPrice As Decimal
                Get
                    Return _RefinedMetalPrice
                End Get
                Set
                    NotifyPropertyChanged(_RefinedMetalPrice, Value)
                    Exchange_Calculate()
                End Set
            End Property

            Private _TotalForSale As String

            Public Property TotalForSale As String
                Get
                    Return _TotalForSale
                End Get
                Set
                    NotifyPropertyChanged(_TotalForSale, Value)
                End Set
            End Property

            Private _Balance As Decimal

            Public Property Balance As Decimal
                Get
                    Return _Balance
                End Get
                Set
                    NotifyPropertyChanged(_Balance, Value)
                End Set
            End Property

            Private _TotalMetal As New Metal

            Public Property TotalMetal As Metal
                Get
                    Return _TotalMetal
                End Get
                Set
                    NotifyPropertyChanged(_TotalMetal, Value)
                End Set
            End Property

            Private _BalanceMetal As String

            Public Property BalanceMetal As String
                Get
                    Return _BalanceMetal
                End Get
                Set
                    NotifyPropertyChanged(_BalanceMetal, Value)
                End Set
            End Property

            ''' <summary>
            ''' 0 Login, 1 Cancel, 2 Logoff, 3 LogOut, 4 ReLogIn
            ''' </summary>
            Private _LoginButtonSetting As New MainWindow.LogInState

            Public Property LoginButtonSetting As MainWindow.LogInState
                Get
                    Return _LoginButtonSetting
                End Get
                Set
                    NotifyPropertyChanged(_LoginButtonSetting, Value)
                End Set
            End Property

            Private _ReLoginPossible As Boolean

            Public Property ReLoginPossible As Boolean
                Get
                    Return _ReLoginPossible
                End Get
                Set
                    NotifyPropertyChanged(_ReLoginPossible, Value)
                End Set
            End Property

            Private _CurrentUser As New User

            Public Property CurrentUser As User
                Get
                    Return _CurrentUser
                End Get
                Set
                    NotifyPropertyChanged(_CurrentUser, Value)
                End Set
            End Property

            Private _AuthAppOpen As New Boolean

            Public Property AuthAppOpen As Boolean
                Get
                    Return _AuthAppOpen
                End Get
                Set
                    NotifyPropertyChanged(_AuthAppOpen, Value)
                End Set
            End Property

            Public Exchange_ToMoney_Keys As Decimal
            Public Exchange_ToMoney_Scrap As Decimal
            Public Exchange_ToMoney_Reclaimed As Decimal
            Private _Exchange_ToMoney_Refined As Decimal

            Public Property Exchange_ToMoney_Refined As Decimal
                Get
                    Return _Exchange_ToMoney_Refined
                End Get
                Set
                    NotifyPropertyChanged(_Exchange_ToMoney_Refined, Value)
                End Set
            End Property

            Public Exchange_SteamTax As Decimal = 15

            Private _CurrentSelectedGame As Integer = 0

            Public Property CurrentSelectedGame As Integer
                Get
                    Return _CurrentSelectedGame
                End Get
                Set
                    NotifyPropertyChanged(_CurrentSelectedGame, Value)
                End Set
            End Property
            Private _BrowserPath As String = "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
            Public Property BrowserPath As String
                Get
                    Return _BrowserPath
                End Get
                Set
                    NotifyPropertyChanged(_BrowserPath, Value)
                End Set
            End Property
            Public Property CurrentGameName As String = "Team Fortress 2"
            Public PreviousSelectedGame As Integer = 0

            Public CurrentAppID As Integer = 440

        End Class

    End Class

    Public Module Generator

        <Serializable>
        Class SettingsItem
            Inherits NotifyPropertyChanged

            Private _Name As String

            <XmlAttribute>
            Public Property Name As String
                Get
                    Return _Name
                End Get
                Set
                    NotifyPropertyChanged(_Name, Value)
                End Set
            End Property

            Private _Description As String

            <XmlIgnore>
            Public Property Description As String
                Get
                    Return _Description
                End Get
                Set
                    NotifyPropertyChanged(_Description, Value)
                End Set
            End Property

            Private _IsEnabled As Boolean = False

            <XmlAttribute>
            Public Property IsEnabled As Boolean
                Get
                    Return _IsEnabled
                End Get
                Set
                    NotifyPropertyChanged(_IsEnabled, Value)
                    SetDefaultValue()
                End Set
            End Property

            Private _InputStyle As String

            <XmlIgnore>
            Public Property InputStyle As String
                Get
                    Return _InputStyle
                End Get
                Set
                    NotifyPropertyChanged(_InputStyle, Value)
                End Set
            End Property

            Private _Input As String

            <XmlAttribute>
            Public Property Input As String
                Get
                    Return _Input
                End Get
                Set
                    NotifyPropertyChanged(_Input, Value)
                    SetDefaultValue()
                End Set
            End Property

            Private _InputProperty As String

            <XmlIgnore>
            Public Property InputProperty As String
                Get
                    Return _InputProperty
                End Get
                Set
                    NotifyPropertyChanged(_InputProperty, Value)
                End Set
            End Property

            Private _Subroutine As String

            <XmlIgnore>
            Public Property Subroutine As String
                Get
                    Return _Subroutine
                End Get
                Set
                    NotifyPropertyChanged(_Subroutine, Value)
                End Set
            End Property

            Private _TextAligment As TextAlignment = TextAlignment.Right

            <XmlIgnore>
            Public Property TextAlignment As TextAlignment
                Get
                    Return _TextAligment
                End Get
                Set
                    NotifyPropertyChanged(_TextAligment, Value)
                End Set
            End Property

            Sub SetDefaultValue()
                If (_InputStyle = "Boolean") Then
                    _Input = _IsEnabled.ToString
                End If
                If (Not String.IsNullOrWhiteSpace(_InputProperty)) AndAlso (Not String.IsNullOrWhiteSpace(_Input)) Then
                    If _IsEnabled Then
                        SetPropertyValueByName(MainWindow.Settings.Container, _InputProperty, _Input)
                    ElseIf (Not _IsEnabled) Then
                        If (My.Settings.PropertyValues.Item(_InputProperty).PropertyValue IsNot Nothing) Then
                            SetPropertyValueByName(MainWindow.Settings.Container, _InputProperty, My.Settings.PropertyValues.Item(_InputProperty).PropertyValue)
                        End If
                    End If
                End If
            End Sub

        End Class

        Sub Set_SettingsDefaultValues()
            For Each Setting In MainWindow.Settings.Collection
                If (Setting.InputProperty IsNot Nothing) Then
                    Dim Value = GetPropertyValueByName(My.Settings, Setting.InputProperty)
                    SetPropertyValueByName(MainWindow.MainWindow, Setting.InputProperty, Value)
                End If
            Next
        End Sub

        Sub Import(Settings As ObservableCollection(Of SettingsItem), Input As List(Of SettingsItem))
            Parallel.ForEach(Settings, Sub(Setting)
                                           Dim Item = Input.FirstOrDefault(Function(F) Setting.Name.Equals(F.Name, StringComparison.CurrentCultureIgnoreCase))

                                           If (Item IsNot Nothing) Then
                                               Setting.Input = Item.Input
                                               Setting.IsEnabled = Item.IsEnabled
                                           End If
                                       End Sub)
        End Sub

        Sub UpdateGrouping()
            If MainWindow.Settings.Container.Grouping_Inventory Then
                MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub()
                                                                 Dim View_Inventory As New ListCollectionView(MainWindow.Binding_Lv_Inventory)
                                                                 View_Inventory.GroupDescriptions.Add(New PropertyGroupDescription("DefIndex"))
                                                                 View_Inventory.IsLiveSorting = True
                                                                 View_Inventory.IsLiveFiltering = True
                                                                 View_Inventory.IsLiveGrouping = True
                                                                 CollectionViewSource.GetDefaultView(View_Inventory).Filter = (New Predicate(Of Object)(AddressOf MainWindow.MainWindow.UserFilter))
                                                                 MainWindow.MainWindow.Lv_Inventory.ItemsSource = View_Inventory
                                                             End Sub)
            Else
                MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub()
                                                                 MainWindow.MainWindow.Lv_Inventory.DataContext = MainWindow.Binding_Lv_Inventory
                                                                 MainWindow.MainWindow.Lv_Inventory.ItemsSource = MainWindow.Binding_Lv_Inventory
                                                                 CollectionViewSource.GetDefaultView(MainWindow.MainWindow.Lv_Inventory.ItemsSource).Filter = (New Predicate(Of Object)(AddressOf MainWindow.MainWindow.UserFilter))
                                                             End Sub)
            End If

            If (MainWindow.Settings.Container.Grouping_Listings = True) Then
                MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub()
                                                                 Dim View_Listings As New ListCollectionView(MainWindow.Binding_Lv_Listings)
                                                                 View_Listings.GroupDescriptions.Add(New PropertyGroupDescription("Name"))
                                                                 View_Listings.IsLiveSorting = True
                                                                 View_Listings.IsLiveFiltering = True
                                                                 View_Listings.IsLiveGrouping = True
                                                                 CollectionViewSource.GetDefaultView(View_Listings).Filter = (New Predicate(Of Object)(AddressOf MainWindow.MainWindow.UserFilter))
                                                                 MainWindow.MainWindow.Lv_Listings.ItemsSource = View_Listings
                                                             End Sub)
            Else
                MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub()
                                                                 MainWindow.MainWindow.Lv_Listings.DataContext = MainWindow.Binding_Lv_Listings
                                                                 MainWindow.MainWindow.Lv_Listings.ItemsSource = MainWindow.Binding_Lv_Listings
                                                                 CollectionViewSource.GetDefaultView(MainWindow.MainWindow.Lv_Listings.ItemsSource).Filter = (New Predicate(Of Object)(AddressOf MainWindow.MainWindow.UserFilter))
                                                             End Sub)
            End If
        End Sub

        Sub Exchange_Calculate()
            If ((MainWindow.Settings.Container.Backpack_Price_Key_InMetal > 0) AndAlso (MainWindow.Settings.Container.Backpack_Price_Refined > 0)) Then
                Dim Price_Key As New Decimal
                Dim Price_Refined As New Decimal
                Dim Price_ExchangeRate As New Decimal

                If (MainWindow.Settings.Collection.Where(Function(F) F.InputProperty.Equals(NameOf(MainWindow.Settings.Container.KeyPrice))).First.IsEnabled = True) Then
                    Price_Key = MainWindow.Settings.Container.KeyPrice
                Else
                    Price_Key = MainWindow.Settings.Container.Backpack_Price_Key_InMetal
                End If

                If (MainWindow.Settings.Collection.Where(Function(F) F.InputProperty.Equals(NameOf(MainWindow.Settings.Container.RefinedMetalPrice))).First.IsEnabled = True) Then
                    Price_Refined = MainWindow.Settings.Container.RefinedMetalPrice
                Else
                    Price_Refined = MainWindow.Settings.Container.Backpack_Price_Refined
                End If

                'Refined price in USD * Key price in refined metal * Exchange rate
                MainWindow.Settings.Container.Exchange_ToMoney_Keys = (Price_Refined * Price_Key * MainWindow.Settings.Container.CustomExchangeRate)

                'Divides the price of refined to its de-craftable versions and applies the exchange rate
                MainWindow.Settings.Container.Exchange_ToMoney_Scrap = ((Price_Refined / 9) * MainWindow.Settings.Container.CustomExchangeRate)
                MainWindow.Settings.Container.Exchange_ToMoney_Reclaimed = ((Price_Refined / 3) * MainWindow.Settings.Container.CustomExchangeRate)
                MainWindow.Settings.Container.Exchange_ToMoney_Refined = (Price_Refined * MainWindow.Settings.Container.CustomExchangeRate)
            End If
        End Sub

        Class Subroutines

            Sub ClearImageCache()
                MainWindow.Settings.Container.OnShutDown.ClearImageCache = True
                Status("Image Cache will be cleared on shutdown.")
            End Sub

            Sub UpdateGameSchema()
                Status("Current game schema will be updated now.")
                Network.Steam.API.UpdateSchema()
            End Sub

            Sub OpenHomeFolder()
                Process.Start(MainWindow.Home)
            End Sub

            Sub Validation()
                If (MsgBox(Box("Are you sure you want to Validation your savable lists?", "In case of error, corruption might occur, take a backup if you haven't already."), MsgBoxStyle.OkCancel, "Confirm Action") = MsgBoxResult.Ok) Then
                    Dim Validator As New Validator
                    Validator.Start(MainWindow.Binding_Lv_Favorite, MainWindow.Binding_Lv_WishList, MainWindow.Binding_Lv_GiftList)
                End If
            End Sub

            Sub CreateBackup()
                If ((Not MainWindow.Settings.Container.IsRunning.Backup) AndAlso MainWindow.Network.IsLoggedIn) Then
                    If (MainWindow.MainWindow.Txbk_Sb_SaveTimeStamp.Text IsNot Nothing) Then
                        MainWindow.Settings.Container.IsRunning.Backup = True
                        Dim Time As String = Date.Now.ToString("dd-MM-yyyy_HH-mm-ss")
                        Dim Path As String = IO.Path.Combine(MainWindow.Home, "Users", MainWindow.Settings.Container.CurrentUser.SteamID.ToString, "Backup", Time + ".xml")
                        Directory.CreateDirectory(Path)

                        Dim Document As New GUI.UserDocument With
                               {
                                    .Favorites = MainWindow.Binding_Lv_Favorite.ToList,
                                    .GiftList = MainWindow.Binding_Lv_GiftList.ToList,
                                    .Inventory = MainWindow.Binding_Lv_Inventory.ToList,
                                    .WishList = MainWindow.Binding_Lv_WishList.ToList,
                                    .Settings = MainWindow.Settings.Collection.ToList
                                }

                        If Document.Save(Path) Then
                            MsgBox(Box("A Backup was Successfully Created!", "Folder Name : " + Time), MsgBoxStyle.OkOnly, "Backup Successful")
                        Else
                            MsgBox("Backup FAILED.", MsgBoxStyle.OkOnly, "Backup Failed")
                        End If

                        MainWindow.Settings.Container.IsRunning.Backup = False
                    Else
                        Status("An Existing save Required; Please do a manual save first.")
                    End If
                Else
                    Status("Login Required.")
                End If
            End Sub

        End Class

    End Module
End Namespace
