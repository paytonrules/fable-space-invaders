module Test.Game

open SpaceInvaders
open Fable.Core.Testing

[<TestFixture>]
module BoxIntersection =
    let laser = { RightForce = true; LeftForce = true } |> Laser

    let cases = [
        ({ X = 0.; Y = 0. }, { X = 11.; Y = 0. }, false);
        ({ X = 0.; Y = 0. }, { X = 10.; Y = 0. }, true);
        ({ X = 0.; Y = 0. }, { X = 9.; Y = 0. }, true);
        ({ X = 0.; Y = 0. }, { X = 0.; Y = 9. }, true);
        ({ X = 0.; Y = 0. }, { X = 0.; Y = 10. }, true);
        ({ X = 0.; Y = 0. }, { X = 0.; Y = 11. }, false);
        ({ X = 11.; Y = 0. }, { X = 0.; Y = 0. }, false);
        ({ X = 10.; Y = 0. }, { X = 0.; Y = 0. }, true);
        ({ X = 9.; Y = 0. }, { X = 0.; Y = 0. }, true);
        ({ X = 0.; Y = 9. }, { X = 0.; Y = 0. }, true);
        ({ X = 0.; Y = 10. }, { X = 0.; Y = 0. }, true);
        ({ X = 0.; Y = 11. }, { X = 0.; Y = 0. }, false);
    ]
    [<Test>]
    let ``box intersection (this is a data driven test)`` () =
        cases |> List.iter (fun (firstPosition, secondPosition, expects) ->
            let firstBox = {
              Position = firstPosition
              Bounds = { Width = 10; Height = 10 }
            }

            let secondBox = {
              Position = secondPosition
              Bounds = { Width = 10; Height = 10 }
            }

            Box.isOverlapping firstBox secondBox |> equal expects)

[<TestFixture>]
module InitialInvaderPositioning =
    [<Test>]
    let ``it puts the first invader at the upper left corner of the invasion`` () =
        let firstInvader = List.head Game.initialInvaders

        equal firstInvader.Location.Position Game.invasionUpperLeftCorner

    [<Test>]
    let ``it puts the next invader the width of the invader to the right`` () =
        let secondInvader = List.item 1 Game.initialInvaders

        let secondPosition = Vector2.add Game.invasionUpperLeftCorner { X = Invasion.columnWidth; Y = 0. }
        equal secondInvader.Location.Position secondPosition

    [<Test>]
    let ``it puts the third invader two steps to the right`` () =
        let thirdInvader = List.item 2 Game.initialInvaders

        let thirdPosition = Vector2.scale { X = Invasion.columnWidth; Y = 0.} 2.
                            |> Vector2.add Game.invasionUpperLeftCorner

        equal thirdInvader.Location.Position thirdPosition

    [<Test>]
    let ``it puts the next rows invader on the next row`` () =
        let rowTwoInvader = List.item Invasion.columns Game.initialInvaders

        let rowTwoPosition = Vector2.add { X = 0.; Y = Invasion.rowHeight} Game.invasionUpperLeftCorner

        equal rowTwoInvader.Location.Position rowTwoPosition

    let sliceInvaderList list (start, slice) =
        list
        |> Seq.skip start
        |> Seq.take slice
        |> Seq.toList

    let sliceInitialInvaders = sliceInvaderList Game.initialInvaders

    let assertAllInvaders (invaders:Invader.InvaderEntity list) ofType =
        invaders
        |> List.forall (fun entity -> entity.Properties.Type = ofType)
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
    let invader:Invader.InvaderEntity = {
        Location = { Position = { X = 0.; Y = 0. };
                     Bounds = { Width = 0; Height = 0; } }
        Properties = { InvaderState = Open; Type = Small }
    }

    let invasion = Invasion.create [invader]

    [<Test>]
    let ``if it is not time to move, because delta is smaller than the SinceLastMove, TimeToMove gap, do not move`` () =
        let delta = (invasion.TimeToMove - 0.01 )

        let updatedInvasion = Invasion.update invasion delta

        let firstInvader = List.head updatedInvasion.Invaders
        equal invader.Location.Position firstInvader.Location.Position

    [<Test>]
    let ``move when it is time to move`` () =
        let delta = invasion.TimeToMove

        let updatedInvasion = Invasion.update invasion delta

        let actual = (List.head updatedInvasion.Invaders).Location.Position
        let expected = Vector2.add invader.Location.Position Invasion.Direction.Right

        equal expected actual

    [<Test>]
    let ``move when it is past time to move`` () =
        let delta = invasion.TimeToMove + 0.1

        let updatedInvasion = Invasion.update invasion delta

        let actual = (List.head updatedInvasion.Invaders).Location.Position
        let expected = Vector2.add invader.Location.Position Invasion.Direction.Right

        equal expected actual

    [<Test>]
    let ``SinceLastMove accumulates, use delta + SinceLastMove to see if it is time to move`` () =
        let sinceLastMove = 0.9
        let delta = invasion.TimeToMove - sinceLastMove

        let updatedInvasion = { invasion with SinceLastMove = sinceLastMove }
                              |> Invasion.update <| delta

        let actual = (List.head updatedInvasion.Invaders).Location.Position
        let expected = Vector2.add invader.Location.Position Invasion.Direction.Right

        equal expected actual

    [<Test>]
    let ``Move all the invaders in the invasion`` () =
        let delta = invasion.TimeToMove
        let secondInvader:Invader.InvaderEntity = {
          Location = { Position = { X = 10.; Y = 0. };
                       Bounds = { Width = 0; Height = 0; } }
          Properties = { InvaderState = Open; Type = Small }
        }

        let updatedInvasion = { invasion with Invaders = [invader; secondInvader] }
                              |> Invasion.update <| delta

        let actual = (List.item 1 updatedInvasion.Invaders).Location.Position
        let expected = Vector2.add secondInvader.Location.Position Invasion.Direction.Right

        equal expected actual

    [<Test>]
    let ``Close the invader when it is Open (the default) and it is time to move`` () =
        let delta = invasion.TimeToMove

        let updatedInvasion = Invasion.update invasion delta

        let actual = (List.head updatedInvasion.Invaders).Properties.InvaderState
        let expected = Closed

        equal expected actual

    [<Test>]
    let ``Open the invader when it is Closed and it is time to move`` () =
        let delta = invasion.TimeToMove
        let closedInvader = { invader with
                                 Properties = { invader.Properties with InvaderState = Closed } }
        let closedInvasion = { invasion with Invaders = [closedInvader] }

        let updatedInvasion = Invasion.update closedInvasion delta
        let updatedInvader = List.head updatedInvasion.Invaders

        equal updatedInvader.Properties.InvaderState Open

[<TestFixture>]
module InvasionBounds =

    [<Test>]
    let ``If there are no Entities then Right and Left are 0`` () =
        let bounds = Invasion.bounds []

        equal bounds.Right 0.

    [<Test>]
    let ``With one invader entity use the X + Width for the right`` () =
        let properties = { InvaderState = Closed ; Type = Small }
        let invader:Invader.InvaderEntity = {
            Location = { Position = { X = 5.; Y = 0. }
                         Bounds = { Width = 5; Height = 0 } }
            Properties = properties
        }
        let bounds = Invasion.bounds [invader]

        equal bounds.Right 10.

    [<Test>]
    let ``With many invaders use the invader farthest to the right`` () =
        let properties = { InvaderState = Closed ; Type = Small }
        let invaderOne:Invader.InvaderEntity = {
            Location = { Position = { X = 5.; Y = 0. }
                         Bounds = { Width = 5; Height = 0 } }
            Properties = properties
        }
        let invaderTwo = Invader.updatePosition invaderOne { X = 10.; Y = 0. }
        let bounds = Invasion.bounds [invaderOne; invaderTwo]

        equal bounds.Right 15.

    [<Test>]
    let ``Left is 0 when there are no entities`` () =
        let bounds = Invasion.bounds []

        equal bounds.Left 0.

    [<Test>]
    let ``Left is the X of the only invader with one invader entity`` () =
        let properties = { InvaderState = Closed ; Type = Small }
        let invader:Invader.InvaderEntity = {
            Location = { Position = { X = 5.; Y = 0. }
                         Bounds = { Width = 5; Height = 0 } }
            Properties = properties
        }
        let bounds = Invasion.bounds [invader]

        equal bounds.Left 5.

    [<Test>]
    let ``With many invaders use the invader farthest to the left`` () =
        let properties = { InvaderState = Closed ; Type = Small }
        let invaderOne:Invader.InvaderEntity = {
            Location = { Position = { X = 5.; Y = 0. }
                         Bounds = { Width = 5; Height = 0 } }
            Properties = properties
        }
        let invaderTwo = Invader.updatePosition invaderOne { X = 10.; Y = 0. }
        let bounds = Invasion.bounds [invaderTwo; invaderOne]

        equal bounds.Left 5.

module InvaderBulletCollision =
    let invaderProps = { InvaderState = Closed; Type = Small }
    let bulletProps = { Velocity = { X = 0.; Y = 0. } }
    let defaultInvader:Invader.InvaderEntity = {
        Location = { Position = { X = 0.; Y = 0.}
                     Bounds = { Width = 10; Height = 10 } }
        Properties = invaderProps }

    let defaultBullet:Bullet.BulletEntity = {
        Location = { Position = { X = 0.; Y = 0.}
                     Bounds = { Width = 10; Height = 10 } }
        Properties = bulletProps }

    [<Test>]
    let ``invaders are unchanged when there the bullet doesn't collide`` () =
        let invader = Invader.updatePosition defaultInvader { X = 0.; Y = 0. }
        let bullet = Bullet.updatePosition defaultBullet { X = 90.; Y = 0. }

        let invaders = [invader]

        let updatedInvaders = Invasion.removeShotInvaders invaders bullet.Location

        equal updatedInvaders invaders

    [<Test>]
    let ``one invader is removed when the bullet intersects it`` () =
        let invader = Invader.updatePosition defaultInvader { X = 0.; Y = 0. }
        let bullet = Bullet.updatePosition defaultBullet { X = 1.; Y = 0. }

        let invaders = [invader]

        let updatedInvaders = Invasion.removeShotInvaders invaders bullet.Location

        List.isEmpty updatedInvaders |> equal true

module LaserTest =
    let findLaserProperties game =
        match game.Laser with
        | None -> failwith "No laser found"
        | Some laser -> laser.Properties

[<TestFixture>]
module MovingLaser =
    let defaultLaserProperties = {LeftForce = false; RightForce = false}

    let laser = {
        Laser.LaserEntity.Location = { Position = { X = 0.; Y = 1. }
                                       Bounds = { Width = 10; Height = 10 } }
        Laser.LaserEntity.Properties = defaultLaserProperties;
    }

    [<Test>]
    let ``move left event applies left force to the laser`` () =
        let game = Game.createGame <| Some laser <| []

        let updatedGame = Game.update game MoveLeft
        let newLaser = LaserTest.findLaserProperties updatedGame
        equal true newLaser.LeftForce

    [<Test>]
    let ``move right applies right force to the laser`` () =
        let game = Game.createGame <| Some laser <| []

        let updatedGame = Game.update game MoveRight
        let newLaser = LaserTest.findLaserProperties updatedGame
        equal true newLaser.RightForce

    [<Test>]
    let ``stop move right removes right force from the laser`` () =
        let properties = { RightForce = true; LeftForce = false }
        let movingRightLaser = Laser.updateProperties laser properties

        let game = Game.createGame
                   <| Some movingRightLaser
                   <| []

        let updatedGame = Game.update game StopMoveRight
        let newLaser = LaserTest.findLaserProperties updatedGame
        equal false newLaser.RightForce

    [<Test>]
    let `` stop move left removes left force from the laser`` () =
        let properties = { LeftForce = true; RightForce = false; }
        let movingLeftLaser = Laser.updateProperties laser properties

        let game = Game.createGame
                   <| Some movingLeftLaser
                   <| []

        let updatedGame = Game.update game StopMoveLeft
        let newLaser = LaserTest.findLaserProperties updatedGame
        equal false newLaser.LeftForce

[<TestFixture>]
module UpdatingLaser =

    let laserProperties = {
        LeftForce = false
        RightForce = false
    }

    let laser = {
        Laser.LaserEntity.Location = { Position = {X = 100.; Y = 1.}
                                       Bounds = {Width = 10; Height = 10} }
        Laser.LaserEntity.Properties = laserProperties
    }

    [<Test>]
    let ``update a laser with no forces, it stays in the same place`` () =
        let newLaser = Laser.update laser 10.

        equal newLaser.Location.Position laser.Location.Position

    [<Test>]
    let ``update a laser with right force, and it moves to the right`` () =
        let properties = { RightForce = true; LeftForce = false }
        let rightForceLaser = Laser.updateProperties laser properties

        let newLaser = Laser.update rightForceLaser 1.

        equal
        <| newLaser.Location.Position.X
        <| (rightForceLaser.Location.Position.X + Laser.speedPerMillisecond)

    [<Test>]
    let ``account for delta when moving the laser`` () =
        let properties = { RightForce = true; LeftForce = false }
        let rightForceLaser = Laser.updateProperties laser properties

        let expectedX = (rightForceLaser.Location.Position.X +
                         (2. * Laser.speedPerMillisecond))
        let newLaser = Laser.update rightForceLaser 2.

        equal newLaser.Location.Position.X expectedX

    [<Test>]
    let ``update a laser with left force, move to the left`` () =
        let laserProperties = { RightForce = false; LeftForce = true }
        let leftForceLaser = Laser.updateProperties laser laserProperties

        let newLaser = Laser.update leftForceLaser 2.
        let expectedX = (leftForceLaser.Location.Position.X -
                         (2. * Laser.speedPerMillisecond))

        equal newLaser.Location.Position.X expectedX

    [<Test>]
    let ``update a laser with both forces, go nowhere`` () =
        let laserProperties = { RightForce = true; LeftForce = true }
        let stuckLaser = Laser.updateProperties laser laserProperties

        let newLaser = Laser.update stuckLaser 2.

        equal newLaser.Location.Position.X stuckLaser.Location.Position.X

    [<Test>]
    let ``stop the laser at the left constraint`` () =
        let laserProperties =  { LeftForce = true; RightForce = false }
        let leftLaser = Laser.updateXPos laser (float Constraints.Bounds.Left)
                        |> Laser.updateProperties
                        <| laserProperties

        let newLaser = Laser.update leftLaser 1.

        equal newLaser.Location.Position.X leftLaser.Location.Position.X

    [<Test>]
    let ``stop the laser at the right constraint`` () =
        let laserProperties = { LeftForce = false; RightForce = true }
        let rightEdge = Constraints.Bounds.Right - laser.Location.Bounds.Width
                        |> float
        let laserAtRightBorder = Laser.updateXPos laser rightEdge
                                 |> Laser.updateProperties
                                 <| laserProperties

        let newLaser = Laser.update laserAtRightBorder 2.

        equal newLaser.Location.Position.X laserAtRightBorder.Location.Position.X

[<TestFixture>]
module ShootBullets =
    let requiredLaser = {
        Laser.LaserEntity.Location = { Position = { X = 0.; Y = 0. }
                                       Bounds = { Width = 1; Height = 1} }
        Laser.LaserEntity.Properties = { RightForce = false
                                         LeftForce = false }
    }

    let game = Game.createGame <| Some requiredLaser <| []

    [<Test>]
    let ``a bullet is created on the Shoot event`` () =
        let updatedGame = Game.update game Shoot

        equal true (updatedGame.Bullet <> None)

    [<Test>]
    let ``the bullet starts at the laser's nozzle`` () =
        let laser = Laser.create { X = 20.; Y = 30.; }
        let game = Game.createGame <| Some laser <| []

        let updatedGame = Game.update game Shoot

        let bullet = updatedGame.Bullet
        let expectedBulletPosition = { X = 20. + Laser.midpoint;
                                       Y = 30. - (float Bullet.Height) ; }
        match bullet with
        | Some bullet ->
            equal expectedBulletPosition bullet.Location.Position |> ignore
        | None -> failwith "No bulllet was created"

    [<Test>]
    let ``a second bullet is not created if a bullet is present`` () =
        let gameWithBullet = Game.update game Shoot
        let laser = Laser.create { X = 60.; Y = 10.; }
        let gameWithMovedLaser = { gameWithBullet with Laser = Some(laser)}
        let secondGame = Game.update gameWithMovedLaser Shoot

        let originalBulletPosition = { X = Laser.midpoint;
                                       Y = - (float Bullet.Height) }
        match secondGame.Bullet with
        | Some bullet ->
            equal originalBulletPosition bullet.Location.Position |> ignore
        | None -> failwith "No bullet was created"

[<TestFixture>]
module UpdateBullet =
    [<Test>]
    let ``a bullet goes up the velocity each update`` () =
        let position = { X = 10.; Y = 10. }
        let properties = { Velocity = { X = 1.; Y = 1.}}
        let bullet = Bullet.create position properties
        let delta = 1.

        let updatedBullet = Bullet.update bullet delta

        equal { X = 11.; Y = 11. } updatedBullet.Location.Position

    [<Test>]
    let ``the going up accounts for the delta`` () =
        let position = { X = 10.; Y = 10. }
        let properties = { Velocity = { X = 1.; Y = 1.}}
        let bullet = Bullet.create position properties
        let delta = 0.5

        let updatedBullet = Bullet.update bullet delta

        equal { X = 10.5; Y = 10.5 } updatedBullet.Location.Position

[<TestFixture>]
module BulletInvaderCollision =

    [<Test>]
    let ``when the bullet doesn't collide with any invaders keep them all`` () =
        let invaderProperties = { Type = Small; InvaderState = Open }
        let invader:Invader.InvaderEntity = {
            Location = { Position = { X = 0.; Y = 0. }
                         Bounds = { Width = 10; Height = 10 } }
            Properties = invaderProperties
        }
        let bulletProps = { Velocity = { X = 0.; Y = 0. } }
        let bullet:Bullet.BulletEntity option = Some {
            Location = { Position = { X = 100.; Y = 100. }
                         Bounds = { Width = 10; Height = 10 } }
            Properties = bulletProps;
        }

        Game.afterCollision bullet [invader] |> equal (bullet, [invader])

    [<Test>]
    let ``when the bullet does collide with an invader remove the invader and the bullet`` () =
        let invaderProperties = { Type = Small; InvaderState = Open }
        let invader:Invader.InvaderEntity = {
            Location = { Position = { X = 0.; Y = 0. }
                         Bounds = { Width = 10; Height = 10 } }
            Properties = invaderProperties
        }
        let bulletProps = { Velocity = { X = 0.; Y = 0. } }
        let bullet:Bullet.BulletEntity option = Some {
            Location = { Position = { X = 1.; Y = 1. }
                         Bounds = { Width = 10; Height = 10 } }
            Properties = bulletProps;
        }

        Game.afterCollision bullet [invader] |> equal (None, [])

    [<Test>]
    let ``when the bullet is the only entity it does not remove itself (check only against invaders)`` () =
        let bulletProps = { Velocity = { X = 0.; Y = 0. } }
        let bullet:Bullet.BulletEntity option = Some {
            Location = { Position = { X = 1.; Y = 1. }
                         Bounds = { Width = 10; Height = 10 } }
            Properties = bulletProps;
        }

        Game.afterCollision bullet [] |> equal (bullet, [])

[<TestFixture>]
module InvasionDirection =
    let invaderWidth = 1;
    let defaultInvader:Invader.InvaderEntity = {
        Location = { Position = { X = 0.; Y = 0. }
                     Bounds = { Width = invaderWidth; Height = 1 } }
        Properties = { InvaderState = Open; Type = Small }
    }

    let timeToMove = 1000.
    let createGameWithInvasion invaders direction =
        Game.createGame None []
        |> Game.updateInvasion <| { Direction = direction;
                                    Invaders = invaders;
                                    TimeToMove = timeToMove;
                                    SinceLastMove = 0. }

    [<Test>]
    let ``the invasion starts moving down when it hits the right edge and is moving right`` () =
        let positionBeforeRightEdge = float Constraints.Bounds.Right -
                                        Invasion.Direction.Right.X - float invaderWidth
        let invader = Invader.updateXPos defaultInvader positionBeforeRightEdge

        let game = createGameWithInvasion [invader] Invasion.Direction.Right
                   |> Game.updateGame <| timeToMove

        equal Invasion.Direction.Down game.Invasion.Direction

    [<Test>]
    let ``the invasion starts moving down when it moves beyond the right edge and is moving right`` () =
        let positionBeyondRightEdge = float Constraints.Bounds.Right
        let invader = Invader.updateXPos defaultInvader positionBeyondRightEdge

        let game = createGameWithInvasion [invader] Invasion.Direction.Right
                   |> Game.updateGame <| timeToMove

        equal Invasion.Direction.Down game.Invasion.Direction

    [<Test>]
    let ``the invasion starts moving left when it is beyond the right edge and is moving down`` () =
        let positionBeyondRightEdge = float Constraints.Bounds.Right
        let invader = Invader.updateXPos defaultInvader positionBeyondRightEdge

        let game = createGameWithInvasion [invader] Invasion.Direction.Down
                   |> Game.updateGame <| timeToMove

        equal Invasion.Direction.Left game.Invasion.Direction

    [<Test>]
    let ``the invasion moves down when it is at the left edge and is moving left`` () =
        let oneMoveUntilYouHitLeftBorder = float Constraints.Bounds.Left
                                            - Invasion.Direction.Left.X
        let invader = Invader.updateXPos defaultInvader oneMoveUntilYouHitLeftBorder

        let game = createGameWithInvasion [invader] Invasion.Direction.Left
                   |> Game.updateGame <| timeToMove

        equal Invasion.Direction.Down game.Invasion.Direction

    [<Test>]
    let ``the invasion moves down when it beyond the left edge and is moving left`` () =
        let onNextMoveBeyondLeftBorder = float Constraints.Bounds.Left
        let invader = Invader.updateXPos defaultInvader onNextMoveBeyondLeftBorder

        let game = createGameWithInvasion [invader] Invasion.Direction.Left
                   |> Game.updateGame <| timeToMove

        equal Invasion.Direction.Down game.Invasion.Direction

    [<Test>]
    let ``the invasion moves right when it beyond the left edge and is moving down`` () =
        let onNextMoveBeyondLeftBorder = float Constraints.Bounds.Left
        let invader = Invader.updateXPos defaultInvader onNextMoveBeyondLeftBorder

        let game = createGameWithInvasion [invader] Invasion.Direction.Down
                   |> Game.updateGame <| timeToMove

        equal Invasion.Direction.Right game.Invasion.Direction

    [<Test>]
    let ``the direction does not change when it is not yet time to move`` () =
        let onNextMoveBeyondLeftBorder = float Constraints.Bounds.Left
        let invader = Invader.updateXPos defaultInvader onNextMoveBeyondLeftBorder

        let game = createGameWithInvasion [invader] Invasion.Direction.Down
                   |> Game.updateGame <| timeToMove - 1.

        equal Invasion.Direction.Down game.Invasion.Direction

module MoveEntities =

    [<Test>]
    let ``moves the laser`` () =
        let laser = Laser.create {X = 10.; Y = 0.;}
        let properties = { RightForce = true; LeftForce = false }
        let rightForceLaser = Laser.updateProperties laser properties
        let game = Game.createGame <| Some(rightForceLaser) <| []

        let newGame = Game.moveEntities game 1.

        match newGame.Laser with
        | None -> failwith "There is no laser present."
        | Some newLaser ->
            (rightForceLaser.Location.Position.X + Laser.speedPerMillisecond)
            |> equal newLaser.Location.Position.X

    [<Test>]
    let ``moves the bullet when moving entities`` () =
        let bullet = Bullet.createWithDefaultProperties { X = 0.; Y = 0. }
        let properties = { Velocity = { X = 1.0; Y = 1.0 }}
        let movingBullet = { bullet with Properties = properties}

        let game = Game.createGame None []
        let gameWithBullet = { game with Bullet = Some(movingBullet) }

        let newGame = Game.moveEntities gameWithBullet 1.

        match newGame.Bullet with
        | None -> failwith "There is no bullet present"
        | Some bullet ->
            equal { X = 1.0; Y = 1.0 } bullet.Location.Position

[<TestFixture>]
module UpdateGame =
    let laser = Laser.create { X = 10.; Y = 10.}
    let bullet = Bullet.createWithDefaultProperties { X = 15.; Y = 100.}

    [<Test>]
    let ``Game.Update moves the laser`` () =
        let properties = { RightForce = true; LeftForce = false }
        let rightForceLaser = Laser.updateProperties laser properties
        let game = Game.createGame <| Some(rightForceLaser) <| []

        let newGame = Game.updateGame game 1.

        match newGame.Laser with
        | None -> failwith "There is no laser present."
        | Some newLaser ->
            (rightForceLaser.Location.Position.X + Laser.speedPerMillisecond)
            |> equal newLaser.Location.Position.X

    [<Test>]
    let ``Game.update should apply the delta when moving the laser`` () =
        let stillLaser = laser
        let properties = { RightForce = true; LeftForce = false }
        let rightForceLaser = Laser.updateProperties laser properties
        let oneUpdate = Game.createGame <| Some(stillLaser) <| []
                        |> Game.updateGame <| 1.
        let gameWithMovingLaser = { oneUpdate with Laser = Some(rightForceLaser) }

        let finalGame = Game.updateGame gameWithMovingLaser 3.

        // Note the 2. multiplier is because update 1 happened at 1.,
        // and the second at 3.
        match finalGame.Laser with
        | None -> failwith "There is no laser present."
        | Some newLaser ->
            (rightForceLaser.Location.Position.X + (2. * Laser.speedPerMillisecond))
            |> equal newLaser.Location.Position.X

    [<Test>]
    let ``move the bullet on the update`` () =
        let initialGame = Game.createGame <| None <| []

        let game = Game.createGame None []
                   |> Game.setBullet <| bullet
                   |> Game.updateGame <| 1.

        match game.Bullet with
        | None -> failwith "There is no bullet present."
        | Some newBullet ->
            newBullet.Location.Position
            |> equal
            <| Vector2.add bullet.Location.Position Bullet.DefaultVelocity

    [<Test>]
    let ``update the timestamp on each update`` () =
        let game = { Game.createGame None [] with LastUpdate = 2.}

        let newGame = Game.updateGame game 3.

        equal newGame.LastUpdate 3.

    [<Test>]
    let ``after update, if the bullet is off the screen then remove it`` () =
        let bulletBounds = { Height = 5; Width = 0}
        let offTheTop = Constraints.Bounds.Top - bulletBounds.Height - 1
        let position = { X = 0.; Y = float offTheTop }
        let bullet = Bullet.createWithDefaultProperties position
                     |> Bullet.updateBounds <| bulletBounds

        let game = Game.createGame None []
                   |> Game.setBullet <| bullet
                   |> Game.updateGame <| 0.

        equal game.Bullet None

    [<Test>]
    let ``if the bullet is still on the screen then keep it`` () =
        let bulletBounds = { Height = 5; Width = 0}
        let offTheTop = Constraints.Bounds.Top - bulletBounds.Height + 1
        let position = { X = 0.; Y = float offTheTop }
        let bullet = Bullet.createWithDefaultProperties position
                     |> Bullet.updateBounds <| bulletBounds

        let game = Game.createGame None []
                   |> Game.setBullet <| bullet
                   |> Game.updateGame <| 0.

        Some(bullet) |> equal game.Bullet

    [<Test>]
    let ``update the invasion step with the time since last move`` () =
        let invasion = Invasion.create []
                       |> Invasion.updateSinceLastMove <| 3.
        let game = { Game.createGame None [] with Invasion = invasion }

        let updatedGame = Game.updateGame game 2.

        equal 5. updatedGame.Invasion.SinceLastMove

    [<Test>]
    let ``reset the invasion step time since last move after passing the time to move`` () =
        let invasion = Invasion.create []
                       |> Invasion.updateSinceLastMove <| 1.
        let game = { (Game.createGame None []) with Invasion = invasion }

        let updatedGame = Game.updateGame game 1000.

        equal 0. updatedGame.Invasion.SinceLastMove
