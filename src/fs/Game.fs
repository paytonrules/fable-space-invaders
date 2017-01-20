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
    Velocity: Vector2 option;
}

type BulletProperties = {
    Entity: Entity;
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

type Game = {
    Entities: Entities
}

type Event = 
| MoveLeft
| MoveRight
| Update
| Shoot
| StopMoveLeft
| StopMoveRight

let initialInvaders = [
    { Entity = { Position = {X = 3.; Y = 20.}
                 Bounds = { Width = 30; Height = 30 }
                 Velocity = None}
      InvaderState = Closed
      Type = Small} |> Invader]

let initialLaser = {
    LaserProperties.Entity = { Position = { X = 105.; Y = 216. } 
                               Bounds = { Width = 20; Height = 30 }
                               Velocity = None }
    RightForce = false
    LeftForce = false
}

let initialGame = { 
    Entities = [ (Laser initialLaser) ] @ initialInvaders
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

let update (game:Game) (event:Event) = 
    match event with 
    | MoveLeft -> { game with Entities = pushLaserLeft game.Entities }
    | MoveRight -> { game with Entities = pushLaserRight game.Entities }
    | StopMoveRight -> { game with Entities = stopPushingLaserRight game.Entities }
    | StopMoveLeft -> { game with Entities = stopPushingLaserLeft game.Entities }
    | Update -> game
