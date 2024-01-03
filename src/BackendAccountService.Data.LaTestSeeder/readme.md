# Introduction
This utility app will generate fake test data into the accounts database and 
will execute when running the laps version of docker compose.

The app can also be executed independently against an existing accounts database.

# Instructions

To run as part of docker compose:

1. From the root of the repository run the following make command in the terminal:
    - `make run-laps`
2. To validate or view the inserted data, open your SQL query tool of choice (e.g. SQL Server Management Studio or Azure Data Studio),
open up and run the SQL script found in src/BackendAccountService.Data.TestSeeder/view_la-user-orgs.sql against the accounts database instance running in docker
3. When exiting the running docker environment via the terminal, this will stop the containers and tear down all running apps, including volumes and data stored, meaning any changes made during runtime will be lost

To run manually against an existing accounts database:

1. Within your IDE, navigate to the BackendAccountService.Data.TestSeeder project
2. Verify the connection string is correct within appsettings.json
3. Then simply run the TestSeeder console application in the IDE

> The tool can be run multiple times, generating unique records each time

### Associating Externally Authenticated Test Users

See [here](TestUsers/readme.md) for instructions on how to add externally authenticated user ids to test users

# Notes

There are 2 methods executed as part of the data generation to insert some deterministic data that can be consistently used for testing purposes, followed by some randomized data.

### Deterministic Data

The `GenerateStableLocalAuthorityData` method will generate test data for the following scenarios: 
- 1 user who works for a Welsh LA
- 1 user who works for a English LA
- 1 user who works for a Scottish LA
- 1 user who works for a NI LA
- 1 user who works for 2 English LA
- 1 user who works for 6 English LA

These records will all have companies house numbers prefixed with ENG, WAL, SCO and NRE followed by the number of associated user orgs
as follows:

- `WAL1####` for 1 user who works for a Welsh LA
- `ENG1####` for 1 user who works for a English LA
- `NRE1####` for 1 user who works for a Scottish LA
- `WAL1####` for 1 user who works for a NI LA
- `ENG2####` for 1 user who works for 2 English LA
- `ENG6####` for 1 user who works for 6 English LA

The companies house numbers are 8 characters long, with the remaining characters filled with random numbers (replacing # in the examples)

### Random Data

The `GenerateRandomLocalAuthorityData` method will generate 400 local authority orgs and 4000 users associated with those orgs.

The random orgs can be differentiated from the deterministic orgs by the companies house number, random orgs are prefixed with `Z` e.g. `Z5189000`