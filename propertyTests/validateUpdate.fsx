
#I @"../packages/"
#load @"../src/fs/Constraints.fs"
#load @"../src/fs/Game.fs"
#r "FsCheck/lib/net45/FsCheck.dll"

#r "xunit.extensibility.core/lib/portable-net45+win8+wp8+wpa81/xunit.core.dll"
#r "xunit.abstractions/lib/portable-net45+win+wpa81+wp80+monotouch+monoandroid+Xamarin.iOS/xunit.abstractions.dll"
#r "FsCheck.Xunit/lib/net45/FsCheck.Xunit.dll"

open SpaceInvaders.Game
open FsCheck
open System
open FsCheck.Xunit



module Diamond = 
    let make letter = "Devil's advocate."

[<Property>]
let ``Diamond is not empty`` (letter : char) =
    printfn "hello"
    let actual = Diamond.make letter

    not (String.IsNullOrWhiteSpace actual)

(*
let invasion = {
    SinceLastMove = 0.;
    TimeToMove = 100.;
    Direction = Invasion.Direction.Down;
}

let game = {
    Entities = [];
    LastUpdate = 0.;
    Invasion = invasion;

}

let gameWithLaser game = 
    findLaser game.Entities <> None

let laserIsNotRemoved game delta =
    let updatedGame = update game (Update delta)

    updatedGame.Entities |> findLaser <> None

type GameUpdateProperties = 
  static member ``laser is never removed on update`` (game:Game) (delta:Delta) = 
    gameWithLaser game ==> (lazy laserIsNotRemoved)

  // Never add a Laser
  // Never go down twice in a row
  // Don't skip any collisions
  // Don't keep the bullet on after a collision
  // Don't ever have laser/bullet collision?
  // Shoot always creates a bullet when one is not present
  // Moving?

let badGame = {Entities = [{Position = {X = 0.0;
                           Y = 0.0;};
               Bounds = {Width = 0;
                         Height = 0;};
               Properties = Laser {LeftForce = false;
                                   RightForce = false;};}];
  LastUpdate = 1.0;
  Invasion = {TimeToMove = 0.0;
              SinceLastMove = 1.0;
              Direction = {X = 0.0;
                           Y = 1.0;};};}

let updatedBadGame = update badGame (Update 0.)

printf "UPDATE %O" updatedBadGame.Entities

*)