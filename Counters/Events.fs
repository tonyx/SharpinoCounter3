
namespace sharpinoCounter
open System
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils
open Sharpino
open Sharpino.Core
open sharpinoCounter.Counter

type IntOrUnit =
    | Int of int
    | Unit
type CounterEvents =
    | Cleared of IntOrUnit 
    | Incremented 
    | Decremented 
        interface Event<Counter> with
            member this.Process (counter: Counter) =
                match this with
                | Cleared Unit -> counter.Clear ()
                | Cleared (Int x)  -> counter.Clear x
                | Incremented  -> counter.Increment ()
                | Decremented  -> counter.Decrement ()
        static member Deserialize (serializer: ISerializer, json: Json) =
            serializer.Deserialize<CounterEvents>(json)    
        member this.Serialize (serializer: ISerializer) =
            this
            |> serializer.Serialize

