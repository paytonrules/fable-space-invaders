namespace Test

open Fable.Core.Testing
open SpaceInvaders.GameLoop

[<TestFixture>]
module GameLoop =
    let equal expected actual = 
        Assert.AreEqual(expected, actual)

    [<Test>]
    let ``calls update with the state and delta`` () =
        let delta = 2
        let gameState = 1
        let update state delta = state * delta
        let stepFunc' = step update
        stepFunc' gameState List.empty delta |> equal 2

    [<Test>]
    let ``runs one event before calling the update`` () =
        let gameState = 1
        let update state delta = state * 2
        let event state delta = state + 1
        let stepFunc' = step update
        stepFunc' gameState [event] 1 |> equal 3

    [<Test>]
    let ``runs all events in reverse (unwind) order before calling the update`` () =
        let delta = 2
        let gameState = 1
        let update state delta = state / 7
        let eventOne state delta = state * 8
        let eventTwo state delta = state - 1
        let stepFunc' = step update
        stepFunc' gameState [eventOne; eventTwo] 1 |> equal -1

    [<Test>]
    let ``game loop runs no steps when check fails`` () =
        let mutable stepTaken = false
        let gameState = 0
        let checkStop _ = false
        let stepFunc gameState delta = 
            stepTaken <- true
            gameState
        let renderer = ignore
        start stepFunc checkStop renderer gameState ()
        equal stepTaken false

    [<Test>]
    let ``game loop runs the step once when check fails after one try`` () = 
        let mutable stepsTaken = 0
        let stepFunc gameState delta = 
            stepsTaken <- stepsTaken + 1
            gameState
        let checkStop = function
        | 0 -> false
        | 1 -> true
        | _ -> false
        let renderer = ignore
        let gameState = 0
        start stepFunc checkStop renderer gameState ()
        equal stepsTaken 1




        

    