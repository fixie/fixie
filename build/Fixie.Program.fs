module Fixie.EntryPoint.Program

open System
open Fixie.Execution

[<STAThread; EntryPoint>]
let main arguments =
    AssemblyRunner.Main(arguments)