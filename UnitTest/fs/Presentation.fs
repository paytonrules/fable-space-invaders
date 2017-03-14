module Test.Presentation

open Fable.Core.Testing

open Presentation
open Image
open SpaceInvaders

[<TestFixture>]
module Presentation =
    let mutable renderedImages:Image<string> list = []
    let renderer images =
        renderedImages <- images

    let lookupImage = function
    | EntityProperties.Laser _ -> "laser"
    | EntityProperties.Bullet _ -> "bullet"
    | EntityProperties.RandomMissile _ -> "randommissile"
    | EntityProperties.TrackingMissile _ -> "trackingmissile"
    | EntityProperties.PoweredMissile _ -> "poweredmissile"
    | Invader e -> "invader"

    let presenter' = presenter renderer lookupImage

    let createMissile t = {
        Position = { X = 0.; Y = 0.}
        Bounds = { Width = 0; Height = 0}
        Properties = { Velocity = { X = 0.; Y = 0. } } |> MissileProperties |> t
    }

    [<SetUp>]
    let setup () =
        renderedImages <- []

    [<Test>]
    let ``every kind of entity gets drawn from the lookup table`` () =
        let entities = [
            Laser.create { X = 0.; Y = 0.};
            Bullet.createWithDefaultProperties { X = 0.; Y = 0.};
            createMissile RandomMissile;
            createMissile TrackingMissile;
            createMissile PoweredMissile;
            Invader.create ({X = 0.; Y = 0.}, Small)
        ]

        let game = Game.createGame entities
        presenter' game |> ignore

        // This is fairly icky - this will throw an exception if any of these
        // are not rendered in the presenter function.
        let images = ["laser"; "bullet"; "randommissile";
                      "trackingmissile"; "poweredmissile"; "invader"];
        let findImage image =
            List.find (fun x -> x.Image = image) renderedImages |> ignore

        List.iter findImage images

    [<Test>]
    let ``it draws the entities at the position in the entity`` () =
        let bullet =  Bullet.createWithDefaultProperties {X = 30.; Y = 40.}
        let laser = Laser.create { X = 10.; Y = 5. }

        let game = Game.createGame [ bullet; laser ]

        presenter' game |> ignore

        let bulletImage = renderedImages |> List.find (fun x -> x.Image = "bullet")
        equal bulletImage.Position {X = 30.; Y = 40.}

        let laserImage = renderedImages |> List.find (fun x -> x.Image = "laser")
        equal laserImage.Position {X = 10.; Y = 5.}
