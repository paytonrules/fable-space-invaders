namespace Test

open Fable.Core.Testing
open SpaceInvaders.Game


[<TestFixture>]
module InitialInvaderPositioning =
    [<Test>]
    let ``it puts the first invader at the upper left corner of the invasion`` () =
        let firstInvader = List.head initialInvaders

        equal firstInvader.Position invasionUpperLeftCorner

    [<Test>]
    let ``it puts the next invader the width of the invader to the right`` () = 
        let secondInvader = List.item 1 initialInvaders 

        let secondPosition = Vector2.add invasionUpperLeftCorner { X = Invasion.columnWidth; Y = 0. }
        equal secondInvader.Position secondPosition

    [<Test>]
    let ``it puts the third invader two steps to the right`` () =
        let thirdInvader = List.item 2 initialInvaders

        let thirdPosition = Vector2.scale { X = Invasion.columnWidth; Y = 0.} 2.
                            |> Vector2.add invasionUpperLeftCorner

        equal thirdInvader.Position thirdPosition

    [<Test>]
    let ``it puts the next rows invader on the next row`` () =
        let rowTwoInvader = List.item Invasion.columns initialInvaders

        let rowTwoPosition = Vector2.add { X = 0.; Y = Invasion.rowHeight} invasionUpperLeftCorner

        equal rowTwoInvader.Position rowTwoPosition

    let sliceInvaderList list (start, slice) = 
        list
        |> Seq.skip start
        |> Seq.take slice
        |> Seq.toList

    let sliceInitialInvaders = sliceInvaderList initialInvaders

    let assertAllInvaders invaders ofType = 
        invaders
        |> List.forall (fun entity -> 
                        match entity.Properties with
                        | Invader e -> e.Type = ofType 
                        | _ -> false) 
        |> equal true 

        
    [<Test>]
    let ``the first two rows are small invaders`` () =
        let firstTwoRows = sliceInitialInvaders (0, Invasion.columns * 2)

        assertAllInvaders firstTwoRows Small

    [<Test>]
    let ``the second two rows are medium invaders`` () =
        let twoRows = Invasion.columns * 2
        let secondTwoRows = sliceInitialInvaders (twoRows, twoRows)

        assertAllInvaders secondTwoRows Medium

    [<Test>]
    let ``the third two rows are Large invaders`` () =
        let twoRows = Invasion.columns * 2
        let secondTwoRows = sliceInitialInvaders (twoRows * 2, twoRows)

        assertAllInvaders secondTwoRows Large

[<TestFixture>]
module InvasionMovement =
    let invaderProperties = { InvaderState = Open; Type = Small }

    let invader = {
        Position = { X = 0.; Y = 0. };
        Bounds = { Width = 0; Height = 0; };
        Properties = invaderProperties |> Invader
    }

    [<Test>]
    let ``if it is not time to move, because delta is smaller than the SinceLastMove, TimeToMove gap, do not move`` () =
        let updatedInvader = Invader.update (invader, invaderProperties)
                                { TimeToMove = 1.0; 
                                  SinceLastMove = 0.0;
                                  Direction = { X = 1.0; Y = 0.0 } }
                                0.99

        equal invader.Position updatedInvader.Position

    [<Test>]
    let ``move when it is time to move`` () =
        let updatedInvader = Invader.update (invader, invaderProperties)
                                { TimeToMove = 1.0; 
                                  SinceLastMove = 0.0;
                                  Direction = { X = 1.0; Y = 0.0 }}
                                1.0

        equal (invader.Position.X + 1.0) updatedInvader.Position.X
 
    [<Test>]
    let ``move when it is past time to move`` () =
        let updatedInvader = Invader.update (invader, invaderProperties)
                                { TimeToMove = 1.0; 
                                  SinceLastMove = 0.0;
                                  Direction = { X = 1.0; Y = 0.0 }}
                                1.1

        equal (invader.Position.X + 1.0) updatedInvader.Position.X

    [<Test>]
    let ``SinceLastMove accumulates, use delta + SinceLastMove to see if it is time to move`` () =
        let updatedInvader = Invader.update (invader, invaderProperties)
                                { TimeToMove = 1.0; 
                                  SinceLastMove = 0.9;
                                  Direction = { X = 1.0; Y = 0.0 }}
                                0.1

        equal (invader.Position.X + 1.0) updatedInvader.Position.X

    [<Test>]
    let ``Close the invader when it is Open (the default) and it is time to move`` () =
        let updatedInvader = Invader.update (invader, invaderProperties)
                                { TimeToMove = 1.0; 
                                  SinceLastMove = 0.0;
                                  Direction = { X = 1.0; Y = 0.0 }}
                                1.1
        
        match updatedInvader.Properties with
        | Invader props -> equal props.InvaderState Closed |> ignore
        | _ -> failwith "the entity was not an invader"

    [<Test>]
    let ``Open the invader when it is Closed and it is time to move`` () =
        let closedInvader = { invaderProperties with InvaderState = Closed }
        let updatedInvader = Invader.update (invader, closedInvader)
                                { TimeToMove = 1.0; 
                                  SinceLastMove = 0.0;
                                  Direction = { X = 1.0; Y = 0.0 }}
                                1.1
        
        match updatedInvader.Properties with
        | Invader props -> equal props.InvaderState Open |> ignore
        | _ -> failwith "the entity was not an invader"

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
        let game = createGame [laser]

        let updatedGame = update game MoveLeft
        let newLaser = LaserTest.findLaserProperties updatedGame.Entities
        equal true newLaser.LeftForce

    [<Test>]
    let ``move right applies right force to the laser`` () =
        let game = createGame [laser]

        let updatedGame = update game MoveRight
        let newLaser = LaserTest.findLaserProperties updatedGame.Entities
        equal true newLaser.RightForce

    [<Test>]
    let ``stop move right removes right force from the laser`` () =
        let properties = { RightForce = true; LeftForce = false } |> Laser
        let movingRightLaser = Laser.updateProperties laser properties

        let game = createGame [movingRightLaser]

        let updatedGame = update game StopMoveRight
        let newLaser = LaserTest.findLaserProperties updatedGame.Entities
        equal false newLaser.RightForce

    [<Test>]
    let `` stop move left removes left force from the laser`` () =
        let properties = { LeftForce = true; RightForce = false; } |> Laser
        let movingLeftLaser = Laser.updateProperties laser properties

        let game = createGame [movingLeftLaser]

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
        let game = createGame [laser]

        let updateEvent = Event.Update 10.
        let updatedGame = update game updateEvent

        let newLaser = LaserTest.findLaser updatedGame.Entities
        equal newLaser.Position laser.Position

    [<Test>]
    let ``update a laser with right force, and it moves to the right`` () =
        let properties = { RightForce = true; LeftForce = false } |> Laser
        let rightForceLaser = Laser.updateProperties laser properties

        let game = createGame [rightForceLaser]

        let updateEvent = Event.Update 1.
        let updatedGame = update game updateEvent

        let newLaser = LaserTest.findLaser updatedGame.Entities
        equal newLaser.Position.X (rightForceLaser.Position.X + Laser.speedPerMillisecond)

    [<Test>]
    let ``account for delta when moving the laser`` () =
        let properties = { RightForce = true; LeftForce = false } |> Laser
        let rightForceLaser = Laser.updateProperties laser properties
        
        let game = { createGame [rightForceLaser] with LastUpdate = 1.}
                   
        let updateEvent = Event.Update 3.
        let updatedGame = update game updateEvent

        let newLaser = LaserTest.findLaser updatedGame.Entities
        equal newLaser.Position.X (rightForceLaser.Position.X + (2. * Laser.speedPerMillisecond))

    [<Test>]
    let ``update a laser with left force, move to the left`` () = 
        let laserProperties = { RightForce = false; LeftForce = true } |> Laser
        let leftForceLaser = Laser.updateProperties laser laserProperties

        let game = { createGame [leftForceLaser] with LastUpdate = 1. }

        let updateEvent = Event.Update 3.
        let updatedGame = update game updateEvent

        let newLaser = LaserTest.findLaser updatedGame.Entities
        equal newLaser.Position.X (leftForceLaser.Position.X - (2. * Laser.speedPerMillisecond))

    [<Test>]
    let ``update a laser with both forces, go nowhere`` () = 
        let laserProperties = { RightForce = true; LeftForce = true } |> Laser
        let stuckLaser = Laser.updateProperties laser laserProperties

        let game = { createGame [stuckLaser] with LastUpdate = 1.}

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
        
        let game = createGame [leftLaser]
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
       
        let game = createGame [laserAtRightBorder]
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

    let game = createGame [requiredLaser]

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
        let game = createGame [laser]

        let updatedGame = update game Shoot

        let bullet = findBullet updatedGame.Entities
        let expectedBulletPosition = { X = 26.; Y = 30. - (float Bullet.Height) ; }
        equal expectedBulletPosition bullet.Position |> ignore

[<TestFixture>]
module UpdateBullet = 
    [<Test>]
    let ``a bullet goes up the velocity each update`` () =
        let position = { X = 10.; Y = 10. }
        let properties = { Velocity = { X = 1.; Y = 1.}} |> Bullet
        let bullet = Bullet.create position properties
        let delta = 1.

        let updatedBullet = Bullet.update bullet delta
        
        equal { X = 11.; Y = 11. } updatedBullet.Position

    [<Test>]
    let ``the going up accounts for the delta`` () =
        let position = { X = 10.; Y = 10. }
        let properties = { Velocity = { X = 1.; Y = 1.}} |> Bullet
        let bullet = Bullet.create position properties
        let delta = 0.5

        let updatedBullet = Bullet.update bullet delta
        
        equal { X = 10.5; Y = 10.5 } updatedBullet.Position

[<TestFixture>]
module UpdateFunc = 
    [<Test>]
    let ``update the timestamp on each update`` () =
        let game = { createGame [] with LastUpdate = 2.}

        let newGame = updateGame game 3.

        equal newGame.LastUpdate 3.

    [<Test>]
    let ``after update, if the bullet is off the screen then remove it`` () =
        let bulletBounds = { Height = 5; Width = 0}
        let offTheTop = SpaceInvaders.Constraints.Bounds.Top - bulletBounds.Height - 1
        let position = { X = 0.; Y = float offTheTop }
        let bullet = { Bullet.createWithDefaultProperties position with Bounds = bulletBounds }

        let game = updateGame (createGame [bullet]) 0. 

        equal List.empty game.Entities

    [<Test>]
    let ``if the bullet is still on the screen then keep it`` () =
        let bulletBounds = { Height = 5; Width = 0}
        let offTheTop = SpaceInvaders.Constraints.Bounds.Top - bulletBounds.Height + 1
        let position = { X = 0.; Y = float offTheTop }
        let bullet = { Bullet.createWithDefaultProperties position with Bounds = bulletBounds }

        let game = updateGame (createGame [bullet]) 0. 

        equal [bullet] game.Entities

    [<Test>]
    let ``update the invasion step with the time since last move`` () =
        let game = updateTimeSinceLastMove (createGame []) 3.

        let updatedGame = updateGame game 2.

        equal 5. updatedGame.Invasion.SinceLastMove

    [<Test>]
    let ``reset the invasion step time since last move after passing the time to move`` () =
        let game = updateTimeSinceLastMove (createGame []) 1.
        
        equal 1. game.Invasion.SinceLastMove

        let updatedGame = updateGame game game.Invasion.TimeToMove

        equal 0. updatedGame.Invasion.SinceLastMove