open Elmish
open Elmish.WPF
open Hop.App.Views
open System
open Hop.Core.All
open Elmish.WPF.Utilities

type Model =
    {
        Query: string
        Items: Item seq
    }

let designItems = 
    [
        { Name = "Turn Off"; Description = "Turn the computer off"; Image = ""; Data = None }
        { Name = "Reboot"; Description = "Reboot the computer off"; Image = ""; Data = None }
    ]

let init () =
    {
        Query = "";
        Items = designItems
    }

type Message =
    | Query of string

let update message model =
    match message with
        | Query query -> { model with Query = query }

let itemBindings () =
    [
        "Name" |> Binding.oneWay (fun (m, i) -> i.Name)
        "Description" |> Binding.oneWay (fun (m, i) -> i.Description)
        "Image" |> Binding.oneWay (fun (m, i) -> i.Image)
    ]

let bindings model dispatch =
    [
        "Query" |> Binding.oneWay (fun m -> m.Query)
        "Items" |> Binding.subBindingSeq id (fun m -> m.Items) (fun i -> i.Name) itemBindings
    ]

[<EntryPoint; STAThread>]
let main argv = 
    Program.mkSimple init update bindings
    |> Program.runWindowWithConfig 
        { ElmConfig.Default with LogConsole = false; LogTrace = false }    
        (MainWindow())
