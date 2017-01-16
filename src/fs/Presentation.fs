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

    let lookupImage someEntity destructor keyLookup =
        someEntity 
        |> Option.map destructor
        |> someEntityToImage
        |> Option.bind <| Map.tryFind keyLookup lookupTable

    let laserImage = lookupImage (Some(game.Laser)) (function | SpaceInvaders.Game.Laser e -> e) Laser
    let bulletImage = lookupImage game.Bullet (function | SpaceInvaders.Game.Bullet e -> e) Bullet
    let invaderImages = 
        game.Invaders
        |> List.map (fun i -> lookupImage (Some(i))
                                          (function | SpaceInvaders.Game.Invader e -> fst e) 
                                          LargeInvaderOpen)

    invaderImages @ [laserImage; bulletImage] |> List.choose id |> renderer