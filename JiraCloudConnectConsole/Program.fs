namespace JiraCloudConnectConsole

open JiraCloudConnect
open ConsoleUtils.ConsoleUtils

module Main = 
    [<EntryPoint>]
    let main argv = 
        let client = Config.GetClient()
        
        // GET PROJECT REST TEST ///////////////////////
        let project = Project.GetProjectRest("JFI")
        
        // GET PROJECT TEST ///////////////////////
        let project =
            Project.GetProject("JFI")
            |> function
            | Ok p -> 
                printfn $"[{p.Key}] {p.Name}"
                let issueTypes = p.GetIssueTypesAsync() |> call
                issueTypes 
                |> function
                | Ok itlist -> itlist |> Seq.iter (fun it -> printfn $" [{it.Id}] {it.Name}\n{it.IconUrl}")
                | Error e -> printfn $"Error: {e}"
            | Error e -> printfn $"Error: {e}"

        // GET PROJECTS TEST ///////////////////////
        //let projects = 
        //    Project.GetAllProjects()
        //    |> function
        //    | Ok plist -> 
        //        plist 
        //        |> Seq.iter (fun p -> 
        //            printfn $"[{p.Key}] {p.Name}"
        //            let issueTypes = p.GetIssueTypesAsync() |> call
        //            issueTypes 
        //            |> function
        //            | Ok itlist -> itlist |> Seq.iter (fun it -> printfn $" [{it.Id}] {it.Name}")
        //            | Error e -> printfn $"Error: {e}"
        //        )
        //    | Error e -> printfn $"Error: {e}"
        //////////////////////////////////////////

        out "Press any key to exit."
        wait()
        0