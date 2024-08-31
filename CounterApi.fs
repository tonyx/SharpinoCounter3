namespace SharpinoCounter
open FsToolkit.ErrorHandling
open Sharpino.CommandHandler

open Sharpino.Storage
open SharpinoCounter.CounterContext
open SharpinoCounter.CounterContextEvents
open SharpinoCounter.CounterContextCommands
open SharpinoCounter.Counter

module SharpinoCounterApi =

    type SharpinoCounterApi (storage: IEventStore, eventBroker: IEventBroker, counterContextStateViewer: StateViewer<CounterContext>, counterViewer: AggregateViewer<Counter>) =

        member this.AddCounter counterId =
            let counter = Counter (counterId, 0)
            result {
                return!  
                    AddCounterReference counterId
                    |> this.RunInitAndCommand counter 
            }

        member this.GetCounter counterId =
            counterViewer counterId
            |> Result.map (fun (_, counter, _, _) -> counter)

        member this.GetCounterReferences () =
            counterContextStateViewer ()
            |> Result.map (fun (_, state, _, _) -> state.CountersReferences)

        member this.Increment counterId =
            Increment 
            |> this.RunAggregateCommand counterId

        member this.Decrement counterId =
            Decrement 
            |> this.RunAggregateCommand counterId

        member this.ClearCounter counterId =
            Clear Unit
            |> this.RunAggregateCommand counterId

        member this.ClearCounter (counterId,  x) =
            Clear (Int x)
            |> this.RunAggregateCommand counterId

        member this.RemoveCounter counterId =
            result {
                return! 
                    RemoveCounterReference counterId
                    |> this.RunCounterContextCommand
            }

        member private this.RunInitAndCommand counter cmd =
            cmd
            |> runInitAndCommand<CounterContext, CounterCountextEvents, Counter> storage eventBroker counterContextStateViewer counter

        member private this.RunAggregateCommand counterId cmd =
            cmd
            |> runAggregateCommand<Counter, CounterEvents> counterId storage eventBroker counterViewer

        member private this.RunCounterContextCommand cmd =
            cmd 
            |> runCommand<CounterContext, CounterCountextEvents> storage eventBroker counterContextStateViewer