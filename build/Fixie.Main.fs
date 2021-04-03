module Fixie.Internal.___

open System
open System.Reflection

// The 'Fixie' package includes this file in test projects so
// that their tests can be executed. Do not modify this file.

[<STAThread; EntryPoint>]
let main customArguments =
    EntryPoint.Main(Assembly.GetExecutingAssembly(), customArguments).GetAwaiter().GetResult();