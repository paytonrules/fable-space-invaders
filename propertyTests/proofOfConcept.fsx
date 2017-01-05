
#I @"../packages/"
#load @"../src/fs/Game.fs"
#r "FsCheck/lib/net45/FsCheck.dll"

open SpaceInvaders.Game
open FsCheck

let revRevIsOrig (xs:list<int>) = List.rev(List.rev xs) = xs



