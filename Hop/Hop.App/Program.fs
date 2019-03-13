open Hop.Core.All
open Hop.App.Views
open System
open System.Windows
open System.Windows.Input
open System.ComponentModel

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

let createCommand execute =
    {
        new ICommand with
            member this.Execute (argument) = execute argument
            member this.CanExecute (_) = true
            member this.add_CanExecuteChanged(_) = ()
            member this.remove_CanExecuteChanged(_) = ()
    }

let update model message =
    match message with
        | Push item ->
            let arguments = { Head = ""; Tail = item :: model.Arguments.Tail }
            let items = autocomplete arguments model.Hop
            { model with Arguments = arguments; Items = items }
        | Pop ->
            let arguments = { model.Arguments with Head = ""; Tail = model.Arguments.Tail.Tail }
            let items = autocomplete arguments model.Hop
            { model with Arguments = arguments; Items = items }
        | Query query ->
            let arguments = { model.Arguments with Head = query }
            let results = autocomplete arguments model.Hop
            { model with Arguments = arguments; Items = results }
        | Execute item ->
            let arguments = item :: model.Arguments.Tail
            let result = execute arguments model.Hop
            { Arguments = { Head = ""; Tail = [] }; Items = []; Hop = model.Hop; Message = result }

type ItemViewModel(model: Item) =
    member val Model = model with get, set
    member this.Name with get () = this.Model.Name
    member this.Description with get () = this.Model.Description
    member this.Image with get () = this.Model.Image
    member this.Module with get () = this.Model.Module

type MainViewModel(model: Model) =
    let ev = new Event<_,_>()
    member val Model = model with get, set
    member this.Query
        with get () = this.Model.Arguments.Head
        and set (value) = this.Update (Query value)
    member this.Arguments = this.Model.Arguments.Tail |> List.map ItemViewModel |> List.rev
    member this.Items = this.Model.Items |> Seq.map ItemViewModel
    member this.PushCommand = createCommand (fun o -> this.Update (Push (o :?> ItemViewModel).Model))
    member this.PopCommand = createCommand (fun _ -> this.Update Pop)
    member this.ExecuteCommand (item: obj) = this.Update (Execute (item :?> ItemViewModel).Model)
    member private this.Update message =
        this.Model <- update this.Model message
        ev.Trigger (this, new PropertyChangedEventArgs "Query")
        ev.Trigger (this, new PropertyChangedEventArgs "Items")
        ev.Trigger (this, new PropertyChangedEventArgs "Arguments")

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member this.PropertyChanged = ev.Publish

let bind model = new MainViewModel(model)

[<EntryPoint>]
[<STAThread>]
let main argv = 
    let app = new Application ()
    let model = init ()
    let viewModel = bind model
    let mainWindow = new MainWindow ( DataContext = viewModel )
    app.Run mainWindow