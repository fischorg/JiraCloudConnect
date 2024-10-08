namespace JiraCloudConnect

open Atlassian.Jira
open ConsoleUtils.ConsoleUtils

module Issue = 
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