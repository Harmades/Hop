module Hop.Core

open System.IO
open System
open System.Diagnostics
open Newtonsoft.Json
open System.Collections.Generic
open System.Drawing
open Newtonsoft.Json.Linq
open System.Management.Automation

let defaultImage = Image.FromFile "./Assets/Hopx40.png"
let pageSize = 20

type Item() =
    member val Name = "" with get, set
    member val Description = "" with get, set
    member val Image = Array.Empty<byte>() with get, set
    [<JsonExtensionData>] member val Data = new Dictionary<string, JToken>() with get, set


type Query = {
    Search: string
    Execute: bool
    Stack: Item list
}

type Module = {
    Name: string
    Path: string
}

type Hop = {
    Modules: Module list
}

let executeModule query hopModule =
    use powershell = PowerShell.Create()
    powershell.AddScript(File.ReadAllText hopModule.Path) |> ignore
    powershell.AddArgument query |> ignore
    let output = powershell.Invoke() |> Seq.last
    output.ToString()

let logException (ex: Exception) =
    sprintf "[%s] %s :%s%s" (DateTime.Now.ToString()) ex.Message Environment.NewLine ex.StackTrace
    |> Trace.WriteLine

let execute query hop =
    let json = query |> JsonConvert.SerializeObject
    hop.Modules
    |> List.collect (fun m ->
        try
            executeModule json m |> JsonConvert.DeserializeObject<Item list>
        with
            ex -> logException ex; []
    )

let loadModule script = {
    Name = Path.GetFileName script
    Path = script
}

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