Imports System.ComponentModel
Imports System.Xml.Serialization

Namespace Network.Backpack.API

    <XmlType(TypeName:="Item")>
    Public Class Item

        <XmlAttribute("Type")>
        Public Property Type As String

        <XmlAttribute("DefIndex")>
        Public Property DefIndex As Integer

        <XmlAttribute("MarketHashID")>
        Public Property MarketHashID As String

        <XmlArray("Prices")>
        Public Property Prices As New List(Of Quality)

    End Class

    <XmlType(TypeName:="Quality")>
    Public Class Quality

        <XmlAttribute("ID")>
        Public Property ID As Integer

        <XmlAttribute("Name")>
        Public Property Name As String

        <XmlAttribute("Steam")>
        <DefaultValue(0)>
        Public Property Steam As Decimal

        <XmlElement("Tradable")>
        Public Property Tradable As New Craftable

        <XmlElement("NonTradable")>
        Public Property NonTradable As New Craftable

    End Class

    <XmlType(TypeName:="Craft")>
    Public Class Craftable

        <XmlArray("Craftable")>
        Public Property Craftable As New List(Of Entry)

        <XmlArray("NonCraftable")>
        Public Property NonCraftable As New List(Of Entry)

    End Class

    <XmlType(TypeName:="Value")>
    Public Class Entry

        <XmlElement("ParticleEffect")>
        Public Property ParticleEffect As Steam.API.IDName

        <XmlElement("Recipe")>
        Public Property Recipe As Recipe

        <XmlAttribute("Value")>
        Public Property Value As String

        <XmlAttribute("Currency")>
        Public Property Currency As String

        <XmlAttribute("Difference")>
        Public Property Difference As String

        <XmlAttribute("Last_update")>
        Public Property Last_update As String

        <XmlAttribute("Value_high")>
        Public Property Value_high As String

    End Class

    <XmlType(TypeName:="Recipe")>
    Public Class Recipe

        <XmlAttribute("Output_ID")>
        Public Property Output_ID As Integer

        <XmlAttribute("Output_Quality")>
        Public Property Output_Quality As Integer

    End Class

    <XmlRoot("Collection")>
    Public Class Collection

        <XmlAttribute("Success")>
        Public Property Success As String

        <XmlAttribute("Current_Time")>
        Public Property Current_time As String

        <XmlAttribute("Raw_USD_Value")>
        Public Property Raw_USD_Value As String

        <XmlAttribute("Usd_Currency")>
        Public Property Usd_Currency As String

        <XmlAttribute("Usd_Currency_Index")>
        Public Property Usd_Currency_Index As String

        <XmlAttribute("KeyPriceInMetal")>
        Public Property KeyPriceInMetal As Decimal

        <XmlArray("Items")>
        Public Property Items As New List(Of Item)

    End Class

End Namespace
