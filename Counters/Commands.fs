
namespace sharpinoCounter
open System
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils
open Sharpino
open Sharpino.Core
open sharpinoCounter.Counter

type CounterCommands =
    | Clear 
    | Increment 
    | Decrement 
        interface Command<Counter, CounterEvents> with
            member this.Execute (counter: Counter):  Result<List<CounterEvents>, string> =
                match this with
                | Clear  -> 
                    counter.Clear ()
                    |> Result.map (fun _ -> [Cleared])
                | Increment  ->
                    counter.Increment ()
                    |> Result.map (fun _ -> [Incremented])
                | Decrement  ->
                    counter.Decrement()
                    |> Result.map (fun _ -> [Decremented])
            member this.Undoer = None