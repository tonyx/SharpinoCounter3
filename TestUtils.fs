module TestUtils

open System
open Sharpino
open FSharpPlus

open sharpinoCounter
open sharpinoCounter.SharpinoCounterApi
open Sharpino.Cache
open sharpinoCounter.CounterContext
open Sharpino
open Sharpino.MemoryStorage
open Sharpino.PgStorage
open Sharpino.TestUtils
open Sharpino.Storage

open Sharpino.CommandHandler
open sharpinoCounter.CounterContextEvents
open Sharpino.KafkaReceiver
open sharpinoCounter.Counter

let connection = 
    "Server=127.0.0.1;"+
    "Database=es_counter;" +
    "User Id=safe;"+
    "Password=safe;"

let memoryStorage: IEventStore = MemoryStorage()
let pgStorage = PgEventStore(connection)

let doNothingBroker =
    { 
        notify = None
        notifyAggregate = None
    }

let counterContextStorageStateViewer =
    getStorageFreshStateViewer<CounterContext, CounterCountextEvents> pgStorage

let counterAggregateStorageStateViewer =
    getAggregateStorageFreshStateViewer<Counter, CounterEvents> pgStorage

// let counterSubscriber = KafkaSubscriber.Create("localhost:9092", "_01", "_counter", "sharpinoClient") |> Result.get
let counterSubscriber = 
    let result =
        try
            KafkaSubscriber.Create("localhost:9092", CounterContext.Version, CounterContext.StorageName, "sharpinoClient") |> Result.get
        with e ->
            failwith (sprintf "KafkaSubscriber.Create failed %A" e)
    result

// will remove this
let getKafkaCounterContextState () =
    let counterViewer = 
        mkKafkaViewer<CounterContext, CounterCountextEvents> counterSubscriber counterContextStorageStateViewer (ApplicationInstance.ApplicationInstance.Instance.GetGuid())

    let counterState = 
        fun () ->
            counterViewer.RefreshLoop() |> ignore
            counterViewer.State()
    counterState

let Setup(eventStore: IEventStore) =
    StateCache<CounterContext>.Instance.Clear()
    eventStore.Reset CounterContext.Version CounterContext.StorageName
    ApplicationInstance.ApplicationInstance.Instance.ResetGuid()

let doNothing whatever =
    ()