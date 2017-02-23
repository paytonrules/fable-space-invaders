#I @"packages/"
#r "FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing

Target "Clean" (fun _ ->
    CleanDirs [ "SpaceInvaders/bin"; "SpaceInvaders/obj";
                "UnitTest/bin"; "UnitTest/obj";
                "dist/umd"; "dist/test"; "PropertyTest/bin";
                "PropertyTest/build"; "PropertyTest/obj"]
)

Target "BrowserDeps" (fun _ -> Shell.Exec("yarn", "install") |> ignore)

Target "Browser" (fun _ -> Shell.Exec("yarn") |> ignore)

Target "Test" (fun _ -> Shell.Exec("yarn", "test") |> ignore)

Target "Watch" (fun _ ->
    let fableWatch = async { Shell.Exec ("yarn", "watch") |> ignore }
    let browser = async { Shell.Exec("yarn", "run start") |> ignore }
    let testsWatch = async { Shell.Exec("yarn", "test -- --watch") |> ignore }
    Async.Parallel [|fableWatch; browser; testsWatch|]
    |> Async.RunSynchronously
    |> ignore
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

Target "All" DoNothing

"BuildPropertyTest"
==> "PropertyTest"

"BrowserDeps"
==> "Watch"

"BrowserDeps"
==> "Browser"

"Browser"
==> "Test"

"Clean"
==> "Browser"
==> "Test"
==> "PropertyTest"
==> "All"

RunTargetOrDefault "All"
