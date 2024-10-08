namespace JiraCloudConnect

open Atlassian.Jira

[<AutoOpen>]
module JiraExtensions = 
    type Issue with
        member this.GetCustomFieldValue (customFieldName: string) =
            this.CustomFields
            |> Seq.tryFind (fun c -> c.Name = customFieldName)
            |> function
            | Some x -> x.Values |> Seq.head |> Some
            | None -> None