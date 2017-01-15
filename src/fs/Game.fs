module SpaceInvaders.Game

type Bounds = 
    {
    Width: int;
    Height: int;
    }

type Vector2 = 
    {
    X: float;
    Y: float;
    }

type InvaderState = Open | Closed

type Position = Vector2

type Entity = 
    {
    Position: Vector2;
    Bounds: Bounds;
    Velocity: Vector2 option;        
    }

type MultiStateEntity = Entity * InvaderState
type Bullet = Bullet of Entity
type Invader = Invader of MultiStateEntity
type Laser = Laser of Entity

type Game =
    {
    Bullet: Bullet option;
    Invaders: Invader list;
    Laser: Laser;
    }

let initialGame =
    {
        Invaders = []; 
        Laser = {Position = {X = 10.; Y = 20.};
                 Bounds = { Width = 20; Height = 30}
                 Velocity = None} |> Laser;
        Bullet = None
    }
let shoot (position:Position) (game:Game) = game
let move (direction:Vector2) (game:Game) = game
let update (game:Game) = game