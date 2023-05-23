Namespace GUI

    Public Class Partner
        Inherits NotifyPropertyChanged

        Private _ProfileVisibility As Integer

        ''' <summary>
        ''' -1 download failed, 0 haven't tried, 1  the profile is not visible to you (Private, Friends Only, etc), 3 the profile is "Public"
        ''' </summary>
        Public Property ProfileVisibility As Integer
            Get
                Return _ProfileVisibility
            End Get
            Set
                NotifyPropertyChanged(_ProfileVisibility, Value)
            End Set
        End Property

        Public Property Name As String
        Public Property ID As String
        Private _EscrowState As Integer

        ''' <summary>
        '''  0 haven't tried, 1 no escrow, 2 has escrow
        ''' </summary>
        Public Property EscrowState As Integer
            Get
                Return _EscrowState
            End Get
            Set
                NotifyPropertyChanged(_EscrowState, Value)
            End Set
        End Property

        Private _Online As Boolean

        Public Property Online As Boolean
            Get
                Return _Online
            End Get
            Set
                NotifyPropertyChanged(_Online, Value)
            End Set
        End Property

        Private _OnlineLast As String

        Public Property OnlineLast As String
            Get
                Return _OnlineLast
            End Get
            Set
                NotifyPropertyChanged(_OnlineLast, Value)
            End Set
        End Property

        Private _Link As Uri

        Public Property Link As Uri
            Get
                Return _Link
            End Get
            Set
                NotifyPropertyChanged(_Link, Value)
            End Set
        End Property

        Private _Backpack_Profile As Uri

        Public Property Backpack_Profile As Uri
            Get
                Return _Backpack_Profile
            End Get
            Set
                NotifyPropertyChanged(_Backpack_Profile, Value)
            End Set
        End Property

        Private _ImageSource As Uri

        Public Property ImageSource As Uri
            Get
                Return _ImageSource
            End Get
            Set
                NotifyPropertyChanged(_ImageSource, Value)
            End Set
        End Property

        Public Property ImageUrl As Uri = Nothing
        Public Property ImageFragment As String
        Public Property TradeUrl As String
        Public Property TradeToken As String
        Public Property TradeID As String
        Private _TokenStatus As Integer

        ''' <summary>
        ''' -1 Failed, 0 Haven't tried, 1 Success
        ''' </summary>
        Public Property TokenStatus As Integer
            Get
                Return _TokenStatus
            End Get
            Set
                NotifyPropertyChanged(_TokenStatus, Value)
            End Set
        End Property

    End Class

End Namespace
