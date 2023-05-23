Imports System.Xml.Serialization
Imports SteamTrader.Network.Steam.API
Imports SteamTrader.TeamFortress2

Namespace GUI.Steam

    <Serializable>
    Public Class Quality
        Inherits NotifyPropertyChanged

        Private _ID As Integer

        <XmlAttribute>
        Public Property ID As Integer
            Get
                Return _ID
            End Get
            Set
                NotifyPropertyChanged(_ID, Value)
            End Set
        End Property

        Private _Name As String

        <XmlAttribute>
        Public Property Name As String
            Get
                Return _Name
            End Get
            Set
                If (Not Equals(_Name, Value)) Then
                    _ID = CInt(GetQualityName(Name:=Value, GiveIDInstead:=True))
                End If

                DisplayName = GetQualityDisplayName(Value)
                NotifyPropertyChanged(_Name, Value)
            End Set
        End Property

        <XmlIgnore>
        Private _DisplayName As String
        <XmlIgnore>
        Public Property DisplayName As String
            Get
                Return _DisplayName
            End Get
            Set
                NotifyPropertyChanged(_DisplayName, Value)
            End Set
        End Property

        Private _ParticleEffect As New IDName
        <XmlIgnore>
        Public Property ParticleEffect() As IDName
            Get
                Return _ParticleEffect
            End Get
            Set
                If (Not Equals(_ParticleEffect.Name, Value.Name)) Then
                    _ID = CInt(GetParticleEffectName(Name:=Value.Name, GiveIDInstead:=True))
                End If

                NotifyPropertyChanged(_ParticleEffect, Value)
            End Set
        End Property

        Private _Paint As New Paint

        <XmlIgnore>
        Public Property Paint As Paint
            Get
                Return _Paint
            End Get
            Set
                If (Not Paint.Color.Equals(Value.Color)) Then
                    Value = GetPaint(Value.Name)
                End If

                NotifyPropertyChanged(_Paint, Value)
            End Set
        End Property

    End Class

End Namespace
