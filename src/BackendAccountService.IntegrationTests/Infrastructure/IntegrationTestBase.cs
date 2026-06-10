using BackendAccountService.Data.Infrastructure;
using BackendAccountService.IntegrationTests.Infrastructure.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace BackendAccountService.IntegrationTests.Infrastructure;

[Collection(AccountApiCollection.Name)]
[Trait("Category", "IntegrationTest")]
public abstract class IntegrationTestBase
{
    protected AccountApiFactory Factory { get; }
    protected HttpClient Client { get; }

    /// <summary>
    /// Fluent test-data entrypoint. See <see cref="TestBuilders"/> for the available builders —
    /// <c>Builder.Producer().Build()</c>, <c>Builder.Regulator().InNation(x).Build()</c>,
    /// <c>Builder.SchemeOperator().WithAdmin().Build()</c>, etc.
    /// </summary>
    protected TestBuilders Builder { get; }

    protected IntegrationTestBase(AccountApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Builder = new TestBuilders(factory);
    }

    /// <summary>
    /// HttpClient with the request headers most Account API endpoints expect: X-EPR-User
    /// (the calling user's externalId) and X-EPR-Organisation (their org). Use this when an
    /// endpoint reads either header (most do — search controllers for `Request.Headers["X-EPR-...`).
    /// </summary>
    protected HttpClient ClientFor(Guid userId, Guid organisationId)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-EPR-User", userId.ToString());
        client.DefaultRequestHeaders.Add("X-EPR-Organisation", organisationId.ToString());
        return client;
    }

    /// <summary>
    /// Ad-hoc seed entrypoint for cases the fluent <see cref="Builder"/> builders don't cover.
    /// Picks the audit-aware SaveChangesAsync overload so audit-tracked entities don't blow
    /// up inside the test setup. Pass userId+organisationId for a user-attributed audit row,
    /// or omit both to fall back to the serviceId path.
    /// </summary>
    protected async Task SeedAsync(
        Func<AccountsDbContext, Task> action,
        Guid? userId = null,
        Guid? organisationId = null,
        string serviceId = TestBuilders.BuilderServiceId)
    {
        using var scope = Factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
        await action(ctx);
        if (userId.HasValue && organisationId.HasValue)
        {
            await ctx.SaveChangesAsync(userId.Value, organisationId.Value);
        }
        else
        {
            await ctx.SaveChangesAsync(serviceId);
        }
    }

    /// <summary>
    /// Read-only counterpart to <see cref="SeedAsync"/> — runs an action against a scoped
    /// AccountsDbContext without calling SaveChanges. Use for assertions that need DB-side
    /// state inspection (e.g. confirm a soft-delete flag flipped, audit row appeared).
    /// </summary>
    protected async Task WithDbContext(Func<AccountsDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
        await action(ctx);
    }
}
