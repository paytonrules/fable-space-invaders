module SpaceInvaders.Presentation

open Fable.Core
open Fable.Import

open SpaceInvaders.Game
open SpaceInvaders.Image

type Image = 
| LargeInvaderOpen 
| MediumInvaderOpen 
| SmallInvaderOpen 
| LargeInvaderClosed 
| MediumInvaderClosed 
| SmallInvaderClosed
| Bullet 
| Laser

type EmailAddress = EmailAddress of string

let presenter (renderer:(list<Image<'HTMLImage>> -> unit)) (lookupTable:Map<Image, 'HTMLImage>) (game:Game) = 
    let entityToImage (entity: Entity) image = 
        { Image = image;
          Position = { X = entity.Position.X;
                       Y = entity.Position.Y;}}

    let someEntityToImage (entity:Entity option) image =
        entity |> Option.map (fun e -> entityToImage e image)

    let laserImage =
        Some(game.Laser)
        |> Option.map (function | SpaceInvaders.Game.Laser e -> e)
        |> someEntityToImage
        |> Option.bind <| Map.tryFind Laser lookupTable

    let bulletImage = 
        game.Bullet
        |> Option.map (function | SpaceInvaders.Game.Bullet e -> e)
        |> someEntityToImage
        |> Option.bind <| Map.tryFind Bullet lookupTable

    let invaderImages = 
        game.Invaders
        |> List.map (fun invader -> 
                        Some(invader)
                        |> Option.map (function | SpaceInvaders.Game.Invader e -> e)
                        |> Option.map fst
                        |> someEntityToImage
                        |> Option.bind <| Map.tryFind LargeInvaderOpen lookupTable)

    invaderImages @ [laserImage; bulletImage] |> List.choose id |> renderer
