# epr-backend-account-microservice

Two .NET 8 services in one repo:

- **BackendAccountService.Api** (WA 407) — account/organisation/enrolment CRUD against `accountsDB` (SQL Server).
- **BackendAccountService.ValidationData.Api** (FA 407) — read-only org lookups for validation functions, hosted on Azure Functions.

Both deploy independently. Shared code lives in `BackendAccountService.Core` and `BackendAccountService.Data`.

## Prerequisites

- .NET 8 SDK
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

**Integration tests** — `BackendAccountService.Data.IntegrationTests` exercises `AccountsDbContext` against a Testcontainers-managed `mcr.microsoft.com/mssql/server:2025-latest` instance (see `Containers/AzureSqlDbContainer.cs`). Needs Docker. Each test class starts and tears down its own SQL container, so the full suite takes **several minutes** to run.

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
