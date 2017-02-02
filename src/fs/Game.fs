module SpaceInvaders.Game

type Delta = float

type Sides = {
    Right: float;
    Left: float
}

type Bounds = {
    Width: int;
    Height: int
}

type Vector2 = {
    X: float;
    Y: float;
}

module Vector2 =
    let add vector1 vector2 = 
        { X = vector1.X + vector2.X; Y = vector1.Y + vector2.Y }

    let scale vector scale = 
        { X = vector.X * scale; Y = vector.Y * scale }

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

module Entity = 
    let updatePosition entity position = 
        { entity with Position = position }

    let updateXPos entity x = 
        { entity with Position = { X = x; Y = entity.Position.Y } }

type InvasionStep = {
    TimeToMove: Delta;
    SinceLastMove: Delta;
    Direction: Vector2;
}


type Entities = Entity list

type Game = {
    Entities: Entities;
    LastUpdate: Delta;
    Invasion: InvasionStep;
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

module Invasion = 
    let columnWidth = 16.
    let rowHeight = 16.
    let columns = 11
    let rows = 6
    let totalInvaders = columns * rows
    let initialTimeToMove = 1000.

    module Direction = 
        let Right = { X = 4.; Y = 0.}
        let Down = { X = 0.; Y = 4. }
        let Left = { X = -4.; Y = 0. }

    let invadersFrom entities = 
        entities |> List.filter ( fun entity -> 
                                    match entity.Properties with
                                    | Invader _ -> true 
                                    | _ -> false )

    let rightBounds invaders = 
        invaders
        |> List.map (fun entity -> entity.Position.X + float entity.Bounds.Width)
        |> List.max

    let leftBounds invaders = 
        invaders
        |> List.map (fun entity -> entity.Position.X)
        |> List.min

    let bounds entities = 
        let invaders = invadersFrom entities
        if invaders |> List.isEmpty
        then { Right = 0.; Left = 0. }
        else { Right = rightBounds invaders; Left = leftBounds invaders }

    let isTimeToMove invasion delta = 
        delta + invasion.SinceLastMove >= invasion.TimeToMove 

    let (|MovingRight|MovingDown|MovingLeft|) direction = 
        if direction = Direction.Right
        then MovingRight
        else if direction = Direction.Left
        then MovingLeft
        else MovingDown

    let (|PastRightEdge|PastLeftEdge|WithinBounds|) entities = 
        let bounds = bounds entities
        if bounds.Right >= float SpaceInvaders.Constraints.Bounds.Right 
        then PastRightEdge
        else if bounds.Left <= float SpaceInvaders.Constraints.Bounds.Left
        then PastLeftEdge
        else WithinBounds

    let nextInvasionDirection game = 
        let updatedInvasion = 
            match game.Entities with
            | PastRightEdge ->
                match game.Invasion.Direction with
                | MovingRight -> { game.Invasion with Direction = Direction.Down }
                | MovingDown -> { game.Invasion with Direction = Direction.Left }
                | MovingLeft -> game.Invasion
            | PastLeftEdge ->
                match game.Invasion.Direction with
                | MovingLeft -> { game.Invasion with Direction = Direction.Down }
                | MovingDown -> { game.Invasion with Direction = Direction.Right }
                | MovingRight -> game.Invasion
            | WithinBounds -> game.Invasion

        { game with Invasion = updatedInvasion }

module Invader = 
    let create (position, invaderType) = 
        { Position = position;
          Bounds = { Width = 30; Height = 30 };
          Properties = { InvaderState = Closed; 
                         Type = invaderType} |> Invader};

    let toggleState = function | Closed -> Open | Open -> Closed

    let update (invader, invaderProps) invasion delta = 
        if Invasion.isTimeToMove invasion delta 
        then 
            let newPosition = Vector2.add invader.Position invasion.Direction  
            let invaderProps = { invaderProps with InvaderState = toggleState invaderProps.InvaderState } |> Invader
            { invader with Position = newPosition; Properties = invaderProps }
        else invader

module Laser = 
    let speedPerMillisecond = 0.200
    let bounds = { Width = 13; Height = 8 }
    let midpoint = (float bounds.Width) / 2. |> floor  // IOW - 6
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

    let create position bulletProperties = 
        { Properties = bulletProperties;
          Position = position;
          Bounds = { Width = 0; Height = 0 }}

    let createWithDefaultProperties position = 
        create position (Bullet { Velocity = { X = 0.; Y = -0.1 }})

    let update bullet delta = 
        let newPosition = match bullet.Properties with
                          | Bullet properties -> 
                                properties.Velocity
                                |> Vector2.scale <| delta
                                |> Vector2.add bullet.Position
                          | _ -> bullet.Position

        { bullet with Position = newPosition }

let isBullet entity = 
    match entity.Properties with
    | Bullet e -> true
    | _ -> false

let findLaser entities = 
    let isLaser entity = 
        match entity.Properties with
        | Laser e -> true
        | _ -> false

    entities
    |> List.filter isLaser
    |> (function | [laser] -> Some laser | _ -> None)

let findBullet entities = 
    entities
    |> List.filter isBullet
    |> (function | [bullet] -> Some bullet | _ -> None)

let addBullet game = 
    match findBullet game.Entities with
    | None -> match findLaser game.Entities with
              | Some laser -> 
                let offset = { X = Laser.midpoint;
                               Y = (float -Bullet.Height) }
                let bullet = Vector2.add laser.Position offset
                             |> Bullet.createWithDefaultProperties 
                { game with Entities = game.Entities @ [bullet] }
              | None -> game
    | Some _ -> game

let invasionUpperLeftCorner = { X = 3.; Y = 30.}

let positionForInvaderAtIndex i = 
    let row = i / Invasion.columns |> float
    let column = i % Invasion.columns |> float
    { X = column * Invasion.columnWidth; Y = row * Invasion.rowHeight }
    |> Vector2.add invasionUpperLeftCorner;

let typeForInvaderAtIndex = function
    | i when i < Invasion.columns * 2 -> Small
    | i when i >= Invasion.columns * 2 && i < Invasion.columns * 4 -> Medium
    | _ -> Large

let positionAndTypeForInvaderAtIndex i = 
    (positionForInvaderAtIndex i, typeForInvaderAtIndex i)

let initialInvaders = [0..(Invasion.columns * Invasion.rows) - 1] 
                      |> List.map (positionAndTypeForInvaderAtIndex >> Invader.create)

let initialLaser = Laser.create { X = 105.; Y = 216. }

let createGame entities = 
    { 
        Entities = entities;
        LastUpdate = 0.;
        Invasion = { TimeToMove = Invasion.initialTimeToMove; 
                     SinceLastMove = 0.; 
                     Direction = Invasion.Direction.Right }
    }
let initialGame = [ initialLaser ] @ initialInvaders |> createGame

let updateInvasion game invasion = 
    { game with Invasion = invasion }

let updateEntities game delta = 
    { game with Entities = game.Entities |> List.map (fun entity -> 
                                                        match entity.Properties with 
                                                        | Laser _ -> Laser.update entity delta
                                                        | Invader props -> Invader.update (entity, props) game.Invasion delta
                                                        | Bullet _ -> Bullet.update entity delta) }

let isPastTheTopOfTheScreen entity = 
    entity.Position.Y + (float entity.Bounds.Height) < (float SpaceInvaders.Constraints.Bounds.Top)

let removeOffscreenBullet game = 
    let isBulletOffScreen entity = isBullet entity && isPastTheTopOfTheScreen entity
    {game with Entities = List.filter (isBulletOffScreen >> not) game.Entities}

let changeInvasionDirection game delta =
    if Invasion.isTimeToMove game.Invasion delta
    then Invasion.nextInvasionDirection game
    else game

let updateTimestamp game timeSinceGameStarted = 
    { game with LastUpdate = timeSinceGameStarted}

let updateTimeSinceLastMove game delta = 
    match game.Invasion.SinceLastMove + delta with
    | sinceLastMove when sinceLastMove < game.Invasion.TimeToMove ->
        updateInvasion game { game.Invasion with SinceLastMove = sinceLastMove }
    | _ -> 
        updateInvasion game { game.Invasion with SinceLastMove = 0. }

let updateGame game timeSinceGameStarted = 
    let delta = (timeSinceGameStarted - game.LastUpdate)
    updateEntities game delta
    |> removeOffscreenBullet
    |> changeInvasionDirection <| delta
    |> updateTimeSinceLastMove <| delta
    |> updateTimestamp <| timeSinceGameStarted

let update (game:Game) (event:Event) = 
    match event with 
    | MoveLeft -> { game with Entities = Laser.pushLaserLeft game.Entities }
    | MoveRight -> { game with Entities = Laser.pushLaserRight game.Entities }
    | StopMoveRight -> { game with Entities = Laser.stopPushingLaserRight game.Entities }
    | StopMoveLeft -> { game with Entities = Laser.stopPushingLaserLeft game.Entities }
    | Update timeSinceGameStarted -> updateGame game timeSinceGameStarted
    | Shoot -> addBullet game