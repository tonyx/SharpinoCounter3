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
    open Sharpino.DoubleAccountDemo.Account

    type SharpinoCounterApi (storage: IEventStore<string>, eventBroker: IEventBroker<string>, counterContextStateViewer: StateViewer<CounterContext>, counterViewer: AggregateViewer<Counter>, accountViewer: AggregateViewer<Account>) =

        member this.AddCounter counterId =
            let counter = Counter (counterId, 0)
            result {
                return!  
                    AddCounterReference counterId
                    |> this.RunInitAndCommand counter 
            }

        member this.AddAccount (account: Account) =
            result {
                return! 
                    AddAccountReference account.Id
                    |> runInitAndCommand storage eventBroker account
            }

        member this.TransferAmountFromAccountToAccount (amount: decimal, fromAccount: Account, toAccount: Account) =
            result {
                let fromAccountCommand = Accountcommands.TransferFrom (fromAccount.Id, amount)
                let toAccountCommand = Accountcommands.TransferTo (toAccount.Id, amount)
                return!
                    runTwoAggregateCommands<Account, AccountEvents, Account, AccountEvents, string> fromAccount.Id toAccount.Id storage eventBroker fromAccountCommand toAccountCommand
            }


        member this.GetCounter counterId =
            result {
                let! (_, counterContext) = counterContextStateViewer ()
                let! counterExists = counterContext.GetCounterReference counterId
                let! (_, counter)  =  counterViewer counterId
                return counter
            }

        member this.GetCounterReferences () =
            result {
                let! (_, counterContext) = counterContextStateViewer ()
                return counterContext.CountersReferences
            }

        member this.Increment counterId =
            result {
                let! (_, counterContext)  = counterContextStateViewer ()
                let counterExists = counterContext.GetCounterReference counterId
                return!
                    Increment
                    |> runAggregateCommand<Counter, CounterEvents, string> counterId storage eventBroker
            }

        member this.Decrement counterId =
            result {
                let! (_, counterContext) = counterContextStateViewer ()
                let! counterExists = counterContext.GetCounterReference counterId
                return!
                    Decrement
                    |> runAggregateCommand<Counter, CounterEvents, string> counterId storage eventBroker
            }

        member this.ClearCounter counterId =
            result {
                let! (_, counterContext) = counterContextStateViewer ()
                let! counterExists = counterContext.GetCounterReference counterId
                return!    
                    Clear Unit
                    |> this.RunAggregateCommand counterId
            }

        member this.ClearCounter (counterId, x) =
            result {
                let! (_, counterContext) = counterContextStateViewer ()
                let! counterExists = counterContext.GetCounterReference counterId
                return!
                    Clear (Int x)
                    |> this.RunAggregateCommand counterId
            }

        member this.RemoveCounter counterId =
            result {
                let! (_, counterContext) = counterContextStateViewer ()
                let! counterExists = counterContext.GetCounterReference counterId
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