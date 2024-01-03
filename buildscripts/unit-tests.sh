#!/usr/bin/env bash

dotnet test src/BackendAccountService.Api.UnitTests/BackendAccountService.Api.UnitTests.csproj --logger "trx;logfilename=testResults.trx"
dotnet test src/BackendAccountService.Core.UnitTests/BackendAccountService.Core.UnitTests.csproj --logger "trx;logfilename=testResults.trx"
dotnet test src/BackendAccountService.ValidationData.Api/BackendAccountService.ValidationData.Api.UnitTests.csproj --logger "trx;logfilename=testResults.trx"