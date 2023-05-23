Imports System.IO
Imports System.Net
Imports System.Threading
Imports SteamTrader.GUI
Imports SteamTrader.MainWindow
Imports U

Public Class Validator
    Dim SlaveBag As List(Of DownloadManager.Item_Url) = DownloadHandler.SlaveBag.ToList
    Dim Backpack As List(Of Backpack.Price) = Binding_Lv_Backpack.ToList
    Dim Needs_Download As New List(Of DownloadManager.Download)
    Dim MissingCount_Field As New Integer
    Dim MissingCount_Image As New Integer
    Dim Validations As New Integer
    Dim AssetPrices As Network.Steam.API.GetAssetPrices.GetAssetPrices_Class

    Sub Specified(List As IEnumerable(Of Item))
        Parallel.ForEach(List, Sub(Item) Singleton_Check(Item))
    End Sub

    Sub Singleton(iItem As Item)
        Singleton_Check(iItem)
    End Sub

    Sub Start(ParamArray Lists() As ObservableCollection(Of Item))
        Create_ClassIDLink()
        Cache_Check()
        DownloadHandler.Start(Needs_Download)
        List_Check(Lists)

        Status(String.Format("Missing Filled : Fields: {0}, Images Downloaded: {1}, Validations: {2}", MissingCount_Field, MissingCount_Image, Validations))
    End Sub

    Private Sub Cache_Check()
        Dim Cache_Path As String = DownloadHandler.Cache.SaveDirectory
        Dim Misc_Path As String = DownloadHandler.Cache_Misc.SaveDirectory

        If File.Exists(Cache_Path) Then
            Cache_Check_Path(DownloadHandler.Cache.Directory, Cache_Path)
        End If
        If File.Exists(Misc_Path) Then
            Cache_Check_Path(DownloadHandler.Cache_Misc.Directory, Misc_Path)
        End If

        Cache_Check_File(DownloadHandler.Cache.Directory, Cache_Path, True)
        Cache_Check_File(DownloadHandler.Cache_Misc.Directory, Misc_Path)
    End Sub

    Private Sub Cache_Check_Path(Cache As List(Of DownloadManager.CacheItem), DirectoryPath As String)
        For Each Item In Cache
            Dim FilePath As String = Path.Combine(DirectoryPath, Item.Filename + ".png")

            If (Not File.Exists(FilePath)) Then
                MissingCount_Image += 1
                Needs_Download.Add(New DownloadManager.Download(Item.ImageName))
            End If
        Next
    End Sub

    Private Sub Cache_Check_File(Cache As List(Of DownloadManager.CacheItem), DirectoryPath As String, Optional NonMisc As Boolean = False)
        For Each File In New DirectoryInfo(DirectoryPath).GetFiles().[Select](Function(F) If(NonMisc, Path.GetFileNameWithoutExtension(F.Name), F.Name)).ToArray()
            If (Not NonMisc) OrElse IsNumeric(File) Then
                Dim C As Integer = Cache.FindIndex(Function(F) File.Equals(F.DefIndex.ToString) OrElse File.Equals(F.Filename))

                If (C = -1) Then
                    Validations += 1

                    If NonMisc Then
                        Cache.Add(New DownloadManager.CacheItem With {.DefIndex = CInt(File), .Filename = File})
                    Else
                        Cache.Add(New DownloadManager.CacheItem With {.Filename = File})
                    End If
                End If
            End If
        Next
    End Sub

    Private Sub List_Check(Lists() As ObservableCollection(Of Item))
        Parallel.ForEach(Lists, Sub(List) Parallel.ForEach(List, Sub(Item) Singleton_Check(Item)))
    End Sub

    Private Sub Singleton_Check(Item As Item)
        With Item
            Dim Local_MissingCount_Field As New Integer
            Dim Local_Validations As New Integer
            Dim MarketID As String = RemoveQualityName(.MarketHashID)

            If (.DefIndex = 0) AndAlso (Not String.IsNullOrWhiteSpace(MarketID)) Then
                Dim Slave = SlaveBag.FirstOrDefault(Function(F) ((Not String.IsNullOrWhiteSpace(F.Name)) AndAlso F.Name.EndsWith(MarketID)))

                If (Slave IsNot Nothing) Then
                    .DefIndex = Slave.Defindex
                    Local_MissingCount_Field += 1
                End If
            End If
            If (.Quality.ID = 0) Then
                .Quality.ID = CInt(GetQualityName(Name:= .Quality.Name, GiveIDInstead:=True))
                Local_MissingCount_Field += 1
            End If

            If (.DefIndex <> 0) Then
                Dim Earliest As New Integer

                For Each Slave In SlaveBag
                    If ((Not String.IsNullOrWhiteSpace(Slave.Name)) AndAlso Slave.Name.Equals(MarketID) AndAlso (Slave.Defindex < .DefIndex)) Then
                        Earliest = Slave.Defindex
                    End If
                Next

                If ((Earliest > 0) AndAlso (.DefIndex <> Earliest)) Then
                    .DefIndex = Earliest
                    Local_Validations += 1
                End If

                Dim Price = Backpack.FirstOrDefault(Function(F) (F.Name = MarketID) AndAlso (F.DefIndex <> .DefIndex))

                If (Price IsNot Nothing) Then
                    .DefIndex = Price.DefIndex
                    Local_Validations += 1
                End If

                If Backpack.Where(Function(F) F.DefIndex.Equals(.DefIndex)).None Then
                    Dim DefIndex_New As Backpack.Price = Backpack.Where(Function(F) F.Name.Equals(MarketID)).FirstOrDefault

                    If (DefIndex_New IsNot Nothing) Then
                        .DefIndex = DefIndex_New.DefIndex
                        Local_Validations += 1
                    End If
                End If
            End If

            If ((Not Equals(RemoveQualityName(.MarketHashID, True), .Name)) AndAlso Equals(.Quality.Name, .MarketHashID)) Then
                .MarketHashID = .Name
                Local_Validations += 1
            End If

            '   If String.IsNullOrWhiteSpace(.Quality.Name) Then
            '       Dim QualityEmbedded = GetQualityName(, .MarketHashID, OnlyResult:=True)
            '
            '       If String.IsNullOrWhiteSpace(QualityEmbedded) Then
            '           For Each QualityAlias In QualityAliases
            '               If .MarketHashID.StartsWith(QualityAlias.Name, StringComparison.InvariantCultureIgnoreCase) Then
            '                   .Quality.ID = QualityAlias.ID
            '                   .Quality.Name = QualityAlias.Name
            '                   Exit For
            '               End If
            '           Next
            '       Else
            '           .Quality.ID = CInt(GetQualityName(Name:=QualityEmbedded, GiveIDInstead:=True))
            '           .Quality.Name = QualityEmbedded
            '       End If
            '
            '       Local_Validations += 1
            '   End If

            If ((Not String.IsNullOrWhiteSpace(.MarketHashID)) AndAlso (Not String.IsNullOrWhiteSpace(.Quality.Name))) Then
                If (Not .MarketHashID.StartsWith(.Quality.Name, StringComparison.InvariantCultureIgnoreCase)) OrElse (Not .Name.StartsWith(.Quality.Name, StringComparison.InvariantCultureIgnoreCase)) Then
                    If (New Integer() {2, 4, 6, 10, 12, 15}.Where(Function(F) F.Equals(.Quality.ID)).None) Then
                        .MarketHashID = .Quality.Name & " " & RemoveQualityName(.MarketHashID, True)
                        .Name = .MarketHashID
                        Local_Validations += 1
                    End If
                End If
            End If

            If (.ClassID Is Nothing) Then
                For Each i In Binding_Lv_Inventory
                    If (i.DefIndex.Equals(.DefIndex) AndAlso i.Quality.Name.Equals(.Quality.Name)) Then
                        .ClassID = i.ClassID
                        .Price.Steam = i.Price.Steam
                        Local_MissingCount_Field += 2
                    End If
                Next
            End If

            If ((.ClassID Is Nothing) AndAlso (AssetPrices IsNot Nothing)) Then
                Dim Asset = AssetPrices.result.assets.FirstOrDefault(Function(F) Equals(CInt(F.name), .DefIndex))

                If (Asset IsNot Nothing) AndAlso (Asset.classid IsNot Nothing) Then
                    .ClassID = Asset.classid
                    Dim SteamPrice As String = Nothing
                    Asset.prices.TryGetValue("EUR", SteamPrice)
                    .Price.Steam = CDec(CInt(SteamPrice) / 100)

                    Local_MissingCount_Field += 2
                End If
            End If

            For Each Slave In DownloadHandler.SlaveBag
                If (Not String.IsNullOrWhiteSpace(Slave.Name)) AndAlso Equals(.MarketHashID, Slave.Name) AndAlso (Not Equals(.DefIndex, Slave.Defindex)) Then
                    .DefIndex_Steam.Add(Slave.Defindex)
                    Local_MissingCount_Field += 1
                End If
            Next

            Interlocked.Add(MissingCount_Field, Local_MissingCount_Field)
            Interlocked.Add(Validations, Local_Validations)
        End With
    End Sub

    Sub Create_ClassIDLink()
        If (Binding_Lv_Backpack.Count > 0) AndAlso (MainWindow.Settings.Container.Steam_APIKey IsNot Nothing) Then
            Dim Link As String = String.Format("http://api.steampowered.com/ISteamEconomy/GetAssetPrices/v0001/?key={0}&appid={1}&currency={2}", MainWindow.Settings.Container.Steam_APIKey, MainWindow.Settings.Container.CurrentAppID, 3)
            Dim Downloaded_String As String = Nothing

            Using Client As New WebClient
                Try
                    Downloaded_String = Client.DownloadString(Link)
                Catch ex As WebException
                    Status("ClassIDLink: Getting Steam API Prices FAILED : " & ex.Message)
                    Exit Sub
                End Try
            End Using
        End If
    End Sub

End Class
