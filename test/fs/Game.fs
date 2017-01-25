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
        Bounds = {Width = 10; Height = 10}
    }

    let laser = {
        Entity = defaultEntity;
        LeftForce = false;
        RightForce = false; } |> Laser


    [<Test>]
    let ``move left applies left force to the laser`` () =
        let game = { Entities = [laser]; LastUpdate = 0.}

        let updatedGame = update game MoveLeft
        let newLaser = findLaser updatedGame.Entities
        equal true newLaser.LeftForce

    [<Test>]
    let ``move right applies right force to the laser`` () =
        let game = { Entities = [laser]; LastUpdate = 0. }

        let updatedGame = update game MoveRight
        let newLaser = findLaser updatedGame.Entities
        equal true newLaser.RightForce

    [<Test>]
    let ``stop move right removes right force from the laser`` () =
        let movingRightLaser = {
            LaserProperties.Entity = defaultEntity;
            LeftForce = false;
            RightForce = true; } |> Laser

        let game = { Entities = [movingRightLaser]; LastUpdate = 0.;}

        let updatedGame = update game StopMoveRight
        let newLaser = findLaser updatedGame.Entities
        equal false newLaser.RightForce

    [<Test>]
    let `` stop move left removes left force from the laser`` () =
        let movingLeftLaser = {
            LaserProperties.Entity = defaultEntity;
            LeftForce = true;
            RightForce = false; } |> Laser

        let game = { Entities = [movingLeftLaser]; LastUpdate = 0.}

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
        Position = {X = 100.; Y = 1.};
        Bounds = {Width = 10; Height = 10};
    }

    [<Test>]
    let ``update a laser with no forces, it stays in the same place`` () =
        let laser = {
            LaserProperties.Entity = defaultEntity;
            LeftForce = false;
            RightForce = false; } |> Laser

        let game = { Entities = [laser]; LastUpdate = 0.; }

        let updateEvent = Event.Update 10.
        let updatedGame = update game updateEvent

        let newLaser = findLaser updatedGame.Entities
        equal newLaser.Entity.Position defaultEntity.Position

    [<Test>]
    let ``update a laser with right force, and it moves to the right`` () =
        let laser = {
            LaserProperties.Entity = defaultEntity;
            LeftForce = false;
            RightForce = true; } |> Laser

        let game = { Entities = [laser]; LastUpdate = 0. }

        let updateEvent = Event.Update 1.
        let updatedGame = update game updateEvent

        let newLaser = findLaser updatedGame.Entities
        equal newLaser.Entity.Position.X (defaultEntity.Position.X + Laser.speedPerMillisecond)

    [<Test>]
    let ``account for delta when moving the laser`` () =
        let laser = {
            LaserProperties.Entity = defaultEntity;
            LeftForce = false;
            RightForce = true; } |> Laser

        let game = { Entities = [laser]; LastUpdate = 1.}

        let updateEvent = Event.Update 3.
        let updatedGame = update game updateEvent

        let newLaser = findLaser updatedGame.Entities
        equal newLaser.Entity.Position.X (defaultEntity.Position.X + (2. * Laser.speedPerMillisecond))

    [<Test>]
    let ``update a laser with left force, move to the left`` () = 
        let laser = {
            LaserProperties.Entity = defaultEntity;
            LeftForce = true;
            RightForce = false; } |> Laser

        let game = { Entities = [laser]; LastUpdate = 1.}

        let updateEvent = Event.Update 3.
        let updatedGame = update game updateEvent

        let newLaser = findLaser updatedGame.Entities
        equal newLaser.Entity.Position.X (defaultEntity.Position.X - (2. * Laser.speedPerMillisecond))

    [<Test>]
    let ``update a laser with both forces, go nowhere`` () = 
        let laser = {
            LaserProperties.Entity = defaultEntity;
            LeftForce = true;
            RightForce = true; } |> Laser

        let game = { Entities = [laser]; LastUpdate = 1.}

        let updateEvent = Event.Update 3.
        let updatedGame = update game updateEvent

        let newLaser = findLaser updatedGame.Entities
        equal newLaser.Entity.Position.X defaultEntity.Position.X

    [<Test>]
    let ``stop the laser at the left constraint`` () =
        let entityAtLeftBorder = { defaultEntity with Position = 
                                                                { defaultEntity.Position with 
                                                                    X = SpaceInvaders.Constraints.Bounds.Left |> float
                                                                }
                                 }
        let laser = {
            LaserProperties.Entity = entityAtLeftBorder;
            LeftForce = true;
            RightForce = false; } |> Laser

        
        let game = { Entities = [laser]; LastUpdate = 0.}
        let updateEvent = Event.Update 3.
        let updatedGame = update game updateEvent

        let newLaser = findLaser updatedGame.Entities
        equal newLaser.Entity.Position.X entityAtLeftBorder.Position.X

    [<Test>]
    let ``stop the laser at the right constraint`` () =
        let entityAtRightBorder = { defaultEntity with Position = 
                                                                { defaultEntity.Position with 
                                                                    X = SpaceInvaders.Constraints.Bounds.Right
                                                                            - defaultEntity.Bounds.Width |> float
                                                                }
                                 }
        let laser = {
            LaserProperties.Entity = entityAtRightBorder;
            LeftForce = false;
            RightForce = true; } |> Laser
        
        let game = { Entities = [laser]; LastUpdate = 0.}
        let updateEvent = Event.Update 3.
        let updatedGame = update game updateEvent

        let newLaser = findLaser updatedGame.Entities
        equal newLaser.Entity.Position.X entityAtRightBorder.Position.X

[<TestFixture>]
module ShootBullets =
    let requiredLaser = {
        Entity = { Position = { X = 0.; Y = 0. }
                   Bounds = { Width = 1; Height = 1; }
                 };
        RightForce = false;
        LeftForce = false } |> Laser

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

        let bullet = updatedGame.Entities
                     |> List.tryPick  (function Bullet e -> Some e | _ -> None)

        equal false (bullet = None)

    [<Test>]
    let ``a second bullet is not created if a bullet is present`` () =
        let firstGame = update game Shoot 
        let secondGame = update firstGame Shoot

        let bulletCount  = secondGame.Entities
                           |> List.filter  (function Bullet e -> true | _ -> false)
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
        equal expectedBulletPosition bullet.Entity.Position |> ignore



[<TestFixture>]
module UpdateFunc = 
    [<Test>]
    let ``update the timestamp on each update`` () =
        let game = { Entities = []; LastUpdate = 2.}
        let updateEvent = Event.Update 3.

        let newGame = update game updateEvent

        equal newGame.LastUpdate 3.
