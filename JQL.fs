namespace JiraCloudConnect

open Atlassian.Jira
open ConsoleUtils.ConsoleUtils

module JQL = 

    let rec ExecuteJQL (jql: string) =
        let rec _callUntilAll startAt (list: Issue list) =
            out $"Issue list has %i{list.Length} entries."
            client.Issues.GetIssuesFromJqlAsync(jql, 100, startAt) 
            |> call
            |> function
            | Ok queryResult ->
                let x = queryResult |> Seq.toList
                if x.Length > 0 then
                    _callUntilAll (startAt+x.Length) (x @ list)
                else
                    out $"Found {list.Length} issues."
                    list |> List.rev |> Ok
            | Error e -> e |> Error
            
        _callUntilAll 0 []

