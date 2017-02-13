#I @"packages/"
#r "FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing

Target "Clean" (fun _ -> 
    CleanDirs [".fake"; "src/bin"; "src/obj"; "test/bin"; "test/obj";
                "dist/umd"; "dist/test"; "PropertyTests/bin"; 
                "PropertyTests/build"; "PropertyTests/obj"]
)

Target "Main" (fun _ ->
    Shell.Exec "yarn" |> ignore
)

Target "Test" (fun _ -> 
    Shell.Exec("yarn", "test") |> ignore
)

Target "BuildPropertyTests" (fun _ -> 
    MSBuildDebug "./PropertyTests/build" "Build" ["./PropertyTests/PropertyTests.fsproj"]
    |> Log "AppBuild-Output: "
)

Target "RunPropertyTests" (fun _ -> 
    let testDir = "./PropertyTests/build/"
    !! (testDir @@ "PropertyTests.dll")
    |> xUnit2 (fun p -> {p with HtmlOutputPath = Some(testDir @@ "html")})
)

"BuildPropertyTests"
==> "RunPropertyTests"

RunTargetOrDefault "Main"