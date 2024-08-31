module TestUtils

open System
open Sharpino
open FSharpPlus

open SharpinoCounter
open SharpinoCounter.SharpinoCounterApi
open Sharpino.Cache
open SharpinoCounter.CounterContext
open Sharpino
open Sharpino.MemoryStorage
open Sharpino.PgStorage
open Sharpino.TestUtils
open Sharpino.Storage

open Sharpino.CommandHandler
open SharpinoCounter.CounterContextEvents
open Sharpino.KafkaReceiver
open SharpinoCounter.Counter

let connection = 
    "Server=127.0.0.1;"+
    "Database=es_counter;" +
    "User Id=safe;"+
    "Password=safe;"

let inMemoryEventStore: IEventStore = MemoryStorage()
let postgresEventStore = PgEventStore(connection)

let doNothingBroker =
    { 
        notify = None
        notifyAggregate = None
    }

let counterContextStorageStateViewer =
    getStorageFreshStateViewer<CounterContext, CounterCountextEvents> postgresEventStore

let counterAggregateStorageStateViewer =
    getAggregateStorageFreshStateViewer<Counter, CounterEvents> postgresEventStore

let counterContextMemoryStateViewer =
    getStorageFreshStateViewer<CounterContext, CounterCountextEvents> inMemoryEventStore

let counterAggregateMemoryStateViewer =
    getAggregateStorageFreshStateViewer<Counter, CounterEvents> inMemoryEventStore

// let counterSubscriber = 
//     let result =
//         try
//             KafkaSubscriber.Create("localhost:9092", CounterContext.Version, CounterContext.StorageName, "sharpinoClient") |> Result.get
//         with e ->
//             failwith (sprintf "KafkaSubscriber.Create failed %A" e)
//     result

// will remove this
// let getKafkaCounterContextState () =
//     let counterViewer = 
//         mkKafkaViewer<CounterContext, CounterCountextEvents> counterSubscriber counterContextStorageStateViewer (ApplicationInstance.ApplicationInstance.Instance.GetGuid())

//     let counterState = 
//         fun () ->
//             counterViewer.RefreshLoop() |> ignore
//             counterViewer.State()
//     counterState

let Setup(eventStore: IEventStore) =
    StateCache<CounterContext>.Instance.Clear()
    eventStore.Reset CounterContext.Version CounterContext.StorageName
    eventStore.Reset Counter.Version Counter.StorageName
    eventStore.ResetAggregateStream Counter.Version Counter.StorageName
    ApplicationInstance.ApplicationInstance.Instance.ResetGuid()

let doNothing whatever =
    ()