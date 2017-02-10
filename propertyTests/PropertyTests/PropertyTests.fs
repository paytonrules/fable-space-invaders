module PropertyTests

open SpaceInvaders.Game
open FsCheck
open FsCheck.Xunit
open System

module Diamond = 
    let make letter = "Devil's advocate."

[<Property>]
let ``Diamond is not empty`` (letter : char) =
    let actual = Diamond.make letter

    not (String.IsNullOrWhiteSpace actual)