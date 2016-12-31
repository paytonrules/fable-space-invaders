module App.Message

open Fable.Core.JsInterop

type ImageData = string // validation?

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
type CharacterName = Invader | Laser | Bullet
type State = Open | Closed

type Character = 
    {
    Name: CharacterName
    }

type MultiStateCharacter = Character * State

let a = ({Name = Invader}, Open)

type Entity = 
    {
    Character: Character;
    Position: Position;
    Bounds: Bounds;
    Velocity: Velocity option;        
    }

type Update = Entity -> unit
let update (entity:Entity) = () 

type Game =
    {
    Bullet: Entity option;
    Invaders: Entity list;
    }

let message = importMember<string> "../js/Message.js"
