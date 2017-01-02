module SpaceInvaders.Game

open Fable.Core.JsInterop

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

type Position = Vector2
type Velocity = Vector2
type CharacterName = LargeInvader | MediumInvader | SmallInvader | Laser | Bullet
type State = Open | Closed

type Character = 
    {
    Name: CharacterName
    }

type MultiStateCharacter = Character * State

type Entity = 
    {
    Character: Character;
    Position: Position;
    Bounds: Bounds;
    Velocity: Velocity option;        
    }

type Game =
    {
    Bullet: Entity option;
    Invaders: Entity list;
    Laser: Entity;
    }
let shoot (position:Position) (game:Game) = game
let move (direction:Vector2) (game:Game) = game
let update (game:Game) = game