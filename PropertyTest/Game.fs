module PropertyTests

open SpaceInvaders
open FsCheck
open FsCheck.Xunit
open System

[<Property>]
let ``the number of invaders is always the same or lower after update`` (game : Game, delta : Delta) =
    let invaderCount = game.Entities
                       |> Invasion.invadersFrom
                       |> List.length

    let newGame = Game.update game (Update delta)

    let updatedInvaderCount = newGame.Entities
                              |> Invasion.invadersFrom
                              |> List.length

    updatedInvaderCount <= invaderCount

type GameWithLaser =
    static member GameWithLaser() =
        Arb.Default.Derive()
        |> Arb.filter (fun g -> Game.findLaser g.Entities <> None)

type GameWithLaserPropertyAttribute() =
    inherit PropertyAttribute(Arbitrary = [| typeof<GameWithLaser> |])


[<GameWithLaserProperty>]
let ``the laser is never removed`` (game : Game, delta : Delta) =
    let newGame = Game.update game (Update delta)

    let laser = Game.findLaser game.Entities

    laser <> None

let ``invaders never go beyond the left side`` () =
    ignore

let ``invaders never go down twice in a row`` () =
    ignore

let ``nothing is ever deleted when there are no bullets`` () =
    ignore
