Imports Newtonsoft.Json

Namespace Network.Inventory.Response

    Public Class Response

        <JsonProperty("success")>
        Public Property Success As Boolean

        <JsonProperty("Error")>
        Public Property Inventory_Error As String

        <JsonProperty("rgInventory")>
        Public Property ItemsInventory As Dictionary(Of String, Item)

        <JsonProperty("rgDescriptions")>
        Public Property ItemsDescriptions As Dictionary(Of String, Descriptions)

    End Class

    Public Class Item

        <JsonProperty("id")>
        Public Property Id As String

        <JsonProperty("classid")>
        Public Property classid As String

        <JsonProperty("instanceid")>
        Public Property instanceid As String

        <JsonProperty("pos")>
        Public Property pos As String

    End Class

    Public Class Descriptions

        <JsonProperty("classid")>
        Public Property classid As String

        <JsonProperty("instanceid")>
        Public Property instanceid As String

        <JsonProperty("icon_url")>
        Public Property icon_url As String

        <JsonProperty("icon_url_large")>
        Public Property icon_url_large As String

        <JsonProperty("name")>
        Public Property name As String

        <JsonProperty("market_hash_name")>
        Public Property market_hash_name As String

        <JsonProperty("market_name")>
        Public Property market_name As String

        <JsonProperty("type")>
        Public Property type As String

        <JsonProperty("tradable")>
        Public Property tradable As String

        <JsonProperty("marketable")>
        Public Property marketable As String

        <JsonProperty("Item")>
        Public Property Item As String

        <JsonProperty("market_tradable_restriction")>
        Public Property market_tradable_restriction As String

        <JsonProperty("market_marketable_restriction")>
        Public Property market_marketable_restriction As String

        <JsonProperty("cache_expiration")>
        Public Property cache_expiration As String

        <JsonProperty("descriptions")>
        Public Property descriptions As List(Of Description)

        <JsonProperty("actions")>
        Public Property Actions As List(Of Action)

        <JsonProperty("market_actions")>
        Public Property MarketActions As List(Of MarketAction)

        <JsonProperty("tags")>
        Public Property Tags As List(Of Tag)

        <JsonProperty("app_data")>
        Public Property AppData As AppData

    End Class

    Public Class Action

        <JsonProperty("name")>
        Public Property name As String

        <JsonProperty("link")>
        Public Property link As String

    End Class

    Public Class MarketAction

        <JsonProperty("name")>
        Public Property name As String

        <JsonProperty("link")>
        Public Property link As String

        <JsonProperty("steam")>
        Public Property steam As String

    End Class

    Public Class Tag

        <JsonProperty("internal_name")>
        Public Property internal_name As String

        <JsonProperty("name")>
        Public Property name As String

        <JsonProperty("category")>
        Public Property category As String

        <JsonProperty("category_name")>
        Public Property category_name As String

    End Class

    Public Class Description

        <JsonProperty("value")>
        Public Property value As String

        <JsonProperty("color")>
        Public Property color As String

        <JsonProperty("app_data")>
        Public Property AppData As AppData

    End Class

    Public Class AppData

        <JsonProperty("def_index")>
        Public Property DefIndex As Integer

        <JsonProperty("quality")>
        Public Property Quality As String

        <JsonProperty("quantity")>
        Public Property Quantity As String

        <JsonProperty("limited")>
        Public Property Limited As String

        <JsonProperty("is_Itemset_name")>
        Public Property is_Itemset_name As String

    End Class

End Namespace
