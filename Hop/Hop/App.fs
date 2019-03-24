module Hop.App

open Hop.App.Views
open Hop.Core
open System
open System.Windows
open System.Windows.Input
open System.ComponentModel

type Model =
    {
        Arguments: Arguments
        Items: Item seq
        Hop: Hop
    }

type Message =
    | Query of string
    | Push of Item
    | Pop
    | Execute of Item

type HopModule = | Hop | Reload

let hopModuleMain arguments =
    match arguments.Tail with
        | [] when arguments.Head = String.Empty || fuzzyMatch "Hop" arguments.Head < 3 ->
            Seq.singleton {
                Name = "Hop"
                Description = "Hop actions and settings"
                Image = DefaultImage
                Data = Hop
                Module = "Hop"
                Action = new Action (id)
            }
        | head :: _ when obj.Equals (head.Data, Hop) ->
            Seq.singleton {
                Name = "Reload"
                Description = "Reload modules"
                Image = DefaultImage
                Data = Reload
                Module = "Hop"
                Action = new Action (id)
            }
        | _ -> Seq.empty
    |> (fun items -> { Items = items })

let init () =
    let loadedHop = load "Modules"
    let hop = { loadedHop with Modules = loadedHop.Modules |> Map.add "Hop" (new Func<Arguments, Result> (hopModuleMain)) }
    let arguments = { Head = ""; Tail = [] }
    let result = execute arguments hop
    { Arguments = arguments; Items = result.Items; Hop = hop }

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
            match execute arguments model.Hop with
                | { Result.Items = items } when Seq.isEmpty items -> model
                | { Result.Items = items } ->
                    { model with Arguments = arguments; Items = items }
        | Pop ->
            let tail =
                match model.Arguments.Tail with
                    | [] -> []
                    | _ :: tail -> tail
            let arguments = { model.Arguments with Head = ""; Tail = tail }
            let result = execute arguments model.Hop
            { model with Arguments = arguments; Items = result.Items }
        | Query query ->
            let arguments = { model.Arguments with Head = query }
            let result = execute arguments model.Hop
            { model with Arguments = arguments; Items = result.Items }
        | Execute item when obj.Equals (item, Reload) ->
            init ()
        | Execute item ->
            item.Action.Invoke()
            let arguments = { Head = ""; Tail = [] }
            let result = execute arguments model.Hop
            { model with Arguments = arguments; Items = result.Items }

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
    member this.ExecuteCommand = createCommand (fun o -> this.Update (Execute (o :?> ItemViewModel).Model))
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