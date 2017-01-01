Space Invaders in F#/Fable
=================

This is an attempt to write Space Invaders for the web in F#. Why F#? Because I started this project in 
ClojureScript and found that I really missed static typing. 

This project was initially setup using the template here: https://github.com/fable-compiler/fable-getting-started. 

Much of the readme has language from the getting started, because it's not wrong, it just sounds a little strange.

Setup
======================

After cloning the repo you'll need to install depedencies. 

Make sure that you have [`node`](https://nodejs.org/) and
[`yarn`](https://yarnpkg.com/) installed. 

You can get [`yarn`](https://yarnpkg.com/) from
[its website](https://yarnpkg.com/en/docs/install). On a mac you can use homebrew.
```

**Note:** Fable works with either `npm` or `yarn`. This repository uses `yarn`
because it has [several advantages over `npm`](https://yarnpkg.com/) (and because it's what the quickstart used).

----

Now you must use [`yarn install`](https://yarnpkg.com/en/docs/cli/install),
which will download all of the necessary dependencies for the project.

After the dependencies are downloaded, it will then automatically compile the
project.

**Note:** you can instead use [`yarn`](https://yarnpkg.com/en/docs/cli/install),
which is exactly the same as [`yarn install`](https://yarnpkg.com/en/docs/cli/install)

You do not need to install [Fable](http://fable.io/) globally:
[Fable](http://fable.io/) will be installed locally inside of the
`node_modules` folder. This makes it easier for other people to contribute to
your project because they do not need to install [Fable](http://fable.io/)
globally, and it also guarantees that everybody is compiling your project with
the correct version of [Fable](http://fable.io/).

Compilation
===========================

The project is automatically compiled when you use
[`yarn`](https://yarnpkg.com/en/docs/cli/install)

You can instead use [`yarn run watch`](https://yarnpkg.com/en/docs/cli/run),
which will compile this project (just like
[`yarn`](https://yarnpkg.com/en/docs/cli/install)), but it will also
automatically recompile your project if you make any changes to your project's
files.

If you want to stop watch mode, just hit the `Enter` or `Return` key.

Note that watch mode currently doesn't look at the raw javascript files.

Unit Tests
====================================

You can use [`yarn test`](https://yarnpkg.com/en/docs/cli/test) which will
compile and run all the unit tests.

Alternatively, you can use [`yarn test -- --watch`](https://yarnpkg.com/en/docs/cli/test)
which is the same as [`yarn test`](https://yarnpkg.com/en/docs/cli/test) except
that it automatically reruns the unit tests whenever you make any changes to your
project's files. This is much faster than using
[`yarn test`](https://yarnpkg.com/en/docs/cli/test)

This will also build the main app, so you really don't need to run watch on the tests and the app simultaneously.

Viewing The Game
=========================================

Deployment to the web doesn't exist yet, although build simply puts it in the dist directory.  

To get live feedback you can run `yarn start` which opens a browser-sync session that will refresh each time the project
is built. This does lead to a midly annoying workflow:

* Open termnal.
* Run yarn test -- --watch.
* Open another terminal.
* Run yarn start.

This will run tests in the command prompt, and auto-update the browser window.

Making changes
===================================

The `.fs` files are in the `src/fs` folder, and the
`.js` files are in the `src/js` folder.

The `.fs` files are F# code, which will be compiled by [Fable](http://fable.io/).

The `.js` files are JavaScript code, which will be compiled by [Babel](http://babeljs.io/).
You can use any [ECMAScript 2015 features](https://github.com/lukehoban/es6features#readme)
which are [supported by Babel](http://babeljs.io/docs/learn-es2015/).

If you want to add more `.fs` files, you will need to edit the
`Main.fsproj` file.

As an example, if you want to add in a new `src/fs/Foo.fs` file, you will need
to add the following code to `Main.fsproj`:

```
<Compile Include="src/fs/Foo.fs" />
```

This should be placed in the same `ItemGroup` as the other `.fs` files.

Also, the order matters! If a file `Foo.fs` uses a file `Bar.fs`, then `Bar.fs`
must be on top of `Foo.fs`

That also means that `Main.fs` must be at the bottom, because it depends on
everything else.

Writing Unit Tests
========================================

Unit tests are placed in the `test/fs` folder. They follow this format:

```
namespace Test

open Fable.Core.Testing

[<TestFixture>]
module Message =
    [<Test>]
    let ``message should be correct`` () =
        App.Message.message |> equal "Hello world!"
```

* The file must be in the `Test` namespace.

* The file must use `open Fable.Core.Testing`

* The file must create one or more inner modules which uses `[<TestFixture>]`

* Each unit test must use the `[<Test>]` attribute.

* The name of the unit test is the same as the name of the `[<Test>]` function.

  In the above example, the unit test is called `message should be correct`

* You can use the `equal` function to write assertions. The first argument is
  the expected value, the second argument is the actual value.

  In the above example, `"Hello world!"` is the expected value, and
  `App.Message.message` is the actual value.

* The unit tests can automatically use any variable which is defined in
  `src/fs`. The above example uses the `App.Message.message` variable which is
  defined in `src/fs/Message.fs`

* The unit test module does **not** need to have the same name as the module
  in `src/fs`, but it is more convenient if they are the same.

If you want to add more unit tests, you will need to edit the
`test/Test.fsproj` file. It follows the same rules as `Main.fsproj` (see
"How to make changes to your project")

Adding Fabile Libraries with yarn
===============================

Most Fable libraries are stored in [npm](https://www.npmjs.com/). If
there is an [npm](https://www.npmjs.com/) package called `foo` that you want
to use in your project, then you can do either of the following:

* Use [`yarn add --dev foo`](https://yarnpkg.com/en/docs/cli/add)

* Manually edit the [`package.json`](https://yarnpkg.com/en/docs/package-json)
  file and add `foo` to the
  [`devDependencies`](https://yarnpkg.com/en/docs/package-json#toc-devdependencies),
  then use [`yarn`](https://yarnpkg.com/en/docs/cli/install)

  **Note:** In certain situations you may need to add the package to
  [`dependencies`](https://yarnpkg.com/en/docs/package-json#toc-dependencies)
  rather than
  [`devDependencies`](https://yarnpkg.com/en/docs/package-json#toc-devdependencies)

Usually the first option is better, but the second option gives you more
precise control over the version of the package.

----

If a Fable library isn't on [npm](https://www.npmjs.com/), you can
instead use a Git URL:

```
yarn add --dev https://foo/bar.git
```

You will need to replace `https://foo/bar.git` with the URL to the Git
repository. Here is an example:

```
yarn add --dev https://github.com/fable-compiler/fable-powerpack.git
```

If the Git repository is hosted on GitHub, you can instead use the shorter
form `author/name`, like this:

```
yarn add --dev fable-compiler/fable-powerpack
```

When using a Git URL, you can also specify a particular commit hash or branch
by adding a `#` at the end, like this:

```
yarn add --dev https://github.com/fable-compiler/fable-powerpack.git#5f1da75baf6c8f2fe0c13fe0e4b531ffaa1078ed
yarn add --dev https://github.com/fable-compiler/fable-powerpack.git#master
```

This also works with the shorter GitHub form:

```
yarn add --dev fable-compiler/fable-powerpack#5f1da75baf6c8f2fe0c13fe0e4b531ffaa1078ed
yarn add --dev fable-compiler/fable-powerpack#master
```

If you don't specify a commit hash or branch, then it uses the `master` branch.

How to download JavaScript libraries
====================================

The directions are exactly the same as "How to download Fable libraries"

How to use Fable libraries
==========================

Most Fable libraries contain a `.dll` file. You will need to edit your
`Main.fsproj` file to include the library's `.dll` file.

As an example, if you want to use the `fable-powerpack` library, you will need
to add the following code to `Main.fsproj`:

```
<Reference Include="./node_modules/fable-powerpack/Fable.PowerPack.dll" />
```

This should be placed in the same `ItemGroup` as the other `.dll` files.

Now you can use the `fable-powerpack` library in your `.fs` files.

How to import JavaScript code into F#
=====================================

First, make sure that you have the following code in your `Main.fsproj`
file:

```
<Reference Include="./node_modules/fable-core/Fable.Core.dll" />
```

Don't worry: this repository already includes the above code in
`Main.fsproj`

Now you can import `.js` code into F# by using
`Fable.Core.JsInterop.importMember`:

```
let foo = Fable.Core.JsInterop.importMember<string> "../js/foo.js"
```

**Note:** The variable must be the same in F# and JavaScript (in the above
example, the variable must be called `foo` in both F# and JavaScript)

**Note:** If the JavaScript code uses `export default` then you might need to
use `importDefault` rather than `importMember`

**Note:** You have to specify the F# type of the variable. If you don't want
to specify a type, you can instead use `obj`, which means "any type":

```
let foo = Fable.Core.JsInterop.importMember<obj> "../js/foo.js"
```

----

If you are using a lot of imports you can do this:

```
open Fable.Core.JsInterop
```

Now you no longer need the `Fable.Core.JsInterop` prefix when importing:

```
let foo = importMember<string> "../js/foo.js"
```

You can see an example in the `src/fs/Message.fs` file.

----

By convention, JavaScript files are placed into `src/js`, but you can import
JavaScript files in any folder.

You can also import builtin [Node.js](https://nodejs.org/) modules or
[npm](https://www.npmjs.com/) packages which have been downloaded with
[`yarn`](https://yarnpkg.com/en/docs/cli/install):

```
let EOL = importMember<string> "os"
```

```
let foo = importMember<string> "some-library/foo.js"
```

If the file path starts with `.` or `..` then it is relative to the `.fs` file
which contains the `importMember`

If the file path does not start with `.` or `..` then it is either a builtin
[Node.js](https://nodejs.org/) module (e.g.
[`path`](https://nodejs.org/dist/latest-v6.x/docs/api/path.html),
[`os`](https://nodejs.org/dist/latest-v6.x/docs/api/os.html), etc.) or it is
a dependency which is listed in the
[`package.json`](https://yarnpkg.com/en/docs/package-json) file.

How to upgrade your dependencies
================================

You can use [`yarn outdated`](https://yarnpkg.com/en/docs/cli/outdated) which
will tell you which of your project's dependencies are out of date.

You can then do one of the following:

* Use [`yarn upgrade foo`](https://yarnpkg.com/en/docs/cli/upgrade) which will
  upgrade the package `foo` to the latest version, and it will also
  automatically edit [`package.json`](https://yarnpkg.com/en/docs/package-json)
  to use the latest version for `foo`

* Use [`yarn upgrade`](https://yarnpkg.com/en/docs/cli/upgrade) which will
  upgrade *all* of your dependencies to the latest versions, and also edits
  [`package.json`](https://yarnpkg.com/en/docs/package-json) to use the latest
  versions.

* Manually edit [`package.json`](https://yarnpkg.com/en/docs/package-json) to
  use the latest versions, and then use
  [`yarn`](https://yarnpkg.com/en/docs/cli/install)

How to remove a dependency
==========================

You can do one of the following:

* Use [`yarn remove foo`](https://yarnpkg.com/en/docs/cli/remove) which will
  remove the package `foo`, and it will also automatically remove `foo` from
  the [`package.json`](https://yarnpkg.com/en/docs/package-json) file.

* Manually edit [`package.json`](https://yarnpkg.com/en/docs/package-json) to
  remove the `foo` package, and then use
  [`yarn`](https://yarnpkg.com/en/docs/cli/install)

Locking down your dependencies
==============================

Whenever you use [`yarn`](https://yarnpkg.com/en/docs/cli/install),
[`yarn add`](https://yarnpkg.com/en/docs/cli/add),
[`yarn upgrade`](https://yarnpkg.com/en/docs/cli/upgrade), or
[`yarn remove`](https://yarnpkg.com/en/docs/cli/remove), it will create a
[`yarn.lock`](https://yarnpkg.com/en/docs/yarn-lock) file which specifies all
of the dependencies that your project depends on (including sub-dependencies,
sub-sub-dependencies, etc.), and it also specifies the exact version for every
dependency.

When using [`yarn`](https://yarnpkg.com/en/docs/cli/install), if a
[`yarn.lock`](https://yarnpkg.com/en/docs/yarn-lock)
file exists, then it will use the exact versions which are specified in the
[`yarn.lock`](https://yarnpkg.com/en/docs/yarn-lock) file.

You should add the [`yarn.lock`](https://yarnpkg.com/en/docs/yarn-lock) file
into Git, because then everybody who contributes to your project is guaranteed
to use the exact same versions as you, which helps to prevent bugs.

You should very frequently use [`yarn outdated`](https://yarnpkg.com/en/docs/cli/outdated)
and [`yarn upgrade`](https://yarnpkg.com/en/docs/cli/upgrade) to ensure that
your dependencies are up to date.

After making any changes (such as [`yarn add`](https://yarnpkg.com/en/docs/cli/add),
[`yarn upgrade`](https://yarnpkg.com/en/docs/cli/upgrade), or
[`yarn remove`](https://yarnpkg.com/en/docs/cli/remove)) you should add
the new [`yarn.lock`](https://yarnpkg.com/en/docs/yarn-lock) into Git.
