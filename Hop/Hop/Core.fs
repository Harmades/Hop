module Hop.Core

open System
open System.Diagnostics
open System.Collections.Generic
open System.Drawing
open System.ComponentModel.Composition.Hosting
open System.ComponentModel.Composition

let defaultImage = Image.FromFile "./assets/hopx40.png"
let pageSize = 20

type Item(name: string, description: string, image: Lazy<Image>, data: obj) =
    member this.Name = name
    member this.Description = description
    member this.Image = image
    member this.Data = data

type Query = {
    Search: string
    Execute: bool
    Stack: Item list
}

[<InheritedExport>]
type IModule =
    abstract member Name: string
    abstract member Query: Query -> Item seq

type Hop = {
    Modules: IModule list
}

let logException (ex: Exception) =
    sprintf "[%s] %s :%s%s" (DateTime.Now.ToString()) ex.Message Environment.NewLine ex.StackTrace
    |> Trace.WriteLine

let executeModule (query: Query) (hopModule: IModule) =
    try
        hopModule.Query query
    with
        ex -> logException ex; Seq.empty

let execute query hop =
    hop.Modules
    |> Seq.collect (executeModule query)

let load() =
    use catalog = new AggregateCatalog()
    catalog.Catalogs.Add(new DirectoryCatalog("./modules"))
    use container = new CompositionContainer(catalog)
    let modules = container.GetExportedValues<IModule>() |> List.ofSeq
    { Modules = modules }

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