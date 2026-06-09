using BackendAccountService.Core.Constants;
using BackendAccountService.Data.Entities;
using OrganisationTypeConst = BackendAccountService.Data.DbConstants.OrganisationType;

namespace BackendAccountService.IntegrationTests.Infrastructure.Builders;

/// <summary>
/// Builds a fresh child Organisation and an active OrganisationRelationship row linking
/// it under the supplied parent. Returns the child Organisation entity.
/// </summary>
public sealed class ChildRelationshipBuilder(TestBuilders builders, BuiltEnrolment parent)
{
    public async Task<Organisation> Build()
    {
        Organisation child = null!;
        await builders.WithDbContextAsync(async ctx =>
        {
            child = new Organisation
            {
                Name = $"Child Subsidiary {Guid.NewGuid():N}".Substring(0, 30),
                ExternalId = Guid.NewGuid(),
                ReferenceNumber = $"R{Random.Shared.Next(100000, 999999)}",
                OrganisationTypeId = OrganisationTypeConst.CompaniesHouseCompany,
                CompaniesHouseNumber = $"SUB{Random.Shared.Next(10000, 99999)}",
            };
            ctx.Organisations.Add(child);
            await ctx.SaveChangesAsync(TestBuilders.BuilderServiceId);

            ctx.OrganisationRelationships.Add(new OrganisationRelationship
            {
                FirstOrganisationId = parent.Organisation.Id,
                SecondOrganisationId = child.Id,
                OrganisationRelationshipTypeId = OrganisationRelationshipTypeIds.Organisation_Relationship_TypeId,
                RelationFromDate = DateTime.UtcNow,
                LastUpdatedById = parent.Person.User!.Id,
                LastUpdatedByOrganisationId = parent.Organisation.Id,
            });
        }, save: true);
        return child;
    }
}
