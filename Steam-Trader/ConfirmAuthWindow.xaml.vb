Imports SteamTrader.GUI

Public Class ConfirmAuthWindow
    Implements IDisposable

    Public Event AffirmClosure()

    Public IsActualClosing As Boolean = False

    Public Shared Binding_Lv_Sent As New ObservableCollection(Of Item)
    Public Shared Binding_Lv_Incoming As New ObservableCollection(Of Item)
    Public Shared Binding_Lv_History As New ObservableCollection(Of Item)

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Lv_Sent.DataContext = Binding_Lv_Sent
        Lv_Sent.ItemsSource = Binding_Lv_Sent
        Lv_Incoming.DataContext = Binding_Lv_Incoming
        Lv_Incoming.ItemsSource = Binding_Lv_Incoming
        Lv_History.DataContext = Binding_Lv_History
        Lv_History.ItemsSource = Binding_Lv_History

    End Sub

    Private Sub Chx_AutoRefresh_Checked(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub Btn_Accept_Click(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub Btn_Deny_Click(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub Btn_Refresh_Click(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub Txb_RefreshInterval_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
        Dim Regex As New Text.RegularExpressions.Regex("[^0-9]+")
        e.Handled = Regex.IsMatch(e.Text)
    End Sub

    Private Sub Window_Closing(sender As Object, e As ComponentModel.CancelEventArgs)
        If (Not IsActualClosing) Then
            RaiseEvent AffirmClosure()
            Me.Hide()
            e.Cancel = True
        End If
    End Sub

    Private Sub Authenticator()
        '"https://steamcommunity.com/steamguard/phone_checksms?bForTwoFactor=1&bRevoke2fOnCancel="

    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        DirectCast(Me, IDisposable).Dispose()
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

End Class
