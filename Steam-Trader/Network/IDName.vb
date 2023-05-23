Imports System.Xml.Serialization
Imports Newtonsoft.Json

Namespace Network.Steam.API

    <XmlType(TypeName:="IDName")>
    Public Class IDName

        <JsonProperty("id")>
        <XmlAttribute("ID")>
        Public Property ID As Integer

        <JsonProperty("name")>
        <XmlAttribute("Name")>
        Public Property Name As String

    End Class

End Namespace
