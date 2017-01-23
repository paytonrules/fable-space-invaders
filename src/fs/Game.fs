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

let speedPerMillisecond = 0.200

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

let pushLaser entities update = 
    entities
    |> List.map (function 
                | Laser e -> update e |> Laser 
                | e -> e)

let pushLaserLeft entities = pushLaser entities (fun e -> {e with LeftForce = true })
let pushLaserRight entities = pushLaser entities (fun e -> {e with RightForce = true })
let stopPushingLaserRight entities = pushLaser entities (fun e -> {e with RightForce = false })
let stopPushingLaserLeft entities = pushLaser entities (fun e -> {e with LeftForce = false })

let updateLaser laserProperties delta = 
    let direction = if laserProperties.RightForce && laserProperties.LeftForce then 0.
                    else if laserProperties.LeftForce then -1.
                    else if laserProperties.RightForce then 1.
                    else 0.

    let xMove = direction * speedPerMillisecond * delta
    let newX = laserProperties.Entity.Position.X + xMove
    let clampedX = match newX with
                   | newX when newX < (float SpaceInvaders.Constraints.Bounds.Left) ->
                    SpaceInvaders.Constraints.Bounds.Left |> float
                   | newX when newX + (float laserProperties.Entity.Bounds.Width) > (float SpaceInvaders.Constraints.Bounds.Right) ->
                    SpaceInvaders.Constraints.Bounds.Right - laserProperties.Entity.Bounds.Width |> float
                   | newX -> newX

    let newPosition = {laserProperties.Entity.Position with X = clampedX }
                    
    { laserProperties with Entity =
                           { laserProperties.Entity with Position = newPosition } }

let updateEntities game delta = 
    {game with Entities = game.Entities |> List.map (function
                                                     | Laser e -> updateLaser e delta |> Laser
                                                     | Invader e -> e |> Invader
                                                     | Bullet e -> e |> Bullet)}

let updateGame game timeSinceGameStarted = 
    let newGame = updateEntities game (timeSinceGameStarted - game.LastUpdate)
    {newGame with LastUpdate = timeSinceGameStarted}
let update (game:Game) (event:Event) = 
    match event with 
    | MoveLeft -> { game with Entities = pushLaserLeft game.Entities }
    | MoveRight -> { game with Entities = pushLaserRight game.Entities }
    | StopMoveRight -> { game with Entities = stopPushingLaserRight game.Entities }
    | StopMoveLeft -> { game with Entities = stopPushingLaserLeft game.Entities }
    | Update timeSinceGameStarted -> updateGame game timeSinceGameStarted
