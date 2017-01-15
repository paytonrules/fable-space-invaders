namespace Test

open Fable.Core.Testing
open SpaceInvaders.GameLoop

[<TestFixture>]
module GameLoop =
    let equal expected actual = 
        Assert.AreEqual(expected, actual)
    let mutable gameAsListOfEvents = []
    let updateFunc game event =
        gameAsListOfEvents <- event :: game
        gameAsListOfEvents
    let mutable callbacks = []
    let requestFrame cb = 
        callbacks <- cb :: callbacks
        ()

    [<SetUp>]
    let setUp () =
        callbacks <- []
        gameAsListOfEvents <- []

    [<Test>]
    let ``creates an updater that encapsulates updating the game state`` () =
        let event = 10. |> Tick
        let eventHandler = createGameEventHandler updateFunc gameAsListOfEvents 
        let currentState = eventHandler event

        equal true ([event] = currentState)

    [<Test>]
    let ``updater uses the initial game`` () =
        let event = 10. |> Tick
        let updatedState = event :: gameAsListOfEvents
        let eventHandler = createGameEventHandler updateFunc updatedState 
        let currentState = eventHandler event

        equal true ([event; event] = currentState)

    [<Test>]
    let ``updater keeps track of all the events`` () =
        let eventOne = 10. |> Tick
        let eventTwo = 20. |> Tick
        let eventHandler = createGameEventHandler updateFunc gameAsListOfEvents

        eventHandler eventOne |> ignore
        let finalState = eventHandler eventTwo

        equal true ([eventTwo; eventOne;] = finalState)

    [<Test>]
    let ``requests a frame immediately on being called`` () =
        start requestFrame ignore ignore 0.

        List.length callbacks |> equal 1

    [<Test>]
    let ``request frame will (eventually) request another frame`` () =
        start requestFrame ignore ignore 0.
        List.head callbacks <| 1.

        List.length callbacks |> equal 2

    [<Test>]
    let ``when the loop is started the update is called with 'now'`` () =
        let eventHandler = createGameEventHandler updateFunc gameAsListOfEvents 
        let start' = start requestFrame eventHandler ignore

        start' 35.

        let expectedEvent = 35. |> Tick
        equal true ([expectedEvent] = gameAsListOfEvents)

    [<Test>]
    let ``the result of the event handler is rendered`` () =
        let mutable renderedGame = [] 
        let renderer game =
            renderedGame <- game
        let eventHandler = createGameEventHandler updateFunc gameAsListOfEvents 

        let start' = start requestFrame eventHandler renderer
        start' 10.

        let expectedGame = Tick 10.
        equal true ([expectedGame] = renderedGame)



    