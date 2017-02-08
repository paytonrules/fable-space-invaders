
#I @"../packages/"
#load @"../src/fs/Constraints.fs"
#load @"../src/fs/Game.fs"
#r "FsCheck/lib/net45/FsCheck.dll"

open SpaceInvaders.Game
open FsCheck

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

type GameUpdateProperties = 
  static member ``laser is never removed`` (game:Game) = 
    true

Check.QuickAll<GameUpdateProperties>()