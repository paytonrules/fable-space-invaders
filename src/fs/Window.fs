module SpaceInvaders.Window

open Fable.Core
open Fable.Import

type Position =
    {
        x: float;
        y: float;
    }

type Image = 
    {
        image: Browser.HTMLImageElement;
        position: Position;
    }
let createRenderer = 
    let canvas = Browser.document.getElementsByTagName_canvas().[0]
    let ctx = canvas.getContext_2d()
    let scale = 2.
    let w = scale * SpaceInvaders.Constraints.width
    let h = scale * SpaceInvaders.Constraints.height
    canvas.width <- w
    canvas.height <- h
    ctx.msImageSmoothingEnabled <- false
    ctx.scale (scale, scale)
    ctx.clearRect (0., 0., SpaceInvaders.Constraints.width, SpaceInvaders.Constraints.height)
    ctx.fillStyle <- U3.Case1 "rgb(0,0,0)"

    let clearToBlack() = ctx.fillRect (0., 0., SpaceInvaders.Constraints.width, SpaceInvaders.Constraints.height)

    let renderer = (fun (images:list<Image>) -> 
        clearToBlack()
    )
    renderer