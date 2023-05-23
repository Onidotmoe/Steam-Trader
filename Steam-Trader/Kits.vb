Imports System.Security
Imports System.Xml
Imports Newtonsoft.Json
Imports SteamTrader.Network.Steam.API
Imports SteamTrader.TeamFortress2
Imports U

Public Module Kits

    Public Function RemoveExtraText(Value As String, Optional TurnDecimal As Boolean = True) As String
        Dim AllowedChars = "01234567890.,"

        If String.IsNullOrWhiteSpace(Value) Then
            Return Nothing
        End If

        Dim Count_A As Integer = Value.Count(Function(F) F = "."c)
        Dim Count_B As Integer = Value.Count(Function(F) F = ","c)

        If ((Count_A + Count_B) > 1) Then
            Dim Index_A As Integer = Value.IndexOf("."c)
            Dim Index_B As Integer = Value.IndexOf(","c)
            Dim Index_Main As Integer = If(Index_A < Index_B, Index_A, Index_B)

            Dim Value_End As String = Value.Remove(0, Index_Main + 1)
            Value_End = Value_End.Replace(","c, Nothing).Replace(".", Nothing)
            Value = Value.Remove(Index_Main + 1, Value_End.Length)
            Value += Value_End
        End If

        If (TurnDecimal = True) Then
            Value = Value.Replace(","c, "."c)
        End If

        Return New String(Value.Where(Function(c) AllowedChars.Contains(c)).ToArray())
    End Function

    Public Function GetPaint(Name As String) As Paint
        Dim Output As New Paint

        For Each Paint In MainWindow.Paints
            If Name.Equals(Paint.Name) Then
                Output = Paint
                Exit For
            End If
        Next

        Return Output
    End Function

    Public Function GetParticleEffectName(Optional ID As Integer = -1, Optional Name As String = Nothing, Optional GiveIDInstead As Boolean = False) As String
        Dim Output As New IDName

        If (Not String.IsNullOrWhiteSpace(Name)) Then
            For Each Particle In MainWindow.ParticleEffects
                If Name.Equals(Particle.Name) OrElse ((ID > -1) AndAlso ID.Equals(Particle.ID)) Then
                    Output = Particle
                    Exit For
                End If
            Next
        End If

        If (Not GiveIDInstead) Then
            Return Output.Name
        Else
            Return Output.ID.ToString
        End If
    End Function

    Public Function GetQualityDisplayName(Name As String) As String
        If (Not String.IsNullOrWhiteSpace(Name)) Then
            If Name.Equals("Rarity1", StringComparison.InvariantCultureIgnoreCase) Then
                Return "Genuine"
            End If
        End If

        Return Name
    End Function

    Public Function GetQualityName(Optional ID As Integer = -1, Optional Name As String = Nothing, Optional GiveIDInstead As Boolean = False, Optional RemoveThePrefix As Boolean = False, Optional OnlyResult As Boolean = False) As String
        If (Not MainWindow.Network.IsLoggedIn) Then
            Return Nothing
        End If

        Dim Output As String = Nothing

        If RemoveThePrefix Then
            Name = If(Name.StartsWith("the ", StringComparison.InvariantCultureIgnoreCase), Name.Remove(0, 4), Name)
        End If

        If (Not String.IsNullOrWhiteSpace(Name)) Then
            For Each Quality In MainWindow.Qualities
                If Name.StartsWith(Quality.Name, StringComparison.InvariantCultureIgnoreCase) Then
                    Name = Name.Remove(Quality.Name.Length, Name.Length - Quality.Name.Length)
                    Exit For
                End If
            Next
        End If

        If (ID > -1) Then
            Output = MainWindow.Qualities.Where(Function(F) F.ID.Equals(ID)).FirstOrDefault.Name

        ElseIf (Not String.IsNullOrWhiteSpace(Name)) Then
            Dim TryOutput As IDName = MainWindow.Qualities.Where(Function(F) F.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault

            If (TryOutput IsNot Nothing) Then
                If (Not GiveIDInstead) Then
                    Output = TryOutput.Name
                Else
                    Output = TryOutput.ID.ToString
                End If
            End If
        End If

        If String.IsNullOrWhiteSpace(Output) Then
            If GiveIDInstead Then
                Return "0"c
            Else
                If (Not OnlyResult) Then
                    Return Name
                Else
                    Return Nothing
                End If
            End If
        End If

        Return Output
    End Function

    Public Function RemoveQualityName(Name As String, Optional RemoveThePrefix As Boolean = False) As String
        Dim Output As String = Name

        If String.IsNullOrWhiteSpace(Name) Then
            Return String.Empty
        End If

        If (RemoveThePrefix = True) Then
            If Name.StartsWith("the ", StringComparison.InvariantCultureIgnoreCase) Then
                Return Name.Remove(0, 4)
            End If
        End If

        For Each Quality In MainWindow.Qualities
            If Name.StartsWith(Quality.Name, StringComparison.InvariantCultureIgnoreCase) Then
                FindString(Name, Quality.Name + " ", Output, "", True)
                Exit For
            End If
        Next

        If String.IsNullOrWhiteSpace(Output) Then
            Return Name
        End If

        Return Output
    End Function

    ''' <summary>
    ''' Converts Total Refined into its Item parts
    ''' </summary>
    ''' <param name="TotalRefined"></param>
    ''' <returns>Metal</returns>
    Public Function FromRefined(TotalRefined As Decimal, Optional ExcludeKeys As Boolean = False) As Metal
        Dim Key As New Integer
        Dim Key_Rest As New Decimal

        If (Not ExcludeKeys) Then
            Key = CInt(Math.Floor(TotalRefined / MainWindow.Settings.Container.Backpack_Price_Key_InMetal))
            Key_Rest = TotalRefined - (Key * MainWindow.Settings.Container.Backpack_Price_Key_InMetal)
        Else
            Key_Rest = TotalRefined
        End If

        Dim Refined As Integer = CInt(Math.Floor(Key_Rest))
        Dim Refined_Rest As Decimal = Key_Rest - Refined
        Dim Reclaimed As Integer = CInt(Math.Floor(Refined_Rest / CDec("0.33")))
        Dim Reclaimed_Rest As Decimal = Refined_Rest - (CDec("0.33") * Reclaimed)
        Dim Scrap As Integer = CInt(Math.Floor(Reclaimed_Rest / CDec("0.11")))
        Dim Scrap_Rest As Decimal = Reclaimed_Rest - (CDec("0.11") * Scrap)
        Dim Weapon As Integer = CInt(Math.Floor(Scrap_Rest / CDec("0.05")))

        Return New Metal(Key, Refined, Reclaimed, Scrap, Weapon)
    End Function

    Public Class BackpackConverter(Of T)
        Inherits JsonConverter

        Public Overrides Sub WriteJson(writer As JsonWriter, value As Object, serializer As JsonSerializer)
        End Sub

        Public Overrides Function CanConvert(objectType As Type) As Boolean
            Return False
        End Function

        Public Overrides Function ReadJson(Reader As JsonReader, ObjectType As Type, ExistingValue As Object, Serializer As JsonSerializer) As Object
            Dim Output As Object = New [Object]

            If (Reader.TokenType = JsonToken.StartObject) Then
                Output = Serializer.Deserialize(Reader, ObjectType)

            ElseIf (Reader.TokenType = JsonToken.StartArray) Then
                Dim List As List(Of T) = DirectCast(Serializer.Deserialize(Reader, GetType(List(Of T))), List(Of T))
                Dim Instance As T = List(0)

                Output = New SortedList(Of String, T) From {{"0", Instance}}
            End If

            Return Output
        End Function

    End Class

    <AttributeUsage(AttributeTargets.[Property], AllowMultiple:=False)>
    Public Class XmlCommentAttribute
        Inherits Attribute
        Private _Comment As String

        Public Property Comment() As String
            Get
                Return _Comment
            End Get
            Set
                _Comment = Comment
            End Set
        End Property

    End Class

    Private Function AddValidation(Name As String) As String
        Dim FirstCharSectionName As Char = CChar(Name.Substring(0, 1))

        If IsNumeric(FirstCharSectionName) Then
            Name = Name.Substring(0, 1).Replace(FirstCharSectionName, "Invalid_" + Name)
        End If

        Name = Name.Replace(" ", "___")
        Name = Name.Replace(":", ".COLON.")
        Name = SecurityElement.Escape(Name)
        Name = XmlConvert.EncodeName(Name)

        Return Name
    End Function

    Private Function RemoveValidation(Name As String) As String
        If Name.Length >= 8 Then
            If (Name.Substring(0, 8) = "Invalid_") = True Then
                Name = Name.Remove(0, 8)
            End If
        End If

        Name = Name.Replace("___", " ")
        Name = Name.Replace(".COLON.", ":")
        Name = SecurityElement.FromString(Name).Text()
        Name = XmlConvert.DecodeName(Name)

        Return Name
    End Function

    Private Function CheckNullConvertToString(Input As Object) As String
        Dim iString As String = ""

        If (Input IsNot Nothing) Then
            iString = Input.ToString

            If String.IsNullOrEmpty(iString) Then
                iString = ""
            End If
        End If

        Return iString
    End Function

    Public Function ConvertToSteamID64(ID32 As String) As ULong
        If (CULng(ID32) >= Integer.MaxValue) Then
            Return CULng(ID32)
        End If

        Return CULng(CLng(ID32) + 76561197960265728)
    End Function

    Public Function ConvertToSteamID32(ID64 As String) As Integer
        If (CULng(ID64) <= Integer.MaxValue) Then
            Return CInt(ID64)
        End If

        Return CInt((CLng(ID64) - 76561197960265728))
    End Function

End Module
