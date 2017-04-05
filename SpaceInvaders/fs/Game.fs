namespace SpaceInvaders

type Speed = int
type Delta = float

type Sides = {
    Right: float;
    Left: float
}

type Bounds = {
    Width: int;
    Height: int
}

type Vector2 = {
    X: float;
    Y: float;
}

type Position = Vector2

type BulletProperties = {
    Velocity: Vector2;
}

type MissileProperties = MissileProperties of BulletProperties

type InvaderState = Open | Closed
type InvaderType = Large | Medium | Small
type InvaderProperties = {
    InvaderState: InvaderState;
    Type: InvaderType;
}

type LaserProperties = {
    LeftForce: bool;
    RightForce: bool;
}

type EntityProperties =
| Bullet of BulletProperties
| Invader of InvaderProperties
| Laser of LaserProperties
| RandomMissile of MissileProperties
| TrackingMissile of MissileProperties
| PoweredMissile of MissileProperties

type Box = {
    Position: Vector2;
    Bounds: Bounds;
}

type Entity  =  {
    Location: Box;
    Properties: EntityProperties;
}

type Entities = Entity list

type Invasion = {
    Invaders: Entities;
    TimeToMove: Delta;
    SinceLastMove: Delta;
    Direction: Vector2;
}
type Event =
| MoveLeft
| MoveRight
| Update of Delta
| Shoot
| StopMoveLeft
| StopMoveRight

module Constraints =
    type Rect = {
        Left: int;
        Right: int;
        Top: int
    }
    let Width = 224.
    let Height = 256.
    let Bounds = {
        Left = 3;
        Right = 220;
        Top = 15
    }

module Vector2 =
    let add vector1 vector2 =
        { X = vector1.X + vector2.X; Y = vector1.Y + vector2.Y }

    let scale vector scale =
        { X = vector.X * scale; Y = vector.Y * scale }

module Entity =
    let updatePosition location position =
        { location with Position = position}

    let updateBounds entity bounds =
        let location = { entity.Location with Bounds = bounds}
        { entity with Location = location }

    let updateXPos location x =
        let position = { X = x; Y = location.Position.Y }
        updatePosition location position

    let bottom entity = (float entity.Location.Bounds.Height + entity.Location.Position.Y)

    let right entity = (float entity.Location.Bounds.Width + entity.Location.Position.X)

    let isOverlapping (entityOne:Entity) entityTwo =
        not (entityTwo.Location.Position.X > right entityOne
              || right entityTwo < entityOne.Location.Position.X
              || entityTwo.Location.Position.Y > bottom entityOne
              || bottom entityTwo < entityOne.Location.Position.Y)

module Invasion =
    let columnWidth = 16.
    let rowHeight = 16.
    let columns = 11
    let rows = 6
    let totalInvaders = columns * rows
    let initialTimeToMove = 1000.

    module Direction =
        let Right = { X = 4.; Y = 0.}
        let Down = { X = 0.; Y = 4. }
        let Left = { X = -4.; Y = 0. }

    let create invaders =
        { TimeToMove = initialTimeToMove;
          Invaders = invaders;
          SinceLastMove = 0.;
          Direction = Direction.Right }

    let rightBounds invaders =
        invaders
        |> List.map (fun entity -> entity.Location.Position.X + float entity.Location.Bounds.Width)
        |> List.max

    let leftBounds invaders =
        invaders
        |> List.map (fun entity -> entity.Location.Position.X)
        |> List.min

    let bounds invaders =
        if invaders |> List.isEmpty
        then { Right = 0.; Left = 0. }
        else { Right = rightBounds invaders; Left = leftBounds invaders }

    let isTimeToMove invasion delta =
        delta + invasion.SinceLastMove >= invasion.TimeToMove

    let (|MovingRight|MovingDown|MovingLeft|) direction =
        if direction = Direction.Right
        then MovingRight
        else if direction = Direction.Left
        then MovingLeft
        else MovingDown

    let (|PastRightEdge|PastLeftEdge|WithinBounds|) entities =
        let bounds = bounds entities
        if bounds.Right >= float Constraints.Bounds.Right
        then PastRightEdge
        else if bounds.Left <= float Constraints.Bounds.Left
        then PastLeftEdge
        else WithinBounds

    let nextInvasionDirection invaders invasion =
        match invaders with
        | PastRightEdge ->
            match invasion.Direction with
            | MovingRight -> { invasion with Direction = Direction.Down }
            | MovingDown -> { invasion with Direction = Direction.Left }
            | MovingLeft -> invasion
        | PastLeftEdge ->
            match invasion.Direction with
            | MovingLeft -> { invasion with Direction = Direction.Down }
            | MovingDown -> { invasion with Direction = Direction.Right }
            | MovingRight -> invasion
        | WithinBounds -> invasion

    let removeShotInvaders invaders bullet =
        invaders |> List.filter (fun invader ->
                                    not <| Entity.isOverlapping invader bullet)

    // Keep in mind that the spaceship actually counts as one of the shots
    // Hot damn see this: http://www.computerarcheology.com/Arcade/SpaceInvaders/
    //
    type ShouldFireMissile = Entities -> Invasion -> Delta -> bool

module Missile =
    type MissileSpeedFunc = Entities -> Speed
    type NewMissileLocation = MissileProperties -> Entities
    type UpdateMissile = MissileSpeedFunc -> Entity -> Position


module Invader =

    let bounds = function
    | Small -> { Width = 8; Height = 8 }
    | Medium -> { Width = 11; Height = 8 }
    | Large -> { Width = 12; Height = 8 }

    let create (position, invaderType) =
        { Location = { Position= position;
                       Bounds = bounds invaderType;}
          Properties = { InvaderState = Closed;
                        Type = invaderType} |> Invader};

    let toggleState = function | Closed -> Open | Open -> Closed

    let updatePosition invader position =
        { invader with
            Location = Entity.updatePosition invader.Location position }

    let updateXPos invader x =
        { invader with
            Entity.Location = Entity.updateXPos invader.Location x }

    let update (invader, invaderProps) invasion delta =
        match Invasion.isTimeToMove invasion delta with
        | true ->
          let newPosition = Vector2.add invader.Location.Position invasion.Direction
          let location = { invader.Location with Position = newPosition }
          let invaderProps = { invaderProps with InvaderState = toggleState invaderProps.InvaderState } |> Invader
          { invader with Location = location; Properties = invaderProps }
        | false -> invader

module Laser =
    type LaserEntity = {
        Location: Box;
        Properties: LaserProperties;
    }
    let speedPerMillisecond = 0.200
    let bounds = { Width = 13; Height = 8 }
    let midpoint = (float bounds.Width) / 2. |> floor  // IOW - 6

    let toEntity laser =
        { Entity.Location = laser.Location
          Properties = laser.Properties |> Laser }

    let updateXPos laser x =
        { laser with
           LaserEntity.Location = Entity.updateXPos laser.Location x }

    let updateProperties laser properties =
        { laser with LaserEntity.Properties = properties }

    let clamp minMax value =
        match value with
        | value when value < fst minMax -> fst minMax
        | value when value > snd minMax -> snd minMax
        | _ -> value

    let create position =
        {
            Location = { Position = position;
                         Bounds = bounds; }
            Properties = { LeftForce = false; RightForce = false}
        }

    // These push functions are gross, but until the laser functions take a
    // laser and not an entity, I'm not sure I have something better
    let pushLaser laser forceUpdater =
        laser
        |> Option.map (fun laser -> { laser with LaserEntity.Properties = forceUpdater laser.Properties })

    let pushLaserLeft laser =
        pushLaser laser (fun e -> {e with LeftForce = true })

    let pushLaserRight laser =
        pushLaser laser (fun e -> {e with RightForce = true })

    let stopPushingLaserRight laser =
        pushLaser laser (fun e -> {e with RightForce = false })

    let stopPushingLaserLeft laser =
        pushLaser laser (fun e -> {e with LeftForce = false })

    let laserDirection laser =
        match laser.Properties with
        | { RightForce = true; LeftForce = true } -> 0.
        | { LeftForce = true; } -> -1.
        | { RightForce = true } -> 1.
        | _ -> 0.

    let calculateLaserMove delta direction =
        direction * speedPerMillisecond * delta

    let calculateNextXpos laser movement =
        laser.Location.Position.X + movement

    let update laser delta =
        let maxRight = Constraints.Bounds.Right -
                        laser.Location.Bounds.Width |> float
        let xRange = (float Constraints.Bounds.Left, maxRight)

        let clampedX = laser
                    |> laserDirection
                    |> calculateLaserMove delta
                    |> calculateNextXpos laser
                    |> clamp xRange

        let position = { laser.Location.Position with X = clampedX }
        let location = { laser.Location with Position = position }

        { laser with Location = location }

module Bullet =
    let Height = 4
    let DefaultVelocity = { X = 0.; Y = -0.1 }

    let create position bulletProperties =
        { Properties = bulletProperties
          Location = { Position = position
                       Bounds = { Width = 0; Height = 0 } } }

    let createWithDefaultProperties position =
        create position (Bullet { Velocity = DefaultVelocity })

    let update (bullet, properties) delta =
        let position = properties.Velocity
                       |> Vector2.scale <| delta
                       |> Vector2.add bullet.Location.Position

        let location = { bullet.Location with Position = position }
        { bullet with Location = location }

type Game = {
    Laser: Laser.LaserEntity option;
    Bullet: Entity option;
    LastUpdate: Delta;
    Invasion: Invasion;
}

module Game =
    let invasionUpperLeftCorner = { X = 3.; Y = 30.}

    let setBullet game bullet =
        { game with Bullet = Some(bullet) }

    let addBullet game =
        match game.Bullet with
        | None -> match game.Laser with
                  | Some laser ->
                    let offset = { X = Laser.midpoint;
                                  Y = (float -Bullet.Height) }
                    let bullet = Vector2.add laser.Location.Position offset
                                |> Bullet.createWithDefaultProperties
                    setBullet game bullet
                  | None -> game
        | Some _ -> game

    let positionForInvaderAtIndex i =
        let row = i / Invasion.columns |> float
        let column = i % Invasion.columns |> float
        { X = column * Invasion.columnWidth; Y = row * Invasion.rowHeight }
        |> Vector2.add invasionUpperLeftCorner;

    let typeForInvaderAtIndex = function
        | i when i < Invasion.columns * 2 -> Small
        | i when i >= Invasion.columns * 2 && i < Invasion.columns * 4 -> Medium
        | _ -> Large

    let positionAndTypeForInvaderAtIndex i =
        (positionForInvaderAtIndex i, typeForInvaderAtIndex i)

    let createGame laser invaders =
        {
            Laser = laser;
            Bullet = None;
            LastUpdate = 0.;
            Invasion = Invasion.create invaders
        }

    let updateInvasion game invasion =
        { game with Invasion = invasion }

    let moveEntities game delta =
        let movedInvaders = game.Invasion.Invaders
                            |> List.map (fun entity ->
                                          match entity.Properties with
                                          | Invader props -> Invader.update (entity, props) game.Invasion delta
                                          | _ -> failwith "Invalid invader!")

        let invasion = { game.Invasion with Invaders = movedInvaders }
        let newLaser = game.Laser |> Option.map (fun laser -> Laser.update laser delta)
        let newBullet = game.Bullet
                        |> Option.map
                          (fun bullet ->
                            match bullet.Properties with
                            | Bullet props ->
                                Bullet.update (bullet, props) delta
                            | _ -> bullet)

        { game with Laser = newLaser;
                    Invasion = invasion;
                    Bullet = newBullet }

    let isPastTheTopOfTheScreen bullet =
        bullet.Location.Position.Y + (float bullet.Location.Bounds.Height) < (float Constraints.Bounds.Top)

    let removeOffscreenBullet = function
      | Some bullet when isPastTheTopOfTheScreen bullet -> None
      | Some bullet -> Some bullet
      | None -> None

    let changeInvasionDirection invaders invasion delta =
        if Invasion.isTimeToMove invasion delta
        then Invasion.nextInvasionDirection invaders invasion
        else invasion

    let afterCollision bullet invaders =
        match bullet with
        | None -> (None, invaders)
        | Some bullet ->
            let invaders' = Invasion.removeShotInvaders invaders bullet
            if List.length invaders = List.length invaders'
            then (Some bullet, invaders)
            else (None, invaders')

    let updateTimeSinceLastMove invasion delta =
      match invasion.SinceLastMove + delta with
      | sinceLastMove when sinceLastMove < invasion.TimeToMove ->
          { invasion with SinceLastMove = sinceLastMove }
      | _ ->
          { invasion with SinceLastMove = 0. }

    let updateGame game timeSinceGameStarted =
        let delta = (timeSinceGameStarted - game.LastUpdate)
        let gameWithMovedEntities = moveEntities game delta

        let (bullet, invaders) = gameWithMovedEntities.Bullet
                                 |> removeOffscreenBullet
                                 |> afterCollision
                                 <| gameWithMovedEntities.Invasion.Invaders
        let newInvasion = { gameWithMovedEntities.Invasion with Invaders = invaders }

        { game with Laser = gameWithMovedEntities.Laser;
                    Bullet = bullet;
                    Invasion = newInvasion
                               |> changeInvasionDirection invaders <| delta
                               |> updateTimeSinceLastMove <| delta;
                    LastUpdate = timeSinceGameStarted }

    let update game event =
        match event with
        | MoveLeft -> { game with Laser = Laser.pushLaserLeft game.Laser }
        | MoveRight -> { game with Laser = Laser.pushLaserRight game.Laser }
        | StopMoveRight -> { game with Laser = Laser.stopPushingLaserRight game.Laser }
        | StopMoveLeft -> { game with Laser = Laser.stopPushingLaserLeft game.Laser }
        | Update timeSinceGameStarted -> updateGame game timeSinceGameStarted
        | Shoot -> addBullet game

    let initialInvaders = [0..(Invasion.columns * Invasion.rows) - 1]
                          |> List.map (positionAndTypeForInvaderAtIndex >> Invader.create)

    let initialLaser = Laser.create { X = 105.; Y = 216. }

    let initialGame =  createGame <| Some(initialLaser) <| initialInvaders
