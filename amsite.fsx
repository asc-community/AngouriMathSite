open System.Diagnostics
open System
open System.IO
open System.Runtime.InteropServices

let (/) a b = Path.Combine(a, b)

let log msg =
    printfn $"Log: {msg}"

let generatorP = "_generator"
let outputP = "generated"
let contentP = "content"

let runProgram name activeDir args =
    let startInfo = ProcessStartInfo()
    startInfo.FileName <- name
    startInfo.Arguments <- args
    startInfo.WorkingDirectory <- activeDir
    use proc = new Process()
    proc.StartInfo <- startInfo
    if proc.Start() |> not then
        raise (Exception $"Failure to start process {name}. Report this bug, please.")
    proc.WaitForExit()

let git = runProgram "git"
let dotnet = runProgram "dotnet"

let cliArgs = Environment.GetCommandLineArgs() |> List.ofArray

let init () =
    if Directory.Exists("." / generatorP / "AngouriMath") then
        log "Skipping AngouriMath cloning..."
    else
        git ("." / generatorP) "clone https://github.com/asc-community/AngouriMath AngouriMath" 

    dotnet ("." / generatorP / "AngouriMath" / "Sources" / "AngouriMath" / "AngouriMath") "build -c release"

    if Directory.Exists("." / generatorP / "Yadg.NET") then
        log "Skipping Yadg.NET cloning..."
    else
        git ("." / generatorP) "clone https://github.com/WhiteBlackGoose/Yadg.NET Yadg.NET"

    if Directory.Exists("." / generatorP / contentP / "_wiki") then
        log "Skipping wiki cloning..."
    else
        git ("." / generatorP / contentP) "clone https://github.com/asc-community/AngouriMath.wiki.git _wiki"


let uninit () =
    if Directory.Exists("." / generatorP / "AngouriMath") |> not then
        log "Skipping deleting AngouriMath..."
    else
        Directory.Delete("." / generatorP / "AngouriMath") 

    if Directory.Exists("." / generatorP / "Yadg.NET") |> not then
        log "Skipping deleting Yadg.NET..."
    else
        Directory.Delete("." / generatorP / "Yadg.NET")

    if Directory.Exists("." / generatorP / contentP / "_wiki") |> not then
        log "Skipping deleting wiki..."
    else
        Directory.Delete("." / generatorP / contentP / "_wiki") 

let build () =
    dotnet (generatorP / "NaiveStaticGenerator") "run"

let run () =
    if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
        runProgram "start" "." ("." / outputP / "index.html")
    else if RuntimeInformation.IsOSPlatform(OSPlatform.OSX) then
        runProgram "open" "." ("." / outputP / "index.html")
    else if RuntimeInformation.IsOSPlatform(OSPlatform.Linux) then
        runProgram "xdg-open" "." ("." / outputP / "index.html")

let clean () =
    Directory.Delete ("." / outputP)

match cliArgs with
| [ _; _; "init" ] -> init ()
| [ _; _; "uninit" ] -> uninit ()
| [ _; _; "build" ] -> build ()
| [ _; _; "run" ] ->
    build ()
    run ()
| [ _; _; "clean" ] -> clean ()
| _ ->
    log $"Unrecognized arguments: {cliArgs}"




