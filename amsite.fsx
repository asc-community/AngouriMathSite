open System.Diagnostics
open System

let runProgram name activeDir args =
    let startInfo = StartInfo()
    startInfo.FileName <- name
    startInfo.Arguments <- args
    startInfo.WorkingDirectory <- activeDir
    let proc = Process(startInfo)
    proc.Start()
    proc.WaitForExit()

let git = runProgram "git"
let dotnet = runProgram "dotnet"

let cliArgs = Environment.GetCommandLineArgs() |> List.ofArray

match cliArgs with
| [ "init" ] ->
    
