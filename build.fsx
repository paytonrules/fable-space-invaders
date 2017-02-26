#I @"packages/"
#r "FAKE/tools/FakeLib.dll"

open Fake
open Fake.Testing

Target "Clean" (fun _ ->
    CleanDirs [ "node_modules"; "SpaceInvaders/bin"; "SpaceInvaders/obj";
                "UnitTest/bin"; "UnitTest/obj";
                "dist/umd"; "dist/test"; "PropertyTest/bin";
                "PropertyTest/build"; "PropertyTest/obj"]
)

Target "Browser" (fun _ -> Shell.Exec("yarn", "install") |> ignore)

Target "Test" (fun _ -> Shell.Exec("yarn", "test") |> ignore)

Target "Outdated" (fun _ -> Shell.Exec("yarn", "outdated") |> ignore)

Target "UpgradeFableDeps" (fun _ ->
    let dep = getBuildParam "dep"
    let upgradeCommand = sprintf "upgrade %s" dep

    Shell.Exec("yarn", upgradeCommand) |> ignore)

Target "RemoveFableDep" (fun _ ->
    let dep = getBuildParam "dep"
    match dep with
    | "" -> failwith "You must provide a dependency to remove"
    | _ ->
        let removeCommand = sprintf "remove %s" dep
        Shell.Exec("yarn", removeCommand) |> ignore)

Target "TestWatch" (fun _ ->
    let testWatch = async { Shell.Exec("yarn", "test -- --watch") |> ignore }

    Async.Parallel [|testWatch|]
    |> Async.RunSynchronously
    |> ignore)

Target "Watch" (fun _ ->
    let fableWatch = async { Shell.Exec ("yarn", "watch") |> ignore }
    let browser = async { Shell.Exec("yarn", "run start") |> ignore }
    let testsWatch = async { Shell.Exec("yarn", "test -- --watch") |> ignore }
    Async.Parallel [|fableWatch; browser; testsWatch|]
    |> Async.RunSynchronously
    |> ignore)

Target "BuildPropertyTest" (fun _ ->
    MSBuildDebug "./PropertyTest/build" "Build" ["./PropertyTest/PropertyTest.fsproj"]
    |> Log "AppBuild-Output: ")

Target "PropertyTest" (fun _ ->
    let testDir = "./PropertyTest/build/"
    !! (testDir @@ "PropertyTests.dll")
    |> xUnit2 (fun p -> {p with HtmlOutputPath = Some(testDir @@ "html")}))

Target "All" DoNothing

"Browser"
==> "Watch"

"Browser"
==> "Test"

"Browser"
==> "TestWatch"

"BuildPropertyTest"
==> "PropertyTest"

"Clean"
<=> "Browser"
<=> "Test"
<=> "BuildPropertyTest"
<=> "PropertyTest"
==> "All"

RunTargetOrDefault "All"
