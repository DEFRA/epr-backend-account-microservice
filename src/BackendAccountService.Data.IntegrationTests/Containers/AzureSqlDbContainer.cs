using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace BackendAccountService.Data.IntegrationTests.Containers;

/// <summary>
/// Per-class isolated database, served from a single SQL Server container shared across
/// the whole test assembly. The first call to <see cref="StartDockerDbAsync()"/> boots the
/// shared container (slow); subsequent calls just CREATE DATABASE inside it (sub-second).
/// <see cref="StopAsync"/> drops the per-class DB but leaves the container running.
/// <see cref="SharedSqlServer.AssemblyCleanup"/> tears the container down once the run is over.
/// </summary>
public class AzureSqlDbContainer
{
    public string? ConnectionString { get; private init; }
    private string DatabaseName { get; init; } = null!;

    public static async Task<AzureSqlDbContainer> StartDockerDbAsync()
    {
        var masterConnectionString = await SharedSqlServer.GetMasterConnectionStringAsync();

        var databaseName = "Accounts_" + Guid.NewGuid().ToString("N").Substring(0, 12);
        await ExecuteAsync(masterConnectionString, $"CREATE DATABASE [{databaseName}]");

        var connectionBuilder = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = databaseName,
        };

        return new AzureSqlDbContainer
        {
            ConnectionString = connectionBuilder.ConnectionString,
            DatabaseName = databaseName,
        };
    }

    /// <summary>
    /// Kept for backwards compatibility with the old port-overload callers — the port
    /// argument is now ignored (the shared container picks its own port).
    /// </summary>
    public static Task<AzureSqlDbContainer> StartDockerDbAsync(int _) => StartDockerDbAsync();

    public async Task StopAsync()
    {
        var masterConnectionString = await SharedSqlServer.GetMasterConnectionStringAsync();
        // SINGLE_USER WITH ROLLBACK kicks any lingering connections so DROP can proceed.
        await ExecuteAsync(
            masterConnectionString,
            $"ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{DatabaseName}]");
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }
}

/// <summary>
/// Owns the single MsSql container shared across the test assembly. The container is
/// lazily started on first request and torn down by <see cref="AssemblyCleanup"/>.
/// </summary>
[TestClass]
public static class SharedSqlServer
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static MsSqlContainer? _container;
    private static string? _masterConnectionString;

    public static async Task<string> GetMasterConnectionStringAsync()
    {
        if (_masterConnectionString is not null) return _masterConnectionString;

        await Gate.WaitAsync();
        try
        {
            if (_masterConnectionString is not null) return _masterConnectionString;

            _container = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
                .WithPassword("Password1!")
                .WithCreateParameterModifier(p => p.HostConfig.Memory = (long)(3.5 * 1024 * 1024 * 1024))
                .Build();
            await _container.StartAsync();
            _masterConnectionString = _container.GetConnectionString();
            return _masterConnectionString;
        }
        finally
        {
            Gate.Release();
        }
    }

    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
            _container = null;
            _masterConnectionString = null;
        }
    }
}
