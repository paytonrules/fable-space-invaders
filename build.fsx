#I @"packages/"
#r "FAKE/tools/FakeLib.dll"

open Fake

Target "Clean" (fun _ -> 
    CleanDirs ["src/bin"; "src/obj"; "test/bin"; "test/obj";
                "dist/umd"; "dist/test"]
)

RunTargetOrDefault "Clean"