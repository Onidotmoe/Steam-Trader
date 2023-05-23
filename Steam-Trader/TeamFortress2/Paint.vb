Imports System.Xml.Serialization

Namespace TeamFortress2

    <XmlType(TypeName:="Paint")>
    Public Class Paint

        <XmlAttribute("DefIndex")>
        Public Property DefIndex As Integer

        <XmlAttribute("Name")>
        Public Property Name As String

        <XmlArray("Color")>
        Public Property Color As List(Of String)

    End Class

End Namespace
