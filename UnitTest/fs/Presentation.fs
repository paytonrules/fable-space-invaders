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
    let ``a laser is displayed`` () =
        let laser = Laser.create { X = 0.; Y = 0. }
        let game = Game.createGame <| Some(laser) <| []

        presenter' game |> ignore

        let laserImage = {
            Image = "laser";
            Position = { X = 0.; Y = 0.}
        }
        equal renderedImages [laserImage]

    [<Test>]
    let ``a bullet is displayed`` () =
        let bullet = Bullet.createWithDefaultProperties { X = 0.; Y = 0. }
        let game = Game.createGame None []
        let gameWithBullet = { game with Bullet = Some(bullet) }

        presenter' gameWithBullet |> ignore

        let bulletImage = {
            Image = "bullet";
            Position = { X = 0.; Y = 0.}
        }
        equal renderedImages [bulletImage]

    [<Test>]
    let ``invaders get drawn`` () =
        let invaders = [
            Invader.create ({X = 10.; Y = 20.}, Small);
            Invader.create ({X = 10.; Y = 20.}, Medium);
        ]

        let game = Game.createGame None invaders
        presenter' game |> ignore

        let expectedImages = ["invader"; "invader"]
                             |> List.map (fun s -> { Image = s; Position = { X = 10.; Y = 20. }})
        equal renderedImages expectedImages

    [<Test>]
    let ``it draws the entities at the position in the entity`` () =
        let laser = Laser.create { X = 10.; Y = 5. }
        let invader = Invader.create({X = 30.; Y = 40.}, Small)

        let game = Game.createGame <| Some(laser) <| [ invader ]

        presenter' game |> ignore

        let bulletImage = renderedImages |> List.find (fun x -> x.Image = "invader")
        equal bulletImage.Position {X = 30.; Y = 40.}

        let laserImage = renderedImages |> List.find (fun x -> x.Image = "laser")
        equal laserImage.Position {X = 10.; Y = 5.}
