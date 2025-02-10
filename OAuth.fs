namespace JiraCloudConnect

open FSharp.Data

module OAuth = 
    type OAuth =  
        {
            ClientId: string
            Scopes: string
            RedirectUri: string
            State: string
            Code: string option
            AccessToken: string option
            RefreshToken: string option
        }
        static member FormatUrl (oauth: OAuth) = 
            $"https://auth.atlassian.com/authorize?audience=api.atlassian.com&client_id={oauth.ClientId}&scope={oauth.Scopes}&redirect_uri={oauth.RedirectUri}&state={oauth.State}&response_type=code&prompt=consent"

    let getOAuth () = 
        let clientId = System.Configuration.ConfigurationManager.AppSettings["JiraApiClientId"]
        let scopes = System.Configuration.ConfigurationManager.AppSettings["JiraApiScopes"]
        let redirectUri = System.Configuration.ConfigurationManager.AppSettings["JiraApiRedirectUri"]
        let state = System.Configuration.ConfigurationManager.AppSettings["JiraApiState"]
        { ClientId = clientId; Scopes = scopes; RedirectUri = redirectUri; State = state; Code = None; AccessToken = None; RefreshToken = None }

    let exchangeCodeForAccessToken (oauth: OAuth) = ()
        //let r = RestRequest("https://auth.atlassian.com/oauth/token")
        //let client = new System.Net.Http.HttpClient()
        //let content = new System.Net.Http.FormUrlEncodedContent([("grant_type", "authorization_code"); ("client_id", oauth.ClientId); ("client_secret", System.Configuration.ConfigurationManager.AppSettings["JiraApiSecret"]); ("code", oauth.Code.Value); ("redirect_uri", oauth.RedirectUri)])
        //let response = client.PostAsync("https://auth.atlassian.com/oauth/token", content).Result
        //let responseString = response.Content.ReadAsStringAsync().Result
        //let responseJson = JsonValue.Parse(responseString)
        //{ oauth with AccessToken = Some responseJson?access_token.AsString; RefreshToken = Some responseJson?refresh_token.AsString }