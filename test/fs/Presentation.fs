namespace Test

open Fable.Core.Testing

open SpaceInvaders.Presentation
open SpaceInvaders.Image
open SpaceInvaders.Game

[<TestFixture>]
module Presentation =
    let mutable renderedImages:Image<string> list = []
    let renderer images = 
        renderedImages <- images

    let imageLookup = [
        Image.Bullet, "bullet";
        Image.Laser, "laser";
        LargeInvaderOpen, "largeInvaderOpen";
        LargeInvaderClosed, "largeInvaderClosed";
        MediumInvaderOpen, "mediumInvaderOpen";
        MediumInvaderClosed, "mediumInvaderClosed";
        SmallInvaderOpen, "smallInvaderOpen";
        SmallInvaderClosed, "smallInvaderClosed";] |> Map

    let presenter' = presenter renderer imageLookup
    [<SetUp>]
    let setup () = 
        renderedImages <- []

    let defaultLaser =  {Position = {X = 10.; Y = 20.}; 
                        Bounds = {Width = 0; Height = 0};
                        Velocity = None}

    [<Test>]
    let ``it draws the laser where it is`` () =
        let game = {
            Laser = defaultLaser |> Laser;
            Bullet = None;
            Invaders = []
        }

        presenter' game |> ignore

        let laserImage = renderedImages |> List.find (fun x -> x.Image = "laser")
        equal defaultLaser.Position.X laserImage.Position.X
        equal defaultLaser.Position.Y laserImage.Position.Y

    [<Test>]
    let ``it doesn't draw the bullet when there isn't one`` () =
        let game = {
            Laser = defaultLaser |> Laser;
            Bullet = None;
            Invaders = []
        }

        presenter' game |> ignore

        let bulletImage = renderedImages |> List.tryFind (fun x -> x.Image = "bullet")
        equal None bulletImage

    [<Test>]
    let ``it does draw the bullet at its position when present`` () =
        let bullet = {
            Position = {X = 5.; Y = 7.};
            Bounds = {Width = 0; Height = 0};
            Velocity = None
        }
        let game = {
            Laser = defaultLaser |> Laser;
            Bullet = Some(bullet |> Bullet);
            Invaders = []
        }

        presenter' game |> ignore

        let bulletImage = renderedImages |> List.tryFind (fun x -> x.Image = "bullet")
        match bulletImage with
        | None -> failwith "The bullet should have been drawn"
        | Some(drawnBullet) -> equal drawnBullet.Position {X = 5.; Y = 7.}   

    [<Test>]
    let ``it will draw a large invader, open`` () =
        let entity = {
            Position = {X = 7.; Y = 1.};
            Bounds = {Width = 0; Height = 0};
            Velocity = None
        }
        let state = Open
        let invader = (entity, state)
        let game = {
            Laser = defaultLaser |> Laser;
            Bullet = None;
            Invaders = [invader |> Invader]
        }

        presenter' game |> ignore

        let invaderImage = renderedImages 
                           |> List.find (fun x -> x.Image = "largeInvaderOpen")
 
        equal entity.Position.X invaderImage.Position.X
        equal entity.Position.Y invaderImage.Position.Y