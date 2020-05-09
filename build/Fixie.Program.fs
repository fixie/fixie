module Fixie.EntryPoint.Program

open System
open Fixie.Internal

[<STAThread; EntryPoint>]
let main arguments =
    AssemblyRunner.Main(typeof<Program>.Assembly, arguments)