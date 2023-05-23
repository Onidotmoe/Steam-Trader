Imports Newtonsoft.Json

Namespace Network.Backpack.Response

    Public Class Response

        <JsonProperty("response")>
        Public Property Response As PriceCollection

    End Class

    Public Class PriceCollection

        <JsonProperty("success")>
        Public Property success As String

        <JsonProperty("current_time")>
        Public Property current_time As String

        <JsonProperty("raw_usd_value")>
        Public Property raw_usd_value As String

        <JsonProperty("usd_currency")>
        Public Property usd_currency As String

        <JsonProperty("usd_currency_index")>
        Public Property usd_currency_index As String

        <JsonProperty("Items")>
        Public Property Items As SortedList(Of String, Item)

    End Class

    Public Class Item

        <JsonProperty("defindex")>
        Public Property defindex As String()

        <JsonProperty("prices")>
        Public Property prices As SortedList(Of Integer, Tradable)

    End Class

    Public Class Tradable

        <JsonProperty("Tradable")>
        Public Property Tradable As Craftable

        <JsonProperty("Non-Tradable")>
        Public Property NonTradable As Craftable

    End Class

    Public Class Craftable

        <JsonConverter(GetType(BackpackConverter(Of Value)))>
        <JsonProperty("Craftable")>
        Public Property Craftable As SortedList(Of String, Value)

        <JsonConverter(GetType(BackpackConverter(Of Value)))>
        <JsonProperty("Non-Craftable")>
        Public Property NonCraftable As SortedList(Of String, Value)

    End Class

    Public Class Value

        <JsonProperty("value")>
        Public Property value As String

        <JsonProperty("currency")>
        Public Property currency As String

        <JsonProperty("difference")>
        Public Property difference As String

        <JsonProperty("last_update")>
        Public Property last_update As String

        <JsonProperty("value_high")>
        Public Property value_high As String

    End Class

End Namespace
