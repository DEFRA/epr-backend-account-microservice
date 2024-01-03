# Use the SDK image to build your application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS installer-env
EXPOSE 80
EXPOSE 443

# Build requires 3.1 SDK
COPY --from=mcr.microsoft.com/dotnet/core/sdk:3.1 /usr/share/dotnet /usr/share/dotnet

# Copy everything
COPY BackendAccountService.ValidationData.Api/. ./BackendAccountService.ValidationData.Api/.
COPY BackendAccountService.Data/. ./BackendAccountService.Data/.

RUN dotnet publish BackendAccountService.ValidationData.Api/*.csproj --output /home/site/wwwroot

FROM mcr.microsoft.com/azure-functions/dotnet:4

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
COPY --from=installer-env --chown=dotnet ["/home/site/wwwroot", "/home/site/wwwroot"]