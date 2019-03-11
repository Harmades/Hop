module FileSystemModule

open Hop.Core.All
open System
open System.IO

let init () =
    { Name = "D:/"; Description = String.Empty; Image = ""; Data = DirectoryInfo "D:/"; Module = "FileSystem" }

let main query =
    match query with
        | Query.Autocomplete arguments ->
            match arguments.Tail with
                | [] -> (DirectoryInfo "D:/").EnumerateFileSystemInfos arguments.Head
                | head :: _ ->
                    match head.Data with
                        | :? DirectoryInfo as directory -> directory.EnumerateFileSystemInfos (arguments.Head + "*")
                        | :? FileInfo as file -> Seq.empty
                        | _ -> Seq.empty
            |> Seq.map (fun fsi ->
                            match fsi with
                                | :? DirectoryInfo as directory -> { Name = directory.Name; Description = ""; Image = ""; Data = directory; Module = "FileSystem" }
                                | :? FileInfo as file -> { Name = file.Name; Description = ""; Image = ""; Data = file; Module = "FileSystem" }
                                | _ -> failwith "Unexpected FileSystemInfo sub-type")
            |> Result.Autocomplete
        | Query.Execute arguments -> Result.Message ""