Public Class LoginWindowForm

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Txb_Username.Focus()
    End Sub

    Private Sub Btn_OK_Click(sender As Object, e As RoutedEventArgs) Handles Btn_OK.Click
        Network.Network.Username = Txb_Username.Text
        Network.Network.Password = Txb_Password.Password
        DialogResult = True
        Close()
    End Sub

    Private Sub Btn_Cancel_Click(sender As Object, e As RoutedEventArgs) Handles Btn_Cancel.Click
        DialogResult = False
        Close()
    End Sub

End Class
