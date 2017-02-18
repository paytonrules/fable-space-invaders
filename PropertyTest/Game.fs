module PropertyTests

open SpaceInvaders.Game
open FsCheck
open FsCheck.Xunit
open System

[<Property>]
let ``the number of invaders is always the same or lower after update`` (game : Game, delta : Delta) =
    let invaderCount = game.Entities
                       |> Invasion.invadersFrom
                       |> List.length

    let newGame = update game (Update delta)

    let updatedInvaderCount = newGame.Entities
                              |> Invasion.invadersFrom
                              |> List.length

    updatedInvaderCount <= invaderCount

type GameWithLaser =
    static member GameWithLaser() =
        Arb.Default.Derive()
        |> Arb.filter (fun g -> findLaser g.Entities <> None)

type GameWithLaserPropertyAttribute() =
    inherit PropertyAttribute(Arbitrary = [| typeof<GameWithLaser> |])


[<GameWithLaserProperty>]
let ``the laser is never removed`` (game : Game, delta : Delta) =
    let newGame = update game (Update delta)

    let laser = findLaser game.Entities

    laser <> None

let ``invaders never go beyond the left side`` () =
    ignore

let ``invaders never go down twice in a row`` () =
    ignore

let ``nothing is ever deleted when there are no bullets`` () =
    ignore


module Diamond =
    let make letter =
        let mirrorAndFuse l =
            l @ (l |> List.rev |> List.tail)

        let makeLine letterCount (letter, letterIndex) =
            let leadingSpaces = String(' ', letterCount - 1 - letterIndex)
            let innerSpace = String(' ', letterCount - 1 - leadingSpaces.Length)
            let left = sprintf "%s%c%s" leadingSpaces letter innerSpace
                       |> Seq.toList
            left
            |> mirrorAndFuse
            |> List.map string
            |> List.reduce (sprintf "%s%s")

        let indexedLetters = ['A' .. letter] |> List.mapi (fun i l -> l, i)
        indexedLetters
        |> mirrorAndFuse
        |> List.map (makeLine (List.length indexedLetters))
        |> List.reduce (fun x y -> sprintf "%s%s%s" x Environment.NewLine y)

type Letters =
    static member Char() =
        Arb.Default.Char()
        |> Arb.filter (fun c -> 'A' <= c && c <= 'Z')

type DiamondPropertyAttribute() =
    inherit PropertyAttribute(Arbitrary = [| typeof<Letters> |])

[<DiamondProperty>]
let ``Diamond is not empty`` (letter : char) =
    let actual = Diamond.make letter

    not (String.IsNullOrWhiteSpace actual)

let split (x : string) =
    x.Split([| Environment.NewLine |], StringSplitOptions.None)

let trim (x : string) = x.Trim()

[<DiamondProperty>]
let ``first row contains 'A'`` (letter : char) =
    let actual = Diamond.make letter

    let rows = split actual
    rows |> Seq.head |> trim = "A"

let leadingSpaces (x : string) =
    let indexOfNonSpace = x.IndexOfAny([|'A' .. 'Z' |])
    x.Substring(0, indexOfNonSpace)

let trailingSpaces (x : string) =
    let lastIndexOfNonSpace = x.LastIndexOfAny([|'A' .. 'Z'|])
    x.Substring(lastIndexOfNonSpace + 1)

[<DiamondProperty>]
let ``All rows must have asymmetric contour`` (letter : char) =
    let actual = Diamond.make letter

    let rows = split actual
    rows |> Array.forall (fun r -> (leadingSpaces r) = (trailingSpaces r))

[<DiamondProperty>]
let ``Top of figure has correct letters in correct order`` (letter : char) =
    let actual = Diamond.make letter

    let expected = ['A' .. letter]
    let rows = split actual
    let firstNonWhiteSpaceLetters =
        rows
        |> Seq.take expected.Length
        |> Seq.map (trim >> Seq.head)
        |> Seq.toList

    expected = firstNonWhiteSpaceLetters

[<DiamondProperty>]
let ``figure is symmetric around the horizontal axis`` (letter : char) =
    let actual = Diamond.make letter

    let rows = split actual
    let topRows =
        rows
        |> Seq.takeWhile (fun x -> not(x.Contains (string letter)))
        |> Seq.toList

    let bottomRows =
        rows
        |> Seq.skipWhile (fun x -> not(x.Contains (string letter)))
        |> Seq.skip 1
        |> Seq.toList
        |> List.rev

    topRows = bottomRows

[<DiamondProperty>]
let ``diamond is as wide as it is high`` (letter : char) =
    let actual = Diamond.make letter

    let rows = split actual
    let expected = Array.length rows

    rows |> Array.forall (fun str -> String.length str = expected)

[<DiamondProperty>]
let ``all rows except top and bottom have two identitical letters`` (letter : char) () =
    let actual = Diamond.make letter

    let isTwoIdenticalLetters x =
        let hasIdenticalLetters = x |> Seq.distinct |> Seq.length = 1
        let hasTwoLetters = x |> Seq.length = 2

        hasTwoLetters && hasIdenticalLetters

    let rows = split actual
    rows
    |> Array.filter (fun x -> not (x.Contains("A")))
    |> Array.map (fun x -> x.Replace(" ", ""))
    |> Array.forall isTwoIdenticalLetters

[<DiamondProperty>]
let ``lower left space is a triangle`` (letter : char) =
    let actual = Diamond.make letter

    let rows = split actual

    let lowerLeftSpace =
        rows
        |> Seq.skipWhile (fun x -> not (x.Contains(string letter)))
        |> Seq.map leadingSpaces
        |> Seq.toList
    let spaceCounts = lowerLeftSpace
                      |> List.map (fun x -> x.Length)
    let expected =
        Seq.initInfinite id
        |> Seq.take spaceCounts.Length
        |> Seq.toList

    expected = spaceCounts
