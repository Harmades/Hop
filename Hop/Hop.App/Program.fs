open Hop.Core.All
open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Windows.Media.Imaging
open System.Windows.Input

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

let view model =
    let argumentsGrid = new Grid ()
    argumentsGrid.ColumnDefinitions.Add (ColumnDefinition( Width = GridLength.Auto ))
    argumentsGrid.ColumnDefinitions.Add (ColumnDefinition())
    let argumentsListView = new ListView ()
    for item in model.Arguments.Tail do
        let nameTextBlock = new TextBlock ( Text = item.Name )
        //let image = new Image ( Source = new BitmapImage (new Uri(item.Image)) )
        let itemGrid = new Grid ()
        itemGrid.ColumnDefinitions.Add (ColumnDefinition())
        itemGrid.ColumnDefinitions.Add (ColumnDefinition())
        itemGrid.Children.Add nameTextBlock |> ignore
        //itemGrid.Children.Add image |> ignore
        Grid.SetColumn (nameTextBlock, 0)
        //Grid.SetColumn (image, 1)
        let listViewItem = new ListViewItem ( Content = itemGrid )
        argumentsListView.Items.Add listViewItem |> ignore
    let queryTextBox = new TextBox ( Text = model.Arguments.Head )
    Grid.SetColumn (argumentsListView, 0)
    Grid.SetColumn (queryTextBox, 1)
    argumentsGrid.Children.Add argumentsListView |> ignore
    argumentsGrid.Children.Add queryTextBox |> ignore

    let itemListView = new ListView ()
    itemListView.InputBindings.Add (new InputBinding(createCommand (fun o -> )))
    for item in model.Items do
        let itemGrid = new Grid ()
        itemGrid.ColumnDefinitions.Add (ColumnDefinition())
        itemGrid.ColumnDefinitions.Add (ColumnDefinition())
        let identifierGrid = new Grid ()
        identifierGrid.RowDefinitions.Add (RowDefinition())
        identifierGrid.RowDefinitions.Add (RowDefinition())
        let nameTextBlock = new TextBlock ( Text = item.Name )
        let descriptionTextBlock = new TextBlock ( Text = item.Description )
        //let image = new Image ( Source = new BitmapImage (new Uri(item.Image)) )
        Grid.SetRow (nameTextBlock, 0)
        Grid.SetRow (descriptionTextBlock, 1)
        identifierGrid.Children.Add nameTextBlock |> ignore
        identifierGrid.Children.Add descriptionTextBlock |> ignore
        Grid.SetColumn (identifierGrid, 0)
        //Grid.SetColumn (image, 1)
        itemGrid.Children.Add identifierGrid |> ignore
        //itemGrid.Children.Add image |> ignore
        let listViewItem = new ListViewItem ( Content = itemGrid )
        itemListView.Items.Add listViewItem |> ignore

    let mainGrid = new Grid ()
    mainGrid.RowDefinitions.Add (RowDefinition( Height = GridLength.Auto ))
    mainGrid.RowDefinitions.Add (RowDefinition())
    Grid.SetRow (argumentsGrid, 0)
    Grid.SetRow (itemListView, 1)
    mainGrid.Children.Add argumentsGrid |> ignore
    mainGrid.Children.Add itemListView |> ignore
    let window = new Window ()
    window.Content <- mainGrid
    window

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
    let model = init ()
    let mainWindow = view model
    app.Run mainWindow