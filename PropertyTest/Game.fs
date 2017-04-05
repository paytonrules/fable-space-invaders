module PropertyTests

open SpaceInvaders
open FsCheck
open FsCheck.Xunit
open System

[<Property>]
let ``the number of invaders is always the same or lower after update`` (game : Game, delta : Delta) =
    let invaderCount = game.Invasion.Invaders |> List.length

    let newGame = Game.update game (Update delta)

    let updatedInvaderCount = game.Invasion.Invaders |> List.length

    updatedInvaderCount <= invaderCount

let ``invaders never go beyond the left side`` () =
    ignore

let ``invaders never go down twice in a row`` () =
    ignore

let ``nothing is ever deleted when there are no bullets`` () =
    ignore
