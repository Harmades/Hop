open Hop.Core.All
open System
open System.Windows
open System.Windows.Controls

type Model =
    {
        Arguments: Arguments
        Items: Item seq
        Message: string
        Hop: Hop
    }

type Message =
    | Query of string
    | Push of Item
    | Pop
    | Execute of Item

let init () =
    let main = FileSystemModule.main
    let arguments = { Head = String.Empty; Tail = [FileSystemModule.init ()] }
    let hop = { Modules = Map.empty |> Map.add "FileSystem" (Func<Query, Result>(main)) }
    let results = autocomplete arguments hop
    { Arguments = arguments; Items = results; Message = String.Empty; Hop = hop }

let view model =
    let mainGrid = new Grid ()
    mainGrid.ColumnDefinitions.Add (ColumnDefinition())
    mainGrid.ColumnDefinitions.Add (ColumnDefinition())
    mainGrid

let update model message =
    match message with
        | Push item ->
            let arguments = { Head = ""; Tail = item :: model.Arguments.Tail }
            let items = autocomplete arguments model.Hop
            { model with Arguments = arguments; Items = items }
        | Pop ->
            let arguments = { model.Arguments with Head = ""; Tail = model.Arguments.Tail.Tail }
            { model with Arguments = arguments}
        | Query query ->
            let arguments = { model.Arguments with Head = query }
            let results = autocomplete arguments model.Hop
            { model with Arguments = arguments; Items = results }
        | Execute item ->
            let arguments = item :: model.Arguments.Tail
            let result = execute arguments model.Hop
            { Arguments = { Head = ""; Tail = [] }; Items = []; Hop = model.Hop; Message = result }

[<EntryPoint>]
[<STAThread>]
let main argv = 
    let app = new Application ()
    let mainWindow = new Window ()
    app.Run mainWindow