Imports System.Xml.Serialization
Imports Newtonsoft.Json
Imports SteamTrader.DownloadManager
Imports SteamTrader.TeamFortress2
Imports U

Namespace Network.Steam.API.Schema

    Public Class Schema

        <XmlArray>
        Public Property Qualities As New List(Of IDName)
        <XmlArray>
        Public Property Particles As New List(Of IDName)
        <XmlArray>
        Public Property Counters As New List(Of Counter)
        <XmlArray>
        Public Property Paints As New List(Of Paint)
        <XmlArray>
        Public Property Items As New List(Of Item_Url)
        <XmlArray>
        Public Property Origins As New List(Of Origin)

        Function Save() As Boolean
            Return XML.Write(IO.Path.Combine(MainWindow.Home, MainWindow.Settings.Container.CurrentGameName, "Schema.xml"), Me)
        End Function

        Shared Function Load() As Schema
            Return XML.Read(Of Schema)(IO.Path.Combine(MainWindow.Home, MainWindow.Settings.Container.CurrentGameName, "Schema.xml"))
        End Function

    End Class

    Namespace Overview

        Public Class SchemaOverview

            <JsonProperty("result")>
            Public Property result As Result

        End Class

        Public Class Result

            <JsonProperty("qualities")>
            Public Property qualities As Dictionary(Of String, Integer)

            <JsonProperty("attribute_controlled_attached_particles")>
            Public Property ParticleEffects As List(Of IDName)

            <JsonProperty("kill_eater_score_types")>
            Public Property CounterData As List(Of Counter)

            <JsonProperty("originNames")>
            Public Property OriginNames As List(Of Origin)

        End Class

    End Namespace

    Namespace Items

        Public Class SchemaItems

            <JsonProperty("result")>
            Public Property result As Result

        End Class

        Public Class Result

            <JsonProperty("Items")>
            Public Property Items As List(Of Item_Url)
            <JsonProperty("next")>
            Public Property [Next] As Integer
        End Class

    End Namespace

    Public Class Origin

        <JsonProperty("origin")>
        Public Property Origin As String

        <JsonProperty("name")>
        Public Property Name As String

    End Class

    <XmlType(TypeName:="Counter")>
    Public Class Counter

        <JsonProperty("type")>
        <XmlAttribute("Type")>
        Public Property Type As Integer

        <JsonProperty("type_name")>
        <XmlAttribute("Name")>
        Public Property Name As String

        <JsonProperty("level_data")>
        <XmlAttribute("Data")>
        Public Property Data As String

    End Class

End Namespace
