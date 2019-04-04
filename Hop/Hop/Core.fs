module Hop.Core

open System
open System.Collections.Generic
open System.Diagnostics
open System.Drawing
open System.IO
open System.Reflection

let defaultImage = new Bitmap "./Assets/Hopx40.png"
let pageSize = 20

type Item =
    {
        Name: string
        Description: string
        Image: Bitmap
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
        Items: Item list
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

let logException (ex: Exception) =
    sprintf "[%s] %s :%s%s" (DateTime.Now.ToString()) ex.Message Environment.NewLine ex.StackTrace
    |> Trace.WriteLine

let load modulesDirectory =
    let mains =
        try
            Directory.GetDirectories modulesDirectory
            |> Array.collect (fun directory -> Directory.GetFiles (directory, "Hop.*.dll"))
        with | ex ->
            logException ex
            Array.Empty<string>()
        |> Array.choose (fun assembly ->
            try assembly |> compose |> Some with ex ->
                logException ex
                None)
        |> Map.ofArray
    { Modules = mains }

let execute query hop =
    hop.Modules
    |> Map.map (fun _ main ->
        try main.Invoke query with ex ->
            logException ex
            { Items = List.empty })
    |> Map.toList
    |> List.collect (fun (_, m) -> if m.Items.Length <= pageSize then m.Items else m.Items |> List.take pageSize)
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