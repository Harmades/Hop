namespace Hop.Core

module Function =
    open System

    type Parameter =
        {
            Name: string
            Type: Type
        }

    type Function =
        {
            Name: string
            Module: string
            Description: string
            Image: string
            Parameters: Parameter list
        }

    type Item =
        {
            Name: string
            Description: string
            Image: string
            Type: Type
        }
