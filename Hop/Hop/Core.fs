module Hop.Core

open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Reflection

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

let min3 a b c =
    min a (min b c)

let memoize(f) =    
  let cache = new Dictionary<_, _>()
  (fun x ->
      let succ, v = cache.TryGetValue(x)
      if succ then v else 
        let v = f(x) 
        cache.Add(x, v)
        v)

let rec private fuzzyMatchImpl (a: string) (b: string) =
    match a, b with
        | c, _ when c.Length = 0 -> b.Length
        | _, d when d.Length = 0 -> a.Length
        | _, _ ->
            let cost = if String.Equals(a, b, StringComparison.InvariantCultureIgnoreCase) then 0 else 1
            min3
                (fuzzyMatchImpl a.[0 .. a.Length - 2] b + 1)
                (fuzzyMatchImpl a b.[0 .. b.Length - 2] + 1)
                (fuzzyMatchImpl a.[0 .. a.Length - 2] b.[0 .. b.Length - 2] + cost)

let fuzzyMatch = fuzzyMatchImpl >> memoize