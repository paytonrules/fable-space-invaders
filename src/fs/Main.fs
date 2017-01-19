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

let bullet = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAAECAYAAABP2FU6AAAAAXNSR0IArs4c6QAAABJJREFUCB1jYPjJ8J+JAQgQBAAhwgH/34iA1wAAAABJRU5ErkJggg=="
let laser = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA0AAAAICAYAAAAiJnXPAAAAAXNSR0IArs4c6QAAADVJREFUGBljYMAFfjL8xyXFiFUCWQM7A4YaJqyaCAhCTEE2mYAGBqDNjAykaIAaSJbz6KcJAG27B/w+8iEEAAAAAElFTkSuQmCC"
let largeClosedInvader = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAwAAAAICAYAAADN5B7xAAAAAXNSR0IArs4c6QAAAFNJREFUGBmVj1EKACAIQ133v7OptFDro4Kg2fQ5iIja3Ue1SAGw//zhyjzVVBxJeDOe3atxpAGBzyvExL7SLyEyOCXnIOVWK6G78aYPAk3MRgrrEypWKAkFEy6iAAAAAElFTkSuQmCC"
let largeOpenInvader = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAwAAAAICAYAAADN5B7xAAAAAXNSR0IArs4c6QAAAFZJREFUGBmVT9EOwEAE0/3/P5vKKsge7iQSpVVgZh5Z4T6gAagZC6LgTNJgNEAxjtmf8GkL0r6fkBv3SbcO+YNc9ItcNiavTupD1swtpKCe1pDNv9DCF4u9KgKqZWdwAAAAAElFTkSuQmCC"
let mediumClosedInvader = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAsAAAAICAYAAAAvOAWIAAAAAXNSR0IArs4c6QAAAFdJREFUGBltjwEKACAMArfo/19eObhh0CAydUoZEXVPVFVkpuAzzi8UGSX4uFG8oibZjY5p7GQSITHyHv2Ctxvn5+5kEj56U+jbDZDOefF8UAYELTlm+QA9CSUOksh/tAAAAABJRU5ErkJggg=="
let mediumOpenInvader = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAsAAAAICAYAAAAvOAWIAAAAAXNSR0IArs4c6QAAAFVJREFUGBmNkAEKwDAIA6Ps/1+2vY2AHUwmlEZNNG1Iqn1UVYoI4BG9niQERGOzOxF89QZ4EmQnvG0494DY4PHhFcOdVg+cuwWP5///DWQ4+dpA3U4X0SkzBBbb4o4AAAAASUVORK5CYII="
let smallClosedInvader = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAExJREFUGBltjgEOwCAMAkv//+dOam4hy0wUhIKqqubsXTOXSkKqhmH6nnwHUvgGGjNr4fZ0jvcPpBObadAm3LgNCJk0d/k2/L1izcEHlGwnBb7DH5AAAAAASUVORK5CYII="
let smallOpenInvader = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAAXNSR0IArs4c6QAAAE1JREFUGBldjgEOACAIAqX//9mEorHaikvUgarquTrdBwG4VMtkk/9kNWThH1g2c62ZHuZ5GTydyjRqUPcNl/wycC0Nm1SdARJFmszaBqswQODqZQ35AAAAAElFTkSuQmCC"


let images = [
    LargeInvaderClosed, createImage largeClosedInvader;
    MediumInvaderOpen, createImage mediumOpenInvader;
    MediumInvaderClosed, createImage mediumClosedInvader;
    SmallInvaderOpen, createImage smallOpenInvader;
    SmallInvaderClosed, createImage smallClosedInvader;
    LargeInvaderOpen, createImage largeOpenInvader;
    Presentation.Image.Bullet, createImage bullet;
    Presentation.Image.Laser, createImage laser;] |> Map

let main() =
    let renderFunc = presenter createRenderer images

    let eventHandler = createGameEventHandler (mapEvents update) initialGame

    document.addEventListener_keydown(fun e -> 
        eventHandler ({key = e.keyCode } |> KeyDown) |> ignore
        null)

    document.addEventListener_keydown(fun e -> 
        eventHandler ({key = e.keyCode } |> KeyUp) |> ignore
        null)

    do start Browser.window.requestAnimationFrame eventHandler renderFunc (Browser.performance.now()) |> ignore

do
    main ()