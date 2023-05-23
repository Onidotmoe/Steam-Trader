Imports System.Windows.Controls.Primitives
Imports SteamTrader.GUI
Imports SteamTrader.MainWindow
Imports SteamTrader.Modify.Single
Imports SteamTrader.Scraper
Imports U

Public Class ResourceDirectory

    Public Sub Hyperlink_RequestNavigate(sender As Object, e As RequestNavigateEventArgs)
        Dim Url As String = e.Uri.AbsoluteUri.ToString

        If Url.Contains("#") Then
            Url = Url.Replace("#", "%23")
        End If

        Process.Start(Url)

        e.Handled = True
    End Sub

    Public Sub Txb_TextChanged(sender As Object, e As TextChangedEventArgs)
        TextChanged(sender, e)
    End Sub

    Public Sub TextChanged(sender As Object, e As EventArgs)
        Dim TextBox As TextBox = CType(sender, TextBox)
        Dim iStart As Integer = TextBox.SelectionStart
        Dim iLength As Integer = TextBox.SelectionLength
        Dim iText As String = Nothing
        Dim Count As Integer = 0

        For Each C As Char In TextBox.Text.ToCharArray()
            If (Char.IsDigit(C)) OrElse (Char.IsControl(C)) OrElse (C = "."c AndAlso Count = 0) Then
                iText += C

                If C = "."c Then
                    Count += 1
                End If
            End If
        Next

        TextBox.Text = iText
        TextBox.SelectionStart = If(iStart <= TextBox.Text.Length, iStart, TextBox.Text.Length)
    End Sub

    Private Async Sub DataGridRow_MouseDoubleClick(sender As Object, e As RoutedEventArgs)
        If (MainWindow.Settings.Container.IsRunning.PropertyBrush = 0) Then
            Dim Parent As DataGrid = GetDataGridParent(sender)

            If (Parent.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected) Then
                Parent.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed
            Else
                Parent.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected
            End If

            Select Case Parent.Name
                Case MainWindow.MainWindow.Lv_Inventory.Name, MainWindow.MainWindow.Lv_Listings.Name
                    Dim Row As DataGridRow = CType(sender, DataGridRow)
                    Dim Item As Item = CType(Row.DataContext, Item)

                    If (Item.Price.CurrentMarketPrices.Steam.Success = 0) Then
                        Await AssignPrice_Steam(Item)
                    End If
                    Item.Update()

                Case MainWindow.MainWindow.Lv_Prices.Name, MainWindow.MainWindow.Lv_Keys.Name, MainWindow.MainWindow.Lv_Favorite.Name, MainWindow.MainWindow.Lv_WishList.Name, MainWindow.MainWindow.Lv_GiftList.Name, MainWindow.MainWindow.Lv_Orders.Name
                    Dim Row As DataGridRow = CType(sender, DataGridRow)
                    Dim Item As Item = CType(Row.DataContext, Item)

                    If ((Item.Price.CurrentMarketPrices.Steam.Success = 0) AndAlso (Item.Price.CurrentMarketPrices.Backpack.Success = 0)) Then
                        Dim Steam As Item = Item
                        Dim Backpack As Item = Item

                        Await Task.WhenAll(AssignPrice_Steam(Steam), AssignPrice_Backpack(Backpack))

                        With Steam
                            .Price.Backpack = Backpack.Price.Backpack
                            .Price.Backpack_Average = Backpack.Price.Backpack_Average
                            .Price.CurrentMarketPrices.Backpack.Buy = Backpack.Price.CurrentMarketPrices.Backpack.Buy
                            .Price.CurrentMarketPrices.Backpack.Sell = Backpack.Price.CurrentMarketPrices.Backpack.Sell
                            .Price.CurrentMarketPrices.Backpack.Success = Backpack.Price.CurrentMarketPrices.Backpack.Success
                        End With

                        Item = Steam

                    ElseIf (Item.Price.CurrentMarketPrices.Steam.Success = 0) Then
                        Await AssignPrice_Steam(Item)
                    End If

                    Item.Update()
            End Select
        End If
    End Sub

    Private Async Function AssignPrice_Steam(Item As Item) As Task(Of Item)
        Item.Price.CurrentMarketPrices.Steam.Success = 2
        Item.Price.CurrentMarketPrices.Steam.Buy = Await GetActualMarketListings_Steam(Item)

        If Item.Price.CurrentMarketPrices.Steam.Buy.None Then
            Item.Price.CurrentMarketPrices.Steam.Success = -1
            Item.SteamObtained = 2
        Else
            Item.Price.Steam = Item.Price.CurrentMarketPrices.Steam.Buy.First.Price
            If (Item.Price.CurrentMarketPrices.Steam.Buy.Count >= 10) Then
                Dim SteamTotal As New Decimal

                For i As Integer = 3 To Item.Price.CurrentMarketPrices.Steam.Buy.Count - 3
                    SteamTotal += Item.Price.CurrentMarketPrices.Steam.Buy(i).Price
                Next

                Item.Price.Steam_Average = SteamTotal / 5
            Else
                Item.Price.Steam_Average = Item.Price.CurrentMarketPrices.Steam.Buy.Sum(Function(F) F.Price) / Item.Price.CurrentMarketPrices.Steam.Buy.Count
            End If

            If (Item.Price.CurrentMarketPrices.Steam.Buy.Count >= 2) Then
                Item.Price.Sell = Item.Price.CurrentMarketPrices.Steam.Buy(1).Price
                If (Item.Price.Sell > CDec(0.01)) Then
                    Item.Price.Sell -= CDec(0.01)
                End If
            End If

            Item.Price.CurrentMarketPrices.Steam.Success = 1
            Item.SteamObtained = 1
        End If

        Return Item
    End Function

    Private Async Function AssignPrice_Backpack(Item As Item) As Task(Of Item)
        Item.Price.CurrentMarketPrices.Backpack.Success = 2
        Await Task.Run(Sub() Network.Backpack.API.API.GetAssetPrices_Singleton(Item))

        If Item.Price.CurrentMarketPrices.Backpack.Buy.None OrElse Item.Price.CurrentMarketPrices.Backpack.Sell.None Then
            Item.Price.CurrentMarketPrices.Backpack.Success = -1
        Else
            Item.Price.CurrentMarketPrices.Backpack.Success = 1
        End If

        Return Item
    End Function

    Private Sub DataGrid_SelectionChanged(sender As Object, e As RoutedEventArgs)
        'Dim x = TryCast(CType(sender, DataGrid).SelectedItem, Item)
        'If x IsNot Nothing Then
        '    Debug.WriteLine(x.DefIndex, x.MarketHashID, x.Name, x.Quality.Name, x.Price.Url_Steam, x.Price.Url_Backpack)
        'End If

        'CType(sender, DataGrid).RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed
    End Sub

    Private Sub DataGrid_Loaded(sender As Object, e As RoutedEventArgs)
        Dim DataGrid = DirectCast(sender, DataGrid)
        DataGrid.CommitEdit()
        DataGrid.CancelEdit()
    End Sub

    Private Sub DataGrid_Unloaded(sender As Object, e As RoutedEventArgs)
        Dim DataGrid = DirectCast(sender, DataGrid)
        DataGrid.CommitEdit()
        DataGrid.CancelEdit()
    End Sub

    Public Shared Function GetDataGridParent(sender As Object) As DataGrid
        Dim Parent = DirectCast(sender, DependencyObject)

        While ((Parent IsNot Nothing) AndAlso (Parent.GetType() IsNot GetType(DataGrid)))
            Parent = VisualTreeHelper.GetParent(Parent)
        End While

        Return CType(Parent, DataGrid)
    End Function

    Private Sub TB_RightClick(sender As Object, e As MouseEventArgs)
        Dim TB_Button As Primitives.ToggleButton = CType(sender, Primitives.ToggleButton)
        Dim Item As Item = CType(TB_Button.DataContext, Item)

        If (Item.SteamObtained = -1) Then
            Item.SteamObtained = 0
        End If
    End Sub

    Private Sub DataGridRow_PreviewMouseLeftButtonDown(sender As Object, e As RoutedEventArgs)
        If (MainWindow.Settings.Container.IsRunning.PropertyBrush > 0) AndAlso (MainWindow.MainWindow.TabControl_Main.SelectedItem IsNot MainWindow.MainWindow.Tab_Backpack) Then
            PropertyBrush_Handle(sender)
        End If
    End Sub

    Private Sub DataGridCell_PreviewMouseLeftButtonDown(sender As Object, e As RoutedEventArgs)
        If (MainWindow.Settings.Container.IsRunning.PropertyBrush > 0) AndAlso (MainWindow.MainWindow.TabControl_Main.SelectedItem Is MainWindow.MainWindow.Tab_Backpack) Then
            PropertyBrush_Handle(sender)
        End If
    End Sub

    Private Sub PropertyBrush_Handle(sender As Object)
        Select Case DirectCast(MainWindow.MainWindow.Hint.Tag, Button).Name
            Case MainWindow.MainWindow.Btn_SelectedFavorite.Name
                AddToList(sender, Binding_Lv_Favorite)
            Case MainWindow.MainWindow.Btn_SelectedRemoveFavorite.Name
                RemoveFromList(sender, Binding_Lv_Favorite)
            Case MainWindow.MainWindow.Btn_SelectedWishlist.Name
                AddToList(sender, Binding_Lv_WishList)
            Case MainWindow.MainWindow.Btn_SelectedRemoveWishlist.Name
                RemoveFromList(sender, Binding_Lv_WishList)
            Case MainWindow.MainWindow.Btn_SelectedGiftlist.Name
                AddToList(sender, Binding_Lv_GiftList)
            Case MainWindow.MainWindow.Btn_SelectedRemoveGiftlist.Name
                RemoveFromList(sender, Binding_Lv_GiftList)

            Case MainWindow.MainWindow.Btn_SelectedMarkedForSell.Name
                MarkUnMark(sender, True)
            Case MainWindow.MainWindow.Btn_SelectedUnMarkedForSell.Name
                MarkUnMark(sender, False)
            Case MainWindow.MainWindow.Btn_SelectedLock.Name
                LockUnlock(sender, True)
            Case MainWindow.MainWindow.Btn_SelectedUnlock.Name
                LockUnlock(sender, False)
        End Select
    End Sub

    Private Sub Cbbx_SelectableQuality_SelectionChanged(sender As Object, e As RoutedEventArgs)
        Dim Item = DirectCast(DirectCast(sender, ComboBox).DataContext, Item)

        Dim MarketID As String = Nothing
        For Each Slave In DownloadHandler.SlaveBag
            If (Slave.Defindex = Item.DefIndex) Then
                MarketID = Slave.Name
                Exit For
            End If
        Next

        If (Item.Quality.ID <> 6) Then
            MarketID = Item.Quality.Name + " " + RemoveQualityName(MarketID, True)
        End If

        Item.MarketHashID = MarketID
        Item.Name = MarketID
    End Sub

    ' We can't simply change the background color of all ComboBoxes through xaml.
    Public Sub ComboBox_Loaded(sender As Object, e As RoutedEventArgs)
        Dim ComboBox As ComboBox = CType(sender, ComboBox)
        Dim ToggleButton As ToggleButton = TryCast(ComboBox.Template.FindName("toggleButton", ComboBox), ToggleButton)

        If (ToggleButton IsNot Nothing) Then
            Dim Border As Border = TryCast(ToggleButton.Template.FindName("templateRoot", ToggleButton), Border)

            If (Border IsNot Nothing) Then
                Border.Background = DirectCast(Application.Current.Resources("DarkBackground"), SolidColorBrush)
            End If
        End If
    End Sub

End Class
