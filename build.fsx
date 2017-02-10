#I @"packages/"
#r "FAKE/tools/FakeLib.dll"

open Fake

Target "Clean" (fun _ -> 
    CleanDirs ["src/bin"; "src/obj"; "test/bin"; "test/obj";
                "dist/umd"; "dist/test"]
)

Target "Main" (fun _ ->
    Shell.Exec "yarn" |> ignore
)

Target "Test" (fun _ -> 
    Shell.Exec("yarn", "test") |> ignore
)

"Clean"
    ==> "Main"
    ==> "Test"
    ==> "RebuildAll"

RunTargetOrDefault "RebuildAll"