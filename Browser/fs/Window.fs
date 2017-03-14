module Window

open Fable.Core
open Fable.Import
open Fable.Core.JsInterop
open SpaceInvaders

open Image

let createImage data =
  let img = Browser.document.createElement_img()
  img.src <- data
  img

let createRenderer =
    let canvas = Browser.document.getElementsByTagName_canvas().[0]
    let ctx = canvas.getContext_2d()
    let scale = 2.
    let w = scale * Constraints.Width
    let h = scale * Constraints.Height
    canvas.width <- w
    canvas.height <- h
    ctx.msImageSmoothingEnabled <- false
    ctx?imageSmoothingEnabled <- false
    ctx?mozImageSmoothingEnabled <- false
    ctx.scale (scale, scale)
    ctx.clearRect (0., 0., Constraints.Width, Constraints.Height)
    ctx.fillStyle <- U3.Case1 "rgb(0,0,0)"

    let clearToBlack() = ctx.fillRect (0., 0., Constraints.Width, Constraints.Height)

    let renderer = (fun (images:list<Image<Browser.HTMLImageElement>>) ->
        clearToBlack()
        images |> List.iter (fun image -> ctx.drawImage(U3.Case1 image.Image, image.Position.X, image.Position.Y))
    )
    renderer
