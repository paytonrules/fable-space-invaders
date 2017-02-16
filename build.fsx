#I @"packages/"
#r "FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing

Target "Clean" (fun _ ->
    CleanDirs [".fake"; "SpaceInvaders/bin"; "SpacceInvaders/obj";
                "UnitTest/bin"; "UnitTest/obj";
                "dist/umd"; "dist/test"; "PropertyTest/bin";
                "PropertyTest/build"; "PropertyTest/obj"]
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
