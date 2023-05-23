Imports U.PropertyManipulation

'Extends ObservableCollection to have Addrange like lists do
Public Class ObservableCollection(Of T)
    Inherits ObjectModel.ObservableCollection(Of T)

    Public Sub New()
    End Sub

    Public Sub New(Items As IEnumerable(Of T))
        MyBase.New(Items)
    End Sub

    Private _suppressNotification As Boolean = False

    Protected Overrides Sub OnCollectionChanged(e As Specialized.NotifyCollectionChangedEventArgs)
        If Not _suppressNotification Then
            MyBase.OnCollectionChanged(e)
        End If
    End Sub

    Public Sub AddRange(list As IEnumerable(Of T))
        If list Is Nothing Then
            Throw New ArgumentNullException("list")
        End If

        _suppressNotification = True

        For Each Item As T In list
            Add(Item)
        Next

        _suppressNotification = False
        OnCollectionChanged(New Specialized.NotifyCollectionChangedEventArgs(Specialized.NotifyCollectionChangedAction.Reset))
    End Sub

    Public Sub ReplaceAll(list As IEnumerable(Of T))
        If list Is Nothing Then
            Throw New ArgumentNullException("list")
        End If

        _suppressNotification = True

        Clear()
        For Each Item As T In list
            Add(Item)
        Next

        _suppressNotification = False
        OnCollectionChanged(New Specialized.NotifyCollectionChangedEventArgs(Specialized.NotifyCollectionChangedAction.Reset))
    End Sub

    Public Sub SetSilentlyAll(Name As String, Value As Object)
        _suppressNotification = True

        For Each Item As T In Me
            SetPropertyValueByName(Item, Name, Value)
        Next

        _suppressNotification = False
        OnCollectionChanged(New Specialized.NotifyCollectionChangedEventArgs(Specialized.NotifyCollectionChangedAction.Reset))
    End Sub

End Class
