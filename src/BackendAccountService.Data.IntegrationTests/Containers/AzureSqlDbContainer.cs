using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace BackendAccountService.Data.IntegrationTests.Containers;

public class AzureSqlDbContainer
{
    public string? ConnectionString { get; private init; }
    private MsSqlContainer Container { get; init; }

    private static readonly Random RandomPortSequence = new(31415);

    public static async Task<AzureSqlDbContainer> StartDockerDbAsync()
    {
        return await StartDockerDbAsync(RandomPortSequence.Next(60000, 64000));
    }

    public static async Task<AzureSqlDbContainer> StartDockerDbAsync(int portNumber)
    {
        const string databaseName = "Accounts";

        var dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
            .WithPassword("Password1!")
            .WithCreateParameterModifier(parameters =>
            {
                parameters.HostConfig.Memory = (long)(3.5 * 1024 * 1024 * 1024);
            })
            .Build();

        try
        {
            await dbContainer.StartAsync();
        }
        catch
        {
            var logs = await dbContainer.GetLogsAsync();
            Console.WriteLine(logs);
            throw;
        }
        
        await dbContainer.ExecScriptAsync($"CREATE DATABASE {databaseName};");

        var masterConnectionString = dbContainer.GetConnectionString();
        var connectionBuilder = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = databaseName
        };

        return new AzureSqlDbContainer()
        {
            ConnectionString = connectionBuilder.ConnectionString,
            Container = dbContainer
        };
    }

    public async Task StopAsync()
    {
        await Container.StopAsync()!;
    }
}
