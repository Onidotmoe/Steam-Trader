Imports System.Globalization

Public Class NullValueConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If value Is Nothing Then
            Return DependencyProperty.UnsetValue
        End If
        Return value
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Binding.DoNothing
    End Function

End Class

Public Class NullImageConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If (value Is Nothing) Then
            Return DependencyProperty.UnsetValue
        End If
        If (MainWindow.DownloadHandler IsNot Nothing) Then
            MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() value = MainWindow.DownloadHandler.Load(New Uri(value.ToString)))
        End If
        If (value Is Nothing) Then
            Return DependencyProperty.UnsetValue
        End If

        Return value
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Binding.DoNothing
    End Function

End Class

Public Class TimeUntilConverter
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If (value Is Nothing) OrElse String.IsNullOrWhiteSpace(CType(value, String)) Then
            Return DependencyProperty.UnsetValue
        End If

        Dim DateValue As Date = CType(value, Date)
        Dim DateToday As Date = Date.Today
        Dim Span As TimeSpan = -(DateValue - DateToday)

        Return Span.ToString("dd\:hh\:mm")
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Binding.DoNothing
    End Function

End Class

Public Class GroupSizeCheckConverter
    Implements IMultiValueConverter

    Public Function Convert(values() As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IMultiValueConverter.Convert
        If (MainWindow.MainWindow.IsLoaded) Then
            If (Not TryCast(values(0), DataGrid)?.Name.Equals(NameOf(MainWindow.MainWindow.Lv_Listings))) Then
                Return (CInt(values(1)) > MainWindow.Settings.Container.Grouping_Min_Inventory)
            Else
                Return (CInt(values(1)) > MainWindow.Settings.Container.Grouping_Min_Listings)
            End If
        Else
            Return False
        End If
    End Function

    Public Function ConvertBack(value As Object, targetTypes() As Type, parameter As Object, culture As CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
        Return Nothing
    End Function

End Class
