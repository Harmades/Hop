module Hop.Core

open System
open System.Reflection
open System.IO

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

let createFromAssemblies assemblies = assemblies |> List.map compose |> Map.ofList

let load modulesDirectory =
    let mains =
        Directory.GetFiles (modulesDirectory, "*.dll")
        |> Array.map compose
        |> Map.ofArray
    { Modules = mains }

let execute query hop =
    hop.Modules
    |> Map.map (fun _ main -> main.Invoke query)
    |> Map.toSeq
    |> Seq.collect (fun (_, m) -> m.Items)
    |> (fun items -> { Items = items })
