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

type BulletProperties = {
    Velocity: Vector2;
}

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

type Entity  =  {
    Position: Vector2;
    Bounds: Bounds;
    Properties: EntityProperties;
}

type Entities = Entity list

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


module Laser = 
    let speedPerMillisecond = 0.200
    let bounds = { Width = 13; Height = 8 }

    let updateProperties laser properties = 
        { laser with Properties = properties }

    let properties entity = 
        match entity.Properties with
        | Laser prop -> Some(prop)
        | _ -> None
    
    let create position = 
        {
            Position = position;
            Bounds = bounds;
            Properties = { LeftForce = false; RightForce = false} |> Laser
        } 

    let pushLaser entities update = 
        entities
        |> List.map (fun entity ->
                        match entity.Properties with
                        | Laser prop -> { entity with Properties = update prop |> Laser  }
                        | _ -> entity)

    let pushLaserLeft entities = pushLaser entities (fun e -> {e with LeftForce = true })
    let pushLaserRight entities = pushLaser entities (fun e -> {e with RightForce = true })
    let stopPushingLaserRight entities = pushLaser entities (fun e -> {e with RightForce = false })
    let stopPushingLaserLeft entities = pushLaser entities (fun e -> {e with LeftForce = false })

    let midpoint laser = 
        (float laser.Bounds.Width) / 2. |> ceil

    let laserDirection laser = 
        let someDirection = properties laser
                            |> Option.map (function 
                                           | { RightForce = true; LeftForce = true } -> 0.
                                           | { LeftForce = true; } -> -1.
                                           | { RightForce = true } -> 1.
                                           | _ -> 0.)

        match someDirection with
        | Some direction -> direction
        | None -> 0.

    let calculateLaserMove delta direction = 
        direction * speedPerMillisecond * delta
            
    let calculateNextXpos laser movement = 
        laser.Position.X + movement

    let update laser delta = 
        let maxRight = SpaceInvaders.Constraints.Bounds.Right - 
                        laser.Bounds.Width |> float
        let xRange = (float SpaceInvaders.Constraints.Bounds.Left, maxRight)

        let clampedX = laser
                    |> laserDirection
                    |> calculateLaserMove delta
                    |> calculateNextXpos laser
                    |> clamp xRange

        let newPosition = {laser.Position with X = clampedX }
                        
        { laser with Position = newPosition }


module Bullet = 
    let Height = 4

    let create position = 
        { Properties = { Velocity = { X = 0.; Y = 0. } } |> Bullet;
          Position = position;
          Bounds = { Width = 0; Height = 0 }}

let findLaser entities = 
    let isLaser entity = 
        match entity.Properties with
        | Laser e -> true
        | _ -> false

    entities
    |> List.filter isLaser
    |> (function | [laser] -> Some laser | _ -> None)

let findBullet entities = 
    let isBullet entity = 
        match entity.Properties with
        | Bullet e -> true
        | _ -> false

    entities
    |> List.filter isBullet
    |> (function | [bullet] -> Some bullet | _ -> None)

let addBullet game = 
    match findBullet game.Entities with
    | None -> match findLaser game.Entities with
              | Some laser -> 
                let offset = { X = Laser.midpoint laser;
                               Y = (float -Bullet.Height) }

                let bullet = Bullet.create { X = laser.Position.X + offset.X;
                                             Y = laser.Position.Y + offset.Y }
                { game with Entities = game.Entities @ [bullet] }
              | None -> game
    | Some _ -> game

let initialInvaders = [
    {  Position = {X = 3.; Y = 20.};
       Bounds = { Width = 30; Height = 30 };
       Properties = { InvaderState = Closed; 
                      Type = Small} |> Invader
    }]

let initialLaser = Laser.create { X = 105.; Y = 216. }

let initialGame = { 
    Entities = [ initialLaser ] @ initialInvaders;
    LastUpdate = 0.
}

let updateEntities game delta = 
    { game with Entities = game.Entities |> List.map (fun entity -> 
                                                        match entity.Properties with 
                                                        | Laser _ -> Laser.update entity delta
                                                        | Invader _ -> entity
                                                        | Bullet _ -> entity) }

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
