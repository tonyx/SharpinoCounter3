# Sharpino Counter

Example of a Counter with Sharpino, with Kafka.

## Table of Contents

Prerequisites: dbmate, postgres, Apache Kafka

- [Installation](#installation)
- [Optional Postgres EvenStore](#postgesEventStore)


## Installation

clone project 

Create a file named .env with content like this (substituting the postgresusername/postgrespassword with your own):
```bash
DATABASE_URL="postgres://postgresusername:postgrespassword@127.0.0.1:5432/es_counter?sslmode=disable"
```
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





