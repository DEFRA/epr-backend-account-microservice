# Introduction 
This repository contains the Account Management Service API and database schema. The Account API will be called from facades and will be a CRUD based API persisting data with a SQL database.

# Getting Started
Ensure Docker is installed and running.

# Build and Test
## Zscaler Certificates
As we run behind a corporate firewall we need to manually add the trusted Zscaler certificate chain when building the custom docker images. If you wish to use container orchestration locally grab the latest certificate found [here](https://kainossoftwareltd.sharepoint.com/security/Shared%20Documents/Forms/AllItems.aspx?ga=1&id=%2Fsecurity%2FShared%20Documents%2FNetwork%20Security%20and%20Least%20Privilege%2FZscaler%20%2D%20Certificates%2FApril%202023%2D2024&viewid=e6c47c59%2D0b48%2D4836%2D804d%2D87159c4333be) and paste it to the src/certs directory. Do not check this into the repo.

## Running locally
A docker-compose file exists in the root of the src folder. Opening a terminal at the src folder and running,
```
docker compose up
```
will build a docker project composing of two containers one for the SQL database and one for the API.

In development the database will migrate automatically to the SQL container, if you wish the database to persist ensure you have mapped a Docker volume.

If ports clash on your local machine change them in the docker compose files.
# Database Migrations
## Add
To add a new migration fun the following command in the src directory
```
dotnet ef migrations add <Migration Name> --context AccountsDbContext --startup-project BackendAccountService.Api --project BackendAccountService.Data
```

# Contributing to this project
Please read the [contribution guidelines](CONTRIBUTING.md) before submitting a pull request.

# Licence
[Licence information](LICENCE.md).