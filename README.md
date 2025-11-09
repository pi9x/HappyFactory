# HappyFactory

HappyFactory is a small example .NET Web API demonstrating a simple event-driven architecture using:
- FastEndpoints for concise endpoint definitions
- An in-memory `EventStore` for emitting application/domain events
- A background `ProjectionService` that projects events into an EF Core InMemory read model
- EF Core InMemory as a read-model (queries / projections)
- Swagger (Swashbuckle) for API discovery

This project is intended as a small learning/demo project rather than a production-ready system.

## Repository layout

- `HappyFactory/HappyFactory.csproj` — main web application project
- `HappyFactory/Program.cs` — application bootstrap
- `HappyFactory/Features/*` — endpoint handlers, requests/responses, and command/query handlers
- `HappyFactory/Models/*` — domain events, domain model classes
- `HappyFactory/Services/*` — in-memory `EventStore`, `ProjectionService`, and `ReadModelDbContext`
- `HappyFactory/HappyFactory.http` — an HTTP client file with quick examples (for editors like VS Code REST Client)

## Prerequisites

- .NET 9 SDK (the project targets `net9.0`)
  - Verify with `dotnet --version` (should be a 9.x SDK)

## Build & run

From the repository root:

1. Restore and build:
- `dotnet restore`
- `dotnet build`

2. Run the app:
- `dotnet run --project HappyFactory/HappyFactory.csproj`

By default the app's development launch configuration typically serves on `http://localhost:5116`. If you need the Swagger UI to be enabled, ensure the environment is `Development`:

- On Linux/macOS:
  - `ASPNETCORE_ENVIRONMENT=Development dotnet run --project HappyFactory/HappyFactory.csproj`
- On Windows PowerShell:
  - `$env:ASPNETCORE_ENVIRONMENT = 'Development'; dotnet run --project HappyFactory/HappyFactory.csproj`

When running in Development, Swagger UI is served at the app root, e.g.:
- `http://localhost:5116/` — Swagger UI

If you prefer to run from inside the project folder:
- `cd HappyFactory`
- `dotnet run`

## API

This service exposes a very small product API.

Base paths:
- POST `/products` — create a new product
- GET `/products/{id}` — get a product by id

Notes:
- The API uses an in-memory event store. When you create a product it emits a `ProductCreated` event into the `EventStore`.
- The `ProjectionService` subscribes to those events and projects them into the EF Core InMemory read model so that queries (GET) read from the read-model.
- Everything is ephemeral — restart the app and the in-memory stores are cleared.

Example requests (replace `localhost:5116` with whichever URL/port your app listens on):

Create a product (returns 201 Created with JSON body containing created `Id`):
$ curl -X POST http://localhost:5116/products -H "Content-Type: application/json" -d '{"name":"Toy Car","sku":"TOYCAR-001"}'

Example JSON body:
- `name` (string) — product name
- `sku` (string) — product SKU

Successful response (example):
- `201 Created`
- Body: `{"id":"<guid>"}`

Query the product:
$ curl http://localhost:5116/products/<id>

Successful response (example):
- `200 OK`
- Body: `{"id":"<guid>","name":"Toy Car","sku":"TOYCAR-001"}`

If the product is not found:
- `404 Not Found`

Swagger UI
- When running in `Development`, Swagger UI is available at `/` and provides an interactive view of available endpoints and request/response schemas.

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

- `HappyFactory/HappyFactory.http` — example HTTP requests usable by REST clients (e.g. VS Code REST Client). Edit/add requests here for quick integration testing.

## Testing

There are no automated tests included in the repository. For manual testing:

1. Start the app.
2. Use Swagger UI or the provided HTTP file / curl commands to exercise the POST and GET endpoints.
3. Optionally inspect logs to verify `ProjectionService` is projecting `ProductCreated` events into the read model.

## Contributing

This is a small demo repository. If you'd like to contribute:
- Open issues for any bugs or clarifications
- Send a pull request for small improvements (documentation, examples, or minor code hygiene)

## License

This repository doesn't include an explicit license file. If you plan to reuse or share, add a `LICENSE` file (for example, `MIT`).

----
Happy hacking!