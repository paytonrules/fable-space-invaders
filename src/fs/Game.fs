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

let initialGame =
    {
        Entities = [
            { LaserProperties.Entity = {
                Position = {X = 105.; Y = 216.};
                Bounds = { Width = 20; Height = 30}
                Velocity = None
            }} |> Laser
        ]
    }
let shoot (position:Position) (game:Game) = game
let move (direction:Vector2) (game:Game) = game
let update (game:Game) = game