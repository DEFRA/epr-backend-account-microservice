using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace BackendAccountService.Data.IntegrationTests.Containers;

public class AzureSqlEdgeDbContainer
{
    public string? ConnectionString { get; private init; }
    private IContainer Container { get; init; }

    private static readonly Random RandomPortSequence = new(31415);

    public static async Task<AzureSqlEdgeDbContainer> StartDockerDbAsync()
    {
        return await StartDockerDbAsync(RandomPortSequence.Next(60000, 64000));
    }

    public static async Task<AzureSqlEdgeDbContainer> StartDockerDbAsync(int portNumber)
    {
        const string database = "Accounts";
        const string saPassword = "Password1!";

        var dbContainer =
            new ContainerBuilder()
                .WithImage("mcr.microsoft.com/azure-sql-edge")
                .WithEnvironment("MSSQL_SA_PASSWORD", saPassword)
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithExposedPort(portNumber)
                .WithPortBinding(portNumber, 1433)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
                .Build();

        await dbContainer.StartAsync();

        return new AzureSqlEdgeDbContainer()
        {
            ConnectionString = $"Server=127.0.0.1,{portNumber};Initial Catalog={database};User Id=sa;Password={saPassword};TrustServerCertificate=True;",
            Container = dbContainer
        };
    }

    public async Task StopAsync()
    {
        await Container.StopAsync()!;
    }
}
