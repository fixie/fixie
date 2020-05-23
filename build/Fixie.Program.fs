module Fixie.Internal.Program

open System

[<STAThread; EntryPoint>]
let main arguments =
    AssemblyRunner.Main(typeof<Program>.Assembly, arguments)