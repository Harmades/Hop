module Hop.Everything

open Hop.Core
open System
open System.Diagnostics
open System.Drawing
open System.IO
open System.Runtime.InteropServices
open System.Text

[<DllImport("Everything32.dll")>]
extern int Everything_SetSearch (string lpSearchString)
[<DllImport("Everything32.dll")>]
extern void Everything_SetMax (uint32 dwMax)
[<DllImport("Everything32.dll")>]
extern bool Everything_Query (bool bWait)
[<DllImport("Everything32.dll")>]
extern int Everything_GetNumResults ()
[<DllImport("Everything32.dll")>]
extern void Everything_GetResultFullPathName (uint32 nIndex, StringBuilder lpString, uint32 nMaxCount)
[<DllImport("Everything32.dll")>]
extern bool Everything_IsFileResult (uint32 nIndex)
[<DllImport("Everything32.dll")>]
extern bool Everything_IsFolderResult (uint32 nIndex)
[<DllImport("Everything32.dll")>]
extern bool Everything_IsVolumeResult (uint32 nIndex)
[<DllImport("Everything32.dll")>]
extern int Everything_GetLastError ()
[<DllImport("Everything32.dll")>]
extern void Everything_Reset ()

type FileSystemType =
    | File
    | Folder
    | Volume

type FileSystem =
    {
        Name: string
        FullPath: string
        FileSystemType: FileSystemType
    }

type EverythingModule =
    | Go

let folderBitmap = new Bitmap "./Folderx32.png"

let bufferSize = 260

let search query =
    if Everything_SetSearch query = 0
    then
        Everything_SetMax (pageSize |> Convert.ToUInt32)
        if Everything_Query true
        then
            let resultCount = Everything_GetNumResults() |> Convert.ToInt32
            [for i in 0 .. resultCount - 1 ->
                let builder = StringBuilder (bufferSize)
                Everything_GetResultFullPathName (i |> Convert.ToUInt32, builder, bufferSize |> Convert.ToUInt32)
                let path = builder.ToString()
                let name = Path.GetFileName path
                let fileSystemType =
                    match i with
                        | i when Everything_IsFileResult (i |> Convert.ToUInt32) -> File
                        | i when Everything_IsFolderResult (i |> Convert.ToUInt32) -> Folder
                        | i when Everything_IsVolumeResult (i |> Convert.ToUInt32) -> Volume
                        | _ -> raise (ApplicationException (sprintf "Invalid result type for %s" name))
                { Name = name; FullPath = path; FileSystemType = fileSystemType }]
        else raise (ApplicationException (sprintf "Everything interop failed with error code %i" (Everything_GetLastError())))
    else raise (ApplicationException (sprintf "Everything interop failed with error code %i" (Everything_GetLastError())))


[<ModuleEntryPoint>]
let main (arguments: Arguments) =
    match arguments.Tail with
        | [] ->
            List.singleton {
                Name = "Go"
                Description = "Reach with Everything"
                Image = defaultImage
                Module = "Everything"
                Data = Go
                Action = new Action (id)
            }
        | head :: _ when obj.Equals (head.Data, Go) ->
            let results = 
                search arguments.Head
                |> List.map (fun fs ->
                    {
                        Name = fs.Name
                        Description = fs.FullPath
                        Image =
                            match fs.FileSystemType with
                                | File when File.Exists fs.FullPath -> (Icon.ExtractAssociatedIcon fs.FullPath).ToBitmap()
                                | Folder when Directory.Exists fs.FullPath -> folderBitmap
                                | Volume -> defaultImage
                                | _ -> defaultImage
                        Module = "Everything"
                        Data = None
                        Action = new Action (fun () -> Process.Start fs.FullPath |> ignore)
                    })
            Everything_Reset()
            results
        | _ -> List.empty
    |> (fun items -> { Items = items })