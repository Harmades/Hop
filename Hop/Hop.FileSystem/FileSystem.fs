module Hop.FileSystem.Core

open Hop.Core
open System.IO
open Hop.FileSystem.Thumbnail
open System.Drawing

type FileSystemModule() =
    interface IModule with
        member this.Name = "FileSystem"
        member this.Query query =
            let root = match query.Stack with | head :: _ -> head.Data :?> DirectoryInfo | _ -> new DirectoryInfo("D:/")
            root.EnumerateFileSystemInfos()
            |> Seq.map (fun fs -> new Item(fs.Name, fs.FullName, lazy(Thumbnail.GetThumbnail(fs.FullName, 32, 32, ThumbnailOptions.None) :> Image), fs))