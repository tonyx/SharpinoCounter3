module Tests

open System
open Sharpino
open FSharpPlus
open FsToolkit.ErrorHandling
open Expecto
open SharpinoCounter.SharpinoCounterApi
open Sharpino.DoubleAccountDemo.Account
open Sharpino.TestUtils
open TestUtils

[<Tests>]
let tests =

    // make sure you properly setup postgres db using dbmate tool if you want to enable the first test line below
    let testConfigs = [
        // ((fun () -> SharpinoCounterApi (pgStorage, doNothingBroker, counterContextStorageStateViewer, counterAggregateStorageStateViewer)), pgStorage)
        ((fun () -> SharpinoCounterApi (inMemoryEventStore, doNothingBroker, counterContextMemoryStateViewer, counterAggregateMemoryStateViewer, accountAggregateMemoryStateViewer)), inMemoryEventStore)
    ]

    testList "samples" [

        multipleTestCase "add an account - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let account: Account = { Id = Guid.NewGuid (); Name = "test"; Balance = 0.0M }
            let accountApi = api ()

            // when
            let accountAdded = accountApi.AddAccount account

            // then
            Expect.isOk accountAdded "should be ok"

        multipleTestCase "add and retrieve an account - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let account: Account = { Id = Guid.NewGuid (); Name = "test"; Balance = 0.0M }
            let accountApi = api ()

            // when
            let accountAdded = accountApi.AddAccount account
            let accountRetrieved = accountApi.GetAccount account.Id

            // then
            Expect.isOk accountRetrieved "should be ok"

        multipleTestCase "add two account and do a transfer - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore

            // given
            let account1: Account = { Id = Guid.NewGuid (); Name = "test"; Balance = 0.0M }
            let account2: Account = { Id = Guid.NewGuid (); Name = "test"; Balance = 1000.0M }
            let accountApi = api ()

            let account1Added = accountApi.AddAccount account1
            let account2Added = accountApi.AddAccount account2

            // when
            let transferAmount = accountApi.TransferAmountFromAccountToAccount (100.0M, account1.Id, account2.Id)

            // then
            Expect.isOk transferAmount "should be ok"

        multipleTestCase "add two account, do a transfer and check the balance - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            let account1: Account = { Id = Guid.NewGuid (); Name = "test"; Balance = 0.0M }
            let account2: Account = { Id = Guid.NewGuid (); Name = "test"; Balance = 1000.0M }
            let accountApi = api ()

            let account1Added = accountApi.AddAccount account1
            let account2Added = accountApi.AddAccount account2

            // when 
            let transferAmount = accountApi.TransferAmountFromAccountToAccount (100.0M, account1.Id, account2.Id)

            // then
            let account1Retrieved = accountApi.GetAccount account1.Id |> Result.get
            let account2Retrieved = accountApi.GetAccount account2.Id |> Result.get

            Expect.equal account1Retrieved.Balance 100.0M "should be 100"
            Expect.equal account2Retrieved.Balance 900.0M "should be 900"


        multipleTestCase "add a counter reference  - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let newCounterId = Guid.NewGuid ()
            let counterApi = api ()

            // when
            let counterReferences = counterApi.AddCounter newCounterId

            // then
            Expect.isOk counterReferences  "should be ok"

        multipleTestCase "add a counter reference and retrieve it - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let newCounterId = Guid.NewGuid ()
            let counterApi = api ()

            // when
            let _ = counterApi.AddCounter newCounterId
            let counterReferences = counterApi.GetCounterReferences ()

            // then
            Expect.isOk counterReferences  "should be ok"
            Expect.equal counterReferences.OkValue [newCounterId] "should be empty"

        multipleTestCase "add a counter and retrieve it - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let newCounterId = Guid.NewGuid ()
            let counterApi = api ()
            let _ = counterApi.AddCounter newCounterId

            // when
            let counterRetrieved = counterApi.GetCounter newCounterId

            // then
            Expect.isOk counterRetrieved  "should be ok"
            let counter = counterRetrieved.OkValue
            Expect.equal counter.Id newCounterId "should be equal"

        multipleTestCase "add a counter and increment it - Ok" testConfigs  <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let newCounterId = Guid.NewGuid ()
            let counterApi = api ()
            let _ = counterApi.AddCounter newCounterId

            // when
            let incrementCounter = counterApi.Increment newCounterId 

            // then
            let counterRetrieved = counterApi.GetCounter newCounterId
            Expect.isOk counterRetrieved "should be ok"
            let counter = counterRetrieved.OkValue
            Expect.equal counter.State 1 "should be 1"

        multipleTestCase "add a counter, increment it twice - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let newCounterId = Guid.NewGuid ()
            let counterApi = api ()
            let _ = counterApi.AddCounter newCounterId

            // when
            let incrementCounter = counterApi.Increment newCounterId 
            let incrementCounterAgain = counterApi.Increment newCounterId 

            // then

            let counterRetrieved = counterApi.GetCounter newCounterId
            Expect.isOk counterRetrieved "should be ok"
            let counter = counterRetrieved.OkValue
            Expect.equal counter.State 2 "should be 1"

        multipleTestCase "add a counter, increment it twice and then decrement it - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let newCounterId = Guid.NewGuid ()
            let counterApi = api ()
            let _ = counterApi.AddCounter newCounterId

            // when
            let incrementCounter = counterApi.Increment newCounterId 
            let incrementCounterAgain = counterApi.Increment newCounterId 
            let decrementCounter = counterApi.Decrement newCounterId

            // then

            let counterRetrieved = counterApi.GetCounter newCounterId
            Expect.isOk counterRetrieved "should be ok"
            let counter = counterRetrieved.OkValue
            Expect.equal counter.State 1 "should be 1"

        multipleTestCase "retrieve an unexisting counter - Error " testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let newCounterId = Guid.NewGuid ()
            let counterApi = api ()

            // when
            let counterRetrieved = counterApi.GetCounter newCounterId

            // then
            Expect.isError counterRetrieved "should be error"

        multipleTestCase "add two counters - Ok" testConfigs <| fun (api, eventStore) -> 
            Setup eventStore
            // given
            let newCounterId = Guid.NewGuid ()
            let newCounterId2 = Guid.NewGuid ()
            let counterApi = api ()
            let _ = counterApi.AddCounter newCounterId
            let _ = counterApi.AddCounter newCounterId2

            // when
            let counterReferences = counterApi.GetCounterReferences ()

            // then
            Expect.isOk counterReferences  "should be ok"
            Expect.equal counterReferences.OkValue [newCounterId; newCounterId2] "should be empty"

        multipleTestCase "add two counters and increment them independently - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let newCounterId = Guid.NewGuid ()
            let newCounterId2 = Guid.NewGuid ()
            let counterApi = api ()
            let _ = counterApi.AddCounter newCounterId
            let _ = counterApi.AddCounter newCounterId2

            // when
            let incrementCounter = counterApi.Increment newCounterId 
            let incrementCounter2 = counterApi.Increment newCounterId2 

            // then

            let counterRetrieved = counterApi.GetCounter newCounterId
            Expect.isOk counterRetrieved "should be ok"
            let counter = counterRetrieved.OkValue
            Expect.equal counter.State 1 "should be 1"

            let counterRetrieved2 = counterApi.GetCounter newCounterId2
            Expect.isOk counterRetrieved2 "should be ok"
            let counter2 = counterRetrieved2.OkValue
            Expect.equal counter2.State 1 "should be 1"

        multipleTestCase "add five counters, increment/decrement them - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore

            // when
            let ids = [Guid.NewGuid (); Guid.NewGuid (); Guid.NewGuid (); Guid.NewGuid (); Guid.NewGuid ()]
            let counterApi = api ()

            let _ = ids |> List.map (fun id -> counterApi.AddCounter id)

            // twice
            [1 .. 2] 
            |>> (fun _ -> counterApi.Increment (List.item 0 ids))
            |> ignore

            let incrementFirst =
                counterApi.Increment (List.item 1 ids) 
            Expect.isOk incrementFirst "should be ok"

            let incrementSecond =
                counterApi.Increment (List.item 2 ids)
            Expect.isOk incrementSecond  "should be ok"

            let incrementThreeTimes =
                [1 .. 3]
                |> List.traverseResultM (fun _ -> counterApi.Increment (List.item 3 ids))
            Expect.isOk incrementThreeTimes "should be ok"

            let incrementTenTimes =
                [1 .. 10]
                |> List.traverseResultM (fun _ -> counterApi.Increment (List.item 4 ids))

            Expect.isOk incrementTenTimes "should be ok"

            // then
            
            let counter0 = counterApi.GetCounter (List.item 0 ids)
            let counter1 = counterApi.GetCounter (List.item 1 ids)
            let counter2 = counterApi.GetCounter (List.item 2 ids)
            let counter3 = counterApi.GetCounter (List.item 3 ids)
            let counter4 = counterApi.GetCounter (List.item 4 ids)

            Expect.isOk counter0 "should be ok"
            Expect.isOk counter1 "should be ok"
            Expect.isOk counter2 "should be ok"
            Expect.isOk counter3 "should be ok"
            Expect.isOk counter4 "should be ok"

            Expect.equal counter0.OkValue.State 2 "should be 2"
            Expect.equal counter1.OkValue.State 1 "should be 1"
            Expect.equal counter2.OkValue.State 1 "should be 1"
            Expect.equal counter3.OkValue.State 3 "should be 3"
            Expect.equal counter4.OkValue.State 10 "should be 10"

        multipleTestCase "add and remove a counter - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let newCounterId = Guid.NewGuid ()
            let counterApi = api ()
            let _ = counterApi.AddCounter newCounterId
            let retrieved = counterApi.GetCounterReferences ()
            Expect.isOk retrieved "should be ok"
            Expect.equal retrieved.OkValue [newCounterId] "should be equal"

            // when
            let removeCounter = counterApi.RemoveCounter newCounterId
            // 
            Expect.isOk removeCounter "should be ok"
            let counters = counterApi.GetCounterReferences ()
            Expect.isOk counters "should be ok"
            Expect.equal counters.OkValue [] "should be empty"

        multipleTestCase "add twice the counter a counter with the same id - Error" testConfigs <| fun (api, eventStore) ->
            Setup eventStore
            // given
            let newCounterId = Guid.NewGuid ()  
            let counterApi = api ()
            let added = counterApi.AddCounter newCounterId
            Expect.isOk added "should be ok"

            // when
            let readded = counterApi.AddCounter newCounterId

            // then
            Expect.isError readded "should be error"
            let (Error e)  = readded
            printf "%A" e   


        multipleTestCase "add a counter, remove it, and readd it - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore

            // given
            let newCounterId = Guid.NewGuid ()
            let counterApi = api ()
            let added = counterApi.AddCounter newCounterId
            Expect.isOk added "should be ok"
            let removed = counterApi.RemoveCounter newCounterId
            Expect.isOk removed "should be ok"

            // when
            let readded = counterApi.AddCounter newCounterId

            // then
            Expect.isOk readded "should be ok"
            let retrieved = counterApi.GetCounterReferences ()
            Expect.isOk retrieved "should be ok"
            Expect.equal retrieved.OkValue [newCounterId] "should be equal"

        multipleTestCase "add a counter, increase it and then clear it to zero - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore

            // given
            let newCounterId = Guid.NewGuid ()
            let counterApi = api ()
            let added = counterApi.AddCounter newCounterId
            Expect.isOk added "should be ok"
            let incremented = counterApi.Increment newCounterId
            Expect.isOk incremented "should be ok"

            // when
            let reset = counterApi.ClearCounter newCounterId

            // then
            Expect.isOk reset "should be ok"
            let retrieved = counterApi.GetCounter newCounterId
            Expect.isOk retrieved "should be ok"
            Expect.equal retrieved.OkValue.State 0 "should be equal"

        multipleTestCase "add a counter, increase it and then clear it to a value - Ok" testConfigs <| fun (api, eventStore) ->
            Setup eventStore

            // given
            let newCounterId = Guid.NewGuid ()
            let counterApi = api ()
            let added = counterApi.AddCounter newCounterId
            Expect.isOk added "should be ok"
            let incremented = counterApi.Increment newCounterId
            Expect.isOk incremented "should be ok"

            // when
            let reset = counterApi.ClearCounter (newCounterId, 10)

            // then
            Expect.isOk reset "should be ok"
            let retrieved = counterApi.GetCounter newCounterId
            Expect.isOk retrieved "should be ok"
            Expect.equal retrieved.OkValue.State 10 "should be equal"
    ] 
    |> testSequenced
