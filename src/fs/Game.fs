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
    Entity: Entity
}

type InvaderState = Open | Closed
type InvaderType = Large | Medium | Small
type InvaderProperties = {
    Entity: Entity
    InvaderState: InvaderState
    Type: InvaderType
}

type LaserProperties = {
    Entity: Entity;
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

let initialInvaders = [
    { Entity = { Position = {X = 3.; Y = 20.}
                 Bounds = { Width = 30; Height = 30 }
                 Velocity = None}
      InvaderState = Closed
      Type = Small} |> Invader]

let initialGame =
    {
        Entities = [
            { LaserProperties.Entity = {
                Position = {X = 105.; Y = 216.}
                Bounds = { Width = 20; Height = 30}
                Velocity = None
            }} |> Laser
        ] @ initialInvaders
    }
let shoot (position:Position) (game:Game) = game
let move (direction:Vector2) (game:Game) = game
let update (game:Game) (event:Event) = game