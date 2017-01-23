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


let initialInvaders = [
    { Entity = { Position = {X = 3.; Y = 20.}
                 Bounds = { Width = 30; Height = 30 }}
      InvaderState = Closed
      Type = Small} |> Invader]

let initialLaser = {
    LaserProperties.Entity = { Position = { X = 105.; Y = 216. } 
                               Bounds = { Width = 13; Height = 30 }}
    RightForce = false
    LeftForce = false
}

let initialGame = { 
    Entities = [ (Laser initialLaser) ] @ initialInvaders;
    LastUpdate = 0.
}

let clamp minMax value = 
    match value with
    | value when value < fst minMax -> fst minMax
    | value when value > snd minMax -> snd minMax
    | _ -> value


module Laser = 
    let speedPerMillisecond = 0.200
    let pushLaser entities update = 
        entities
        |> List.map (function 
                    | Laser e -> update e |> Laser 
                    | e -> e)

    let pushLaserLeft entities = pushLaser entities (fun e -> {e with LeftForce = true })
    let pushLaserRight entities = pushLaser entities (fun e -> {e with RightForce = true })
    let stopPushingLaserRight entities = pushLaser entities (fun e -> {e with RightForce = false })
    let stopPushingLaserLeft entities = pushLaser entities (fun e -> {e with LeftForce = false })

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
    let create position = 
        { Velocity = { X = 0.; Y = 0. };
          Entity = {Position = { X = 0.; Y = 0. };
          Bounds = { Width = 0; Height = 0 }}} |> Bullet

let findBullet entities = 
    entities
    |> List.tryPick  (function Bullet e -> Some e | _ -> None)

let addBullet game = 
    match findBullet game.Entities with
    | Some _ -> game
    | None ->
        let bullet = Bullet.create { X = 0.; Y = 0. }

        { game with Entities = game.Entities @ [bullet] }

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
