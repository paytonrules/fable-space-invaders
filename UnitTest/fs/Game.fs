module Test.Game

open SpaceInvaders
open Fable.Core.Testing

[<TestFixture>]
module EntityIntersection =
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
    let ``entity intersection (this is a data driven test)`` () =
        cases |> List.iter (fun (firstBox, secondBox, expects) ->
            let firstBox = {
                Position = firstBox;
                Bounds = { Width = 10; Height = 10 };
                Properties = laser
            }

            let secondBox = {
                Position = secondBox;
                Bounds = { Width = 10; Height = 10 };
                Properties = laser
            }

            Entity.isOverlapping firstBox secondBox |> equal expects)

[<TestFixture>]
module InitialInvaderPositioning =
    [<Test>]
    let ``it puts the first invader at the upper left corner of the invasion`` () =
        let firstInvader = List.head Game.initialInvaders

        equal firstInvader.Position Game.invasionUpperLeftCorner

    [<Test>]
    let ``it puts the next invader the width of the invader to the right`` () =
        let secondInvader = List.item 1 Game.initialInvaders

        let secondPosition = Vector2.add Game.invasionUpperLeftCorner { X = Invasion.columnWidth; Y = 0. }
        equal secondInvader.Position secondPosition

    [<Test>]
    let ``it puts the third invader two steps to the right`` () =
        let thirdInvader = List.item 2 Game.initialInvaders

        let thirdPosition = Vector2.scale { X = Invasion.columnWidth; Y = 0.} 2.
                            |> Vector2.add Game.invasionUpperLeftCorner

        equal thirdInvader.Position thirdPosition

    [<Test>]
    let ``it puts the next rows invader on the next row`` () =
        let rowTwoInvader = List.item Invasion.columns Game.initialInvaders

        let rowTwoPosition = Vector2.add { X = 0.; Y = Invasion.rowHeight} Game.invasionUpperLeftCorner

        equal rowTwoInvader.Position rowTwoPosition

    let sliceInvaderList list (start, slice) =
        list
        |> Seq.skip start
        |> Seq.take slice
        |> Seq.toList

    let sliceInitialInvaders = sliceInvaderList Game.initialInvaders

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
                                  Invaders = [];
                                  SinceLastMove = 0.0;
                                  Direction = { X = 1.0; Y = 0.0 } }
                                0.99

        equal invader.Position updatedInvader.Position

    [<Test>]
    let ``move when it is time to move`` () =
        let updatedInvader = Invader.update (invader, invaderProperties)
                                { TimeToMove = 1.0;
                                  Invaders = [];
                                  SinceLastMove = 0.0;
                                  Direction = { X = 1.0; Y = 0.0 }}
                                1.0

        equal (invader.Position.X + 1.0) updatedInvader.Position.X

    [<Test>]
    let ``move when it is past time to move`` () =
        let updatedInvader = Invader.update (invader, invaderProperties)
                                { TimeToMove = 1.0;
                                  Invaders = [];
                                  SinceLastMove = 0.0;
                                  Direction = { X = 1.0; Y = 0.0 }}
                                1.1

        equal (invader.Position.X + 1.0) updatedInvader.Position.X

    [<Test>]
    let ``SinceLastMove accumulates, use delta + SinceLastMove to see if it is time to move`` () =
        let updatedInvader = Invader.update (invader, invaderProperties)
                                { TimeToMove = 1.0;
                                  Invaders = [];
                                  SinceLastMove = 0.9;
                                  Direction = { X = 1.0; Y = 0.0 }}
                                0.1

        equal (invader.Position.X + 1.0) updatedInvader.Position.X

    [<Test>]
    let ``Close the invader when it is Open (the default) and it is time to move`` () =
        let updatedInvader = Invader.update (invader, invaderProperties)
                                { TimeToMove = 1.0;
                                  Invaders = [];
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
                                  Invaders = [];
                                  SinceLastMove = 0.0;
                                  Direction = { X = 1.0; Y = 0.0 }}
                                1.1

        match updatedInvader.Properties with
        | Invader props -> equal props.InvaderState Open |> ignore
        | _ -> failwith "the entity was not an invader"

[<TestFixture>]
module InvasionBounds =

    [<Test>]
    let ``If there are no Entities then Right and Left are 0`` () =
        let bounds = Invasion.bounds []

        equal bounds.Right 0.

    [<Test>]
    let ``With one invader entity use the X + Width for the right`` () =
        let properties = { InvaderState = Closed ; Type = Small } |> Invader
        let invader = {
            Position = { X = 5.; Y = 0. };
            Bounds = { Width = 5; Height = 0 };
            Properties = properties
        }
        let bounds = Invasion.bounds [invader]

        equal bounds.Right 10.

    [<Test>]
    let ``With many invaders use the invader farthest to the right`` () =
        let properties = { InvaderState = Closed ; Type = Small } |> Invader
        let invaderOne = {
            Position = { X = 5.; Y = 0. };
            Bounds = { Width = 5; Height = 0 };
            Properties = properties
        }
        let invaderTwo = { invaderOne with Position = { X = 10.; Y = 0. }}
        let bounds = Invasion.bounds [invaderOne; invaderTwo]

        equal bounds.Right 15.

    [<Test>]
    let ``Only include invaders`` () =
        let invaderProps = { InvaderState = Closed ; Type = Small } |> Invader
        let bulletProps = { Velocity = { X = 0.; Y = 0. } } |> Bullet
        let invader = {
            Position = { X = 5.; Y = 0. };
            Bounds = { Width = 5; Height = 0 };
            Properties = invaderProps
        }
        let bullet = { invader with Position = { X = 10.; Y = 0. }; Properties = bulletProps }
        let bounds = Invasion.bounds [invader; bullet]

        equal bounds.Right 10.

    [<Test>]
    let ``Left is 0 when there are no entities`` () =
        let bounds = Invasion.bounds []

        equal bounds.Left 0.

    [<Test>]
    let ``Left is the X of the only invader with one invader entity`` () =
        let properties = { InvaderState = Closed ; Type = Small } |> Invader
        let invader = {
            Position = { X = 5.; Y = 0. };
            Bounds = { Width = 5; Height = 0 };
            Properties = properties
        }
        let bounds = Invasion.bounds [invader]

        equal bounds.Left 5.

    [<Test>]
    let ``With many invaders use the invader farthest to the left`` () =
        let properties = { InvaderState = Closed ; Type = Small } |> Invader
        let invaderOne = {
            Position = { X = 5.; Y = 0. };
            Bounds = { Width = 5; Height = 0 };
            Properties = properties
        }
        let invaderTwo = { invaderOne with Position = { X = 10.; Y = 0. }}
        let bounds = Invasion.bounds [invaderTwo; invaderOne]

        equal bounds.Left 5.

    [<Test>]
    let ``Only include invaders for calculating min`` () =
        let invaderProps = { InvaderState = Closed ; Type = Small } |> Invader
        let bulletProps = { Velocity = { X = 0.; Y = 0. } } |> Bullet
        let invader = {
            Position = { X = 5.; Y = 0. };
            Bounds = { Width = 5; Height = 0 };
            Properties = invaderProps
        }
        let bullet = { invader with Position = { X = 1.; Y = 0. }; Properties = bulletProps }
        let bounds = Invasion.bounds [invader; bullet]

        equal bounds.Left 5.

module InvaderBulletCollision =
    let invaderProps = { InvaderState = Closed; Type = Small } |> Invader
    let bulletProps = { Velocity = { X = 0.; Y = 0. }} |> Bullet
    let defaultInvader = { Position = { X = 0.; Y = 0.};
                           Bounds = { Width = 10; Height = 10 };
                           Properties = invaderProps }

    let defaultBullet = { Position = { X = 0.; Y = 0.};
                          Bounds = { Width = 10; Height = 10 };
                          Properties = bulletProps }

    [<Test>]
    let ``invaders are unchanged when there the bullet doesn't collide`` () =
        let invader = { defaultInvader with Position = { X = 0.; Y = 0.} }
        let bullet = { defaultBullet with Position = { X = 90.; Y = 0.} }

        let invaders = [invader]

        let updatedInvaders = Invasion.removeShotInvaders invaders bullet

        equal updatedInvaders invaders

    [<Test>]
    let ``one invader is removed when the bullet intersects it`` () =
        let invader = { defaultInvader with Position = { X = 0.; Y = 0.} }
        let bullet = { defaultBullet with Position = { X = 1.; Y = 0.} }

        let invaders = [invader]

        let updatedInvaders = Invasion.removeShotInvaders invaders bullet

        List.isEmpty updatedInvaders |> equal true


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
    let defaultLaserProperties = {LeftForce = false; RightForce = false} |> Laser

    let laser = {
        Position = { X = 0.; Y = 1. };
        Bounds = { Width = 10; Height = 10 };
        Properties = defaultLaserProperties;
    }

    [<Test>]
    let ``move left event applies left force to the laser`` () =
        let game = Game.createGame <| Some laser <| [laser]

        let updatedGame = Game.update game MoveLeft
        let newLaser = LaserTest.findLaserProperties updatedGame.Entities
        equal true newLaser.LeftForce

    [<Test>]
    let ``move right applies right force to the laser`` () =
        let game = Game.createGame <| Some laser <| [laser]

        let updatedGame = Game.update game MoveRight
        let newLaser = LaserTest.findLaserProperties updatedGame.Entities
        equal true newLaser.RightForce

    [<Test>]
    let ``stop move right removes right force from the laser`` () =
        let properties = { RightForce = true; LeftForce = false } |> Laser
        let movingRightLaser = Laser.updateProperties laser properties

        let game = Game.createGame <| Some movingRightLaser <| [movingRightLaser]

        let updatedGame = Game.update game StopMoveRight
        let newLaser = LaserTest.findLaserProperties updatedGame.Entities
        equal false newLaser.RightForce

    [<Test>]
    let `` stop move left removes left force from the laser`` () =
        let properties = { LeftForce = true; RightForce = false; } |> Laser
        let movingLeftLaser = Laser.updateProperties laser properties

        let game = Game.createGame <| Some movingLeftLaser <| [movingLeftLaser]

        let updatedGame = Game.update game StopMoveLeft
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
        let newLaser = Laser.update laser 10.

        equal newLaser.Position laser.Position

    [<Test>]
    let ``update a laser with right force, and it moves to the right`` () =
        let properties = { RightForce = true; LeftForce = false } |> Laser
        let rightForceLaser = Laser.updateProperties laser properties

        let newLaser = Laser.update rightForceLaser 1.

        equal newLaser.Position.X (rightForceLaser.Position.X + Laser.speedPerMillisecond)

    [<Test>]
    let ``account for delta when moving the laser`` () =
        let properties = { RightForce = true; LeftForce = false } |> Laser
        let rightForceLaser = Laser.updateProperties laser properties

        let newLaser = Laser.update rightForceLaser 2.
        equal newLaser.Position.X (rightForceLaser.Position.X + (2. * Laser.speedPerMillisecond))

    [<Test>]
    let ``update a laser with left force, move to the left`` () =
        let laserProperties = { RightForce = false; LeftForce = true } |> Laser
        let leftForceLaser = Laser.updateProperties laser laserProperties

        let newLaser = Laser.update leftForceLaser 2.
        equal newLaser.Position.X (leftForceLaser.Position.X - (2. * Laser.speedPerMillisecond))

    [<Test>]
    let ``update a laser with both forces, go nowhere`` () =
        let laserProperties = { RightForce = true; LeftForce = true } |> Laser
        let stuckLaser = Laser.updateProperties laser laserProperties

        let newLaser = Laser.update stuckLaser 2.

        equal newLaser.Position.X stuckLaser.Position.X

    [<Test>]
    let ``stop the laser at the left constraint`` () =
        let laserProperties =  { LeftForce = true; RightForce = false } |> Laser
        let leftPosition = { laser.Position with X = Constraints.Bounds.Left |> float }
        let leftLaser = { laser with Position = leftPosition }
                        |> Laser.updateProperties <| laserProperties

        let newLaser = Laser.update leftLaser 1.

        equal newLaser.Position.X leftLaser.Position.X

    [<Test>]
    let ``stop the laser at the right constraint`` () =
        let laserProperties = { LeftForce = false; RightForce = true } |> Laser
        let rightEdge = Constraints.Bounds.Right - laser.Bounds.Width
        let rightPosition = { laser.Position with X = rightEdge |> float }
        let laserAtRightBorder = { laser with Position = rightPosition }
                                 |> Laser.updateProperties <| laserProperties

        let newLaser = Laser.update laserAtRightBorder 2.

        equal newLaser.Position.X laserAtRightBorder.Position.X

    [<Test>]
    let ``moveEntities moves the laser`` () =
        let properties = { RightForce = true; LeftForce = false } |> Laser
        let rightForceLaser = Laser.updateProperties laser properties
        let game = Game.createGame <| Some(rightForceLaser) <| []

        let newGame = Game.moveEntities game 1.

        match newGame.Laser with
        | None -> failwith "There is no laser present."
        | Some newLaser ->
            (rightForceLaser.Position.X + Laser.speedPerMillisecond)
            |> equal newLaser.Position.X


    [<Test>]
    let ``Game.Update moves the laser`` () =
        let properties = { RightForce = true; LeftForce = false } |> Laser
        let rightForceLaser = Laser.updateProperties laser properties
        let game = Game.createGame <| Some(rightForceLaser) <| []

        let newGame = Game.updateGame game 1.

        match newGame.Laser with
        | None -> failwith "There is no laser present."
        | Some newLaser ->
            (rightForceLaser.Position.X + Laser.speedPerMillisecond)
            |> equal newLaser.Position.X

    [<Test>]
    let ``Game.update should have some way to be sure you properly calculate delta`` () = ()



[<TestFixture>]
module ShootBullets =
    let requiredLaser = {
        Position = { X = 0.; Y = 0. };
        Bounds = { Width = 1; Height = 1; };
        Properties = { RightForce = false;
                       LeftForce = false } |> Laser
    }

    let game = Game.createGame <| Some requiredLaser <| [requiredLaser]

    let findBullet entities =
        let bullet = SpaceInvaders.Game.findBullet entities
        match bullet with
        | None -> failwith "bullet not found"
        | Some bullet -> bullet

    [<Test>]
    let ``a bullet is created on the Shoot event`` () =
        let updatedGame = Game.update game Shoot

        let bullet = SpaceInvaders.Game.findBullet updatedGame.Entities

        equal false (bullet = None)

    [<Test>]
    let ``a second bullet is not created if a bullet is present`` () =
        let firstGame = Game.update game Shoot
        let secondGame = Game.update firstGame Shoot

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
        let game = Game.createGame <| Some laser <| [laser]

        let updatedGame = Game.update game Shoot

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
module BulletInvaderCollision =

    [<Test>]
    let ``when the bullet doesn't collide with any invaders keep them all`` () =
        let invaderProperties = { Type = Small; InvaderState = Open } |> Invader
        let invader = {
            Position = { X = 0.; Y = 0. };
            Bounds = { Width = 10; Height = 10 };
            Properties = invaderProperties
        }
        let bulletProps = { Velocity = { X = 0.; Y = 0. } } |> Bullet
        let bullet = Some {
            Position = { X = 100.; Y = 100. };
            Bounds = { Width = 10; Height = 10 };
            Properties = bulletProps;
        }

        Game.afterCollision bullet [invader] |> equal (bullet, [invader])

    [<Test>]
    let ``when the bullet does collide with an invader remove the invader and the bullet`` () =
        let invaderProperties = { Type = Small; InvaderState = Open } |> Invader
        let invader = {
            Position = { X = 0.; Y = 0. };
            Bounds = { Width = 10; Height = 10 };
            Properties = invaderProperties
        }
        let bulletProps = { Velocity = { X = 0.; Y = 0. } } |> Bullet
        let bullet = Some {
            Position = { X = 1.; Y = 1. };
            Bounds = { Width = 10; Height = 10 };
            Properties = bulletProps;
        }

        Game.afterCollision bullet [invader] |> equal (None, [])

    [<Test>]
    let ``when the bullet is the only entity it does not remove itself (check only against invaders)`` () =
        let bulletProps = { Velocity = { X = 0.; Y = 0. } } |> Bullet
        let bullet = Some {
            Position = { X = 1.; Y = 1. };
            Bounds = { Width = 10; Height = 10 };
            Properties = bulletProps;
        }

        Game.afterCollision bullet [] |> equal (bullet, [])

[<TestFixture>]
module UpdateFunc =
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
        let bullet = { Bullet.createWithDefaultProperties position with Bounds = bulletBounds }

        let game = Game.updateGame (Game.createGame None [bullet]) 0.

        equal List.empty game.Entities

    [<Test>]
    let ``if the bullet is still on the screen then keep it`` () =
        let bulletBounds = { Height = 5; Width = 0}
        let offTheTop = Constraints.Bounds.Top - bulletBounds.Height + 1
        let position = { X = 0.; Y = float offTheTop }
        let bullet = { Bullet.createWithDefaultProperties position with Bounds = bulletBounds }

        let game = Game.updateGame (Game.createGame None [bullet]) 0.

        equal [bullet] game.Entities

    [<Test>]
    let ``update the invasion step with the time since last move`` () =
        let invasion = { SinceLastMove = 3.;
                         Invaders = [];
                         TimeToMove = 1000.;
                         Direction = Invasion.Direction.Down }
        let game = { Game.createGame None [] with Invasion = invasion }

        let updatedGame = Game.updateGame game 2.

        equal 5. updatedGame.Invasion.SinceLastMove

    [<Test>]
    let ``reset the invasion step time since last move after passing the time to move`` () =
        let invasion = { SinceLastMove = 1.;
                         TimeToMove = 1000.;
                         Direction = Invasion.Direction.Down;
                         Invaders = [] }
        let game = { (Game.createGame None []) with Invasion = invasion }

        let updatedGame = Game.updateGame game 1000.

        equal 0. updatedGame.Invasion.SinceLastMove

[<TestFixture>]
module InvasionDirection =
    let invaderWidth = 1;
    let defaultInvader = {
        Position = { X = 0.; Y = 0. };
        Bounds = { Width = invaderWidth; Height = 1};
        Properties = { InvaderState = Open; Type = Small }  |> Invader
    }

    let timeToMove = 1000.
    let createGameWithInvasion invaders direction =
        Game.createGame None invaders
        |> Game.updateInvasion <| { Direction = direction;
                                    Invaders = [];
                                    TimeToMove = timeToMove;
                                    SinceLastMove = 0. }

    [<Test>]
    let ``the invasion starts moving down when it hits the right edge and is moving right`` () =
        let positionBeforeRightEdge = float Constraints.Bounds.Right -
                                        Invasion.Direction.Right.X - float invaderWidth
        let invader = Entity.updateXPos defaultInvader positionBeforeRightEdge

        let game = createGameWithInvasion [invader] Invasion.Direction.Right
                   |> Game.updateGame <| timeToMove

        equal Invasion.Direction.Down game.Invasion.Direction

    [<Test>]
    let ``the invasion starts moving down when it moves beyond the right edge and is moving right`` () =
        let positionBeyondRightEdge = float Constraints.Bounds.Right
        let invader = Entity.updateXPos defaultInvader positionBeyondRightEdge

        let game = createGameWithInvasion [invader] Invasion.Direction.Right
                   |> Game.updateGame <| timeToMove

        equal Invasion.Direction.Down game.Invasion.Direction

    [<Test>]
    let ``the invasion starts moving left when it is beyond the right edge and is moving down`` () =
        let positionBeyondRightEdge = float Constraints.Bounds.Right
        let invader = Entity.updateXPos defaultInvader positionBeyondRightEdge

        let game = createGameWithInvasion [invader] Invasion.Direction.Down
                   |> Game.updateGame <| timeToMove

        equal Invasion.Direction.Left game.Invasion.Direction

    [<Test>]
    let ``the invasion moves down when it is at the left edge and is moving left`` () =
        let oneMoveUntilYouHitLeftBorder = float Constraints.Bounds.Left
                                            - Invasion.Direction.Left.X
        let invader = Entity.updateXPos defaultInvader oneMoveUntilYouHitLeftBorder

        let game = createGameWithInvasion [invader] Invasion.Direction.Left
                   |> Game.updateGame <| timeToMove

        equal Invasion.Direction.Down game.Invasion.Direction

    [<Test>]
    let ``the invasion moves down when it beyond the left edge and is moving left`` () =
        let onNextMoveBeyondLeftBorder = float Constraints.Bounds.Left
        let invader = Entity.updateXPos defaultInvader onNextMoveBeyondLeftBorder

        let game = createGameWithInvasion [invader] Invasion.Direction.Left
                   |> Game.updateGame <| timeToMove

        equal Invasion.Direction.Down game.Invasion.Direction

    [<Test>]
    let ``the invasion moves right when it beyond the left edge and is moving down`` () =
        let onNextMoveBeyondLeftBorder = float Constraints.Bounds.Left
        let invader = Entity.updateXPos defaultInvader onNextMoveBeyondLeftBorder

        let game = createGameWithInvasion [invader] Invasion.Direction.Down
                   |> Game.updateGame <| timeToMove

        equal Invasion.Direction.Right game.Invasion.Direction

    [<Test>]
    let ``the direction does not change when it is not yet time to move`` () =
        let onNextMoveBeyondLeftBorder = float Constraints.Bounds.Left
        let invader = Entity.updateXPos defaultInvader onNextMoveBeyondLeftBorder

        let game = createGameWithInvasion [invader] Invasion.Direction.Down
                   |> Game.updateGame <| timeToMove - 1.

        equal Invasion.Direction.Down game.Invasion.Direction
