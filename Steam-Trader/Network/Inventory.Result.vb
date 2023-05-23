Imports System.Xml.Serialization
Imports Newtonsoft.Json

Namespace Network.Inventory.Result

    <XmlRoot("result")>
    Public Class Result

        <JsonProperty("status")>
        <XmlElement("status")>
        Public Property Status As Integer

        <JsonProperty("num_backpack_slots")>
        <XmlElement("num_backpack_slots")>
        Public Property Backpack_Slots As Integer

        <JsonProperty("Items")>
        <XmlArray("Items"), XmlArrayItem("Item")>
        Public Property Items As New List(Of Item)

    End Class

    <XmlType(TypeName:="Item")>
    <XmlRoot("Item")>
    Public Class Item

        <JsonProperty("id")>
        <XmlElement("id")>
        Public Property ID As String

        <JsonProperty("original_id")>
        <XmlElement("original_id")>
        Public Property ID_Original As String

        <JsonProperty("defindex")>
        <XmlElement("defindex")>
        Public Property DefIndex As Integer

        <JsonProperty("level")>
        <XmlElement("level")>
        Public Property Level As String

        <JsonProperty("quality")>
        <XmlElement("quality")>
        Public Property Quality As Integer

        <JsonProperty("inventory")>
        <XmlElement("inventory")>
        Public Property Inventory As String

        <JsonProperty("quantity")>
        <XmlElement("quantity")>
        Public Property Quantity As Integer

        <JsonProperty("origin")>
        <XmlElement("origin")>
        Public Property Origin As String

        <JsonProperty("flag_cannot_trade")>
        <XmlElement("flag_cannot_trade")>
        Public Property NonTradable As Boolean

        <JsonProperty("flag_cannot_craft")>
        <XmlElement("flag_cannot_craft")>
        Public Property NonCraftable As Boolean

        <JsonProperty("equipped")>
        <XmlArray("equipped"), XmlArrayItem("equipped")>
        Public Property Equipped As New List(Of Equipped)

        <JsonProperty("style")>
        <XmlElement("style")>
        Public Property Style As Integer

        <JsonProperty("custom_name")>
        <XmlElement("custom_name")>
        Public Property Custom_Name As String

        <JsonProperty("custom_desc")>
        <XmlElement("custom_desc")>
        Public Property Custom_Description As String

        <JsonProperty("attributes")>
        <XmlArray("attributes"), XmlArrayItem("attribute")>
        Public Property Attributes As New List(Of Attribute)

    End Class

    <XmlType(TypeName:="Attribute")>
    <XmlRoot("attribute")>
    Public Class Attribute

        <JsonProperty("defindex")>
        <XmlElement("defindex")>
        Public Property DefIndex As String

        <JsonProperty("value")>
        <XmlElement("value")>
        Public Property Value As String

        <JsonProperty("float_value")>
        <XmlElement("float_value")>
        Public Property Value_Float As String

    End Class

    <XmlType(TypeName:="Equipped")>
    Public Class Equipped

        <JsonProperty("class")>
        <XmlElement("class")>
        Public Property OnClass As Integer

        <JsonProperty("slot")>
        <XmlElement("slot")>
        Public Property Slot As Integer

    End Class

End Namespace
