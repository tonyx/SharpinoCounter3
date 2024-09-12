
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
        | AddAccountReference of Guid
        | RemoveAccountReference of Guid
            interface Command<CounterContext, CounterCountextEvents> with
                member this.Execute (counter: CounterContext) =
                    match this with
                    | AddCounterReference id ->
                        counter.AddCounterReference id
                        |> Result.map (fun s -> (s, [CounterAdded id]))
                    | RemoveCounterReference id ->
                        counter.RemoveCounterReference id
                        |> Result.map (fun s -> (s,[CounterRemoved id]))
                    | AddAccountReference id ->
                        counter.AddAccountReference id
                        |> Result.map (fun s -> (s,[AccountAdded id]))
                    | RemoveAccountReference id ->
                        counter.RemoveAccountReference id
                        |> Result.map (fun s -> (s,[AccountRemoved id]))
                member this.Undoer = None