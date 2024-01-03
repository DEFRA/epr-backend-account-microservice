using System;
using System.Linq;
using System.Threading.Tasks;
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
}