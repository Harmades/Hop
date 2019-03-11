open Hop.Core.All
open System
open System.Windows.Forms
open System.Drawing

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
    //let searchPanel = new FlowLayoutPanel ()
    //searchPanel.Dock <- DockStyle.Top
    //for argument in model.Arguments.Tail do
    //    let panel = new FlowLayoutPanel ()
    //    let nameLabel = new Label ()
    //    nameLabel.Text <- argument.Name
    //    nameLabel.Dock <- DockStyle.Fill
    //    panel.Controls.Add nameLabel
    //    let pictureBox = new PictureBox ()
    //    if argument.Image <> String.Empty then
    //        let bitmap = new Bitmap (argument.Image)
    //        pictureBox.Image <- bitmap
    //        pictureBox.Dock <- DockStyle.Right
    //        panel.Controls.Add pictureBox
    //    searchPanel.Controls.Add panel
    //let queryTextBox = new TextBox ()
    //queryTextBox.Text <- model.Arguments.Head
    //queryTextBox.Dock <- DockStyle.Right
    //searchPanel.Controls.Add queryTextBox
    
    let itemsLayoutPanel = new FlowLayoutPanel ()
    itemsLayoutPanel.Dock <- DockStyle.Fill
    itemsLayoutPanel.FlowDirection <- FlowDirection.TopDown
    for item in model.Items do
        let panel = new FlowLayoutPanel ()
        let nameLabel = new Label ()
        nameLabel.Text <- item.Name
        //nameLabel.Dock <- DockStyle.Top
        panel.Controls.Add nameLabel
        itemsLayoutPanel.Controls.Add nameLabel
        let descriptionLabel = new Label ()
        descriptionLabel.Text <- item.Description
        descriptionLabel.Dock <- DockStyle.Bottom
        panel.Controls.Add descriptionLabel
        let pictureBox = new PictureBox ()
        if item.Image <> String.Empty then
            let bitmap = new Bitmap (item.Image)
            pictureBox.Image <- bitmap
            pictureBox.Dock <- DockStyle.Right
            panel.Controls.Add pictureBox
        //itemsLayoutPanel.Controls.Add panel
    
    let rootPanel = new FlowLayoutPanel ()
    rootPanel.Controls.Add itemsLayoutPanel

    let form = new Form ()
    form.Controls.Add itemsLayoutPanel
    form

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
    Application.EnableVisualStyles ()
    Application.SetCompatibleTextRenderingDefault false
    let model = init ()
    let view = view model
    Application.Run view
    0