Imports System.ComponentModel
Imports System.Xml.Serialization

Namespace GUI

    <XmlRoot("User")>
    Public Class User
        Inherits NotifyPropertyChanged

        <XmlAttribute("SteamID")>
        Public Property SteamID As ULong

        <XmlAttribute("AccountName")>
        Public Property AccountName As String

        <XmlIgnore>
        Private _PersonaName As String

        <XmlAttribute("PersonaName")>
        Public Property PersonaName As String
            Get
                Return _PersonaName
            End Get
            Set
                NotifyPropertyChanged(_PersonaName, Value)
            End Set
        End Property

        <XmlIgnore>
        Public Property ImageUrl As Uri

        <XmlIgnore>
        Private _ImageSource As Uri

        <XmlIgnore>
        Public Property ImageSource As Uri
            Get
                Return _ImageSource
            End Get
            Set
                NotifyPropertyChanged(_ImageSource, Value)
            End Set
        End Property

        <XmlAttribute("WalletBalance")>
        Public Property WalletBalance As String

        <XmlIgnore>
        Public Property IsSelected As Boolean

        <XmlIgnore>
        Public Property Link As Uri

        <XmlAttribute("Link")>
        <Browsable(False), EditorBrowsable(EditorBrowsableState.Never)>
        Public Property Link_String() As String
            Get
                Return Link?.AbsoluteUri
            End Get
            Set
                Link = If(Value IsNot Nothing, New Uri(Value), Nothing)
            End Set
        End Property

    End Class

End Namespace
