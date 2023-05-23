Imports Newtonsoft.Json

Namespace Network.Steam

    Public Module Login

        Public Class RSA_Response

            <JsonProperty("success")>
            Public Property Success As Boolean

            <JsonProperty("publickey_mod")>
            Public Property Key_Modulus As String

            <JsonProperty("publickey_exp")>
            Public Property Key_Exponent As String

            <JsonProperty("timestamp")>
            Public Property TimeStamp As String

        End Class

        Public Class Login_Response

            <JsonProperty("success")>
            Public Property Success As Boolean

            <JsonProperty("emailauth_needed")>
            Public Property Email_Needed As Boolean

            <JsonProperty("captcha_needed")>
            Public Property Captcha_Needed As Boolean

            <JsonProperty("message")>
            Public Property Message As String

            <JsonProperty("captcha_gid")>
            Public Property Captcha_ID As String

            <JsonProperty("emailsteamid")>
            Public Property Email_ID As String

            <JsonProperty("bad_captcha")>
            Public Property Captcha_Bad As Boolean

            <JsonProperty("requires_twofactor")>
            Public Property RequiresTwoFactor As Boolean

            <JsonProperty("login_complete")>
            Public Property LoginComplete As Boolean

            <JsonProperty("transfer_urls")>
            Public Property Transfer_Urls As String()

            <JsonProperty("transfer_parameters")>
            Public Property Transfer_Parameters As Login_Response_Parameters

        End Class

        Public Class Login_Response_Parameters

            <JsonProperty("token")>
            Public Property Token As String

            <JsonProperty("auth")>
            Public Property Auth As String

            <JsonProperty("webcookie")>
            Public Property WebCookie As String

            <JsonProperty("token_secure")>
            Public Property Token_Secure As String

            <JsonProperty("steamid")>
            Public Property SteamID As ULong

        End Class

    End Module
End Namespace
