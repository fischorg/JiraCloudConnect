namespace JCC

open Atlassian.Jira
open System.ComponentModel
open ConsoleUtils.ConsoleUtils
open System.Diagnostics

[<AutoOpen>]
module Config =
    let c = System.Configuration.ConfigurationManager.AppSettings
    let baseUrl = c.["JiraURL"]
    let username = c.["JiraUser"]
    let password = c.["JiraApiToken"]

    type PossibleErrors = 
        | FailedToRunTask of string
        | FailedToGetIssue of PossibleErrors
        | FailedToGetIssues of PossibleErrors
        | FailedToGetProjects of PossibleErrors
        | FailedToAddComment of PossibleErrors
        | FailedToCreateIssue of PossibleErrors
        static member Stringify =
            function
            | FailedToRunTask s -> $"Failed to run task: %s{s}"
            | FailedToAddComment e -> $"Failed to add comment: %s{e |> PossibleErrors.Stringify}"
            | FailedToGetIssue e -> $"Failed to get issue: %s{e |> PossibleErrors.Stringify}"
            | FailedToGetIssues e -> $"Failed to get issues: %s{e |> PossibleErrors.Stringify}"
            | FailedToGetProjects e -> $"Failed to get projects: %s{e |> PossibleErrors.Stringify}"
            | FailedToCreateIssue e -> $"Failed to create issue: %s{e |> PossibleErrors.Stringify}"
    
    let myError = FailedToAddComment (FailedToRunTask "oh no task Failed")
    let myError2 = FailedToGetIssue ( FailedToAddComment (FailedToRunTask "oh no task Failed"))
    let x = PossibleErrors.Stringify myError //Failed to add comment: Failed to run task: oh no task Failed
    let x2 = PossibleErrors.Stringify myError2 //Failed to get issue: Failed to add comment: Failed to run task: oh no task Failed

    [<DebuggerHidden>]
    let inline Tee fnct obj =
        fnct obj
        obj
    let inline (|&) obj fnct = Tee fnct obj
    let CIC (str: string) (str2: string) = str.ToLower() = str2.ToLower()

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