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
        }

    type Main = Func<obj list, string, Item seq>

    [<AllowNullLiteral>]
    type ModuleEntryPointAttribute() = inherit Attribute()

    let findEntryPoint (assembly: Assembly) =
        assembly.GetExportedTypes()
        |> Array.collect(fun t -> t.GetMethods())
        |> Array.filter(fun m -> m.GetCustomAttribute<ModuleEntryPointAttribute>() <> null)
        |> Array.map(fun m -> Delegate.CreateDelegate(typeof<Main>, m) :?> Main)
        |> Array.head

    let compose = Assembly.LoadFrom >> findEntryPoint

    type Hop =
        {
            EntryPoints: Main list
        }

    let create assemblies = { EntryPoints = assemblies |> List.map compose }

    let execute arguments query hop = hop.EntryPoints |> Seq.collect (fun e -> e.Invoke(arguments, query))