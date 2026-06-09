# Agent Instructions — epr-backend-account-microservice

Human-facing setup is in [`README.md`](README.md). System-wide agent guidance lives in [DEFRA/epr-local-environment/agents/](https://github.com/DEFRA/epr-local-environment/tree/main/agents) (architecture, glossary, gotchas) — clone as a sibling of this repo if you haven't already; the agents docs assume that layout for cross-repo grep.

This repo owns `AccountsDb` (SQL Server). EF Core migrations live under `src/BackendAccountService.Data/Migrations`.

## Inspecting the local database

When the compose stack is up, `docker exec` + `sqlcmd` gives you arbitrary SQL — aggregates and joins drop the answer in one shot, and the output is plain text.

```bash
# 1. Find the running SQL container (name varies between compose contexts)
docker ps --filter ancestor=mcr.microsoft.com/azure-sql-edge --format '{{.Names}}'

# 2. Read the SA password from container env (do NOT hardcode — varies:
#    src/docker-compose.yml uses Password1!,
#    https://github.com/DEFRA/epr-local-environment/blob/main/compose.yml uses s4usag3s!)
docker inspect <container> --format '{{range .Config.Env}}{{println .}}{{end}}' | grep MSSQL_SA_PASSWORD

# 3. Run any SQL
docker exec <container> /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P '<password>' -d AccountsDb -W -s '|' \
  -Q "SELECT t.name, p.rows FROM sys.tables t
      JOIN sys.partitions p ON p.object_id = t.object_id
      WHERE p.index_id < 2 ORDER BY p.rows DESC;"
```

That starter query is a good "what's in here right now?" — row counts per table, showing empty vs seeded at a glance. From there, `dbo.ComplianceSchemes` (~70 rows) is the main seeded reference table; `dbo.Organisations`/`Persons`/`Users` typically have just the bootstrap `System Organisation` / `system@dummy.com` row.

## Conventions worth knowing

- EF Core conventions on every entity: int PK + `ExternalId` (uniqueidentifier), `CreatedOn` / `LastUpdatedOn` (`datetimeoffset`), soft-delete via `IsDeleted` (`bit`).
- `NationId` FK appears on anything with a regulator dimension (1=England, 2=NI, 3=Scotland, 4=Wales).
- Many `ComplianceSchemes` rows are soft-delete-and-replace duplicates (deleted predecessor with `NationId=NULL`, replacement carries the nation). Don't treat duplicates by `Name` or `CompaniesHouseNumber` as a data bug.
