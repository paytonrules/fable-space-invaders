module SpaceInvaders.GameLoop

let step update state events delta = 
    update :: events
    |> List.fold (fun state f -> f state delta) state
let start stepFunc (checkStop:('a -> bool)) (render:('a -> unit)) initialState  () = () 

let addEvent (eventFunc:('a -> 'a)) = ()