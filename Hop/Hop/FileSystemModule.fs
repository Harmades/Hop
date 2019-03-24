module Hop.FileSystemModule

open Hop.Core
open System
open System.Diagnostics
open System.IO

let cDriveItem =
    {
        Name = "C:/"
        Description = "C:/ Drive"
        Image = DefaultImage
        Data = DirectoryInfo "C:/"
        Module = "FileSystem"
        Action = new Action (fun () -> Process.Start "C:/" |> ignore)
    }

let dDriveItem =
    {
        Name = "D:/"
        Description = "D:/ Drive"
        Image = DefaultImage
        Data = DirectoryInfo "D:/"
        Module = "FileSystem"
        Action = new Action (fun () -> Process.Start "D:/" |> ignore)
    }

let main arguments =
    match arguments.Tail with
        | [] when arguments.Head = String.Empty -> [cDriveItem; dDriveItem] |> List.toSeq
        | [] when arguments.Head = String.Empty || fuzzyMatch "C:/" arguments.Head < 2 -> Seq.singleton cDriveItem
        | [] when arguments.Head = String.Empty || fuzzyMatch "D:/" arguments.Head < 2 -> Seq.singleton dDriveItem
        | head :: _ ->
            match head.Data with
                | :? DirectoryInfo as directory ->
                    directory.EnumerateFileSystemInfos (arguments.Head + "*")
                    |> Seq.map (fun fsInfo ->
                        {
                            Name = fsInfo.Name
                            Description = sprintf "Date modified %s" (fsInfo.LastWriteTime.ToShortDateString())
                            Image = DefaultImage
                            Data = fsInfo
                            Module = "FileSystem"
                            Action = new Action (fun () -> Process.Start fsInfo.FullName |> ignore)
                        })
                | :? FileInfo as file -> Seq.empty
                | _ -> Seq.empty

        | _ -> Seq.empty

    |> (fun items -> { Items = items })