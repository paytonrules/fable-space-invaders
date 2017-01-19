namespace Test

open Fable.Core.Testing
open SpaceInvaders.Game


[<TestFixture>]
module InitialInvaderPositioning =
    [<Test>]
    let ``it puts the first invader at 3. 20.`` () =
        let (Invader firstInvader) = List.head initialInvaders

        equal firstInvader.Entity.Position {X = 3.; Y = 20.} 

[<TestFixture>]
module MovingLaser =
    let defaultEntity = {
        Position = {X = 0.; Y = 1.}
        Velocity = None
        Bounds = {Width = 10; Height = 10}
    }

    let laser = {
        Entity = defaultEntity;
        LeftForce = false;
        RightForce = false; } |> Laser


    [<Test>]
    let ``move left applies left force to the laser`` () =
        let game = {
            Entities = [laser]
        }

        let updatedGame = update game MoveLeft
        let (Laser newLaser) = List.head updatedGame.Entities

        equal true newLaser.LeftForce

    [<Test>]
    let ``move right applies right force to the laser`` () =
        let game = {
            Entities = [laser]
        }

        let updatedGame = update game MoveRight
        let (Laser newLaser) = List.head updatedGame.Entities

        equal true newLaser.RightForce

    [<Test>]
    let ``stop move right removes right force from the laser`` () =
        let movingRightLaser = {
            LaserProperties.Entity = defaultEntity;
            LeftForce = false;
            RightForce = true; } |> Laser

        let game = {
            Entities = [movingRightLaser]
        }

        let updatedGame = update game StopMoveRight
        let (Laser newLaser) = List.head updatedGame.Entities

        equal false newLaser.RightForce

    [<Test>]
    let `` stop move left removes left force from the laser`` () = 
        let movingLeftLaser = {
            LaserProperties.Entity = defaultEntity;
            LeftForce = true;
            RightForce = false; } |> Laser

        let game = {
            Entities = [movingLeftLaser]
        }

        let updatedGame = update game StopMoveLeft
        let (Laser newLaser) = findLaser updatedGame.Entities

        equal false newLaser.LeftForce

[<TestFixture>]
module UpdatingLaser =
    let defaultEntity = {
        Position = {X = 0.; Y = 1.}
        Velocity = None
        Bounds = {Width = 10; Height = 10}
    }

    [<Test>]
    let ``update a laser with no forces, it stays in the same place`` () = 
        let laser = {
            LaserProperties.Entity = defaultEntity;
            LeftForce = false;
            RightForce = false; } |> Laser
        
        let game = {
            Entities = [laser]
        }

        update game Update

        let (Laser laser) = findLaser game.Entities

        equal laser.Entity.Position defaultEntity.Position