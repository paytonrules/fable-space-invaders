namespace Test

open Fable.Core.Testing
open SpaceInvaders.Game


[<TestFixture>]
module Game =
    [<Test>]
    let ``it puts the first invader at 3. 20.`` () =
        let (Invader firstInvader) = List.head initialInvaders

        equal firstInvader.Entity.Position {X = 3.; Y = 20.} 