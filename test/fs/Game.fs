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
    let findLaser entities =
        entities 
        |> List.tryPick  (function Laser e -> Some e | _ -> None)
        |> function Some laser -> laser | None -> failwith "laser not found"

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
        let newLaser = findLaser updatedGame.Entities
        equal true newLaser.LeftForce

    [<Test>]
    let ``move right applies right force to the laser`` () =
        let game = {
            Entities = [laser]
        }

        let updatedGame = update game MoveRight
        let newLaser = findLaser updatedGame.Entities
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
        let newLaser = findLaser updatedGame.Entities
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
        let newLaser = findLaser updatedGame.Entities
        equal false newLaser.LeftForce

[<TestFixture>]
module UpdatingLaser =

    let findLaser entities =
        entities 
        |> List.tryPick  (function Laser e -> Some e | _ -> None)
        |> function Some laser -> laser | None -> failwith "laser not found"

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

        let updatedGame = update game Update

        let newLaser = findLaser updatedGame.Entities
        equal newLaser.Entity.Position defaultEntity.Position