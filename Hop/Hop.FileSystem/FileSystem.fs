module Hop.FileSystem.Core

open Hop.Core
open System.IO
open Hop.FileSystem.Thumbnail
open System.Drawing
open System.Diagnostics
open System

let rootModuleItem = new Item("Files", "Search filesystem.", lazy(defaultImage), "FileSystemModule")

let toItem (fileSystemInfo: FileSystemInfo) =
    new Item(fileSystemInfo.Name, fileSystemInfo.FullName, lazy(Thumbnail.GetThumbnail(fileSystemInfo.FullName, 32, 32, ThumbnailOptions.None) :> Image), fileSystemInfo)

let toFileSystemSearch search =
    sprintf "*%s*" search

type FileSystemModule() =
    interface IModule with
        member this.Name = "FileSystem"
        member this.Query query =
            match query.Stack with
                | head :: _ ->
                    match head.Data with
                        | :? string as m when obj.Equals(m, rootModuleItem.Data) -> DriveInfo.GetDrives() |> Seq.map(fun d -> d.RootDirectory) |> Seq.map toItem |> Seq.filter (fun i -> String.IsNullOrEmpty query.Search || fuzzyMatch i.Name query.Search < 3)
                        | :? DirectoryInfo as directory when query.Execute -> Process.Start(directory.FullName) |> ignore; Seq.empty
                        | :? DirectoryInfo as directory when not query.Execute -> directory.EnumerateFileSystemInfos(toFileSystemSearch query.Search) |> Seq.map toItem
                        | :? FileInfo as file when query.Execute -> Process.Start(file.FullName) |> ignore; Seq.empty
                        | _ -> Seq.empty
                | [] -> Seq.singleton rootModuleItem
