Namespace GUI

    Public Class AdvancedItem
        Inherits NotifyPropertyChanged
        Property Index As Integer
        Property IsExpanded As Boolean = True
        Property IsSelected As Boolean
        Property IsOurOffer As Boolean
        Property Refined As Decimal
        Property Partner As New Partner
        Property Price As New Price
        Property ID As String
        Property State As String
        Property OfferState As Integer
        Property StateID As Integer
        Private _Status As Integer

        ''' <summary>
        ''' -2 Cancel Failed, -1 Cancel Successful
        ''' </summary>
        Property Status As Integer
            Get
                Return _Status
            End Get
            Set
                NotifyPropertyChanged(_Status, Value)
            End Set
        End Property
        Property ImageMemory As BitmapSource
        Property Initiated As String
        Property Expiration As String
        Property Updated As String
        Property EscrowEnd As String
        Property Outgoing As New List(Of BasicItem)
        Property Incoming As New List(Of BasicItem)
    End Class

End Namespace
