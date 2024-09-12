namespace Sharpino.DoubleAccountDemo
open System
open FSharpPlus
open FsToolkit.ErrorHandling
open Sharpino.Core
open Sharpino
open SharpinoCounter.Commons

open Sharpino.Lib.Core.Commons

module Account =

    type Account = {
        Id: Guid
        Name: string
        Balance: decimal
    }
    with 
        member this.TransferFrom(id: Guid, amount: decimal) =
            result {
                let! amountIsPositive = 
                    amount > 0.0M
                    |> Result.ofBool "Amount must be positive"
                let! amountIsLowerThanLimit =
                    amount <= 1000.0M
                    |> Result.ofBool "Amount must be lower than 1000"
                return { this with Balance = this.Balance + amount }    
            }

        member this.TransferTo(id: Guid, amount: decimal) =
            result {
                let! amountIsPositive = 
                    amount > 0.0M
                    |> Result.ofBool "Amount must be positive"
                let! amountIsLowerThanLimit =
                    amount <= 1000.0M
                    |> Result.ofBool "Amount must be lower than 1000"
                return { this with Balance = this.Balance - amount }    
            }

        member this.Serialize = 
            globalSerializer.Serialize this
        static member Deserialize (json: string) =
            globalSerializer.Deserialize<Account> json
        
        static member StorageName =
            "_account"
        static member Version =
            "_01"
        static member SnapshotsInterval = 10

        interface Aggregate<string> with
            member this.Id = 
                this.Id
            member this.Serialize =
                this.Serialize
        interface Entity with
            member this.Id = 
                this.Id

    type AccountEvents =
        | TransferredFrom of Guid * decimal
        | TransferredTo of Guid * decimal
            interface Event<Account>  with
                member this.Process account =
                    match this with
                    | TransferredFrom(id, amount) -> account.TransferFrom(id, amount)
                    | TransferredTo(id, amount) -> account.TransferTo(id, amount)
        member this.Serialize = 
            globalSerializer.Serialize this
        static member Deserialize x =
            globalSerializer.Deserialize<AccountEvents> x

    type Accountcommands =
        | TransferFrom of Guid * decimal
        | TransferTo of Guid * decimal
            interface AggregateCommand<Account,AccountEvents> with
                member this.Execute account =
                    match this with
                    | TransferFrom(id, amount) -> 
                        account.TransferFrom(id, amount)
                        |> Result.map (fun s -> (s, [TransferredFrom (id, amount)]))
                    | TransferTo(id, amount) -> 
                        account.TransferTo(id, amount)
                        |> Result.map (fun s -> (s, [TransferredTo (id, amount)]))
                member this.Undoer = None


