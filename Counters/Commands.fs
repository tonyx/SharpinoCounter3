
namespace SharpinoCounter
open System
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils
open Sharpino
open Sharpino.Core
open SharpinoCounter.Counter

type CounterCommands =
    | Clear of IntOrUnit
    | Increment 
    | Decrement 
        interface AggregateCommand<Counter, CounterEvents> with
            member this.Execute (counter: Counter):  Result<Counter*List<CounterEvents>, string> =
                match this with
                | Clear Unit -> 
                    counter.Clear () 
                    |> Result.map (fun s -> (s, [Cleared Unit]))
                | Clear (Int x) ->
                    counter.Clear x
                    |> Result.map (fun s -> (s, [Cleared (Int x)]))
                | Increment  ->
                    counter.Increment ()
                    |> Result.map (fun s -> (s, [Incremented]))
                | Decrement  ->
                    counter.Decrement()
                    |> Result.map (fun s -> (s, [Decremented]))
            member this.Undoer = None