using BackendAccountService.Data.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BackendAccountService.IntegrationTests.Infrastructure.Builders;

/// <summary>
/// Fluent entry point for integration-test data construction. Each factory method returns a
/// builder whose chained configuration calls refine the setup; <c>Build()</c> materialises the row.
/// Usage: <c>var producer = await Builder.Producer().Build();</c>
/// </summary>
public sealed class TestBuilders
{
    /// <summary>Author string the audit-tracked SaveChangesAsync overload records on builder-side writes.</summary>
    internal const string BuilderServiceId = "integration-test-builder";

    private readonly AccountApiFactory _factory;

    public TestBuilders(AccountApiFactory factory) => _factory = factory;

    internal async Task WithDbContextAsync(Func<AccountsDbContext, Task> action, bool save)
    {
        using var scope = _factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
        await action(ctx);
        if (save) await ctx.SaveChangesAsync(BuilderServiceId);
    }

    public OrganisationBuilder Organisation(string name) => new(this, name);
    public ProducerBuilder Producer() => new(this);
    public RegulatorBuilder Regulator() => new(this);
    public SchemeOperatorBuilder SchemeOperator() => new(this);
    public ChangeHistoryBuilder ChangeHistoryFor(BuiltEnrolment producer) => new(this, producer);
    public ChildRelationshipBuilder RelationshipFor(BuiltEnrolment parent) => new(this, parent);
    public SchemeMembershipBuilder SchemeMembership(BuiltEnrolment producer, BuiltComplianceSchemeOperatorAdmin operatorAdmin)
        => new(this, producer, operatorAdmin.Admin.Organisation, operatorAdmin.Scheme);
    public SchemeMembershipBuilder SchemeMembership(BuiltEnrolment producer, BuiltComplianceScheme scheme)
        => new(this, producer, scheme.OperatorOrganisation, scheme.Scheme);
}
