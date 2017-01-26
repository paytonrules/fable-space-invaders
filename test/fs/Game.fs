namespace Test

open Fable.Core.Testing
open SpaceInvaders.Game


[<TestFixture>]
module InitialInvaderPositioning =
    [<Test>]
    let ``it puts the first invader at 3. 20.`` () =
        let firstInvader = List.head initialInvaders

        equal firstInvader.Position {X = 3.; Y = 20.}

module LaserTest = 

    let findLaser entities = 
        match SpaceInvaders.Game.findLaser entities with
        | Some laser -> laser
        | None -> failwith "No Laser Found"
        
    let findLaserProperties entities =
        let laser = findLaser entities
        match laser.Properties with 
        | Laser props -> props
        | _ -> failwith "Laser did not have laser properties which is impossible"

[<TestFixture>]
module MovingLaser =
    let defaultLaserProperties = { LeftForce = false; RightForce = false } |> Laser

    let laser = {
        Position = { X = 0.; Y = 1. };
        Bounds = { Width = 10; Height = 10 };
        Properties = defaultLaserProperties;
    }

    [<Test>]
    let ``move left applies left force to the laser`` () =
        let game = { Entities = [laser]; LastUpdate = 0.}

        let updatedGame = update game MoveLeft
        let newLaser = LaserTest.findLaserProperties updatedGame.Entities
        equal true newLaser.LeftForce

    [<Test>]
    let ``move right applies right force to the laser`` () =
        let game = { Entities = [laser]; LastUpdate = 0. }

        let updatedGame = update game MoveRight
        let newLaser = LaserTest.findLaserProperties updatedGame.Entities
        equal true newLaser.RightForce

    [<Test>]
    let ``stop move right removes right force from the laser`` () =
        let properties = { RightForce = true; LeftForce = false } |> Laser
        let movingRightLaser = Laser.updateProperties laser properties

        let game = { Entities = [movingRightLaser]; LastUpdate = 0.;}

        let updatedGame = update game StopMoveRight
        let newLaser = LaserTest.findLaserProperties updatedGame.Entities
        equal false newLaser.RightForce

    [<Test>]
    let `` stop move left removes left force from the laser`` () =
        let properties = { LeftForce = true; RightForce = false; } |> Laser
        let movingLeftLaser = Laser.updateProperties laser properties

        let game = { Entities = [movingLeftLaser]; LastUpdate = 0.}

        let updatedGame = update game StopMoveLeft
        let newLaser = LaserTest.findLaserProperties updatedGame.Entities
        equal false newLaser.LeftForce

[<TestFixture>]
module UpdatingLaser =

    let laserProperties = {
        LeftForce = false;
        RightForce = false} |> Laser

    let laser = {
        Position = {X = 100.; Y = 1.};
        Bounds = {Width = 10; Height = 10};
        Properties = laserProperties
    }

    [<Test>]
    let ``update a laser with no forces, it stays in the same place`` () =
        let game = { Entities = [laser]; LastUpdate = 0.; }

        let updateEvent = Event.Update 10.
        let updatedGame = update game updateEvent

        let newLaser = LaserTest.findLaser updatedGame.Entities
        equal newLaser.Position laser.Position

    [<Test>]
    let ``update a laser with right force, and it moves to the right`` () =
        let properties = { RightForce = true; LeftForce = false } |> Laser
        let rightForceLaser = Laser.updateProperties laser properties

        let game = { Entities = [rightForceLaser]; LastUpdate = 0. }

        let updateEvent = Event.Update 1.
        let updatedGame = update game updateEvent

        let newLaser = LaserTest.findLaser updatedGame.Entities
        equal newLaser.Position.X (rightForceLaser.Position.X + Laser.speedPerMillisecond)

    [<Test>]
    let ``account for delta when moving the laser`` () =
        let properties = { RightForce = true; LeftForce = false } |> Laser
        let rightForceLaser = Laser.updateProperties laser properties
        
        let game = { Entities = [rightForceLaser]; LastUpdate = 1.}

        let updateEvent = Event.Update 3.
        let updatedGame = update game updateEvent

        let newLaser = LaserTest.findLaser updatedGame.Entities
        equal newLaser.Position.X (rightForceLaser.Position.X + (2. * Laser.speedPerMillisecond))

    [<Test>]
    let ``update a laser with left force, move to the left`` () = 
        let laserProperties = { RightForce = false; LeftForce = true } |> Laser
        let leftForceLaser = Laser.updateProperties laser laserProperties

        let game = { Entities = [leftForceLaser]; LastUpdate = 1.}

        let updateEvent = Event.Update 3.
        let updatedGame = update game updateEvent

        let newLaser = LaserTest.findLaser updatedGame.Entities
        equal newLaser.Position.X (leftForceLaser.Position.X - (2. * Laser.speedPerMillisecond))

    [<Test>]
    let ``update a laser with both forces, go nowhere`` () = 
        let laserProperties = { RightForce = true; LeftForce = true } |> Laser
        let stuckLaser = Laser.updateProperties laser laserProperties

        let game = { Entities = [stuckLaser]; LastUpdate = 1.}

        let updateEvent = Event.Update 3.
        let updatedGame = update game updateEvent

        let newLaser = LaserTest.findLaser updatedGame.Entities
        equal newLaser.Position.X stuckLaser.Position.X

    [<Test>]
    let ``stop the laser at the left constraint`` () =
        let laserProperties =  { LeftForce = true; RightForce = false } |> Laser
        let leftPosition = { laser.Position with X = SpaceInvaders.Constraints.Bounds.Left |> float }
        let leftLaser = { laser with Position = leftPosition }
                        |> Laser.updateProperties <| laserProperties
        
        let game = { Entities = [leftLaser]; LastUpdate = 0.}
        let updateEvent = Event.Update 3.
        let updatedGame = update game updateEvent

        let newLaser = LaserTest.findLaser updatedGame.Entities
        equal newLaser.Position.X leftLaser.Position.X

    [<Test>]
    let ``stop the laser at the right constraint`` () =
        let laserProperties = { LeftForce = false; RightForce = true } |> Laser
        let rightEdge = SpaceInvaders.Constraints.Bounds.Right - laser.Bounds.Width
        let rightPosition = { laser.Position with X = rightEdge |> float }
        let laserAtRightBorder = { laser with Position = rightPosition }  
                                 |> Laser.updateProperties <| laserProperties
       
        let game = { Entities = [laserAtRightBorder]; LastUpdate = 0.}
        let updateEvent = Event.Update 3. 
        let updatedGame = update game updateEvent

        let newLaser = LaserTest.findLaser updatedGame.Entities
        equal newLaser.Position.X laserAtRightBorder.Position.X

[<TestFixture>]
module ShootBullets =
    let requiredLaser = {
        Position = { X = 0.; Y = 0. };
        Bounds = { Width = 1; Height = 1; };
        Properties = { RightForce = false;
                       LeftForce = false } |> Laser
    }

    let game = {
        Entities = [requiredLaser];
        LastUpdate = 0.
    }

    let findBullet entities = 
        let bullet = SpaceInvaders.Game.findBullet entities 
        match bullet with
        | None -> failwith "bullet not found"
        | Some bullet -> bullet

    [<Test>]
    let ``a bullet is created on the Shoot event`` () =
        let updatedGame = update game Shoot

        let bullet = SpaceInvaders.Game.findBullet updatedGame.Entities

        equal false (bullet = None)

    [<Test>]
    let ``a second bullet is not created if a bullet is present`` () =
        let firstGame = update game Shoot 
        let secondGame = update firstGame Shoot

        let bulletCount  = secondGame.Entities
                           |> List.filter  (fun entity ->
                                                match entity.Properties with
                                                | Bullet _ -> true
                                                | _ -> false)
                           |> List.length

        equal 1 bulletCount

    [<Test>]
    let ``the bullet starts at the laser's nozzle`` () =
        let laser = Laser.create { X = 20.; Y = 30.; }
        let game = { 
            Entities = [laser];
            LastUpdate = 0.
        }

        let updatedGame = update game Shoot

        let bullet = findBullet updatedGame.Entities
        let expectedBulletPosition = { X = 26.; Y = 30. - (float Bullet.Height) ; }
        equal expectedBulletPosition bullet.Position |> ignore


[<TestFixture>]
module UpdateFunc = 
    [<Test>]
    let ``update the timestamp on each update`` () =
        let game = { Entities = []; LastUpdate = 2.}
        let updateEvent = Event.Update 3.

        let newGame = update game updateEvent

        equal newGame.LastUpdate 3.
