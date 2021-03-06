﻿module Constants

open Xelmish.Model

let highScoreFile = "./highscore.txt"

let gridWidth = 10
let gridHeight = 20
let padding = 30
let tiledim = 20
let resWidth = padding + (tiledim * gridWidth) + padding + (tiledim * 6) + padding
let resHeight = padding + (tiledim * gridHeight) + padding

let scoreFor count =
    match count with
    | 1 -> 10
    | 2 -> 30
    | 3 -> 60
    | 4 -> 100
    | _ -> 0
let level score = int (floor (float score / 150.)) + 1
let minDrop = 100L
let dropPerLevel = 100L

type Shape = {
    rotations: ((int * int) list) []
    colour: Colour
}

let shapes = [
    {   rotations = [|  [0,0; 1,0; 0,1; 1,1] |] // O
        colour = Colour.Cyan }
    {   rotations = [|  [0,0; 1,0; 2,0; 3,0]    // I
                        [2,0; 2,1; 2,2; 2,3] |]
        colour = Colour.Red }
    {   rotations = [|  [0,0; 1,0; 1,1; 2,1]    // Z
                        [2,0; 2,1; 1,1; 1,2] |] 
        colour = Colour.Green }
    {   rotations = [|  [1,0; 2,0; 1,1; 0,1]    // S
                        [1,0; 1,1; 2,1; 2,2] |] 
        colour = Colour.Blue }
    {   rotations = [|  [0,0; 1,0; 2,0; 0,1]    // L
                        [0,0; 1,0; 1,1; 1,2]
                        [0,1; 1,1; 2,1; 2,0]
                        [1,0; 1,1; 1,2; 2,2] |] 
        colour = Colour.Orange }
    {   rotations = [|  [0,0; 1,0; 2,0; 2,1]    // J
                        [1,0; 1,1; 1,2; 0,2]
                        [0,0; 0,1; 1,1; 2,1]
                        [1,0; 2,0; 1,1; 1,2] |] 
        colour = Colour.Magenta }
    {   rotations = [|  [0,0; 1,0; 2,0; 1,1]    // T
                        [1,0; 1,1; 1,2; 0,1]
                        [0,1; 1,1; 2,1; 1,0]
                        [1,0; 1,1; 1,2; 2,1] |]
        colour = Colour.Silver }
]