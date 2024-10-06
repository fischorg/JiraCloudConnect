namespace JCC

open Atlassian.Jira

module Project = 
    let GetAllProjects () = 
        client.Projects.GetProjectsAsync() 
        |> call
        |> Result.map Seq.toList
        |> Result.mapError FailedToGetProjects

    let GetProject (key: string) = client.Projects.GetProjectAsync(key) |> call