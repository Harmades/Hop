open Elmish
open Elmish.WPF
open Hop.App.Views
open System

type Model =
    {
        Text: string
    }

let init () =
    {
        Text = "Hay"
    }

type Message =
    | Change

let update message model =
    match message with
        | Change -> { model with Text = if model.Text = "Hay" then "Hoy" else "Hay" }

let bindings model dispatch =
    [
        "Change" |> Binding.cmd (fun m -> Change)
        "Text" |> Binding.oneWay (fun m -> m.Text)
    ]

[<EntryPoint; STAThread>]
let main argv = 
    Program.mkSimple init update bindings
    |> Program.runWindowWithConfig 
        { ElmConfig.Default with LogConsole = false; LogTrace = false }    
        (MainWindow())