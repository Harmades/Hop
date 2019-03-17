module FileSystemModule

open Hop.Core.All
open System
open System.IO
open System.Diagnostics

let init () =
    {
        Name = "D:/"
        Description = String.Empty
        Image = ""
        Data = DirectoryInfo "D:/"
        Module = "FileSystem"
        Action = new Action (fun () -> Process.Start "D:/" |> ignore)
    }

let main arguments =
    match arguments.Tail with
        | [] -> (DirectoryInfo "D:/").EnumerateFileSystemInfos (arguments.Head + "*")
        | head :: _ ->
            match head.Data with
                | :? DirectoryInfo as directory -> directory.EnumerateFileSystemInfos (arguments.Head + "*")
                | :? FileInfo as file -> Seq.empty
                | _ -> Seq.empty
    |> Seq.map (fun fsInfo ->
        {
            Name = fsInfo.Name
            Description = sprintf "Date modified %s" (fsInfo.LastWriteTime.ToShortDateString())
            Image = ""
            Data = fsInfo
            Module = "FileSystem"
            Action = new Action (fun () -> Process.Start fsInfo.FullName |> ignore)
        })
    |> (fun items -> { Items = items })