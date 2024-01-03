FROM defradigital/dotnetcore:dotnet6.0 AS base
USER root
ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080

RUN apk update && apk --no-cache add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

FROM defradigital/dotnetcore-development:dotnet6.0 AS build
USER root
WORKDIR /src
COPY ["BackendAccountService.Api/BackendAccountService.Api.csproj", "BackendAccountService.Api/"]
COPY ["BackendAccountService.Core/BackendAccountService.Core.csproj", "BackendAccountService.Core/"]
COPY ["BackendAccountService.Data/BackendAccountService.Data.csproj", "BackendAccountService.Data/"]
RUN dotnet restore "BackendAccountService.Api/BackendAccountService.Api.csproj"

COPY --chown=dotnet:dotnet BackendAccountService.Api/. ./BackendAccountService.Api/.
COPY --chown=dotnet:dotnet BackendAccountService.Core/. ./BackendAccountService.Core/.
COPY --chown=dotnet:dotnet BackendAccountService.Data/. ./BackendAccountService.Data/.

WORKDIR "/src/BackendAccountService.Api"
RUN dotnet build "BackendAccountService.Api.csproj" -c Release /p:AzureBuild=true -o /app/build

FROM build AS publish
RUN dotnet publish "BackendAccountService.Api.csproj" -c Release /p:AzureBuild=true -o /app/publish

FROM base AS final

# Switch to the non-root user
USER dotnet

WORKDIR /app
COPY --from=publish /app/publish .
USER dotnet
ENTRYPOINT ["dotnet", "BackendAccountService.Api.dll"]
