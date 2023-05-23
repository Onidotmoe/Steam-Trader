Namespace Archive
    Public Module Archive

        Public Class Steam
            Inherits NotifyPropertyChanged

            Private _IsSelected As Boolean

            Public Property IsSelected As Boolean
                Get
                    Return _IsSelected
                End Get
                Set
                    NotifyPropertyChanged(_IsSelected, Value)
                End Set
            End Property
            Public Property ID As String
            Public Property Price As Decimal
            Public Property Quantity As Decimal
            Public Property My As Boolean
        End Class

        Class Backpack
            Inherits NotifyPropertyChanged

            Private _IsSelected As Boolean

            Public Property IsSelected As Boolean
                Get
                    Return _IsSelected
                End Get
                Set
                    NotifyPropertyChanged(_IsSelected, Value)
                End Set
            End Property
            Public Property Buy As List(Of SteamTrader.Network.Backpack.API.Listing)
            Public Property Sell As List(Of SteamTrader.Network.Backpack.API.Listing)
            Public Property Refined As Decimal
            Public Property Average As Decimal
            Public Property Money As Decimal
        End Class

    End Module
End Namespace
