module Hop.Core

open System
open System.Reflection
open System.IO
open System.Diagnostics

let DefaultImage = "pack://application:,,,/Assets/Hopx40.png"

type Item =
    {
        Name: string
        Description: string
        Image: string
        Data: obj
        Module: string
        Action: Action
    }

type Arguments =
    {
        Head: string
        Tail: Item list
    }

type Result =
    {
        Items: Item seq
    }

type Main = Func<Arguments, Result>

[<AllowNullLiteral>]
type ModuleEntryPointAttribute () = inherit Attribute ()

let findMain (assembly: Assembly) =
    let entryPoint =
        assembly.GetExportedTypes ()
        |> Array.collect(fun t -> t.GetMethods ())
        |> Array.filter(fun m -> m.GetCustomAttribute<ModuleEntryPointAttribute> () <> null)
        |> Array.map(fun m -> Delegate.CreateDelegate (typeof<Main>, m) :?> Main)
        |> Array.head
    assembly.FullName, entryPoint

let compose = Assembly.LoadFrom >> findMain

type Hop =
    {
        Modules: Map<string, Main>
    }

let load modulesDirectory =
    let mains =
        try Directory.GetFiles (modulesDirectory, "*.dll") with | ex ->
            Trace.WriteLine (ex.Message + " : " + ex.StackTrace)
            Array.Empty<string>()
        |> Array.choose (fun assembly ->
            try assembly |> compose |> Some with ex ->
                Trace.WriteLine (ex.Message + " : " + ex.StackTrace)
                None)
        |> Map.ofArray
    { Modules = mains }

let execute query hop =
    hop.Modules
    |> Map.map (fun _ main ->
        try main.Invoke query with ex ->
            Trace.WriteLine (ex.Message + " : " + ex.StackTrace)
            { Items = Seq.empty })
    |> Map.toSeq
    |> Seq.collect (fun (_, m) -> m.Items)
    |> (fun items -> { Items = items })
