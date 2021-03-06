module Engine.EventMapping

open SpaceInvaders
open FSharp.Core.LanguagePrimitives

type KeyCodes =
| LeftArrow = 37
| RightArrow = 39
| Spacebar = 32

let keyDown updateFunc game key =
    match EnumOfValue (key |> int) with
    | KeyCodes.LeftArrow -> updateFunc game MoveLeft
    | KeyCodes.RightArrow -> updateFunc game MoveRight
    | KeyCodes.Spacebar -> updateFunc game Shoot
    | _ -> game

let keyUp updateFunc game key =
    match EnumOfValue (key |> int) with
    | KeyCodes.LeftArrow -> updateFunc game StopMoveLeft
    | KeyCodes.RightArrow -> updateFunc game StopMoveRight
    | _ -> game

let mapEvents updateFunc =
    let keyDown' = keyDown updateFunc
    let keyUp' = keyUp updateFunc

    (fun game event ->
      match event with
      | GameLoop.KeyDown e -> keyDown' game e.key
      | GameLoop.KeyUp e -> keyUp' game e.key
      | GameLoop.Tick e -> updateFunc game (Update e))
