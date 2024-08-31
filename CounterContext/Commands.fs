
namespace SharpinoCounter
open System
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils
open Sharpino
open Sharpino.Core
open SharpinoCounter.CounterContext
open SharpinoCounter.CounterContextEvents

module CounterContextCommands =
    type CounterContextCommands =
        | AddCounterReference of Guid
        | RemoveCounterReference of Guid
            interface Command<CounterContext, CounterCountextEvents> with
                member this.Execute (counter: CounterContext) =
                    match this with
                    | AddCounterReference id ->
                        counter.AddCounter id
                        |> Result.map (fun s -> (s, [CounterAdded id]))
                    | RemoveCounterReference id ->
                        counter.RemoveCounter id
                        |> Result.map (fun s -> (s,[CounterRemoved id]))
                member this.Undoer = None