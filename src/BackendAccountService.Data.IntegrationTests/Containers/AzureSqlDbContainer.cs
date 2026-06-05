using BackendAccountService.Data.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace BackendAccountService.Data.IntegrationTests.Containers;

/// <summary>
/// Per-class isolated database, served from a single SQL Server container shared across
/// the whole test assembly. The first call to <see cref="StartDockerDbAsync()"/> boots the
/// shared container (slow) and primes a schema-only template DB (slow); subsequent calls
/// RESTORE from that template (sub-second), which is much faster than re-emitting the EF
/// model DDL per class via EnsureCreatedAsync.
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
        await SharedSqlServer.RestoreFromTemplateAsync(databaseName);

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
        await SharedSqlServer.ExecuteAsync(
            masterConnectionString,
            $"ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{DatabaseName}]");
    }
}

/// <summary>
/// Owns the single MsSql container shared across the test assembly. The container is
/// lazily started on first request, used to materialise a schema-only template DB
/// (<see cref="TemplateName"/>), and torn down by <see cref="AssemblyCleanup"/>.
/// Per-class DBs are then cheap RESTOREs of that template instead of re-running
/// EnsureCreatedAsync (~5s of DDL emit) every time.
/// </summary>
[TestClass]
public static class SharedSqlServer
{
    private const string TemplateName = "AccountsTemplate";
    private const string TemplateBackupPath = "/var/opt/mssql/data/AccountsTemplate.bak";
    private const string DataDir = "/var/opt/mssql/data";

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

            await PrepareTemplateAsync(_masterConnectionString);

            return _masterConnectionString;
        }
        finally
        {
            Gate.Release();
        }
    }

    public static async Task RestoreFromTemplateAsync(string targetDb)
    {
        var master = await GetMasterConnectionStringAsync();
        // Logical file names follow the original DB name. WITH MOVE redirects the physical
        // files so each restored DB gets its own mdf/ldf; REPLACE allows a clean overwrite
        // if a target of the same name somehow already exists.
        await ExecuteAsync(master, $"""
            RESTORE DATABASE [{targetDb}] FROM DISK = N'{TemplateBackupPath}'
            WITH MOVE N'{TemplateName}' TO N'{DataDir}/{targetDb}.mdf',
                 MOVE N'{TemplateName}_log' TO N'{DataDir}/{targetDb}_log.ldf',
                 REPLACE
            """);
    }

    internal static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task PrepareTemplateAsync(string masterConnectionString)
    {
        // Fresh start: drop any stale template (e.g. from a previous interrupted run reusing a docker volume).
        await ExecuteAsync(masterConnectionString, $"""
            IF DB_ID(N'{TemplateName}') IS NOT NULL
            BEGIN
                ALTER DATABASE [{TemplateName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{TemplateName}];
            END
            """);

        await ExecuteAsync(masterConnectionString, $"CREATE DATABASE [{TemplateName}]");

        // Materialise the schema once via EF Core — this is the slow step we're trying to
        // amortise. EnsureCreatedAsync emits CREATE TABLE / CREATE INDEX / FK for every
        // entity in the AccountsDbContext model.
        var templateConnection = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = TemplateName,
        }.ConnectionString;

        await using (var ctx = new AccountsDbContext(
            new DbContextOptionsBuilder<AccountsDbContext>()
                .UseSqlServer(templateConnection)
                .Options))
        {
            // Schema via EnsureCreatedAsync — gives us empty tables (no seed data inserts
            // from migrations, which several tests assume away).
            await ctx.Database.EnsureCreatedAsync();

            // Then create __EFMigrationsHistory and mark every known migration as applied,
            // so the ~8 test classes that call MigrateAsync see a fully-migrated DB and no-op
            // instead of re-running CREATE TABLE on existing tables.
            var historyRepo = ctx.Database.GetService<IHistoryRepository>();
            var migrationsAssembly = ctx.Database.GetService<IMigrationsAssembly>();
            await ExecuteAsync(templateConnection, historyRepo.GetCreateScript());
            foreach (var migrationId in migrationsAssembly.Migrations.Keys)
            {
                var insertSql = historyRepo.GetInsertScript(new HistoryRow(migrationId, ProductInfo.GetVersion()));
                await ExecuteAsync(templateConnection, insertSql);
            }
        }

        // BACKUP to a file inside the container's data dir. RESTORE per class will read this.
        await ExecuteAsync(masterConnectionString, $"""
            BACKUP DATABASE [{TemplateName}] TO DISK = N'{TemplateBackupPath}'
            WITH INIT, FORMAT, COMPRESSION
            """);
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
