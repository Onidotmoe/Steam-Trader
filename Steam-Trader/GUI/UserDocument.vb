Imports System.Xml.Serialization

Namespace GUI

    <Serializable>
    <XmlRoot("Data")>
    Public Class UserDocument

        <XmlArray>
        Property Favorites As List(Of Item)

        <XmlArray>
        Property Inventory As List(Of Item)

        <XmlArray>
        Property WishList As List(Of Item)

        <XmlArray>
        Property GiftList As List(Of Item)

        <XmlArray>
        Property Settings As List(Of Settings.Generator.SettingsItem)

        Public Function Save(Path As String) As Boolean
            Return U.XML.Write(Path, Me)
        End Function

    End Class

End Namespace
