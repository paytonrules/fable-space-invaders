module SpaceInvaders.GameLoop
open System

type Delta = float
type KeyCode = int
type KeyDown = 
    {
        key: KeyCode
    }

type KeyUp = 
    {
        key: KeyCode
    }

type Event =
    | Tick of Delta
    | KeyDown of KeyDown

type Update<'Game> = 'Game -> Event -> 'Game
type Render<'Game> = 'Game -> unit
type EventHandler<'Game> = Event -> 'Game

let createGameEventHandler update initialGame :EventHandler<'Game> =
    let mutable currentState = initialGame
    (fun event -> 
        currentState <- (update currentState event)
        currentState)

let convertToTick delta =
    delta |> Tick
let start requestFrame eventHandler renderer now = 
    let rec step delta =
        delta |> convertToTick |> eventHandler |> renderer |> ignore
        requestFrame step |> ignore

    step(now)