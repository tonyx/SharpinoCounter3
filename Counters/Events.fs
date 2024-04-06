
namespace sharpinoCounter
open System
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils
open Sharpino
open Sharpino.Core
open sharpinoCounter.Counter

type CounterEvents =
    | Cleared 
    | Incremented 
    | Decremented 
        interface Event<Counter> with
            member this.Process (counter: Counter) =
                match this with
                | Cleared  -> counter.Clear ()
                | Incremented  -> counter.Increment ()
                | Decremented  -> counter.Decrement ()
        static member Deserialize (serializer: ISerializer, json: Json) =
            serializer.Deserialize<CounterEvents>(json)    
        member this.Serialize (serializer: ISerializer) =
            this
            |> serializer.Serialize

