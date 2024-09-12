namespace JiraCloudConnectConsole

open JiraCloudConnect
open ConsoleUtils.ConsoleUtils

module Main = 
    [<EntryPoint>]
    let main argv = 
        let client = Config.GetClient()
        let projects = 
            APICalls.GetAllProjects()
            |> Seq.iter (fun p -> 
                out p.Name
            )
        out "Press any key to exit."
        wait()
        0