namespace JiraCloudConnect

open Atlassian.Jira
open ConsoleUtils.ConsoleUtils
open Atlassian.Jira.Remote
open Newtonsoft.Json
open RestSharp

module Issue = 
    let private getJiraUser reporterId =
        client.Users.GetUserAsync(reporterId) 
        |> call
        |> function
        | Ok u -> u
        | Error _ -> failwith "Failed to get user."

    let InitIssue (projectKey: string) (summary: string) = 
        let issue = client.CreateIssue(projectKey)
        issue.Summary <- summary
        issue

    let CreateQuickIssue (projectKey: string) (issueTypeId: string) (summary: string) (description: string) = 
        let client = Config.GetClient()
        let issue = client.CreateIssue(projectKey)
        issue.Type <- new IssueType(issueTypeId)
        issue.Summary <- summary
        issue.Description <- description
        issue.SaveChanges()

    let CreateQuickIssue2 (projectKey: string) (issueTypeId: string) (summary: string) (description: string) (reporter: string) = 
        let client = Config.GetClient()
        let issue = client.CreateIssue(projectKey)
        issue.Type <- new IssueType(issueTypeId)
        issue.Summary <- summary
        issue.Description <- description
        issue.SaveChanges()
        issue

    let Rest_CreateIssue (projectKey: string) (issueTypeId: string) (summary: string) (description: string) (reporterId: string) =
        let payload =
                {| 
                    fields = 
                        {| 
                            summary = summary
                            project = 
                                {| 
                                    key = projectKey
                                |}
                            issuetype = 
                                {| 
                                    id = issueTypeId
                                |}
                            reporter = 
                                {| 
                                    id = reporterId
                                |}
                        |}
                |}
        let json = JsonConvert.SerializeObject(payload)
        let r = RestRequest($"{Config.apiUrl}/issue")
        let rr = r.AddJsonBody(json)
        rr.Method <- Method.POST
        let res = restClient.Post(rr)
        match res.IsSuccessful with
        | true -> 
            res.Content
            |> ParseCreatedIssueResponse
            |> Ok
        | false -> failwith res.Content
            // StandardErrors.AnUnexpectedErrorOccurredAsMessage "Call successful but API said no while trying to create an issue.\n" |> Error

    let UpdateJiraIssue (issue: Issue) = 
        issue.SaveChangesAsync().Wait()

    //let CreateJiraIssueBase fn (prefix: string) (msg: string) (e: 'dbRecord) =
    //    out msg
    //    try
    //        let issue = client.CreateIssue(prefix)
    //        fn e issue
    //        printfn $"Creating {issue.Description}..."
    //        issue.SaveChangesAsync().Wait()
    //        out "Success"
    //        Ok()
    //    with ex -> 
    //        out ex.Message
    //        failwith ex.Message
            //StandardErrors.AnUnexpectedErrorOccurredDetailed("Failed to add object to Jira", ex) |> Error

    //let CreateIssueBase (projectKey: string) (issueTypeId: string) (reporterId: string) (summary: string) (description: string) = 
    //    let issue = new Issue()
    //    issue.Reporter <- reporterId
    //    issue.Summary <- summary
    //    issue.Description <- description
    //    CreateIssue issue

    let GetIssue (key: string) = 
        client.Issues.GetIssueAsync(key) 
        |> call
        |> Result.mapError FailedToGetIssue

    let AddCommentToIssue (key: string) (comment: string) (author: string) =
        out $"Adding comment to {key}"
        client.Issues.GetIssueAsync(key) 
        |> call
        |> Result.map (fun i -> 
            let c = new Comment()
            c.Body <- comment
            c.Author <- author
            i,c)
        |> Result.bind 
            (fun (issue,comment) -> 
                issue.AddCommentAsync(comment) |> call
            )
        |> Result.mapError FailedToAddComment

    let Rest_AddAttachmentToIssue (key: string) (path: string) =
        let r = RestRequest($"{Config.apiUrl}/issue/{key}/attachments")
        r.AddHeader("X-Atlassian-Token", "no-check") |> ignore
        r.AddHeader("Content-Type", "multipart/form-data") |> ignore
        r.AddHeader("file", path) |> ignore
        r.AddFile("file", path) |> ignore
        let res = restClient.Post(r)
        match res.IsSuccessful with
        | true -> Ok()
        | false -> failwith res.Content