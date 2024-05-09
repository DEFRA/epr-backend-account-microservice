using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.ValidationData.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.ValidationData.Api.Services;

public class OrganisationDataService : IOrganisationDataService
{
    private readonly AccountsDbContext _accountsDbContext;

    public OrganisationDataService(AccountsDbContext accountsDbContext)
    {
        _accountsDbContext = accountsDbContext;
    }

    public async Task<OrganisationResponse> GetOrganisationByExternalId(Guid organisationExternalId)
    {
        return await _accountsDbContext.Organisations
            .AsNoTracking()
            .Where(org => org.ExternalId == organisationExternalId)
            .Select(org => new OrganisationResponse
            {
                ReferenceNumber = org.ReferenceNumber,
                IsComplianceScheme = org.IsComplianceScheme
            })
            .FirstOrDefaultAsync();
    }

    public async Task<OrganisationMembersResponse> GetOrganisationMembersByComplianceSchemeId(Guid organisationExternalId, Guid? complianceSchemeId)
    {
        var organisationWithMembers = await GetMembers(organisationExternalId, complianceSchemeId);

        if (organisationWithMembers is null)
        {
            return null;
        }

        return new OrganisationMembersResponse
        {
            MemberOrganisations = organisationWithMembers
        };
    }

    public async Task<OrganisationsResponse> GetExistingOrganisationsByReferenceNumber(IEnumerable<string> referenceNumbers)
    {
        var organisations = await _accountsDbContext.Organisations
            .AsNoTracking()
            .Where(organisation => referenceNumbers.Contains(organisation.ReferenceNumber))
            .Select(org => org.ReferenceNumber.ToString())
            .ToListAsync();

        return new OrganisationsResponse
        {
            ReferenceNumbers = organisations
        };
    }

    private async Task<List<string>> GetMembers(Guid organisationExternalId, Guid? complianceSchemeId)
    {
       var complianceScheme = await _accountsDbContext.ComplianceSchemes
           .AsNoTracking()
           .Where(complianceScheme => complianceScheme.ExternalId == complianceSchemeId)
           .Select(complianceScheme => new
           {
               complianceScheme.Id,
           })
           .FirstOrDefaultAsync();

        if (complianceScheme is null)
        {
            return null;
        }

        return await _accountsDbContext.SelectedSchemes
            .AsNoTracking()
            .Where(selectedScheme =>
                selectedScheme.ComplianceSchemeId == complianceScheme.Id &&
                selectedScheme.OrganisationConnection.FromOrganisationRoleId == InterOrganisationRole.Producer &&
                selectedScheme.OrganisationConnection.ToOrganisation.ExternalId == organisationExternalId &&
                selectedScheme.OrganisationConnection.ToOrganisationRoleId == InterOrganisationRole.ComplianceScheme
            )
            .Select(member => member.OrganisationConnection.FromOrganisation.ReferenceNumber.ToString())
            .ToListAsync();
    }
}