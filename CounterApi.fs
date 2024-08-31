namespace SharpinoCounter
open FsToolkit.ErrorHandling
open Sharpino.CommandHandler

open Sharpino.Storage
open Sharpino.Core
open SharpinoCounter.CounterContext
open SharpinoCounter.CounterContextEvents
open SharpinoCounter.CounterContextCommands
open SharpinoCounter.Counter

module SharpinoCounterApi =

    type SharpinoCounterApi (storage: IEventStore<string>, eventBroker: IEventBroker<string>, counterContextStateViewer: StateViewer<CounterContext>, counterViewer: AggregateViewer<Counter>) =

        member this.AddCounter counterId =
            let counter = Counter (counterId, 0)
            result {
                return!  
                    AddCounterReference counterId
                    |> this.RunInitAndCommand counter 
            }

        member this.GetCounter counterId =
            counterViewer counterId
            |> Result.map (fun (_, counter) -> counter)

        member this.GetCounterReferences () =
            counterContextStateViewer ()
            |> Result.map (fun (_, state) -> state.CountersReferences)

        member this.Increment counterId =
            Increment
            |> runAggregateCommand<Counter, CounterEvents, string> counterId storage eventBroker 

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
            |> runInitAndCommand<CounterContext, CounterCountextEvents, Counter, string> storage eventBroker counter

        member private this.RunAggregateCommand counterId cmd =
            cmd
            |> runAggregateCommand<Counter, CounterEvents, string> counterId storage eventBroker 

        member private this.RunCounterContextCommand cmd =
            cmd 
            |> runCommand<CounterContext, CounterCountextEvents, string> storage eventBroker