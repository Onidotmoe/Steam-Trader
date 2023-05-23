Imports SteamTrader.GUI
Imports SteamTrader.MainWindow

Public Class ChangeUserWindow
    Public Property Users As New ObservableCollection(Of User)

    Private Async Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        DataContext = Me
        Users_DataGrid.Focus()
        Users.ReplaceAll(Await CollectUsers())
    End Sub

    Private Sub Btn_OK_Click(sender As Object, e As RoutedEventArgs) Handles Btn_OK.Click
        Dim Selected As Integer = Users.Where(Function(F) F.IsSelected).Count

        If (Selected = 1) Then
            DialogResult = True
            Close()
        ElseIf (Selected > 1) Then
            MsgBox("You can only SignIn with 1 user at a time.", MsgBoxStyle.OkOnly, "Not Possible")
        ElseIf (Selected = 0) Then
            MsgBox("Nothing was selected.", MsgBoxStyle.OkOnly, "Not Possible")
        End If
    End Sub

    Private Sub Btn_Cancel_Click(sender As Object, e As RoutedEventArgs) Handles Btn_Cancel.Click
        DialogResult = False
        Close()
    End Sub

    Private Async Function CollectUsers() As Task(Of List(Of User))
        Dim List As New List(Of User)

        Await Task.Run(Sub()
                           Dim UsersPath As String = IO.Path.Combine(Home, "Users")
                           If (IO.Directory.Exists(UsersPath)) Then
                               Dim UsersDirectories As String() = IO.Directory.GetDirectories(UsersPath)

                               For Each D In UsersDirectories
                                   Dim NecessaryFiles As New Integer
                                   For Each F In IO.Directory.GetFiles(D)
                                       If F.EndsWith(".intel") Then
                                           NecessaryFiles += 1
                                       ElseIf F.EndsWith(".spy") Then
                                           NecessaryFiles += 1
                                       ElseIf F.EndsWith(".xml") Then
                                           NecessaryFiles += 1
                                       End If
                                       If (NecessaryFiles = 3) Then
                                           Exit For
                                       End If
                                   Next
                                   If (NecessaryFiles = 3) Then
                                       Dim User As New User
                                       Dim Path As String = IO.Path.Combine(D, (D.Split("\"c).Last + ".xml"))
                                       User = U.XML.Read(Of User)(Path)
                                       User.ImageSource = New Uri(IO.Path.Combine(Home, "Users", User.SteamID.ToString, User.SteamID.ToString + ".jpg"))
                                       List.Add(User)
                                   End If
                               Next
                           End If
                       End Sub)
        Return List
    End Function

    'Clone from Resource Directory.vb
    Public Sub Hyperlink_RequestNavigate(sender As Object, e As RequestNavigateEventArgs)
        Dim Url As String = e.Uri.AbsoluteUri.ToString

        If Url.Contains("#") Then
            Url = Url.Replace("#", "%23")
        End If

        Process.Start(New ProcessStartInfo(Url))
        e.Handled = True
    End Sub

End Class
