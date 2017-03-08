module SpaceInvaders.Main

open System
open SpaceInvaders.Window
open SpaceInvaders.GameLoop
open SpaceInvaders.Presentation
open SpaceInvaders.EventMapping
open SpaceInvaders.Game
open Fable.Core
open Fable.Import
open Fable.Import.Browser

let bullet = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAAECAYAAABP2FU6AAAAAXNSR0IArs4c6QAAABJJREFUCB1jYPjJ8J+JAQgQBAAhwgH/34iA1wAAAABJRU5ErkJggg==" |> createImage
let laser = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA0AAAAICAYAAAAiJnXPAAAAAXNSR0IArs4c6QAAADVJREFUGBljYMAFfjL8xyXFiFUCWQM7A4YaJqyaCAhCTEE2mYAGBqDNjAykaIAaSJbz6KcJAG27B/w+8iEEAAAAAElFTkSuQmCC" |> createImage
let largeClosedInvader = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAwAAAAICAYAAADN5B7xAAAAAXNSR0IArs4c6QAAAFNJREFUGBmVj1EKACAIQ133v7OptFDro4Kg2fQ5iIja3Ue1SAGw//zhyjzVVBxJeDOe3atxpAGBzyvExL7SLyEyOCXnIOVWK6G78aYPAk3MRgrrEypWKAkFEy6iAAAAAElFTkSuQmCC" |> createImage
let largeOpenInvader = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAwAAAAICAYAAADN5B7xAAAAAXNSR0IArs4c6QAAAFZJREFUGBmVT9EOwEAE0/3/P5vKKsge7iQSpVVgZh5Z4T6gAagZC6LgTNJgNEAxjtmf8GkL0r6fkBv3SbcO+YNc9ItcNiavTupD1swtpKCe1pDNv9DCF4u9KgKqZWdwAAAAAElFTkSuQmCC" |> createImage
let mediumClosedInvader = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAsAAAAICAYAAAAvOAWIAAAAAXNSR0IArs4c6QAAAFdJREFUGBltjwEKACAMArfo/19eObhh0CAydUoZEXVPVFVkpuAzzi8UGSX4uFG8oibZjY5p7GQSITHyHv2Ctxvn5+5kEj56U+jbDZDOefF8UAYELTlm+QA9CSUOksh/tAAAAABJRU5ErkJggg==" |> createImage
let mediumOpenInvader = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAsAAAAICAYAAAAvOAWIAAAAAXNSR0IArs4c6QAAAFVJREFUGBmNkAEKwDAIA6Ps/1+2vY2AHUwmlEZNNG1Iqn1UVYoI4BG9niQERGOzOxF89QZ4EmQnvG0494DY4PHhFcOdVg+cuwWP5///DWQ4+dpA3U4X0SkzBBbb4o4AAAAASUVORK5CYII=" |> createImage
let smallClosedInvader = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAExJREFUGBltjgEOwCAMAkv//+dOam4hy0wUhIKqqubsXTOXSkKqhmH6nnwHUvgGGjNr4fZ0jvcPpBObadAm3LgNCJk0d/k2/L1izcEHlGwnBb7DH5AAAAAASUVORK5CYII=" |> createImage
let smallOpenInvader = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAE1JREFUGBldjgEOACAIAqX//9mEorHaikvUgarquTrdBwG4VMtkk/9kNWThH1g2c62ZHuZ5GTydyjRqUPcNl/wycC0Nm1SdARJFmszaBqswQODqZQ35AAAAAElFTkSuQmCC" |> createImage
let plungerFrameOne = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAMAAAAGCAYAAAAG5SQMAAAAAXNSR0IArs4c6QAAABtJREFUCB1jYGBg+A8CIJoJSMABkRxGqGawPgDUAgoANXcezgAAAABJRU5ErkJggg==" |> createImage
let plungerFrameTwo = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAMAAAAGCAYAAAAG5SQMAAAAAXNSR0IArs4c6QAAACZJREFUCB1j+A8EDAwMIOo/E5ABBygcRqAwSBkYMEG0gDUxoCgDABu9D/oMcWZiAAAAAElFTkSuQmCC" |> createImage
let plungerFrameThree = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAMAAAAGCAYAAAAG5SQMAAAAAXNSR0IArs4c6QAAACVJREFUCB1j+A8EDAwMIOo/E5ABB4xAFkgGDJggqsDqGFCUoXAAdlUP+oWfm1YAAAAASUVORK5CYII=" |> createImage
let plungerFrameFour = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAMAAAAGCAYAAAAG5SQMAAAAAXNSR0IArs4c6QAAAB1JREFUCB1jZGBg+A/EYMD0/z+EDaKZYKIgmkgOAKyFBwWZ11cdAAAAAElFTkSuQmCC" |> createImage

let lookupImage = function
| EntityProperties.Laser _ -> laser
| EntityProperties.Bullet _ -> bullet
| EntityProperties.RandomMissile _ -> plungerFrameOne
| EntityProperties.TrackingMissile _ -> plungerFrameTwo
| EntityProperties.PoweredMissile _ -> plungerFrameThree
| Invader e -> match (e.Type, e.InvaderState) with
               | Large, Open -> largeOpenInvader
               | Large, Closed ->  largeClosedInvader
               | Medium, Open -> mediumOpenInvader
               | Medium, Closed -> mediumClosedInvader
               | Small, Open -> smallOpenInvader
               | Small, Closed -> smallClosedInvader
let main() =
    let renderFunc = presenter createRenderer lookupImage

    let eventHandler = createGameEventHandler (mapEvents update) initialGame

    document.addEventListener_keydown(fun e ->
        eventHandler ({key = e.keyCode } |> KeyDown) |> ignore
        null)

    document.addEventListener_keyup(fun e ->
        eventHandler ({key = e.keyCode } |> KeyUp) |> ignore
        null)

    do start Browser.window.requestAnimationFrame eventHandler renderFunc |> ignore

do
    main ()
