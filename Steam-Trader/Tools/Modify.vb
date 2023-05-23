Imports System.Collections.Concurrent
Imports SteamTrader.GUI
Imports SteamTrader.MainWindow
Imports U

Namespace Modify

    Public Module [Single]

        Sub AddToList(Sender As Object, List As ObservableCollection(Of Item))
            Dim SelectedDataGrid As DataGrid = MainWindow.MainWindow.GetSelectedDataGrid()

            If Equals(SelectedDataGrid, MainWindow.MainWindow.Lv_Backpack) Then
                Dim Item As New Item
                Dim SelectedCell As DataGridCell = DirectCast(Sender, DataGridCell)
                Dim Price As Backpack.Price = TryCast(SelectedCell.DataContext, Backpack.Price)
                Dim Validator As New Validator

                If (Price IsNot Nothing) Then
                    Dim QualityName As String = Nothing
                    FindString(SelectedCell.Column.SortMemberPath, Nothing, QualityName, ".Money")
                    Dim Quality = TryCast(Price.GetType.GetProperty(QualityName)?.GetValue(Price), Backpack.Quality)

                    If (Quality IsNot Nothing) Then
                        With Item
                            Dim MarketHashID = If(Quality.ID <> 6, Quality.Name + " " + RemoveQualityName(Price.Name, True), Price.Name)
                            .MarketHashID = MarketHashID
                            .Name = MarketHashID
                            .Quality.ID = Quality.ID
                            .Quality.Name = Quality.Name
                            .AppID = MainWindow.Settings.Container.CurrentAppID
                            .Craftable = Price.Craftable
                            .Tradable = Price.Tradable
                            .UsableInCrafting = Price.UsableInCrafting
                            .Type = Price.Type
                            .DefIndex = Price.DefIndex
                            .WikiURL = Price.WikiURL
                            .ImageSource = Price.ImageSource
                            .Price.Url_Steam = Quality.Url_Steam
                            .Price.Url_Backpack = Quality.Url_Backpack
                            .Price.Backpack = Quality.Money
                        End With

                        Validator.Singleton(Item)

                        List.Add(Item)
                    End If
                End If
            Else
                List.Add(DirectCast(DirectCast(Sender, DataGridRow).Item, Item))
            End If
        End Sub

        Sub RemoveFromList(Sender As Object, List As ObservableCollection(Of Item))
            Dim Item = DirectCast(DirectCast(Sender, DataGridRow).Item, Item)
            Dim i = List.IndexOf(Item)

            If (i > -1) Then
                List.RemoveAt(i)
            End If
        End Sub

        Sub MarkUnMark(Sender As Object, Toggle As Boolean)
            DirectCast(DirectCast(Sender, DataGridRow).Item, Item).MarkedForSell = Toggle
        End Sub

        Sub LockUnlock(Sender As Object, Toggle As Boolean)
            DirectCast(DirectCast(Sender, DataGridRow).Item, Item).Locked = Toggle
        End Sub

    End Module

    Public Module Multiple

        Sub AddToList(List As ObservableCollection(Of Item))
            Dim SelectedDataGrid As DataGrid = MainWindow.MainWindow.GetSelectedDataGrid()

            If Equals(SelectedDataGrid, MainWindow.MainWindow.Lv_Backpack) Then
                Dim Source As ObservableCollection(Of Backpack.Price) = DirectCast(MainWindow.MainWindow.GetSelectedDataGridSource(), ObservableCollection(Of Backpack.Price))
                Dim Output As New ConcurrentBag(Of Item)
                Dim Validator As New Validator

                Parallel.ForEach(Source,
                             Sub(Level_1)
                                 Parallel.ForEach(Qualities,
                                                  Sub(Q)
                                                      Dim Level_2 = Level_1.GetType.GetProperty(Q.Name)

                                                      If (Level_2 IsNot Nothing) Then
                                                          Dim Level_3 As Backpack.Quality = DirectCast(Level_2.GetValue(Level_1), Backpack.Quality)

                                                          If (Level_3 IsNot Nothing) AndAlso (Level_3.IsSelected = True) Then
                                                              Dim iItem As New Item

                                                              With iItem
                                                                  Dim MarketHashID = If(Q.ID <> 6, Q.Name + " " + RemoveQualityName(Level_1.Name, True), Level_1.Name)
                                                                  .MarketHashID = MarketHashID
                                                                  .Name = MarketHashID
                                                                  .Quality.ID = Q.ID
                                                                  .Quality.Name = Q.Name
                                                                  .AppID = MainWindow.Settings.Container.CurrentAppID
                                                                  .Craftable = Level_1.Craftable
                                                                  .Tradable = Level_1.Tradable
                                                                  .UsableInCrafting = Level_1.UsableInCrafting
                                                                  .Type = Level_1.Type
                                                                  .DefIndex = Level_1.DefIndex
                                                                  .WikiURL = Level_1.WikiURL
                                                                  .ImageSource = Level_1.ImageSource
                                                                  .Price.Url_Steam = Level_3.Url_Steam
                                                                  .Price.Url_Backpack = Level_3.Url_Backpack
                                                                  .Price.Backpack = Level_3.Money
                                                              End With

                                                              Validator.Singleton(iItem)

                                                              Output.Add(iItem)
                                                          End If
                                                      End If
                                                  End Sub)
                             End Sub)

                List.AddRange(Output)
            Else
                Dim Source As ObservableCollection(Of Item) = DirectCast(MainWindow.MainWindow.GetSelectedDataGridSource(), ObservableCollection(Of Item))

                For i As Integer = 0 To Source.Count - 1
                    If (Source(i).IsSelected = True) Then
                        Dim sItem As Item = Source(i).Clone()
                        sItem.IsSelected = False
                        List.Add(sItem)
                    End If
                Next
            End If
        End Sub

        Sub RemoveFromList(List As ObservableCollection(Of Item))
            For i As Integer = (List.Count - 1) To 0 Step -1
                If List(i).IsSelected Then
                    List.RemoveAt(i)
                End If
            Next
        End Sub

        Sub MarkUnMark(Mark As Boolean)
            Dim Source As ObservableCollection(Of Item) = DirectCast(MainWindow.MainWindow.GetSelectedDataGridSource(), ObservableCollection(Of Item))

            For i As Integer = (Source.Count - 1) To 0 Step -1
                If (Source(i).IsSelected AndAlso (Not Source(i).InTransit) AndAlso (Not Source(i).Locked)) Then
                    Source(i).MarkedForSell() = Mark
                End If
            Next

            MainWindow.MainWindow.UpdateMetalCount()
        End Sub

        Sub LockUnlock(Lock As Boolean)
            Dim Source As ObservableCollection(Of Item) = DirectCast(MainWindow.MainWindow.GetSelectedDataGridSource(), ObservableCollection(Of Item))

            For i As Integer = (Source.Count - 1) To 0 Step -1
                If (Source(i).IsSelected AndAlso (Not Source(i).InTransit)) Then
                    Source(i).Locked() = Lock
                End If
            Next
        End Sub

    End Module

End Namespace
