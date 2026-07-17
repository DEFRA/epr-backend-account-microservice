# epr-backend-account-microservice

Two .NET 10 services in one repo:

- **BackendAccountService.Api** (WA 407) — account/organisation/enrolment CRUD against `accountsDB` (SQL Server).
- **BackendAccountService.ValidationData.Api** (FA 407) — read-only org lookups for validation functions, hosted on Azure Functions.

Both deploy independently. Shared code lives in `BackendAccountService.Core` and `BackendAccountService.Data`.

## Prerequisites

- .NET 10 SDK
- Docker

## Run locally in docker

[`src/docker-compose.yml`](src/docker-compose.yml) brings up a SQL Server container and the API both running in docker:

```bash
cd src
docker compose up -d
```

You can then access the api at: <http://localhost:5000/swagger/>

### Run locally with docker mssql

To run the sql db in docker and the api locally:

```bash
cd src
docker compose up -d accountsdb_sql
dotnet run --project BackendAccountService.Api --launch-profile local-docker-db
```

or use the launch settings in your IDE (Rider / Visual Studio)

Stop the API container first if the full compose stack is already up — both bind port 5000.

### With seeded LA test data

```bash
make run-laps
```

Brings up the DB and API alongside `BackendAccountService.Data.LaTestSeeder`, which inserts a fixed set of local-authority orgs and users. See `src/BackendAccountService.Data.LaTestSeeder/readme.md`.

## Azure access

Nothing in Azure is *required* to build, run, or test locally:

- The only required setting is `ConnectionStrings__AccountsDatabase`. `docker-compose.yml` sets it to the local SQL container.
- Application Insights is registered in `BackendAccountService.Api/Program.cs` but does not fail if no connection string is present; telemetry simply isn't shipped.
- No Key Vault, Azure AD, or managed-identity calls on the startup path.

If you want to point the API at an Azure dev SQL instance instead of the docker container, override `ConnectionStrings__AccountsDatabase` (env var, user-secrets, or `appsettings.Development.json`).

## Tests

Run everything from the solution:

```bash
dotnet test src/BackendAccountService.sln
```

**Unit tests** — `BackendAccountService.{Api,Core,Data,ValidationData.Api}.UnitTests`. xUnit, no external dependencies.

**Data integration tests** — `BackendAccountService.Data.IntegrationTests` exercises `AccountsDbContext` and its query extensions (soft-delete filter, audit logs, etc.) against a real SQL Server. Controllers are instantiated directly here (no HTTP); see the outside-in project below for the HTTP surface. Needs Docker — one `mcr.microsoft.com/mssql/server:2025-latest` container boots once for the assembly run, each test class gets its own isolated database within it (`Containers/AzureSqlDbContainer.cs`).

**Outside-in integration tests** — `BackendAccountService.IntegrationTests` hosts the API in-process via `WebApplicationFactory<Program>` and sends real HTTP requests, asserting on the deserialised JSON response. Proves the wire contract — routing, model binding, middleware, status codes, JSON shape — that the data project can't see. Also needs Docker; one Testcontainers SQL instance per test class. See [its README](src/BackendAccountService.IntegrationTests/README.md) for the project shape and how it differs from the data tests.

## Database migrations

From `src/`:

```bash
# Add a migration
dotnet ef migrations add <Name> \
  --context AccountsDbContext \
  --startup-project BackendAccountService.Api \
  --project BackendAccountService.Data

# Generate an idempotent SQL script
dotnet ef migrations script \
  --context AccountsDbContext \
  --startup-project BackendAccountService.Api \
  --project BackendAccountService.Data \
  --idempotent \
  --output BackendAccountService.Data/Scripts/migrations.sql
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md)

## Licence

See [LICENCE.md](LICENCE.md)
