module SpaceInvaders.Presentation

open SpaceInvaders.Game
open SpaceInvaders.Image

let presenter (renderer:(list<Image<'HTMLImage>> -> unit)) (imageLookup:(EntityProperties -> 'HTMLImage)) (game:Game) =

    let positionImage (image:'HTMLImage) (entity:Entity) =
        {Image = image;
         Position = {X = entity.Position.X;
                     Y = entity.Position.Y}}

    let imagesToDraw = game.Entities
                       |> List.map (fun entity ->
                                     imageLookup entity.Properties
                                     |> positionImage <| entity )

    renderer imagesToDraw
