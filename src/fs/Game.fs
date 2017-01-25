module SpaceInvaders.Game

type Bounds = {
    Width: int;
    Height: int;
}

type Vector2 = {
    X: float;
    Y: float;
}

type Position = Vector2

type Entity = {
    Position: Vector2;
    Bounds: Bounds;
}

type BulletProperties = {
    Entity: Entity;
    Velocity: Vector2;
}

type InvaderState = Open | Closed
type InvaderType = Large | Medium | Small
type InvaderProperties = {
    Entity: Entity;
    InvaderState: InvaderState;
    Type: InvaderType;
}

type LaserProperties = {
    Entity: Entity;
    LeftForce: bool;
    RightForce: bool;
}

type EntityType = 
| Bullet of BulletProperties
| Invader of InvaderProperties
| Laser of LaserProperties

type Entities = EntityType list

type Delta = float
type Game = {
    Entities: Entities
    LastUpdate: Delta
}

type Event = 
| MoveLeft
| MoveRight
| Update of Delta
| Shoot
| StopMoveLeft
| StopMoveRight

let clamp minMax value = 
    match value with
    | value when value < fst minMax -> fst minMax
    | value when value > snd minMax -> snd minMax
    | _ -> value

let replaceEntity entityType newEntity = 
    match entityType with 
    | Laser laser -> { laser with Entity = newEntity } |> Laser
    | Bullet bullet -> { bullet with Entity = newEntity } |> Bullet
    | Invader invader -> { invader with Entity = newEntity } |> Invader

let getEntity entityType = 
    match entityType with 
    | Laser laser -> laser.Entity
    | Bullet bullet -> bullet.Entity 
    | Invader invader -> invader.Entity


module Laser = 
    let speedPerMillisecond = 0.200
    let bounds = { Width = 11; Height = 8 }

    let create position = 
        let entity = { Position = position;
                       Bounds = bounds }

        {
            LaserProperties.Entity = entity;
            LeftForce = false;
            RightForce = false
        } |> Laser

    let pushLaser entities update = 
        entities
        |> List.map (function 
                    | Laser e -> update e |> Laser 
                    | e -> e)

    let pushLaserLeft entities = pushLaser entities (fun e -> {e with LeftForce = true })
    let pushLaserRight entities = pushLaser entities (fun e -> {e with RightForce = true })
    let stopPushingLaserRight entities = pushLaser entities (fun e -> {e with RightForce = false })
    let stopPushingLaserLeft entities = pushLaser entities (fun e -> {e with LeftForce = false })

    let midpoint laser = 
        (float laser.Entity.Bounds.Width) / 2. |> ceil

    let laserDirection laserProperties = 
        match laserProperties with 
        | { RightForce = true; LeftForce = true } -> 0.
        | { LeftForce = true } -> -1.
        | { RightForce = true } -> 1.
        | _ -> 0.

    let calculateLaserMove delta direction = 
        direction * speedPerMillisecond * delta
            
    let calculateNextXpos laserProperties movement = 
        laserProperties.Entity.Position.X + movement

    let update laserProperties delta = 
        let maxRight = SpaceInvaders.Constraints.Bounds.Right - 
                        laserProperties.Entity.Bounds.Width |> float
        let xRange = (float SpaceInvaders.Constraints.Bounds.Left, maxRight)

        let clampedX = laserProperties
                    |> laserDirection
                    |> calculateLaserMove delta
                    |> calculateNextXpos laserProperties
                    |> clamp xRange

        let newPosition = {laserProperties.Entity.Position with X = clampedX }
                        
        { laserProperties with Entity =
                                        { laserProperties.Entity with Position = newPosition } }


module Bullet = 
    let Height = 4

    let create position = 
        { Velocity = { X = 0.; Y = 0. };
          Entity = {Position = position;
                    Bounds = { Width = 0; Height = 0 }}} |> Bullet

let findLaser entities = 
    entities
    |> List.tryPick  (function Laser e -> Some e | _ -> None)

let findBullet entities = 
    entities
    |> List.tryPick  (function Bullet e -> Some e | _ -> None)

let addBullet game = 
    match findBullet game.Entities with
    | None -> match findLaser game.Entities with
              | Some laser -> 
                let offset = { X = Laser.midpoint laser;
                               Y = (float -Bullet.Height) }

                let bullet = Bullet.create { X = laser.Entity.Position.X + offset.X;
                                             Y = laser.Entity.Position.Y + offset.Y }
                { game with Entities = game.Entities @ [bullet] }
              | None -> game
    | Some _ -> game

let initialInvaders = [
    { Entity = { Position = {X = 3.; Y = 20.}
                 Bounds = { Width = 30; Height = 30 }}
      InvaderState = Closed
      Type = Small} |> Invader]

let initialLaser = {
    LaserProperties.Entity = { Position = { X = 105.; Y = 216. } 
                               Bounds = { Width = 11; Height = 8 }}
    RightForce = false
    LeftForce = false
}

let initialGame = { 
    Entities = [ (Laser initialLaser) ] @ initialInvaders;
    LastUpdate = 0.
}


let updateEntities game delta = 
    {game with Entities = game.Entities |> List.map (function
                                                     | Laser e -> Laser.update e delta |> Laser
                                                     | Invader e -> e |> Invader
                                                     | Bullet e -> e |> Bullet)}

let updateGame game timeSinceGameStarted = 
    let newGame = updateEntities game (timeSinceGameStarted - game.LastUpdate)
    {newGame with LastUpdate = timeSinceGameStarted}
let update (game:Game) (event:Event) = 
    match event with 
    | MoveLeft -> { game with Entities = Laser.pushLaserLeft game.Entities }
    | MoveRight -> { game with Entities = Laser.pushLaserRight game.Entities }
    | StopMoveRight -> { game with Entities = Laser.stopPushingLaserRight game.Entities }
    | StopMoveLeft -> { game with Entities = Laser.stopPushingLaserLeft game.Entities }
    | Update timeSinceGameStarted -> updateGame game timeSinceGameStarted
    | Shoot -> addBullet game
