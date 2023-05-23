Public Class AuthCodeWindow

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Txb_AuthCode.Focus()
    End Sub

    Private Sub Btn_OK_Click(sender As Object, e As RoutedEventArgs) Handles Btn_OK.Click
        MainWindow.TwoFactorAuth = Txb_AuthCode.Text
        DialogResult = True
        Close()
    End Sub

    Private Sub Btn_Cancel_Click(sender As Object, e As RoutedEventArgs) Handles Btn_Cancel.Click
        DialogResult = False
        Close()
    End Sub

End Class
