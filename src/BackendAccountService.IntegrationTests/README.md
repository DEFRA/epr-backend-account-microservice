# BackendAccountService.IntegrationTests

[Outside-in integration tests](https://0x5.uk/2024/03/27/why-do-automated-tests-matter/#:~:text=Outside%2Din%20tests) for `BackendAccountService.Api`. Each test sends real HTTP requests to an in-process instance of the API hosted by `WebApplicationFactory<Program>`, which talks to a Testcontainers-managed SQL Server.

## Why this project exists alongside `BackendAccountService.Data.IntegrationTests`

The two projects target different layers:

- **`.Data.IntegrationTests`** instantiates controllers directly (`new ConnectionsController(...)` and calls action methods). It proves the EF Core mappings, soft-delete query filter, audit log behaviour, and query extensions against a real SQL Server. It does **not** exercise routing, model binding, middleware, content negotiation, or HTTP status code resolution. Keep it for data-layer specifics.
- **`.IntegrationTests`** (this project) sends HTTP requests via `HttpClient` and asserts on the deserialised JSON response. It proves the full request pipeline — including everything `.Data.IntegrationTests` skips, and including provider-specific SQL behaviour that EF Core's in-memory provider lies about.

Use this project for any test where the wire contract matters (status codes, routing, JSON shape, model binding, validation). Use `.Data.IntegrationTests` for query-translation correctness and data-layer invariants.

## Running

```bash
dotnet test src/BackendAccountService.IntegrationTests
```

Requires Docker. One MsSql container + one migrated database + one `WebApplicationFactory<Program>` are shared across every test class via `AccountApiCollection` (xUnit collection fixture). Cost: ~20s container/factory boot once, then sub-second per test. Trade-off: classes in the collection run serially — fine because there's no per-class fixture cost left to parallelise away. Tests stay independent by using `Guid.NewGuid()` or per-class label prefixes (e.g. `LA-001…LA-006`); a `ContainSingle(x => x.Name == "...")` predicate is safe as long as the literal is unique across the whole suite.

## Notes

- **No WireMock or test auth.** The Account API doesn't call any external HTTP services — the `NotificationsService` is misleadingly named; it's a DB-only in-app feed. There's no app-level auth either (security is at the Azure infra layer), so tests just set the X-EPR headers the API expects.
- **The data-generator copy.** `Infrastructure/Builders/` holds a copy of `DatabaseDataGenerator` and `RandomModelData` rather than a project reference to `.Data.IntegrationTests`. Deliberate — keeping the two test projects independent means a refactor in one can't accidentally break the other.
