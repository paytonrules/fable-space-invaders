module Presentation

open SpaceInvaders
open Image

let presenter (renderer:(list<Image<'HTMLImage>> -> unit)) (imageLookup:(EntityProperties -> 'HTMLImage)) (game:Game) =

    let positionImage (image:'HTMLImage) (entity:Entity) =
        {Image = image;
         Position = {X = entity.Location.Position.X;
                     Y = entity.Location.Position.Y}}

    let imagesToDraw = game.Invasion.Invaders
                       |> List.map (fun invader ->
                                     let invaderAsEntity = Invader.toEntity invader
                                     imageLookup invaderAsEntity.Properties
                                     |> positionImage <| invaderAsEntity )

    let laserImage = game.Laser |> Option.map (fun laser ->
                                                let laserAsEntity = Laser.toEntity laser
                                                imageLookup laserAsEntity.Properties
                                                |> positionImage <| laserAsEntity)

    let bulletImage = game.Bullet |> Option.map (fun bullet ->
                                                  let bulletAsEntity = Bullet.toEntity bullet
                                                  imageLookup bulletAsEntity.Properties
                                                  |> positionImage <| bulletAsEntity)

    let images' = match laserImage with
                  | Some image -> List.append imagesToDraw [image]
                  | None -> imagesToDraw

    let images = match bulletImage with
                 | Some image -> List.append images' [image]
                 | None -> images'

    renderer images
