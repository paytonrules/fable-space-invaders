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

let presenter (renderer:(list<Image<'HTMLImage>> -> unit)) (lookupTable:Map<Image, 'HTMLImage>) (game:Game) =
    let lookupImageKey = function
    | EntityProperties.Laser _ -> Laser
    | EntityProperties.Bullet _ -> Bullet
    | Invader e -> match (e.Type, e.InvaderState) with
                              | Large, Open -> LargeInvaderOpen
                              | Large, Closed ->  LargeInvaderClosed
                              | Medium, Open -> MediumInvaderOpen
                              | Medium, Closed -> MediumInvaderClosed
                              | Small, Open -> SmallInvaderOpen
                              | Small, Closed -> SmallInvaderClosed

    let lookupImage imageKey = Map.tryFind imageKey lookupTable

    let positionImage image (entity:Entity) =
        {Image = image;
         Position = {X = entity.Position.X;
                     Y = entity.Position.Y}}

    let imagesToDraw = game.Entities
                       |> List.map (fun entity ->
                                        lookupImageKey entity.Properties
                                        |> lookupImage
                                        |> Option.map (fun img -> positionImage img entity))

    imagesToDraw |> List.choose id |> renderer
