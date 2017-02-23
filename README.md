# Space Invaders in F#/Fable

This is a port of Space Invaders for the web in F#. Why F#? Because I started this project in ClojureScript and found that I really missed static typing, and because since F# is a .NET framework language much of this code is theoretically portable to Monogame or Unity3D.

Also because I like to put a yak in front of me, and shave it.

This project was initially setup using the template here: https://github.com/fable-compiler/fable-getting-started. Since then I've modified it a bit including setting up FAKE so that you can use only one command-line interface, and adding property based testing.

## TLDR; Setup

If you just want to get started and figure out things as you go do this:

* Clone this repo.
* Ensure you have node, yarn and mono installed.
* Start a development version of the app by running `./build.sh Watch` or `build.cmd Watch` on Windows.
* Run the full build process by using the default FAKE task, so `./build.sh` or `build.cmd` will clean, run tests, run property tests and put the release version of the application in `dist/umd/Main.js`.

This will run a clean build, and start three processes. One rebuilds the app on any file change. Another runs unit tests on any file change. Finally browser-sync is started so that the app is automatically refreshed in the browser on any change. By default the game should be loaded at http://localhost:3000. Stop the process using `ctrl-c` and not `enter` like the console says. Hitting enter will only stop the test runner.

Figure out the rest by looking through `build.fsx`.

## Project Structure

The application consists of four projects. There is the `SpaceInvaders` project which contains most of the game logic as well as the game loop and is platform independent. There is the `Browser` project which contains Fable specific code for rendering in a browser environment, the `UnitTest` project which runs all the tests using the NUnit-to-Fable plugin and finally a `PropertyTest` project. The `PropertyTest` project uses XUnit and FSCheck to run property based tests and runs as a >NET binary, therefore it can only run those tests on the `SpaceInvaders` project.

The dependency chain looks something like:

SpaceInvaders -> None

Browser -> SpaceInvaders, Fable

UnitTest -> Browser

PropertyTest -> Game

You can build all of this and run the tests by just running `./build.sh` on Linux/OSX and `./build.cmd` on a Mac.

## Game Architecture

The overwhelming amount of code is in the `SpaceInvaders` project. It contains all the game logic, and the game loop. Most coding goes here, and is easily unit tested because it's primarily side-effect free. In fact the game has exactly one mutable variable, and I'm kinda mad about it.

The GameLoop is written to be JS compatible but `requestAnimationFrame` is actually injected by the Browser layer. This means the loop could be used by other platforms, although in practice it probably wouldn't be.

You can think of the layers like this:

Browser -> Game Loop -> Game -> Entities

Entities are things like the Laser, Invaders and Bullets. Note that none of these are classes, but modules with functions as I've chosen not to use any of the OO features of F# unless I have to for interop. The Game Layer contains the functions related to interaction of the various entities, and the all important Update function. Every call to Update takes the current game state, the event (such as Tick, Shoot, or Move) and returns a new game state. It is a pure function, and as such everything in the Game layer is a pure function.

The only mutuable state takes place in the GameLoop, which keeps track of the current global game state for each loop. The mutable state is initialized in the `createEventHandler` function which creates an event handler function out of a passed in function. To see it in use check out `Browser/Main.fs` but honestly you are unlikely to need to change this. The one event handler created from an initial game state and the game's `update` function is injected into the game loop function to be called on every tick. It's also injected into event handlers at the browser layer, so that button clicks can be converted into events.

## Writing Tests

### Unit Tests

The FAKE test `Test` will compile and run all the unit tests. Alternatively, you can use `./build.sh Watch` or `yarn TestWatch`. The latter will only run the unit tests every time they change, whereas the former will compile the tests, the app, and run a browser-sync process so changes are processed instantaneously. Note that while the screen will say "press enter to terminate process" you actually need to use the traditional Ctrl-C. Running the Watch processes means the tests run significantly faster than running `Test`, however you will occasionally have to restart the process when changes aren't picked up, especially when new files are created.

Unit tests are placed in the `UnitTest/fs` folder. They follow this format:

```
namespace Test

open Fable.Core.Testing
open SpaceInvaders.EventMapping
...

[<TestFixture>]
module EventMapping =
    let mutable events = []

    [<SetUp>]
    let setup () =
        events <- []

    [<Test>]
    let ``map leftarrow to moveleft event`` () =
        ...
        equal 1 updatedGame
```

* The file must be in the `Test` namespace.

* The file must use `open Fable.Core.Testing`

* The file usually opens the namespace under test (here `SpaceInvaders.EventMapping`)

* The file must create one or more inner modules which uses `[<TestFixture>]`

* Each unit test must use the `[<Test>]` attribute.

* The name of the unit test is the same as the name of the `([<Test>])` function.

  In the above example, the unit test is called `message should be correct`

* You can use the `equal` function to write assertions. The first argument is the expected value, the second argument is the actual value.

* The unit test module does **not** need to have the same name as the module in `SpaceInvaders`, but it is more convenient if they are the same. Unit Tests need to be added to the `UnitTest.fsproj`, following the same rules as a typical F# project.

* NUnit is not fully supported, in fact the only supported assertion is AreEqual. SetUp is supported, as you can see in the example above.

### Property Based Tests

Property Based Tests are written using XUnit and FSCheck, and only test the SpaceInvaders project, because XUnit and FSCheck are not supported by Fable.

The positive of this development is there is now a clean separation of platform independent code, in the SpaceInvaders project, and platform dependent code in the Browser and UnitTest projects. Furthermore anything supported by XUnit3 and FSCheck is supported, because it is running on the .NET/Mono runtime. The disadvantage is multiple builds and additional complexity.

## Adding Dependencies

### Files And Project References

Files and references are added in the .fsproj file as in your standard F# project. There is one major quirk with dependencies which is that Fable does not look at the ProjectReference tag in fsproj files. For example in `Browser.fsproj` there is the line:

```
<ProjectReference Include="../SpaceInvaders/SpaceInvaders.fsproj">
```

Fable does not respect this line, only looking at the `fableconfig.json` for depedencies between projects. So why have it? Because your editor does! This ensures you ddon't get a bunch of squiggly lines in your favorite editor, which is spacemacs.

### Fable/JS Dependencies with Yarn

Most external dependencies you use should be in the form of Fable plugins installed from NPM with Yarn. You can use the `yarn` command line tool directly but I've wrapped what you should need into FAKE tasks for consistency.

#### Upgrading your dependencies

You can use `./build.sh Outdated` which will tell you which of your project's dependencies are out of date.

You can then do one of the following:

* Use `./build.sh Upgrade` which will upgrade *all* of your dependencies to the latest versions, and also edits `package.json` to use the latest versions.

* Use `./build.sh Upgrade dep=foo` which will upgrade the package `foo` to the latest version, and it will also automatically edit `package.json` to use the latest version for `foo`

* You can manually edit `package.json` to use the latest versions, and then run `.\build.sh BrowserDeps` to run the entire build, which will install dependencies. In general you shouldn't do this until you understand the setup.

You should check-in changes to `package.json` and `yarn.lock` to source control.

#### Removing dependencies

You can do one of the following:

* Use `.\build.sh RemoveFableDep dep=foo` which will remove the package `foo`, and it will also automatically remove `foo` from the `package.json` file.

* Manually edit `package.json` to remove the `foo` package, and then use `.\build.sh Browser` to rebuild dependencies.

#### Locking down your dependencies

Whenever you use the tasks around Fable dependencies, you'll need to check in the updated `package.json` and `yarn.lock` files. These files ensure that anybody who uses the `BrowserDeps` task will get the same versions of the dependencies, including the Fable compiler.

You should very frequently use the `Outdated` and `UpgradeFableDeps` to ensure that your dependencies are up to date.

### Paket Dependencies

The dependencies installed by Paket are only used by the `PropertyTests` for running property-based tests on the .NET/Mono runtime, and to run FAKE itself. The `build.sh` or `build.cmd` command will bootstrap the Paket program, and the packages in `packet.dependencies` will be installed.

More information on paket can be found at https://fsprojects.github.io/Paket/.

# Building The Long Way

The toolchain for this project isn't actually particularly complicated, but it takes some getting used to especially if you're new to Fable or F#. There's four project files here, and two compilers...okay maybe it is particularly complicated.

## Prerequisites

While the FAKE toolchain does it's best to bootstrap itself, you're going to need some tools to get started.

* Mono/.NET: If you're on Windows I don't believe you have to do anything, but on a Mac/Linux system you'll need to install Mono. I'm on a Mac and have it installed via homebrew.
* Node.js: Node.js is required for transpiling to JavaScript (because you need a command line interpreter). You can find at https://nodejs.org.
* Yarn: This project uses yarn for JavaScript package (as opposed to Paket for .NET packages). management Yarn can be found at https://yarnpkg.com.

I'm on a Mac and am able to install all three of these tools via homebrew. Windows users...um good luck I guess?

Note that you do not need to install Paket or Fable. Paket is bootstrapped by the `build.cmd` or `build.sh`. If you're familiar with Fable and F# projects you probably don't need to read beyond this. Really, the build is not as complicated as I'm about to make it sound.

## Definitions

The setup takes a bit of getting used to, especially for newcomers to Fable. Hence the rather large readme.

*Fable* - Transpiler that lets us write F# code that runs in the browser. Because it transpiles F# to JavaScript it uses the JavaScript ecosystem. When we say something isn't "compatible with Fable" it really means that it depends on the .s NET runtime in some system specific way. Many libraries can likely be compiled wth Fable, but nobody has tried.

*FAKE* - FSharp version of MAKE. The build.fsx file is written in FAKE, and makes it easier to manage all the different dependencies.

*FSCheck* - A .NET tool for running Property Based Testing. This is _not_ ported to JavaScript/Fable, which is why it is managed in a seperate fsProject.

*NPM* - Package repository for javascript dependencies. Fable plugins that replace NuGet packages are found here, including the package that allows us to write NUnit tests in the UnitTest project.

*NuGet* - Package repository for .NET. It is also the name of the original program you would use to manage these dependencies.

*Paket* - Paket is NuGet replacement that is more source control friendly. This is how we manage .NET dependencies, like FSCheck.

*Project* - .NET projects and Fable both support the .fsproject file format for managing the files and dependencies in your build. When I refer to a project I usually mean one of these. This project has four projects (SpaceInvaders.fsproj, UnitTest.fsproj, PropertyTest.fsproj and Browser.fsproj).

*Yarn* - An alternative to the default NPM tool for managing JavaScript dependencies. So that means we are using Yarn as a default to NPM, and Paket as a default to NuGet. While these have advantages I admit it's confusing. Most of the time you should only need to use FAKE.

# About The Build

The build is fairly complex consdering the size of the code. I hope I've made it so you don't have to understand every inch of it to procoeed, but just in case I've documented most of it here.

## Building For The Browser

Building for the browser is done with the Fable compiler, which is automated by Yarn, which is called by FAKE. Fortunaely you don't have to worry about all that as the build script provides a unified interface. That said it helps to understand what's going on under the hood when you run `./build.sh Browser`.

The process starts in the `build.sh` file (Or `build.cmd ` on Windows) which begins by updating the Paket program via its bootstrapper. If you are not on a network this step gets skipped, but otherwise it ensures you hae Paket installed. It then runs `.paket/paket.exe restore` to update your Paket dependencies. Most of the time this does nothing, as those do not change much, and it it gets skipped if you are off the network. It then delegates to the FAKE program. FAKE is installed as a Paket dependency, so provided this step was successful it will be present.

Since we're running the `Browser` task in this example you can look at its target in `build.fsx` to see that it's a very simple shell call to `yarn`.

```
Target "Browser" (fun _ -> Shell.Exec("yarn") |> ignore)
```

You can also scan to the bottom of the build.fsx file to see its dependency chain. As of this writing it looks like:

```
"BrowserDeps"
==> "Browser"
```

So `Browser` depends on `BrowserDeps`. A quick check to see what `BrowserDeps` does and you'll see it's also a call out to `yarn`.

```
Target "BrowserDeps" (fun _ -> Shell.Exec("yarn", "install") |> ignore)
```

So running the `Browser` task just makes two calls to `yarn`, `install` and the default. Now what do those do...(deeper into the rabbit hole we go).

`yarn install` installs any JavaScript dependecies from NPM, the same as `npm` does if you're familiar with that. It gets those from the file `package.json` it gets those from a traditional JavaScript project.

>*Bug Alert* The confusion here is part of what prompted me to write the unified FAKE build and this Readme to end all Readme's. Paket installs .NET/Mono runtime dependencies like   FAKE and FSCheck. These are not included in the final browser JavaScript. Yarn installs JavaScript dependencies like the fable-compiler, fable-powerpack and mocha. Some of these (like fable-powerpack) end up transpiled and included in the final compiled JavaScript and some are run exclusively on Node runtime like binaries, like the Fable compiler itself. If you're going to add a depencency that's in the final program, you have to add it from NPM and not from NuGet via Paket.

After `yarn install` completes then `yarn` is called. `yarn` executes the default task in the package.json, which is `build`. `build` calls the fable compiler via `fable --target umd`. Down the rabbit hole we continue...

The Fable compiler uses its own JSON configuration file, called `fableconfig.json`. I'm not going to go into it in detail here, in part because I don't understand it all, but note that our current one has two targets: umd and test. In the umd target we reference two project files: `SpaceInvaders.fsproj` and `Browser.fsproj`. In the test we reference three by inclding `UnitTest.fsproj` and adding the plugin `Fable.Plugins.NUnit.dll` which takes our NUnit tests and converts them to the JavaScript testing framework Mocha.

Now that the Fable compiler knows what project to include and their order, it will use the project files to actually compile the projects. Those use the fsproj file format, and are pretty well documented via comments in the fsproj files themselves (not by me, but I don't know who to credit, possibly @pauan). In Browser.fsproj you'll see these files included:

```
  <ItemGroup>
    <Compile Include="fs/Window.fs"/>
    <Compile Include="fs/Presentation.fs"/>
    <Compile Include="fs/Main.fs"/>
  </ItemGroup>
```

It also specifies its project dependencies:

```
  <ItemGroup>
    <Reference Include="../node_modules/fable-core/Fable.Core.dll"/>
    <Reference Include="../node_modules/fable-powerpack/Fable.PowerPack.dll"/>
    <ProjectReference Include="../SpaceInvaders/SpaceInvaders.fsproj">
      <Project>{C8152C89-3216-40CF-91A6-E88DA89E6548}</Project>
      <Name>SpaceInvaders</Name>
    </ProjectReference>
  </ItemGroup>
```

>*Bugish Alert* Your fsproj files specify the files being compiled and the order they are compiled in. They also specify any dependent projects, however Fable does not look at any `ProjectReference` items. So for instance even tbough `Browser.fsproj` specifies a dependency on `SpaceInvaders.fsproj` in it, Fable still requires it in the `fableconfig.json`. So why have it in the fsproj file at all? Because your editor does look at that entry, and does not look at `fableconfig.json`. This can lead to situations where your editor is telling you that you have compiler errors, but yarn is able to build just fine. Fable does respect the Reference key in the item group, which is how the projects specify their dependencies on Fable.Core.dll and Fable.PowerPack.dll.

This is how building for the browser works. The end result is a file in `dist/umd/Main.js` and `dist/umd/Main.js.map` which contains your JavaScript. It's included in `dist/index.html` which is how you view the game. If you browse in `dist` you'll see a subdirectory for `test` which contains the test build which is more files. This is because we do not "rollup" the files into one in test, as we want clearer error messages.
