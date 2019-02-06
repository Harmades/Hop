namespace Hop.Core

module IO =

    open Hop.Core.Function
    open System
    open System.Reflection
    open System.IO
    
    type ModuleAttribute() = inherit Attribute()

    type IOErrors =
        | NotFound of string
        | BadImageFormat of string
        | LoadException of string

    let mapParameter (parameter: ParameterInfo) =
        {
            Name = parameter.Name
            Type = parameter.ParameterType
        }

    let mapMethod moduleName (method: MethodInfo) =
        let parameters = 
            method.GetParameters()
            |> Array.map mapParameter
            |> List.ofArray
        {
            Name = method.Name
            Module = moduleName
            Parameters = parameters
            Type = method.ReturnType
        }

    let getFunctions (container: Type) =
        container.GetMethods(BindingFlags.Public)
        |> Array.map (mapMethod container.Name)

    let getTypes (assembly: Assembly) =
        assembly.GetExportedTypes()
        |> Array.filter (fun t -> t.GetCustomAttribute(typeof<ModuleAttribute>) <> null)

    let loadAssembly (assembly: Assembly) =
        assembly
        |> getTypes
        |> Array.collect getFunctions

    let loadFrom file =
        try
            file |> Assembly.LoadFrom |> Ok
        with
            | :? FileNotFoundException -> file |> NotFound |> Error
            | :? BadImageFormatException -> file |> BadImageFormat |> Error
            | :? FileLoadException -> file |> LoadException |> Error

    let loadModule path =
        loadFrom path
        |> Result.map (fun a -> a, loadAssembly a)

    let invoke (assembly: Assembly) func arguments =
        let container = assembly.GetType func.Module
        try
            container.InvokeMember(func.Name, BindingFlags.InvokeMethod, null, null, arguments) |> Ok
        with
            | ex -> Error ex
