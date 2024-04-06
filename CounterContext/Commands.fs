
namespace sharpinoCounter
open System
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils
open Sharpino
open Sharpino.Core
open sharpinoCounter.CounterContext
open sharpinoCounter.CounterContextEvents

module CounterContextCommands =
    type CounterContextCommands =
        | AddCounterReference of Guid
        | RemoveCounterReference of Guid
            interface Command<CounterContext, CounterCountextEvents> with
                member this.Execute (counter: CounterContext):  Result<List<CounterCountextEvents>, string> =
                    match this with
                    | AddCounterReference id ->
                        counter.AddCounter id
                        |> Result.map (fun _ -> [CounterAdded id])
                    | RemoveCounterReference id ->
                        counter.RemoveCounter id
                        |> Result.map (fun _ -> [CounterRemoved id])
                member this.Undoer = None