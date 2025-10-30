FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

WORKDIR /src

COPY ./BackendAccountService.Data.LaTestSeeder ./BackendAccountService.Data.LaTestSeeder
COPY ./BackendAccountService.Data ./BackendAccountService.Data

WORKDIR BackendAccountService.Data.LaTestSeeder
RUN dotnet restore
RUN dotnet publish -c Release -o out -p:AzureBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /src/BackendAccountService.Data.LaTestSeeder/out .
CMD ["sh", "-c", "sleep 10s && dotnet BackendAccountService.Data.LaTestSeeder.dll"]