namespace JiraCloudConnect

open Config
open ConsoleUtils.ConsoleUtils
open Atlassian.Jira
open RestSharp
open Newtonsoft.Json

module APICalls =

    let client = GetClient ()
    let restClient = client.RestClient.RestSharpClient

    let customFieldExistOnIssue (issue: Issue) (cf: string) =
        issue.CustomFields
        |> Seq.tryFind (fun c -> c.Name = cf)
        |> function
        | Some x -> true
        | None -> false

    let addFieldToIssue (cfName: string) (cfValue: string) (issue: Issue) =
        if not (customFieldExistOnIssue issue cfName) then
            issue.CustomFields.Add(cfName, cfValue) |> ignore

    let StartNewIssue (key: string) = client.CreateIssue(key)

    let CreateJiraIssueTask (key: string) (issue: Issue) = 
        issue.SaveChangesAsync().Wait()

    let CreateJiraIssueBase fn (prefix: string) (msg: string) (e: 'dbRecord) =
        out msg
        try
            let issue = client.CreateIssue(prefix)
            fn e issue
            printfn $"Creating {issue.Description}..."
            issue.SaveChangesAsync().Wait()
            out "Success"
            Ok()
        with ex -> 
            out ex.Message
            failwith ex.Message
            //StandardErrors.AnUnexpectedErrorOccurredDetailed("Failed to add object to Jira", ex) |> Error

    let call fn = Async.AwaitTask(fn) |> Async.RunSynchronously

    let rec CallUntilAll (jql: string) : Issue list =
        out $"CAlling JQL query: {jql}"
        let rec _callUntilAll startAt (list: Issue list) =
            out $"Issue list has %i{list.Length} entries."
            let queryResult = client.Issues.GetIssuesFromJqlAsync(jql, 100, startAt) |> call
            let x = queryResult |> Seq.toList
            if x.Length > 0 then
                _callUntilAll (startAt+x.Length) (x @ list)
            else
                out $"Found {list.Length} issues. Returning."
                list |> List.rev
        _callUntilAll 0 []

    let GetAllProjects () = client.Projects.GetProjectsAsync() |> call |> Seq.toList

    let GetProjectByKey (key: string) =
        GetAllProjects()
        |> Seq.where (fun p -> p.Key = key)
        |> Seq.toList

    let GetLocations () = CallUntilAll "project = loc and status = active"
        //query {
        //    for i in client.Issues.Queryable do
        //        where (i.Project = "Locations")
        //        select i
        //}

    let GetEmployees () = CallUntilAll "project = emp and status = active"

    let PrintCustomFieldsOnIssue key =
        let issue = client.Issues.GetIssueAsync(key) |> call
        issue.CustomFields
        |> Seq.iter (fun cf -> printfn $"[{cf.Id}] {cf.Name}")

    let UpsertCustomFieldOnIssue (issue: Issue) propName propValue : DirtLevel =
        out $"Checking '{propName}'"
        let customField = if isNull issue.[propName] then None else issue.[propName].Value |> Some
        customField
        |> function
        | Some cfv ->
            if cfv <> propValue then
                let reason = $"Setting {propName} from '{cfv}' to '{propValue}'"
                issue.CustomFields.[propName].Values <- [|propValue|]
                out reason
                Dirty reason
            else
                out "No changes needed"
                NotDirty
        | None ->
            issue.CustomFields.Add(propName, propValue) |> ignore
            $"Adding CustomField {propName} ({propValue})"
            |& out
            |> Dirty

    let private isSummaryDirty (issue: Issue) propValue =
        if CIC issue.Summary propValue |> not then
            issue.Summary <- propValue
            Dirty $"Summary is different. {issue.Summary} != {propValue} Lengths: {issue.Summary.Length} != {propValue.Length}"
        else NotDirty

    let TransitionIssue (issue: Issue) (workflowActionName: string) = issue.WorkflowTransitionAsync(workflowActionName).Wait()

    let TransitionIssueUsingKey  (issueKey: string) (workflowActionName: string) =
        out $"Transitioning {issueKey} to {workflowActionName}"
        let issue = client.Issues.GetIssueAsync(issueKey) |> call
        TransitionIssue issue workflowActionName

    let TransitionIssueToInactive (issue: Issue) = TransitionIssue issue "Inactive"

    let SetIssueActiveState (issue: Issue) (activeState: string) =
        let xstate = activeState.ToLower() = "true"
        xstate
        |> function
        | true -> NotDirty
        | false ->
            if issue.Status.Name.ToLower() = "active" then
                issue.SaveChanges()
                issue.WorkflowTransitionAsync("31").Wait()
            NotDirty

    let GetCustomFieldDropdownOptions (url: string) : CustomDropdownValueForReading list =
        let req (url: string) =
            let rr = RestRequest(url)
            let x = restClient.Get(rr)
            let res = JsonConvert.DeserializeObject<CustomDropdownForReading> x.Content
            //if box res = null then BREAK()
            res

        let rec getAllDropdownValues startAt ddlList =
            let xUrl = $"{url}?startAt={startAt}"
            let xddl = req xUrl
            let vals = xddl.Values
            if xddl.IsLast then
                (vals @ ddlList)
            else
                getAllDropdownValues (startAt+100) (vals @ ddlList)

        let initialDdl = req url
        if initialDdl.IsLast then
            initialDdl.Values
        else
            getAllDropdownValues 100 initialDdl.Values

    let AddOptionsToDropdown (url: string) (ddl: CustomDropdownFieldForSending)  =
        let json = JsonConvert.SerializeObject(ddl)
        let r = RestRequest(url)
        let rr = r.AddJsonBody(json)
        try
            let res = restClient.Post(rr)
            match res.IsSuccessful with
            | true -> Ok()
            | false -> failwith res.Content
                //StandardErrors.AnUnexpectedErrorOccurredAsMessage $"Call successful but API said no while trying to add an option to the dropdown. {url}\n" |> Error
        with ex -> failwith ex.Message
            //StandardErrors.AnUnexpectedErrorOccurredAsMessage "Call to API failed.\n" |> Error

    let RemoveOptionFromDropdown (url: string) (valId: string) =
        let r = RestRequest($"{url}/{valId}")
        try
            let res = restClient.Delete(r)
            match res.IsSuccessful with
            | true -> res.Content |> Ok
            | false -> failwith res.Content
                //StandardErrors.AnUnexpectedErrorOccurredAsMessage $"Call successful but API said no while trying to remove an option ({valId}) from the dropdown. {url}\n" |> Error
        with ex -> failwith ex.Message
            //StandardErrors.AnUnexpectedErrorOccurredAsMessage "Call to API failed.\n" |> Error

    let AddCommentToIssue (issueKey: string) (comment: string) =
        out $"Adding comment to {issueKey}"
        let issue = client.Issues.GetIssueAsync(issueKey) |> call
        let c = new Comment()
        c.Body <- comment
        c.Author <- "author"
        try
            issue.AddCommentAsync(c) |> call
            |> Ok
        with ex ->
            //Error ex
            raise ex