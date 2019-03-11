namespace Hop.Core

module All =
    open System
    open System.Reflection

    type Item =
        {
            Name: string
            Description: string
            Image: string
            Data: obj
            Module: string
        }

    type Arguments =
        {
            Head: string
            Tail: Item list
        }

    type Query =
        | Autocomplete of Arguments
        | Execute of Item list

    type Result =
        | Message of string
        | Autocomplete of Item seq

    type Main = Func<Query, Result>

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

    let private executeImpl query hop = hop.Modules |> Map.map (fun _ main -> main.Invoke query) |> Map.toList |> List.map (fun (_, m) -> m)

    let autocomplete arguments hop =
        executeImpl (Query.Autocomplete arguments) hop
        |> Seq.choose (fun result -> match result with | Result.Autocomplete items -> Some items | _ -> None)
        |> Seq.collect id

    let execute (items: Item list) hop =
        let main = hop.Modules.[items.Head.Module]
        match main.Invoke (Query.Execute items) with
            | Result.Message message -> message
            | _ -> failwith "Execute yielded invalid result"