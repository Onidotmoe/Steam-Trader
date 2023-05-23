Imports System.Xml.Serialization
Imports Newtonsoft.Json

Namespace Network.Steam

    Public Module Steam

        Public Class SteamPriceOverview

            <JsonProperty("success")>
            Public Property Success As Boolean

            <JsonProperty("lowest_price")>
            Public Property Lowest_price As String

            <JsonProperty("volume")>
            Public Property Volume As String

            <JsonProperty("median_price")>
            Public Property Median_price As String

        End Class

        'Steam GetTradeOffers Response
        <XmlRoot("response")>
        Public Class ActiveTradeOffers

            <XmlArray("trade_offers_sent"), XmlArrayItem("message")>
            Public Property Offers As New List(Of TradeMessage)

        End Class

        <XmlType(TypeName:="TradeOffer")>
        <XmlRoot("message")>
        Public Class TradeMessage

            <XmlElement("tradeofferid")>
            Public Property ID As String

            <XmlElement("accountid_other")>
            Public Property PartnerID As String

            <XmlElement("expiration_time")>
            Public Property ExpirationTime As String

            <XmlElement("trade_offer_state")>
            Public Property State As Integer

            <XmlArray("Items_to_give"), XmlArrayItem("message")>
            Public Property Outgoing As New List(Of TradeItem)

            <XmlArray("Items_to_receive"), XmlArrayItem("message")>
            Public Property Incoming As New List(Of TradeItem)

            <XmlElement("is_our_offer")>
            Public Property IsOurOffer As Boolean

            <XmlElement("time_created")>
            Public Property TimeCreated As String

            <XmlElement("time_updated")>
            Public Property TimeUpdated As String

            <XmlElement("from_real_time_trade")>
            Public Property FromRealTimeTrade As Boolean

            <XmlElement("escrow_end_date")>
            Public Property EscrowEndDate As String

            <XmlElement("confirmation_method")>
            Public Property ConfirmationMethod As Integer

        End Class

        <JsonObject("tradeoffer")>
        Public Class TradeOffer
            Public Property newversion As Boolean = True
            Public Property version As Integer = 2

            <JsonProperty("me")>
            Public Property Outgoing As New TradeParameters

            <JsonProperty("them")>
            Public Property Incoming As New TradeParameters

        End Class

        Public Class TradeParameters

            <JsonProperty("assets")>
            Public Property Items As New List(Of TradeItem)

            Public Property currency As New List(Of String)
            Public Property ready As Boolean = False
        End Class

        <XmlType(TypeName:="Item")>
        <XmlRoot("message")>
        <JsonObject("Item")>
        Public Class TradeItem

            <XmlElement("appid")>
            <JsonProperty("appid")>
            Public Property AppID As Integer

            <XmlElement("contextid")>
            <JsonProperty("contextid")>
            Public Property ContextID As Integer

            <XmlElement("amount")>
            <JsonProperty("amount")>
            Public Property Amount As Integer

            <XmlElement("assetid")>
            <JsonProperty("assetid")>
            Public Property AssetID As String

            <JsonIgnore>
            <XmlElement("classid")>
            Public Property ClassID As String

            <JsonIgnore>
            <XmlElement("instanceid")>
            Public Property InstanceID As String

            <JsonIgnore>
            <XmlElement("missing")>
            Public Property IsMissing As Boolean

        End Class

    End Module
End Namespace
