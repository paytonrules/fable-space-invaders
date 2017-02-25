namespace Test

open Fable.Core.Testing
open SpaceInvaders.EventMapping
open SpaceInvaders.GameLoop
open SpaceInvaders.Game

[<TestFixture>]
module EventMapping =

    let initialGame = -1;

    let mutable updatedGame = initialGame
    let mutable events = []

    let updateFunc game event =
        updatedGame <- game
        events <- events @ [event]
        updatedGame

    [<SetUp>]
    let setup () =
        events <- []
        updatedGame <- initialGame

    [<Test>]
    let ``map leftarrow to moveleft event`` () =
        let mapper = mapEvents updateFunc

        mapper 1 ({key = KeyCodes.LeftArrow |> float } |> KeyDown) |> ignore

        equal true (events = [ MoveLeft ])
        equal 1 updatedGame

    [<Test>]
    let ``map rightarrow to moveright event`` () =
        let mapper = mapEvents updateFunc

        mapper 1 ({key = KeyCodes.RightArrow |> float } |> KeyDown) |> ignore

        equal true (events = [ MoveRight ])
        equal 1 updatedGame

    [<Test>]
    let ``map spacebar to shoot event`` () =
        let mapper = mapEvents updateFunc

        mapper 1 ({key = KeyCodes.Spacebar |> float } |> KeyDown) |> ignore

        equal true (events = [ Shoot ])
        equal 1 updatedGame

    [<Test>]
    let ``update returns the updated game`` () =
        let mapper = mapEvents updateFunc

        let newGame = mapper 1 ({key = KeyCodes.Spacebar |> float } |> KeyDown)

        equal newGame updatedGame

    [<Test>]
    let ``do nothing if the key isn't mapped`` () =

        let mapper = mapEvents updateFunc

        let newGame = mapper 1 ({key = 1.} |> KeyDown)

        List.isEmpty events |> equal true
        equal initialGame updatedGame

    [<Test>]
    let ``map keyup LeftArrow to StopMoveLeft event`` () =
        let mapper = mapEvents updateFunc

        mapper 1 ({key = KeyCodes.LeftArrow |> float } |> KeyUp) |> ignore

        equal true (events = [ StopMoveLeft ])

    [<Test>]
    let ``map keyup RightArrow to StopMoveRight event`` () =
        let mapper = mapEvents updateFunc

        mapper 1 ({key = KeyCodes.RightArrow |> float } |> KeyUp) |> ignore

        equal true (events = [ StopMoveRight ])

    [<Test>]
    let ``map keyup Spacebar to ...nothing`` () =
        let mapper = mapEvents updateFunc

        mapper 1 ({key = KeyCodes.Spacebar |> float } |> KeyUp) |> ignore

        List.isEmpty events |> equal true

    [<Test>]
    let ``tick maps to an update`` () =
        let mapper = mapEvents updateFunc

        let tickEvent = Tick 3.
        mapper 1 tickEvent |> ignore

        (events = [ Update 3.] ) |> equal true
