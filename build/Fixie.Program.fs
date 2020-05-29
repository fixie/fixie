module Fixie.Internal.Program

open System
open System.Reflection

[<STAThread; EntryPoint>]
let main arguments =
    EntryPoint.Main(Assembly.GetExecutingAssembly(), arguments)