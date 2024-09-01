# Sharpino Counter

Example of a Counter with Sharpino

## Table of Contents

Prerequisites: dbmate, postgres (optional as can use in memory eventstore)

- [Installation](#installation)
- [Optional Postgres EvenStore](#postgesEventStore)


## Installation

clone project 

## Run project using memory as event-store
then run the commands:

```bash
dbmate up
```
and run the following command:

```bash
dotnet run
```

## Run project using postgres as event-store
Create a file named .env with content like this (substituting the postgresusername/postgrespassword with your own):
```bash
DATABASE_URL="postgres://postgresusername:postgrespassword@127.0.0.1:5432/es_counter?sslmode=disable"
```
then uncomment the following line in the Tests.fs:
        // ((fun () -> SharpinoCounterApi (pgStorage, doNothingBroker, counterContextStorageStateViewer, counterAggregateStorageStateViewer)), pgStorage)
and optionally comment out the following line in the Tests.fs:
        // ((fun () -> SharpinoCounterApi (memoryStorage, doNothingBroker, counterContextStorageStateViewer, counterAggregateStorageStateViewer)), memoryStorage)

then run the commands:

```bash
dbmate up
```
and run the following command:

```bash
dotnet run
```


A 6 min reading article about this example can be found 
[here](https://www.linkedin.com/pulse/example-counter-event-sourcing-sharpino-ino-antonio-lucca-hzlof)





