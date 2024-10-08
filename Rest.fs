namespace JiraCloudConnect

open FSharp.Data

module Rest = 
    let private base64EncodeAuth = 
        let auth = $"{Config.username}:{Config.password}"
        let bytes = System.Text.Encoding.UTF8.GetBytes(auth)
        System.Convert.ToBase64String(bytes)
    let BasicHeader = ["Content-Type", "Application/json"]
    let private authHeader = ["Authorization", "Basic " + base64EncodeAuth] @ BasicHeader
    let GetHeaderWithAuth () = authHeader
