
namespace SharpinoCounter
open SharpinoCounter.Commons
open System
open Sharpino
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Definitions
open Sharpino.Utils

module CounterContext =
    type CounterContext(state: int, counterRefs: List<Guid>, accountRefs: List<Guid>) =

        member this.CountersReferences = counterRefs
        member this.AccountsReferences = accountRefs

        member this.AddCounterReference (counterId: Guid) = 
            result {
                do! 
                    counterRefs |> List.exists (fun x -> x = counterId)
                    |> not
                    |> Result.ofBool "counter already exists"
                return CounterContext (state, counterRefs @ [counterId], accountRefs) 
            }
        member this.RemoveCounterReference (counterId: Guid) =
            result {
                do! 
                    counterRefs |> List.exists (fun x -> x = counterId)
                    |> Result.ofBool "counter does not exist"
                return CounterContext (state, counterRefs |> List.filter (fun x -> x <> counterId), accountRefs)
            }

        member this.AddAccountReference (accoutnId: Guid) =
            result {
                do! 
                    accountRefs |> List.exists (fun x -> x = accoutnId)
                    |> not
                    |> Result.ofBool "account already exists"
                return CounterContext (state, counterRefs, accountRefs @ [accoutnId])
            }

        member this.RemoveAccountReference (accoutnId: Guid) =
            result {
                do! 
                    accountRefs |> List.exists (fun x -> x = accoutnId)
                    |> Result.ofBool "account does not exist"
                return CounterContext (state, counterRefs, accountRefs |> List.filter (fun x -> x <> accoutnId))
            }

        member this.GetCounterReference (counterId: Guid) =
            result {
                do! 
                    counterRefs |> List.exists (fun x -> x = counterId)
                    |> Result.ofBool "counter does not exist"
                return counterId
            }
        member this.GetAccountReference (accountId: Guid) =
            result {
                do! 
                    accountRefs |> List.exists (fun x -> x = accountId)
                    |> Result.ofBool "account does not exist"
                return accountId
            }

        member this.State = state

// -------
        static member Zero = CounterContext (0, [], []) 
        static member StorageName = "_countercontext"
        static member Version = "_01"
        static member SnapshotsInterval = 15
        static member Deserialize (json: Json) =
            globalSerializer.Deserialize<CounterContext> json

        member this.Serialize =
            globalSerializer.Serialize this
