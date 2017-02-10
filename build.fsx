#I @"packages/"
#r "FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing

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

Target "QuickTest" (fun _ -> 
    let testDir = "propertyTests/PropertyTests/bin/Debug/"
    !! (testDir @@ "PropertyTests.dll")
    |> xUnit2 (fun p -> {p with HtmlOutputPath = Some(testDir @@ "html")})
)

RunTargetOrDefault "Main"