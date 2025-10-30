# Use the SDK image to build your application
# FROM defradigital/dotnetcore-development:dotnet8.0 AS installer-env
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS installer-env
EXPOSE 80
EXPOSE 443

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy everything
COPY BackendAccountService.ValidationData.Api/. ./BackendAccountService.ValidationData.Api/.
COPY BackendAccountService.Data/. ./BackendAccountService.Data/.

RUN dotnet publish BackendAccountService.ValidationData.Api/*.csproj --output /home/site/wwwroot

# FROM defradigital/dotnetcore:dotnet8.0
FROM mcr.microsoft.com/azure-functions/dotnet:4-dotnet8

# Create a non-root user and set permissions
RUN groupadd -r dotnet && \
    useradd -r -g dotnet dotnet && \
    chown -R dotnet /home

# Switch to the non-root user for the final image
USER dotnet

# Set environment variables
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

# Copy the published application from the installer-env image
COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]