Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Module ExtensionProperty

    <Extension>
    Public Async Sub Status(Status As String)
        Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub()
                                                               MainWindow.MainWindow.Txbk_Sb_Ready.Text = Status
                                                               MainWindow.MainWindow.Lv_Status.Items.Add(Date.Now.ToLongTimeString + " : " + Status)
                                                           End Sub, Windows.Threading.DispatcherPriority.ApplicationIdle)
    End Sub

    <Extension>
    Public Async Sub UpdateSaveStamp(TimeStamp As String)
        Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() MainWindow.MainWindow.Txbk_Sb_SaveTimeStamp.Text = TimeStamp, Windows.Threading.DispatcherPriority.ApplicationIdle)
    End Sub

    <Extension>
    Public Async Sub Progress(Value As Integer)
        Await MainWindow.MainWindow.Dispatcher.BeginInvoke(Sub() MainWindow.MainWindow.Sb_ProgressBar.Value = Value)
    End Sub

    'Public Class Property
    '    Public Shared Operator !()

    '        Return Nothing
    '    End Operator
    'End Class

    'make a double exit entity: exit for for for, 1 "for" per exit to go out of

    'Simplify notifypropertychanged to this instead:
    'Implements INotifyPropertyChanged(NotifyPropertyChanged)

    'Not it will use the sub "NotifyPropertyChanged" for all "Set" events and will do like autoproperties and supply the "_PropertyName" on its own
    'You can supply "Get" or "Set" individually, further reducing the lines on screen
    'Public Property PropertyName As Boolean
    '    Get
    '        Return _PropertyName
    '    End Get
    '    Set
    '        NotifyPropertyChanged(_PropertyName, Value)
    '    End Set
    'End Property

    'ignore below for now
    '<Extension()>
    'Public Sub Notifier(Of T)( Prop As T)
    '    Debug.WriteLine(Prop)
    'End Sub

    <Extension>
    Public Iterator Function DistinctBy(Of TSource, TKey)(source As IEnumerable(Of TSource), keySelector As Func(Of TSource, TKey)) As IEnumerable(Of TSource)
        Dim seenKeys As New HashSet(Of TKey)()
        For Each element As TSource In source
            If seenKeys.Add(keySelector(element)) Then
                Yield element
            End If
        Next
    End Function

    <Extension>
    Public Function SelectAll(Of T)(source As IEnumerable(Of T)) As IEnumerable(Of T)
        If (source IsNot Nothing) Then
            Return source.Select(Function(F) F)
        Else
            Return Nothing
        End If
    End Function

End Module

Public Class NotifyPropertyChanged
    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Overridable Sub NotifyPropertyChanged(Of T)(ByRef field As T, value As T, <CallerMemberName> Optional Name As String = "")
        If Not EqualityComparer(Of T).[Default].Equals(field, value) Then
            field = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(Name))
        End If
    End Sub

    Public Sub Update()
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(String.Empty))
    End Sub

End Class
