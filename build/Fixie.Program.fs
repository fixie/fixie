module Fixie.Internal.Program

open System
open System.Reflection

[<STAThread; EntryPoint>]
let main customArguments =
    EntryPoint.Main(Assembly.GetExecutingAssembly(), customArguments)