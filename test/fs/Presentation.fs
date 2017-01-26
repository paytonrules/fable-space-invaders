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

    [<Test>]
    let ``it draws the laser where it is`` () =
        let laserProperties = { RightForce = false; LeftForce = false } |> Laser
        let laser =  { Position = {X = 10.; Y = 20.}; 
                       Bounds = {Width = 0; Height = 0};
                       Properties = laserProperties }

        let game = { Entities = [ laser ];
                     LastUpdate = 0. }

        presenter' game |> ignore

        let laserImage = renderedImages |> List.find (fun x -> x.Image = "laser")
        equal laserImage.Position {X = 10.; Y = 20.}

    [<Test>]
    let ``it draws the bullet at its position when present`` () =
        let bulletProperties = { Velocity = { X = 0.; Y = 0. } } |> Bullet
        let bullet =  { Position = {X = 30.; Y = 40.}; 
                        Bounds = {Width = 0; Height = 0}
                        Properties = bulletProperties }

        let game = { 
            Entities = [ bullet ];
            LastUpdate = 0.
        }

        presenter' game |> ignore

        let bulletImage = renderedImages |> List.find (fun x -> x.Image = "bullet")
        equal bulletImage.Position {X = 30.; Y = 40.}   

    [<Test>]
    let ``it will draw the invader types`` () =
        let invaderTypes = [(Large, Open, "largeInvaderOpen"); 
                            (Large, Closed, "largeInvaderClosed"); 
                            (Medium, Open, "mediumInvaderOpen");
                            (Medium, Closed, "mediumInvaderClosed");
                            (Small, Open, "smallInvaderOpen");
                            (Small, Closed, "smallInvaderClosed")]
        let position = { X = 7.; Y = 1. }
        let bounds = { Width = 0; Height = 0 }

        // This is the function that validates every image in the the table above
        let validateImageRendered (invaderType, state, expectedImage) =
            let invaderProperties = { InvaderState = state;
                                      Type = invaderType } |> Invader
            let invader = { 
                Position = position; 
                Bounds = bounds; 
                Properties = invaderProperties }

            let game = {
                Entities = [ invader ];
                LastUpdate = 0.;
            }

            presenter' game |> ignore

            let image = renderedImages |> List.find (fun x -> x.Image = expectedImage)

            equal image.Position { X = 7.; Y = 1. }

        // Loop through all entries above and check for presence
        invaderTypes |> List.iter validateImageRendered