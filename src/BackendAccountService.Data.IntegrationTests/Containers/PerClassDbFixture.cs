using Microsoft.Data.SqlClient;

namespace BackendAccountService.Data.IntegrationTests.Containers;

/// <summary>
/// Per-class isolated database, served from the shared SQL Server container.
/// <see cref="InitializeAsync"/> RESTOREs from the template (sub-second), which is much faster
/// than re-emitting the EF model DDL per class via EnsureCreatedAsync.
/// <see cref="DisposeAsync"/> drops the per-class DB but leaves the container running.
/// </summary>
public class PerClassDbFixture : IAsyncLifetime
{
    private readonly SharedSqlFixture _shared;
    private string _databaseName = null!;

    public string ConnectionString { get; private set; } = null!;

    public PerClassDbFixture(SharedSqlFixture shared)
    {
        _shared = shared;
    }

    public virtual async Task InitializeAsync()
    {
        _databaseName = "Accounts_" + Guid.NewGuid().ToString("N").Substring(0, 12);
        await _shared.RestoreFromTemplateAsync(_databaseName);

        ConnectionString = new SqlConnectionStringBuilder(_shared.MasterConnectionString)
        {
            InitialCatalog = _databaseName,
        }.ConnectionString;
    }

    public virtual async Task DisposeAsync()
    {
        // SINGLE_USER WITH ROLLBACK kicks any lingering connections so DROP can proceed.
        await SharedSqlFixture.ExecuteAsync(
            _shared.MasterConnectionString,
            $"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_databaseName}]");
    }
}
