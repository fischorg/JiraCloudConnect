namespace JiraCloudConnect

open Atlassian.Jira
open FSharp.Data
open Rest

module Project = 
    let GetAllProjects () = 
        client.Projects.GetProjectsAsync() 
        |> call
        |> Result.map Seq.toList
        |> Result.mapError FailedToGetProjects

    let GetProject (key: string) = client.Projects.GetProjectAsync(key) |> call

    let GetBodyResponse response = 
        response.Body
        |> function
        | HttpResponseBody.Text t -> t
        | _ -> failwith "Error: Expected text response."

    let GetProjectRest (key: string) = 
        let url = $"{Config.apiUrl}/project/{key}"
        let r = Http.Request(url, headers = GetHeaderWithAuth())
        let resp = GetBodyResponse r
        let project = ProjectResponse.Parse(resp)
        BuildProject project