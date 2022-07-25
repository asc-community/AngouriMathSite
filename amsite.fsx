
let runProgram name activeDir args =
    System.Diagnostics.Process.Start(fileName: name, activeDir: activeDir, arguments: args)
