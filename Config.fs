namespace JiraCloudConnect

open Atlassian.Jira
open System.ComponentModel
open ConsoleUtils.ConsoleUtils

[<AutoOpen>]
module Config =
    let c = System.Configuration.ConfigurationManager.AppSettings
    let baseUrl = c.["JiraURL"]
    let username = c.["JiraUser"]
    let password = c.["JiraApiToken"]

    let apiUrl = $"{baseUrl}/rest/api/3"
    
    let private getDropdownUrl customFieldId fieldConfigSchemeId = $"{apiUrl}/field/customfield_{customFieldId}/context/{fieldConfigSchemeId}/option"

    let GetClient () =
        let client = Jira.CreateRestClient(baseUrl, username, password)
        let projects = 
            client.Projects.GetProjectsAsync() 
            |> Async.AwaitTask 
            |> Async.RunSynchronously 
            |> Seq.toList
        out $"Connected. {projects.Length} projects found."
        client

    [<DisplayName("Phone Number")>]
    type PhoneNumber = PhoneNumber of string with
        //member this.PropName = "Phone Number"
        member this.AsString() =  this |> function PhoneNumber x -> x

    [<DisplayName("Email Address")>]
    type EmailAddress = EmailAddress of string with
        member this.PropName = "Email Address"
        member this.AsString() = this |> function EmailAddress x -> x

    type DirtLevel =
        | Dirty of string
        | NotDirty

    type CustomDropdownFieldOptionsForSending =
        {
            disabled: bool // fsharplint:disable-line RecordFieldNames
            value: string // fsharplint:disable-line RecordFieldNames
        }

    type CustomDropdownFieldForSending =
        {
            options: CustomDropdownFieldOptionsForSending list // fsharplint:disable-line RecordFieldNames
        }

    type CustomDropdownValueForReading =
        {
            Id: string
            Value: string
            Disabled: bool
        }

    type CustomDropdownForReading =
        {
            MaxResults: int
            StartAt: int
            Total: int
            IsLast: bool
            Values: CustomDropdownValueForReading list
        }