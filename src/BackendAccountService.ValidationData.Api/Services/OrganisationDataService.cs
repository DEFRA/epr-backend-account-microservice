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
        var organisation = _accountsDbContext.Organisations
            .AsNoTracking()
            .FirstOrDefault(org => org.ExternalId == organisationExternalId);

        if (organisation is null)
        {
            return null;
        }

        return new OrganisationResponse
        {
            ReferenceNumber = organisation.ReferenceNumber,
            IsComplianceScheme = organisation.IsComplianceScheme
        };
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

    private async Task<List<string>> GetMembers(Guid organisationExternalId, Guid? complianceSchemeId)
    {
        var complianceScheme = _accountsDbContext.ComplianceSchemes
            .AsNoTracking()
            .FirstOrDefault(complianceScheme => complianceScheme.ExternalId == complianceSchemeId);

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