Public Class CaptchaWindow

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Txb_Captcha.Focus()
    End Sub

    Private Sub Btn_OK_Click(sender As Object, e As RoutedEventArgs) Handles Btn_OK.Click
        MainWindow.CaptchaCode = Txb_Captcha.Text
        DialogResult = True
        Close()
    End Sub

    Private Sub Btn_Cancel_Click(sender As Object, e As RoutedEventArgs) Handles Btn_Cancel.Click
        DialogResult = False
        Close()
    End Sub

End Class
