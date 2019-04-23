﻿open Elmish
open Xelmish.Model
open Xelmish.Viewables
open Config

type PlayingModel = {
    playerX: int
    invaders: (int * int * int * int * InvaderKind) list
    invaderDirection: int
    bunkers: (int * int) list
    projectiles: (int * int * int) list
    lastShuffle: int64
    shuffleInterval: int64
    freeze: bool
} and InvaderKind = Small | Medium | Large

let init () = 
    {
        playerX = resWidth / 2 - (playerDim / 2)
        invaders = 
            [0..invadersPerRow*invaderRows-1]
            |> List.map (fun i ->
                let row = i / invadersPerRow
                let col = i % invadersPerRow
                let size = match row with 0 -> Small | 1 | 2 -> Medium | _ -> Large
                let w, h = match size with Small -> smallSize | Medium -> mediumSize | Large -> largeSize
                let lw, _ = largeSize
                let offset = (lw - w) / 2
                padding + col * (lw + padding) + offset, padding + row * (h + padding), w, h, size)
        invaderDirection = 1
        bunkers = []
        projectiles = []
        lastShuffle = 0L
        shuffleInterval = 500L
        freeze = false
    }, Cmd.none

type Message = 
    | MovePlayer of dir: int
    | FireProjectile of x: int * y: int * velocity: int
    | ShuffleInvaders of int64
    | MoveProjectiles
    | PlayerHit
    | Victory

let shuffleInvaders time model = 
    let (newInvaders, valid) = 
        (([], true), model.invaders)
        ||> List.fold (fun (acc, valid) (x, y, w, h, kind) ->
            if not valid then (acc, valid)
            else
                let nx = x + invaderShuffleAmount * model.invaderDirection
                if nx < padding || nx + w > (resWidth - padding) then acc, false
                else (nx, y, w, h, kind)::acc, true)
    if not valid then
        { model with 
            invaders = model.invaders |> List.map (fun (x, y, w, h, kind) -> x, y + h/2, w, h, kind)
            invaderDirection = model.invaderDirection * -1
            lastShuffle = time
            shuffleInterval = max 50L (model.shuffleInterval - invaderShuffleIncrease) }, 
        Cmd.none
    else
        let command = 
            let playerRect = rect model.playerX playerY playerDim playerDim
            if List.exists (fun (x, y, w, h, _) -> (rect x y w h).Intersects(playerRect)) model.invaders 
            then Cmd.ofMsg PlayerHit else Cmd.none
        { model with invaders = newInvaders; lastShuffle = time }, command

let moveProjectiles model =
    let playerProjectile (acc, playerHit, invadersHit) (x, y, v) =
        let newY = y + v
        if newY < 0 then acc, false, invadersHit
        else
            let projectileRect = rect x y 1 projectileHeight
            let hitInvaders = 
                model.invaders 
                |> List.filter (fun (ix, iy, iw, ih, _) -> 
                    projectileRect.Intersects(rect ix iy iw ih))
            if hitInvaders <> [] then
                acc, playerHit, hitInvaders @ invadersHit
            else
                (x, newY, v)::acc, playerHit, invadersHit

    let invaderProjectile (acc, playerHit, invadersHit) (x, y, v) =
        let newY = y + v
        if newY > resHeight then acc, false, invadersHit
        else
            let overlapsPlayer = 
                x >= model.playerX && x < model.playerX + playerDim
                && newY >= playerY
            if overlapsPlayer then acc, true, invadersHit
            else (x, newY, v)::acc, playerHit, invadersHit

    let newProjectiles, playerHit, invadersHit =
        (([], false, []), model.projectiles)
        ||> List.fold (fun (acc, playerHit, invadersHit) (x, y, v) ->
            if v > 0 then invaderProjectile (acc, playerHit, invadersHit) (x, y, v)
            else playerProjectile (acc, playerHit, invadersHit) (x, y, v))
            
    let newInvaders = List.except invadersHit model.invaders
    let command = 
        if playerHit then Cmd.ofMsg PlayerHit 
        elif newInvaders = [] then Cmd.ofMsg Victory 
        else Cmd.none
    { model with projectiles = newProjectiles; invaders = newInvaders }, command

let update message model =
    match message with
    | MovePlayer dir ->
        let newPos = min (resWidth - padding - playerDim) (max padding (model.playerX + dir * playerSpeed))
        { model with playerX = newPos }, Cmd.none
    | FireProjectile (x, y, v) ->
        { model with projectiles = (x, y, v)::model.projectiles }, Cmd.none
    | ShuffleInvaders time -> shuffleInvaders time model        
    | MoveProjectiles -> moveProjectiles model
    | PlayerHit -> { model with freeze = true }, Cmd.none
    | Victory -> { model with freeze = true }, Cmd.none

let view model dispatch =
    [
        yield! model.invaders 
            |> List.map (fun (x, y, w, h, _) ->
                colour Colour.Green (w, h) (x, y))

        yield colour Colour.Red (playerDim, playerDim) (model.playerX, playerY)

        yield! model.projectiles
            |> List.map (fun (x, y, _) ->
                colour Colour.White (1, projectileHeight) (x, y))

        if not model.freeze then
            yield fun _ inputs _ -> 
                if inputs.totalGameTime - model.lastShuffle > model.shuffleInterval then
                    dispatch (ShuffleInvaders inputs.totalGameTime)

            yield fun _ _ _ -> dispatch MoveProjectiles

            yield whilekeydown Keys.Left (fun () -> dispatch (MovePlayer -1))
            yield whilekeydown Keys.Right (fun () -> dispatch (MovePlayer 1))

        yield onkeydown Keys.Space (fun () -> 
            if not (List.exists (fun (_, _, v) -> v < 0) model.projectiles) then
                let x = model.playerX + playerDim / 2
                let y = resHeight - (playerDim + padding) - projectileHeight - 1
                dispatch (FireProjectile (x, y, -projectileSpeed)))

        yield onkeydown Keys.Escape exit
    ]

[<EntryPoint>]
let main _ =
    let config: GameConfig = {
        clearColour = Some Colour.Black
        resolution = Windowed (resWidth, resHeight)
        assetsToLoad = []
        mouseVisible = false
        showFpsInConsole = true
    }

    Program.mkProgram init update view
    |> Xelmish.Program.runGameLoop config

    0