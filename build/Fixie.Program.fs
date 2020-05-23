module Fixie.Internal.Program

open System

[<STAThread; EntryPoint>]
let main arguments =
    EntryPoint.Main(typeof<Program>.Assembly, arguments)