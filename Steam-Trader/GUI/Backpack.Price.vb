Namespace GUI.Backpack

    Public Class Price
        Property Index As Integer
        Property IsSelected As Boolean
        Property Name As String
        Property Type As String
        Property Counter As Integer
        Property Counter_Type As String
        Property WikiURL As Uri
        Property DefIndex As Integer
        Property ImageSource As Uri
        Property ImageMemory As BitmapSource
        Property Tradable As Boolean = True
        Property Craftable As Boolean = True
        Property Achievement As Boolean
        Property MarkedForSell As Boolean
        Property UsableInCrafting As Boolean = True

        ''' <summary>
        ''' ID 6
        ''' </summary>
        Property Unique As Quality

        ''' <summary>
        ''' ID 3
        ''' </summary>
        Property Vintage As Quality

        ''' <summary>
        ''' ID 1
        ''' </summary>
        Property Genuine As Quality

        ''' <summary>
        ''' ID 11
        ''' </summary>
        Property Strange As Quality

        ''' <summary>
        ''' ID 13
        ''' </summary>
        Property Haunted As Quality

        ''' <summary>
        ''' ID 14
        ''' </summary>
        Property Collectors As Quality

        Property BestProfit As Quality

        Function Clone() As Price
            Return DirectCast(MemberwiseClone(), Price)
        End Function

    End Class

End Namespace
