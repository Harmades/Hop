open Hop.App.Views
open Hop.Core
open System
open System.ComponentModel
open System.Diagnostics
open System.Windows
open System.Windows.Input

let errorsLog = "errors.log"

type Model = {
    Query: Query
    Results: Item seq
    Hop: Hop
}

type Message =
    | Search of string
    | Push of Item
    | Pop
    | Execute of Item

let init () =
    let hop = load()
    let query = { Search = ""; Stack = []; Execute = false }
    let results = execute query hop
    { Query = query; Results = results; Hop = hop }

let createCommand execute = {
    new ICommand with
        member this.Execute (argument) = execute argument
        member this.CanExecute (_) = true
        member this.add_CanExecuteChanged(_) = ()
        member this.remove_CanExecuteChanged(_) = ()
}

let update model message =
    match message with
        | Push item ->
            let query = { Search = ""; Stack = item :: model.Query.Stack; Execute = false }
            let results = execute query model.Hop
            { model with Query = query; Results = results }
        | Pop ->
            let query = { model.Query with Search = ""; Stack = if model.Query.Stack.IsEmpty then List.empty else model.Query.Stack.Tail }
            let results = execute query model.Hop
            { model with Query = query; Results = results }
        | Search search ->
            let query = { model.Query with Search = search }
            let results = execute query model.Hop
            { model with Query = query; Results = results }
        | Execute item ->
            let query = { Search = ""; Stack = item :: model.Query.Stack; Execute = true }
            execute query model.Hop |> Seq.toList |> ignore
            let resetQuery = { Search = ""; Stack = []; Execute = false }
            let results = execute resetQuery model.Hop
            { model with Query = resetQuery; Results = results }

type ItemViewModel(model: Item) =
    member val Model = model with get, set
    member this.Name with get () = this.Model.Name
    member this.Description with get () = this.Model.Description
    member this.Image with get () = this.Model.Image.Value

type MainViewModel(model: Model) =
    let ev = new Event<_,_>()
    member val Model = model with get, set
    member this.Query
        with get () = this.Model.Query.Search
        and set (value) = this.Update (Search value)
    member this.Stack = this.Model.Query.Stack |> List.map ItemViewModel |> List.rev
    member this.Results = this.Model.Results |> Seq.map ItemViewModel
    member this.PushCommand = createCommand (fun o -> this.Update (Push (o :?> ItemViewModel).Model))
    member this.PopCommand = createCommand (fun _ -> this.Update Pop)
    member this.ExecuteCommand = createCommand (fun o -> this.Update (Execute (o :?> ItemViewModel).Model))
    member private this.Update message =
        this.Model <- update this.Model message
        ev.Trigger (this, new PropertyChangedEventArgs "Query")
        ev.Trigger (this, new PropertyChangedEventArgs "Results")
        ev.Trigger (this, new PropertyChangedEventArgs "Stack")

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member this.PropertyChanged = ev.Publish

let bind model = new MainViewModel(model)

[<EntryPoint>]
[<STAThread>]
let main _ = 
    Trace.Listeners.Add (new TextWriterTraceListener (errorsLog, "HopListener")) |> ignore
    Trace.AutoFlush <- true
    let app = new Application ()
    let model = init ()
    let viewModel = bind model
    let mainWindow = new MainWindow ( DataContext = viewModel )
    app.Run mainWindow