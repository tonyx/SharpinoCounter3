namespace sharpinoCounter
open System
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils
open Sharpino
open Sharpino.Core
open Sharpino.Utils
open Sharpino.CommandHandler
open Sharpino.StateView
open Sharpino.Definitions

open Sharpino.Storage
open sharpinoCounter.CounterContext
open sharpinoCounter.CounterContextEvents
open sharpinoCounter.CounterContextCommands

module SharpinoCounterApi =
    open Counter

    type SharpinoCounterApi (storage: IEventStore, eventBroker: IEventBroker, counterContextStateViewer: StateViewer<CounterContext>, counterViewer: AggregateViewer<Counter>) =

        member this.AddCounter counterId =
            result {
                let counter = Counter (counterId, 0)
                let addCounterReference = AddCounterReference counterId
                let! result = 
                    this.RunInitAndCommand counter addCounterReference
                return result
            }

        member this.GetCounter counterId =
            counterViewer counterId
            |> Result.map (fun (_, counter, _, _) -> counter)

        member this.GetCounterReferences () =
            counterContextStateViewer ()
            |> Result.map (fun (_, state, _, _) -> state.CountersReferences)

        member this.Increment counterId =
            CounterCommands.Increment 
            |> this.RunAggregateCommand counterId

        member this.Decrement counterId =
            CounterCommands.Decrement 
            |> this.RunAggregateCommand counterId

        member private this.RunInitAndCommand counter cmd =
            cmd
            |> runInitAndCommand<CounterContext, CounterCountextEvents, Counter> storage eventBroker counterContextStateViewer counter

        member private this.RunAggregateCommand counterId cmd =
            cmd
            |> runAggregateCommand<Counter, CounterEvents> counterId storage eventBroker counterViewer

        member private this.RunCounterContextCommand cmd =
            cmd 
            |> runCommand<CounterContext, CounterCountextEvents> storage eventBroker counterContextStateViewer