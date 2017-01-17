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
    let lookupImageKey = function
    | EntityType.Laser _ -> Laser
    | EntityType.Bullet _ -> Bullet
    | EntityType.Invader e -> match (e.Type, e.InvaderState) with 
                              | Large, Open -> LargeInvaderOpen
                              | Large, Closed ->  LargeInvaderClosed
                              | Medium, Open -> MediumInvaderOpen
                              | Medium, Closed -> MediumInvaderClosed
                              | Small, Open -> SmallInvaderOpen
                              | Small, Closed -> SmallInvaderClosed

    let lookupImage imageKey = Map.tryFind imageKey lookupTable

    let positionEntity image (entity:Entity) = 
        {Image = image; 
         Position = {X = entity.Position.X; 
                     Y = entity.Position.Y}}

    let positionImage image entity =
        match entity with 
        | EntityType.Laser e -> positionEntity image e.Entity
        | EntityType.Bullet e -> positionEntity image e.Entity
        | EntityType.Invader e -> positionEntity image e.Entity

    let imagesToDraw = game.Entities
                       |> List.map (fun entity ->
                                        lookupImageKey entity
                                        |> lookupImage 
                                        |> Option.map (fun img -> positionImage img entity))

    imagesToDraw |> List.choose id |> renderer