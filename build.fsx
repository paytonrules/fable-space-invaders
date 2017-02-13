#I @"packages/"
#r "FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing

Target "Clean" (fun _ -> 
    CleanDirs [".fake"; "src/bin"; "src/obj"; "test/bin"; "test/obj";
                "dist/umd"; "dist/test"; "PropertyTests/bin"; 
                "PropertyTests/build"; "PropertyTests/obj"]
)

Target "Browser" (fun _ ->
    Shell.Exec "yarn" |> ignore
)

Target "Test" (fun _ -> 
    Shell.Exec("yarn", "test") |> ignore
)

Target "BuildPropertyTest" (fun _ -> 
    MSBuildDebug "./PropertyTest/build" "Build" ["./PropertyTest/PropertyTest.fsproj"]
    |> Log "AppBuild-Output: "
)

Target "PropertyTest" (fun _ -> 
    let testDir = "./PropertyTest/build/"
    !! (testDir @@ "PropertyTests.dll")
    |> xUnit2 (fun p -> {p with HtmlOutputPath = Some(testDir @@ "html")})
)

"BuildPropertyTest"
==> "PropertyTest"

RunTargetOrDefault "Main"