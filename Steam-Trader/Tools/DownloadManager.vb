Imports System.Collections.Concurrent
Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Net.Http
Imports System.Xml.Serialization
Imports Newtonsoft.Json
Imports SteamTrader.GUI
Imports SteamTrader.MainWindow
Imports U

Public Class DownloadManager
    Public Cache As New CacheDirectory
    Public Cache_Misc As New CacheDirectory
    Private Loaded As New ConcurrentDictionary(Of Uri, BitmapSource)
    Private Cache_Trader As New CacheDirectory_Trader
    Public SlaveBag As ConcurrentBag(Of Item_Url)
    Public CacheBag As ConcurrentBag(Of CacheItem) = Cache.Load(True)
    Public CacheBag_Misc As ConcurrentBag(Of CacheItem) = Cache_Misc.Load(True, True)
    Private CacheBag_Trader As ConcurrentBag(Of CacheItem_Trader) = Cache_Trader.Load(True)
    Private Waiting As New List(Of Download)
    Private Queue As New BlockingCollection(Of Download)
    Private Images As New BlockingCollection(Of Download)
    Private IsRunning As Boolean = False
    Private IsWriting As Boolean = False
    Private Retry As Boolean = False
    Private Retry_Writing As Boolean = False

    Public Sub New()
        Dim Schema = SteamTrader.Network.Steam.API.Schema.Schema.Load()

        If (Schema IsNot Nothing) Then
            SlaveBag = New ConcurrentBag(Of Item_Url)(Schema.Items)
        End If
    End Sub

    Private Sub Handle_Avatar(ByRef Partner As Partner)
        Dim Download As New Download
        Dim PartnerID As String = Partner.ID

        Download.SavePath = Path.Combine(Home, "Traders", Partner.ID + ".png")
        Download.Url = Partner.ImageUrl.ToString
        Partner.ImageSource = New Uri(Download.SavePath)

        Dim Item As CacheItem_Trader = CacheBag_Trader.FirstOrDefault(Function(F) F.SteamID = PartnerID)
        If Equals(Item, Nothing) Then
            CacheBag_Trader.Add(New CacheItem_Trader With {.SteamID = Partner.ID})

            SyncLock Waiting
                Waiting.Add(Download)
            End SyncLock
        End If
    End Sub

    Private Sub Handle(DefIndex As Integer, AppID As Integer, ByRef ImageSource As Uri, ByRef ImageMemory As BitmapSource, Optional ImageUrl As String = Nothing, Optional ClassID As String = Nothing, Optional Name As String = Nothing)
        If (AppID > 0) Then
            Dim Filename As String = Nothing
            Dim SaveDirectory As String = Nothing
            Dim DownloadUrl As String = Nothing

            If (AppID = MainWindow.Settings.Container.CurrentAppID) Then
                SaveDirectory = Cache.SaveDirectory
                Dim SlaveItem As Item_Url = Nothing

                If (DefIndex > 0) Then
                    Filename = DefIndex.ToString
                    If Equals(CacheBag.FirstOrDefault(Function(F) F.DefIndex.Equals(DefIndex)), Nothing) Then
                        SlaveItem = SlaveBag.FirstOrDefault(Function(F) (F.Defindex = DefIndex))
                    End If

                ElseIf (Not String.IsNullOrWhiteSpace(Name)) Then
                    Dim SearchName As String = RemoveQualityName(Name)
                    SlaveItem = SlaveBag.FirstOrDefault(Function(F) If(String.IsNullOrWhiteSpace(F.Name), False, F.Name.Contains(SearchName)))

                    If (Not Equals(SlaveItem, Nothing)) Then
                        Filename = SlaveItem.Defindex.ToString
                        Dim CacheItem As CacheItem = CacheBag.FirstOrDefault(Function(F) F.DefIndex.Equals(SlaveItem.Defindex))

                        If (Not Equals(CacheBag.FirstOrDefault(Function(F) F.DefIndex.Equals(DefIndex)), Nothing)) Then
                            SlaveItem = Nothing
                        End If
                    End If
                End If

                If (Not Equals(SlaveItem, Nothing)) Then
                    Filename = SlaveItem.Defindex.ToString
                    DownloadUrl = SlaveItem.UrlLarge
                    CacheBag.Add(New CacheItem With {.DefIndex = SlaveItem.Defindex, .Filename = Filename})
                End If
            Else
                SaveDirectory = Cache_Misc.SaveDirectory

                If (Not String.IsNullOrWhiteSpace(ClassID)) Then
                    Dim CacheItem As CacheItem = CacheBag_Misc.FirstOrDefault(Function(F) F.AppID.Equals(AppID) AndAlso F.ClassID.Equals(CInt(ClassID)))
                    Filename = AppID.ToString + "_" + ClassID

                    If (Equals(CacheItem, Nothing)) Then
                        CacheBag_Misc.Add(New CacheItem With {.ClassID = CInt(ClassID), .AppID = AppID, .Filename = Filename})
                        DownloadUrl = "http://cdn.steamcommunity.com/economy/image/" + ImageUrl
                    End If
                ElseIf (Not String.IsNullOrWhiteSpace(Name)) Then
                    Dim CacheItem As CacheItem = CacheBag_Misc.FirstOrDefault(Function(F) F.AppID.Equals(AppID) AndAlso If(String.IsNullOrWhiteSpace(F.Name), Nothing, (F.Name.Equals(Name)) OrElse F.Name.EndsWith(Name)))

                    Dim ValidName As String = String.Join("_", Name.Split(IO.Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd("."c)
                    Filename = AppID.ToString + "_" + ValidName

                    If (Equals(CacheItem, Nothing)) Then
                        CacheBag_Misc.Add(New CacheItem With {.Name = Name, .AppID = AppID, .Filename = Filename})
                        DownloadUrl = "http://cdn.steamcommunity.com/economy/image/" + ImageUrl
                    End If
                ElseIf (Not String.IsNullOrWhiteSpace(ImageUrl)) Then
                    Dim CacheItem As CacheItem = CacheBag_Misc.FirstOrDefault(Function(F) F.AppID.Equals(AppID) AndAlso If(String.IsNullOrWhiteSpace(F.ImageName), Nothing, F.ImageName.Equals(ImageUrl)))

                    If (Not Equals(CacheItem, Nothing)) Then
                        If (String.IsNullOrWhiteSpace(CacheItem.Filename)) Then
                            Dim ValidName As String = System.Guid.NewGuid.ToString
                            Filename = AppID.ToString + "_" + ValidName
                            CacheItem.Filename = Filename
                        Else
                            Filename = CacheItem.Filename
                        End If
                    Else
                        Dim ValidName As String = System.Guid.NewGuid.ToString
                        Filename = AppID.ToString + "_" + ValidName
                        CacheBag_Misc.Add(New CacheItem With {.ImageName = ImageUrl, .AppID = AppID, .Filename = Filename})
                        DownloadUrl = "http://cdn.steamcommunity.com/economy/image/" + ImageUrl
                    End If
                End If
            End If

            If (Not String.IsNullOrWhiteSpace(Filename)) Then
                ImageSource = New Uri(Path.Combine(SaveDirectory, Filename + ".png"))
                ImageMemory = Load(ImageSource)

                If (ImageMemory Is Nothing) Then
                    If (Not String.IsNullOrWhiteSpace(DownloadUrl)) Then
                        Dim Download As New Download With
                        {
                            .Url = DownloadUrl,
                            .SavePath = Path.Combine(SaveDirectory, Filename + ".png")
                        }

                        SyncLock Waiting
                            Waiting.Add(Download)
                        End SyncLock
                    End If
                End If
            End If
        End If
    End Sub

    Public Function Load(Uri As Uri) As BitmapSource
        If (Uri Is Nothing) Then
            Return Nothing
        End If

        Dim Source As BitmapSource = Nothing

        If Loaded.TryGetValue(Uri, Source) Then
            Return Source
        Else
            If File.Exists(Uri.LocalPath) Then
                Dim BitmapSource As BitmapSource = New BitmapImage(Uri)
                Loaded.TryAdd(Uri, BitmapSource)
                Return BitmapSource
            Else
                Return Nothing
            End If
        End If
    End Function

    Private Function Convert(Image As Bitmap) As BitmapSource
        Return Interop.Imaging.CreateBitmapSourceFromHBitmap(Image.GetHbitmap(Color.Transparent), IntPtr.Zero, New Int32Rect(0, 0, Image.Width, Image.Height), Nothing)
    End Function

    Public Sub Start(Optional Source As IEnumerable = Nothing, Optional ByRef Partner As Partner = Nothing)
        If ((Not Equals(Partner, Nothing)) AndAlso (Not Equals(Partner.ImageUrl, Nothing))) Then
            Handle_Avatar(Partner)
        End If
        If ((Source IsNot Nothing) AndAlso (Source(0) IsNot Nothing)) Then
            Select Case Source(0).GetType()
                Case GetType(Item)
                    Dim List As New ObservableCollection(Of Item)(DirectCast(Source, IEnumerable(Of Item)))
                    Parallel.ForEach(List, Sub(F) Handle(F.DefIndex, F.AppID, F.ImageSource, F.ImageMemory, F.ImageName, F.ClassID, If(F.MarketHashID, F.Name)))

                Case GetType(BasicItem)
                    Dim List As New ObservableCollection(Of BasicItem)(DirectCast(Source, ObservableCollection(Of BasicItem)))
                    Parallel.ForEach(List, Sub(F) Handle(F.DefIndex, F.AppID, F.ImageSource, F.ImageMemory, F.ImageDownloadUrl))

                Case GetType(AdvancedItem)
                    Dim List As New ObservableCollection(Of AdvancedItem)(DirectCast(Source, ObservableCollection(Of AdvancedItem)))
                    Parallel.ForEach(List, Sub(F1) Parallel.ForEach(F1.Incoming.Concat(F1.Outgoing), Sub(F2) Handle(F2.DefIndex, F2.AppID, F2.ImageSource, F2.ImageMemory, Name:=F2.MarketHashID)))

                Case GetType(Backpack.Price)
                    Dim List As New ObservableCollection(Of Backpack.Price)(DirectCast(Source, ObservableCollection(Of Backpack.Price)))
                    Parallel.ForEach(List, Sub(F) Handle(F.DefIndex, MainWindow.Settings.Container.CurrentAppID, F.ImageSource, F.ImageMemory, Name:=F.Name))

                Case GetType(Download)
                    Dim List As List(Of Download) = DirectCast(Source, List(Of Download))

                    SyncLock Waiting
                        Waiting.AddRange(List)
                    End SyncLock
            End Select
        End If

        Dim Clone As New List(Of Download)
        SyncLock Waiting
            Clone.AddRange(Waiting)
            Waiting.Clear()
        End SyncLock

        For Each Download In Clone.GroupBy(Function(F) F.SavePath).[Select](Function(Group) Group.First())
            Queue.TryAdd(Download)
        Next

        If (Not IsRunning) Then
            Downloading()
        Else
            Retry = True
        End If
    End Sub

    Private Async Sub Downloading()
        IsRunning = True

        While (Queue.Count > 0)
            Dim Download As New Download
            Dim Failed As New Boolean
            Queue.TryTake(Download)

            Using Client As New HttpClient
                Try
                    Using MemoryStream As New MemoryStream(Await Client.GetByteArrayAsync(Download.Url))
                        Download.Image = Image.FromStream(MemoryStream)
                    End Using
                Catch ex As HttpRequestException
                    Failed = True
                End Try
            End Using

            If (Not Failed) Then
                Images.TryAdd(Download)
            End If
        End While

        IsRunning = False
        Write()

        If Retry Then
            Retry = False
            Downloading()
        End If
    End Sub

    Private Sub Write()
        If (Not IsWriting) Then
            IsWriting = True

            While (Images.Count > 0)
                Dim Download As New Download
                Images.TryTake(Download)

                If (Not File.Exists(Download.SavePath)) Then
                    If (CInt(Download.Image.Width * 0.5) >= 256) AndAlso (CInt(Download.Image.Height * 0.5) >= 256) Then
                        Dim Image_Small As New Bitmap(CInt(Download.Image.Width * 0.5), CInt(Download.Image.Height * 0.5))
                        Dim Graphic As Graphics = Graphics.FromImage(Image_Small)
                        Graphic.DrawImage(Download.Image, 0, 0, Image_Small.Width + 1, Image_Small.Height + 1)
                        Image_Small.Save(Download.SavePath)
                    Else
                        Dim Image_Small As New Bitmap(Download.Image.Width, Download.Image.Height)
                        Dim Graphic As Graphics = Graphics.FromImage(Image_Small)
                        Graphic.DrawImage(Download.Image, 0, 0, Image_Small.Width, Image_Small.Height)
                        Image_Small.Save(Download.SavePath)
                    End If
                End If
            End While

            IsWriting = False

            If Retry_Writing Then
                Retry_Writing = False
                Write()
            Else
                Cache_Misc.Update(CacheBag_Misc)
                Cache_Misc.Integrity()
                Cache.Update(CacheBag)
                Cache.Integrity()
                SyncLock CacheBag_Misc
                    Cache_Misc.Save()
                End SyncLock
                SyncLock Cache
                    Cache.Save()
                End SyncLock
                SyncLock CacheBag_Trader
                    Cache_Trader.Save(CacheBag_Trader)
                End SyncLock
            End If
        Else
            Retry_Writing = True
        End If
    End Sub

#Region "Classes"

    Public Class Download
        Property Url As String
        Property SavePath As String
        Property Image As Image

        Sub New()
        End Sub

        Sub New(Url As String, SavePath As String)
            Me.Url = Url
            Me.SavePath = SavePath
        End Sub

        Sub New(Url As String)
            Me.Url = Url
        End Sub

    End Class

    <XmlType(TypeName:="Url")>
    Public Class Item_Url

        <JsonProperty("defindex")>
        <XmlAttribute("Defindex")>
        <DefaultValue(0)>
        Property Defindex As Integer

        <JsonProperty("Item_name")>
        <XmlAttribute("Name")>
        <DefaultValue("")>
        Property Name As String

        <JsonProperty("Item_type_name")>
        <XmlAttribute("Type")>
        <DefaultValue("")>
        Property Type As String

        <JsonProperty("image_url_large")>
        <XmlAttribute("UrlLarge")>
        <DefaultValue("")>
        Property UrlLarge As String

        <JsonProperty("name")>
        <XmlIgnore>
        Property DefName As String

        <JsonProperty("attributes")>
        <XmlIgnore>
        Property Attributes As New List(Of Json_Paint_Attributes)

    End Class

    Public Class Json_Paint_Attributes

        <JsonProperty("value")>
        Property Value As String

        <JsonProperty("class")>
        Property ClassName As String

    End Class

    'ImageCache
    <XmlType(TypeName:="Item")>
    Public Class CacheItem

        <XmlAttribute("DefIndex")>
        <DefaultValue(0)>
        Property DefIndex As Integer = Nothing

        <XmlAttribute("ClassID")>
        <DefaultValue(0)>
        Property ClassID As Integer = Nothing

        <XmlAttribute("AppID")>
        <DefaultValue(0)>
        Property AppID As Integer = Nothing

        <XmlAttribute("ImageName")>
        <DefaultValue("")>
        Property ImageName As String = Nothing

        <XmlAttribute("Name")>
        <DefaultValue("")>
        Property Name As String = Nothing

        <XmlAttribute("Filename")>
        <DefaultValue("")>
        Property Filename As String = Nothing

        Overrides Function Equals(obj As Object) As Boolean
            Dim c As CacheItem = TryCast(obj, CacheItem)
            Return c IsNot Nothing AndAlso c.DefIndex = DefIndex AndAlso c.ClassID = ClassID AndAlso c.AppID = AppID AndAlso c.ImageName = ImageName AndAlso c.Name = Name AndAlso c.Filename = Filename
        End Function

        Overrides Function GetHashCode() As Integer
            Return DefIndex.GetHashCode() Xor ClassID.GetHashCode() Xor AppID.GetHashCode() Xor (If(String.IsNullOrWhiteSpace(ImageName), 0, ImageName.GetHashCode())) Xor (If(String.IsNullOrWhiteSpace(Name), 0, Name.GetHashCode())) Xor (If(String.IsNullOrWhiteSpace(Filename), 0, Filename.GetHashCode()))
        End Function

    End Class

    <XmlRoot("Cache")>
    Public Class CacheDirectory

        Sub New()
            IO.Directory.CreateDirectory(SaveDirectory)
        End Sub

        <XmlIgnore>
        Public Cache_Path As String = IO.Path.Combine(Home, MainWindow.Settings.Container.CurrentGameName, "Cache", "ImageCache.xml")

        <XmlIgnore>
        Public SaveDirectory As String = IO.Path.Combine(Home, MainWindow.Settings.Container.CurrentGameName, "Cache")

        <XmlArray("Directory")>
        <XmlArrayItem(IsNullable:=False)>
        Public Property Directory As New List(Of CacheItem)

        Public Sub Save(Optional ReplacementDirectory As ConcurrentBag(Of CacheItem) = Nothing)
            If (ReplacementDirectory IsNot Nothing) Then
                Directory = New List(Of CacheItem)(ReplacementDirectory)
            End If

            XML.Write(Cache_Path, Me)
        End Sub

        Public Function Load(Optional AsConcurrentBag As Boolean = False, Optional IsMisc As Boolean = False) As ConcurrentBag(Of CacheItem)
            If IsMisc Then
                SaveDirectory = IO.Path.Combine(Home, "Misc", "Cache")
                Cache_Path = IO.Path.Combine(Home, "Misc", "Cache", "ImageCache.xml")

                IO.Directory.CreateDirectory(SaveDirectory)
            End If

            If (Not IO.File.Exists(Cache_Path)) Then
                XML.Write(Cache_Path, Me)
            End If

            Directory = XML.Read(Of CacheDirectory)(Cache_Path).Directory

            If (AsConcurrentBag = True) Then
                Return New ConcurrentBag(Of CacheItem)(Directory)
            End If

            Return Nothing
        End Function

        Public Sub Update(ReplacementDirectory As ConcurrentBag(Of CacheItem))
            Directory = New List(Of CacheItem)(ReplacementDirectory)
        End Sub

        Public Sub Integrity()
            If (Directory.Count > 0) Then
                Dim HashSet As New HashSet(Of CacheItem)(Directory)
                Directory = New List(Of CacheItem)(HashSet)
            End If
        End Sub

    End Class

    'ImageCache_Trader
    <XmlType(TypeName:="Item")>
    Public Class CacheItem_Trader

        <XmlAttribute("SteamID")>
        Public Property SteamID As String

    End Class

    <XmlRoot("Cache")>
    Public Class CacheDirectory_Trader

        Sub New()
            Directory = New List(Of CacheItem_Trader)

            IO.Directory.CreateDirectory(SaveDirectory)
        End Sub

        <XmlIgnore>
        Public Cache_Path As String = IO.Path.Combine(Home, "Traders", "ImageCache.xml")

        <XmlIgnore>
        Public SaveDirectory As String = IO.Path.Combine(Home, "Traders")

        <XmlArray("Directory")>
        <XmlArrayItem(IsNullable:=False)>
        Public Property Directory As List(Of CacheItem_Trader)

        Public Sub Save(Optional ReplacementDirectory As ConcurrentBag(Of CacheItem_Trader) = Nothing)
            If (ReplacementDirectory IsNot Nothing) Then
                Directory = New List(Of CacheItem_Trader)(ReplacementDirectory)
            End If

            XML.Write(Cache_Path, Me)
        End Sub

        Public Function Load(Optional AsConcurrentBag As Boolean = False) As ConcurrentBag(Of CacheItem_Trader)
            If (Not IO.File.Exists(Cache_Path)) Then
                XML.Write(Cache_Path, Me)
            End If

            Directory = XML.Read(Of CacheDirectory_Trader)(Cache_Path).Directory

            If (AsConcurrentBag = True) Then
                Return New ConcurrentBag(Of CacheItem_Trader)(Directory)
            End If

            Return Nothing
        End Function

    End Class

#End Region

End Class
