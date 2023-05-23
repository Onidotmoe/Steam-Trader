Namespace TeamFortress2

    Public Class Metal
        Public Property Key As Integer
        Public Property Refined As Integer
        Public Property Reclaimed As Integer
        Public Property Scrap As Integer
        Public Property Weapon As Integer

        Sub New(Optional Key As Integer = 0, Optional Refined As Integer = 0, Optional Reclaimed As Integer = 0, Optional Scrap As Integer = 0, Optional Weapon As Integer = 0)
            Me.Key = Key
            Me.Refined = Refined
            Me.Reclaimed = Reclaimed
            Me.Scrap = Scrap
            Me.Weapon = Weapon
        End Sub

        Function Total(Optional ExcludeKeys As Boolean = False) As Decimal
            If (Not ExcludeKeys) Then
                Return MainWindow.MainWindow.ToRefined(Key, Refined, Reclaimed, Scrap, Weapon)
            Else
                Return MainWindow.MainWindow.ToRefined(Nothing, Refined, Reclaimed, Scrap, Weapon)
            End If
        End Function

        Function IsEmpty(Optional ExcludeKeys As Boolean = False) As Boolean
            If (Not ExcludeKeys) Then
                Return ((Key + Refined + Reclaimed + Scrap + Weapon) = 0)
            Else
                Return ((Refined + Reclaimed + Scrap + Weapon) = 0)
            End If
        End Function

        Function InCash() As Decimal
            Return Total() * MainWindow.Settings.Container.Exchange_ToMoney_Refined
        End Function

        Public Function Abs() As Metal
            Return New Metal(Math.Abs(Key), Math.Abs(Refined), Math.Abs(Reclaimed), Math.Abs(Scrap), Math.Abs(Weapon))
        End Function

        Function MinZero() As Metal
            Key = If(Key < 0, 0, Key)
            Refined = If(Refined < 0, 0, Refined)
            Reclaimed = If(Reclaimed < 0, 0, Reclaimed)
            Scrap = If(Scrap < 0, 0, Scrap)
            Weapon = If(Weapon < 0, 0, Weapon)
            Return New Metal(Key, Refined, Reclaimed, Scrap, Weapon)
        End Function

        Function HasNegative() As Boolean
            For Each Prop In Me.GetType.GetProperties
                If (DirectCast(Me.GetType.GetProperty(Prop.Name).GetValue(Me), Integer) < 0) Then
                    Return True
                End If
            Next
            Return False
        End Function

        Function Realize() As Metal
            Return MainWindow.Settings.Container.TotalMetal.Equalize(FromRefined(MainWindow.MainWindow.ToRefined(Key, Refined, Reclaimed, Scrap, Weapon)))
        End Function

        Function Equalize(Cost As Metal) As Metal
            If (Key < Cost.Key) Then
                Dim RequiredKey As Integer = (Cost.Key - Key)
                Cost.Key -= RequiredKey
                Cost += FromRefined(RequiredKey * MainWindow.Settings.Container.Backpack_Price_Key_InMetal, True).MinZero
            End If
            If (Refined < Cost.Refined) Then
                Dim RequiredMetal As Integer = (Cost.Refined - Refined)
                Cost.Refined -= RequiredMetal
                Cost.Reclaimed += (RequiredMetal * 3)
            End If
            If (Reclaimed < Cost.Reclaimed) Then
                Dim RequiredMetal As Integer = (Cost.Reclaimed - Reclaimed)
                Cost.Reclaimed -= RequiredMetal
                Cost.Scrap += (RequiredMetal * 3)
            End If
            If (Scrap < Cost.Scrap) Then
                Dim RequiredMetal As Integer = (Cost.Scrap - Scrap)
                Cost.Scrap -= RequiredMetal
                Cost.Weapon += (RequiredMetal * 2)
            End If
            If (Cost.Weapon > 0) AndAlso (Weapon < Cost.Weapon) Then
                Cost.Weapon = -1
            End If

            Return Cost
        End Function

        Overrides Function ToString() As String
            Return String.Format("Key: {0}, Refined: {1}, Reclaimed: {2}, Scrap: {3}, Weapon: {4}", Key, Refined, Reclaimed, Scrap, Weapon)
        End Function

        Public Shared Operator -(M1 As Metal, M2 As Metal) As Metal
            Return New Metal(M1.Key - M2.Key, M1.Refined - M2.Refined, M1.Reclaimed - M2.Reclaimed, M1.Scrap - M2.Scrap, M1.Weapon - M2.Weapon)
        End Operator

        Public Shared Operator +(M1 As Metal, M2 As Metal) As Metal
            Return New Metal(M1.Key + M2.Key, M1.Refined + M2.Refined, M1.Reclaimed + M2.Reclaimed, M1.Scrap + M2.Scrap, M1.Weapon + M2.Weapon)
        End Operator

        Public Shared Operator *(M1 As Metal, i As Integer) As Metal
            Return New Metal(M1.Key * i, M1.Refined * i, M1.Reclaimed * i, M1.Scrap * i, M1.Weapon * i)
        End Operator

    End Class

End Namespace
