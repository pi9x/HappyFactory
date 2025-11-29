# HappyFactory

HappyFactory is a small example .NET Web API demonstrating a simple event sourcing architecture using:
- FastEndpoints for concise endpoint definitions
- An in-memory `EventStore` for emitting application/domain events
- A background `ProjectionService` that projects events into an EF Core InMemory read model
- EF Core InMemory as a read-model (queries/projections)
- Swagger for API discovery

This project is intended as a small learning/demo project rather than a production-ready system.

## Prerequisites

- .NET 10 SDK (the project targets `net10.0`)
  - Verify with `dotnet --version` (should be a 10.x SDK)

## Build & run

From the repository root:

1. Restore and build:
- `dotnet restore`
- `dotnet build`

2. Run the app:
- `dotnet run --project src/HappyFactory/HappyFactory.csproj`

If you need the Swagger UI to be enabled, ensure the environment is `Development`:

- On Linux/macOS:
  - `ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/HappyFactory/HappyFactory.csproj`
- On Windows PowerShell:
  - `$env:ASPNETCORE_ENVIRONMENT = 'Development'; dotnet run --project src/HappyFactory/HappyFactory.csproj`

When running in Development, Swagger UI is served at `/swagger`, e.g.:
- `http://localhost:5116/swagger` — Swagger UI

## API

This service exposes a very small product API.

Base paths:
- POST `/products` — create a new product
- GET `/products/{id}` — get a product by id

Notes:
- The API uses an in-memory event store. When you create a product it emits a `ProductCreated` event into the `EventStore`.
- The `ProjectionService` subscribes to those events and projects them into the EF Core InMemory read model so that queries (GET) read from the read-model.
- Everything is ephemeral — restart the app and the in-memory stores are cleared.

## Development notes & architecture

- Event store
  - `Services/EventStore.cs` is a small synchronous in-memory event bus and store.
  - It holds events in a list and notifies subscribers synchronously when events are appended.
  - Not durable — suited only to demos or tests.

- Projections
  - `Services/ProjectionService.cs` subscribes to `EventStore.EventAppended`.
  - Each event is handled with a fresh scoped `ReadModelDbContext` to avoid long-lived DbContext lifetimes.
  - Known events:
    - `ProductEvents.ProductCreated` → creates a `Product` in the read model and creates an `InventoryItem` with quantity 0.
    - `InventoryItemEvents.InventoryReserved` → reduces the `EndingQuantity` in the read model (non-negative).

- Read model
  - `Services/ReadModelDbContext.cs` uses EF Core InMemory provider (configured in `Program.cs`) for simple query/projection examples.

- Endpoints
  - FastEndpoints is used to define endpoints in the vertical-slice style (each endpoint + handler co-located).

If you want to make the system durable:
- Replace `EventStore` with a persistent event store (e.g. EventStoreDB, a relational store, or Kafka) and wire projections to read from a durable stream.
- Use a persistent read-model (e.g. SQL Server/Postgres) for `ReadModelDbContext`.

## Helpful files

- `src/HappyFactory/HappyFactory.http` — example HTTP requests usable by REST clients (e.g. VS Code REST Client). Edit/add requests here for quick integration testing.

## License

This repository doesn't include an explicit license file. If you plan to reuse or share, add a `LICENSE` file (for example, `MIT`).

----
Happy hacking!