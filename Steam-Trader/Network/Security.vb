Imports System.IO
Imports System.Net
Imports System.Security.Cryptography
Imports System.Text

Namespace Network.Security

    Public Class Secret
        Public Property sessionid As String
        Public Property steamCountry As String
        Public Property steamLogin As String
        Public Property steamLoginSecure As String
        Public Property steamMachineAuth As Tuple(Of String, String) = New Tuple(Of String, String)("", "")
        Public Property steamRememberLogin As String

        ''' <summary>
        ''' Clears the current Secret and deletes the file. To be used for Logoff
        ''' </summary>
        Public Sub Clear()
            U.Clear(Me)

            If Check() Then
                Dim Path = IO.Path.Combine(MainWindow.Home, "Users", MainWindow.Settings.Container.CurrentUser.SteamID.ToString)

                File.Delete(IO.Path.Combine(Path, MainWindow.Settings.Container.CurrentUser.SteamID.ToString + ".intel"))
                File.Delete(IO.Path.Combine(Path, MainWindow.Settings.Container.CurrentUser.SteamID.ToString + ".spy"))
            End If
        End Sub

        ''' <summary>
        ''' Checks if Secret exists
        ''' </summary>
        Public Function Check() As Boolean
            Dim Path As String = IO.Path.Combine(MainWindow.Home, "Users", MainWindow.Settings.Container.CurrentUser.SteamID.ToString)
            Dim Path_Intel As String = IO.Path.Combine(Path, MainWindow.Settings.Container.CurrentUser.SteamID.ToString + ".intel")
            Dim Path_Spy As String = IO.Path.Combine(Path, MainWindow.Settings.Container.CurrentUser.SteamID.ToString + ".spy")

            If (IO.File.Exists(Path_Intel) AndAlso IO.File.Exists(Path_Spy)) Then
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' Refreshes this Secret with values in the current cookie
        ''' </summary>
        Public Sub Refresh()
            For Each Page In Network.Cookie.GetCookies(New Uri("https://steamcommunity.com/"))
                Dim Line As String = Page.ToString

                For Each Prop In Me.GetType.GetProperties
                    If FindProperty(Line, Prop.Name) Then
                        Exit For
                    End If
                Next
            Next
        End Sub

        Private Function FindProperty(Source As String, PropertyName As String) As Boolean
            Dim RemoveString As String = PropertyName + "="
            Dim SpecialString As String = NameOf(steamMachineAuth)

            If Source.Contains(RemoveString) Then
                Source = Source.Remove(0, RemoveString.Length)
                Me.GetType.GetProperty(PropertyName).SetValue(Me, Source)
                Return True

            ElseIf Source.Contains(SpecialString) Then
                Source = Source.Remove(0, SpecialString.Length)
                Dim Index As Integer = Source.IndexOf("="c)
                Dim Item1 As String = Source.Substring(0, Index)
                Dim Item2 As String = Source.Substring(Index + 1, Source.Length - (Index + 1))

                Dim Value As New Tuple(Of String, String)(Item1, Item2)

                Me.GetType.GetProperty(SpecialString).SetValue(Me, Value)
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' Updates the current cookie with values from this Secret
        ''' </summary>
        Public Sub Reload()
            Network.Cookie = New CookieContainer
            Network.InsertSteamCookie()

            For Each Prop In [GetType].GetProperties
                Dim Eater As New Cookie With {.Domain = "steamcommunity.com"}

                If (Prop.Name = NameOf(steamMachineAuth)) Then
                    Eater.Name = (Prop.Name + steamMachineAuth.Item1)
                    Eater.Value = steamMachineAuth.Item2
                Else
                    Eater.Name = Prop.Name
                    Eater.Value = [GetType].GetProperty(Prop.Name).GetValue(Me).ToString
                End If

                Eater.Expires = Date.Now.AddDays(30)
                Network.Cookie.Add(Eater)
            Next
        End Sub

        ''' <summary>
        ''' Loads and decrypts the Secret into this instance
        ''' </summary>
        Public Sub Load()
            Dim Path As String = IO.Path.Combine(MainWindow.Home, "Users", MainWindow.Settings.Container.CurrentUser.SteamID.ToString)
            Dim Path_Intel As String = IO.Path.Combine(Path, MainWindow.Settings.Container.CurrentUser.SteamID.ToString + ".intel")
            Dim Path_Spy As String = IO.Path.Combine(Path, MainWindow.Settings.Container.CurrentUser.SteamID.ToString + ".spy")

            Dim Intel() As Byte
            Dim Spy() As Byte

            If (IO.File.Exists(Path_Intel) AndAlso IO.File.Exists(Path_Spy)) Then
                Intel = Convert.FromBase64String(File.ReadAllText(Path_Intel))
                Spy = Convert.FromBase64String(File.ReadAllText(Path_Spy))
            Else
                Exit Sub
            End If

            Dim Documents As String = Nothing

            Using Printer As New AesManaged
                Printer.Padding = PaddingMode.PKCS7
                Printer.Mode = CipherMode.CBC
                Printer.KeySize = 256
                Printer.Key = Spy
                Printer.IV = Spy

                Using Memory As New MemoryStream(Intel)
                    Using Crypto As New CryptoStream(Memory, Printer.CreateDecryptor, CryptoStreamMode.Read)
                        Dim OutputBuffer(0 To CType(Memory.Length - 1, Integer)) As Byte
                        Dim ReadBytes As Integer = Crypto.Read(OutputBuffer, 0, CType(Memory.Length, Integer))
                        Documents = Encoding.Unicode.GetString(OutputBuffer, 0, ReadBytes)
                    End Using
                End Using
            End Using

            Dim Binder() As String = Documents.Split(","c)

            sessionid = Binder(0)
            steamCountry = Binder(1)
            steamLogin = Binder(2)
            steamLoginSecure = Binder(3)
            steamMachineAuth = New Tuple(Of String, String)(Binder(4), Binder(5))
            steamRememberLogin = Binder(6)
        End Sub

        ''' <summary>
        ''' Encrypts and saves this Secret
        ''' </summary>
        Public Sub Save()
            Dim ExpirationDate As Long = Date.Now.Ticks
            Dim LibraryCard As String = Environment.UserName
            Dim Earbuds As String = New Random().Next.ToString
            Dim Cover As String = ExpirationDate.ToString + Math.Abs(LibraryCard.GetHashCode).ToString + Earbuds

            Dim Spawner As Rfc2898DeriveBytes = New Rfc2898DeriveBytes(Cover, New Random().Next)
            Dim Documents As String = (String.Join(","c, {sessionid, steamCountry, steamLogin, steamLoginSecure, steamMachineAuth.Item1, steamMachineAuth.Item2, steamRememberLogin}))
            Dim IntelRAW As String = Nothing
            Dim Spy() As Byte

            Using Printer As New AesManaged
                Printer.Padding = PaddingMode.PKCS7
                Printer.Mode = CipherMode.CBC
                Printer.KeySize = 256
                Spy = Spawner.GetBytes(CType(Printer.BlockSize / 8, Integer))
                Printer.Key = Spy
                Printer.IV = Spy

                Using Memory As New MemoryStream()
                    Using Crypto As New CryptoStream(Memory, Printer.CreateEncryptor, CryptoStreamMode.Write)
                        Dim Intel() As Byte = Encoding.Unicode.GetBytes(Documents)
                        Crypto.Write(Intel, 0, Intel.Length)
                        Crypto.FlushFinalBlock()
                        IntelRAW = Convert.ToBase64String(Memory.ToArray())
                    End Using
                End Using
            End Using

            Dim FilePath As String = Path.Combine(MainWindow.Home, "Users", MainWindow.Settings.Container.CurrentUser.SteamID.ToString)
            IO.Directory.CreateDirectory(FilePath)

            File.WriteAllText(Path.Combine(FilePath, MainWindow.Settings.Container.CurrentUser.SteamID.ToString + ".spy"), Convert.ToBase64String(Spy))
            File.WriteAllText(Path.Combine(FilePath, MainWindow.Settings.Container.CurrentUser.SteamID.ToString + ".intel"), IntelRAW)
        End Sub

    End Class

End Namespace
