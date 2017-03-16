module Presentation

open SpaceInvaders
open Image

let presenter (renderer:(list<Image<'HTMLImage>> -> unit)) (imageLookup:(EntityProperties -> 'HTMLImage)) (game:Game) =

    let positionImage (image:'HTMLImage) (entity:Entity) =
        {Image = image;
         Position = {X = entity.Position.X;
                     Y = entity.Position.Y}}

    let imagesToDraw = game.Entities
                       |> List.map (fun entity ->
                                     imageLookup entity.Properties
                                     |> positionImage <| entity )

    let laserImage = game.Laser |> Option.map (fun laser ->
                                                imageLookup laser.Properties
                                                |> positionImage <| laser)

    let images = match laserImage with
                 | Some image -> List.append imagesToDraw [image]
                 | None -> imagesToDraw

    renderer images
