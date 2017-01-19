module SpaceInvaders.EventMapping

open SpaceInvaders.GameLoop
open SpaceInvaders.Game
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

let mapEvents updateFunc = 
    let keyDown' = keyDown updateFunc
    (fun game event -> 
        match event with
        | KeyDown e -> keyDown' game e.key 
        | _ -> game)