using BackendAccountService.Data.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace BackendAccountService.Data.IntegrationTests.Containers;

/// <summary>
/// Owns the single MsSql container shared across the test assembly. The container is started
/// once via <see cref="InitializeAsync"/>, used to materialise a schema-only template DB
/// (<see cref="TemplateName"/>), and torn down by <see cref="DisposeAsync"/>. Per-class DBs
/// are then cheap RESTOREs of that template instead of re-running EnsureCreatedAsync (~5s of
/// DDL emit) every time.
/// Wired into test classes via <see cref="SharedSqlCollection"/>.
/// </summary>
public class SharedSqlFixture : IAsyncLifetime
{
    private const string TemplateName = "AccountsTemplate";
    private const string TemplateBackupPath = "/var/opt/mssql/data/AccountsTemplate.bak";
    private const string DataDir = "/var/opt/mssql/data";

    private MsSqlContainer? _container;

    public string MasterConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // The 3.5 GiB cap pairs with maxParallelThreads = 4 in xunit.runner.json: four
        // concurrent per-class DBs fit comfortably under this limit. Raise both together
        // if the suite grows; raising one without the other risks OOM in the container.
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
            .WithPassword("Password1!")
            .WithCreateParameterModifier(p => p.HostConfig.Memory = (long)(3.5 * 1024 * 1024 * 1024))
            .Build();
        await _container.StartAsync();
        MasterConnectionString = _container.GetConnectionString();

        await PrepareTemplateAsync(MasterConnectionString);
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
            _container = null;
        }
    }

    public async Task RestoreFromTemplateAsync(string targetDb)
    {
        // Logical file names follow the original DB name. WITH MOVE redirects the physical
        // files so each restored DB gets its own mdf/ldf; REPLACE allows a clean overwrite
        // if a target of the same name somehow already exists.
        await ExecuteAsync(MasterConnectionString, $"""
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
}

/// <summary>
/// Collection definition that binds <see cref="SharedSqlFixture"/> as a single
/// instance shared across every test class that opts into <c>[Collection(Name)]</c>.
/// </summary>
[CollectionDefinition(Name)]
public class SharedSqlCollection : ICollectionFixture<SharedSqlFixture>
{
    public const string Name = "SharedSql";
}
