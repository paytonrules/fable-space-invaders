module Test.EventMapping

open Fable.Core.Testing
open Engine
open Engine.GameLoop

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
        let mapper = EventMapping.mapEvents updateFunc

        mapper 1 ({key = EventMapping.KeyCodes.LeftArrow |> float }
                  |> KeyDown) |> ignore

        equal true (events = [ SpaceInvaders.MoveLeft ])
        equal 1 updatedGame

    [<Test>]
    let ``map rightarrow to moveright event`` () =
        let mapper = EventMapping.mapEvents updateFunc

        mapper 1 ({key = EventMapping.KeyCodes.RightArrow |> float }
                  |> KeyDown) |> ignore

        equal true (events = [ SpaceInvaders.MoveRight ])
        equal 1 updatedGame

    [<Test>]
    let ``map spacebar to shoot event`` () =
        let mapper = EventMapping.mapEvents updateFunc

        mapper 1 ({key = EventMapping.KeyCodes.Spacebar |> float }
                  |> KeyDown) |> ignore

        equal true (events = [ SpaceInvaders.Shoot ])
        equal 1 updatedGame

    [<Test>]
    let ``update returns the updated game`` () =
        let mapper = EventMapping.mapEvents updateFunc

        let newGame = mapper 1 ({key = EventMapping.KeyCodes.Spacebar |> float }
                                |> KeyDown)

        equal newGame updatedGame

    [<Test>]
    let ``do nothing if the key isn't mapped`` () =
        let mapper = EventMapping.mapEvents updateFunc

        let newGame = mapper 1 ({key = 1.} |> KeyDown)

        List.isEmpty events |> equal true
        equal initialGame updatedGame

    [<Test>]
    let ``map keyup LeftArrow to StopMoveLeft event`` () =
        let mapper = EventMapping.mapEvents updateFunc

        mapper 1 ({key = EventMapping.KeyCodes.LeftArrow |> float }
                  |> KeyUp) |> ignore

        equal true (events = [ SpaceInvaders.StopMoveLeft ])

    [<Test>]
    let ``map keyup RightArrow to StopMoveRight event`` () =
        let mapper = EventMapping.mapEvents updateFunc

        mapper 1 ({key = EventMapping.KeyCodes.RightArrow |> float }
                  |> KeyUp) |> ignore

        equal true (events = [ SpaceInvaders.StopMoveRight ])

    [<Test>]
    let ``map keyup Spacebar to ...nothing`` () =
        let mapper = EventMapping.mapEvents updateFunc

        mapper 1 ({key = EventMapping.KeyCodes.Spacebar |> float }
                  |> KeyUp) |> ignore

        List.isEmpty events |> equal true

    [<Test>]
    let ``tick maps to an update`` () =
        let mapper = EventMapping.mapEvents updateFunc

        let tickEvent = Tick 3.
        mapper 1 tickEvent |> ignore

        (events = [ SpaceInvaders.Update 3.] ) |> equal true
