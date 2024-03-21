using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Infrastructure;
using BackendAccountService.ValidationData.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.ValidationData.Api.Services;

public class CompanyDetailsDataService : ICompanyDetailsDataService
{
    private readonly AccountsDbContext _accountsDbContext;

    public CompanyDetailsDataService(AccountsDbContext accountsDbContext)
    {
        _accountsDbContext = accountsDbContext;
    }

    public async Task<CompanyDetailsResponse> GetCompanyDetailsByOrganisationReferenceNumber(string referenceNumber)
    {
        var organisations = await _accountsDbContext.Organisations
            .AsNoTracking()
            .Where(organisation => organisation.ReferenceNumber == referenceNumber)
            .ToListAsync();

        if (organisations is null || !organisations.Any())
        {
            return null;
        }

        return GenerateCompanyDetailsResponse(organisations);
    }

    public async Task<CompanyDetailsResponse> GetCompanyDetailsByOrganisationReferenceNumberAndComplianceSchemeId(string referenceNumber, Guid? complianceSchemeId)
    {
        var complianceScheme = _accountsDbContext.ComplianceSchemes
            .AsNoTracking()
            .FirstOrDefault(complianceScheme => complianceScheme.ExternalId == complianceSchemeId);

        if (complianceScheme is null)
        {
            return null;
        }

        var selectedSchemes = await _accountsDbContext.SelectedSchemes
            .AsNoTracking()
            .Where(selectedScheme =>
                selectedScheme.ComplianceSchemeId == complianceScheme.Id &&
                selectedScheme.OrganisationConnection.FromOrganisationRoleId == Data.DbConstants.InterOrganisationRole.Producer &&
                selectedScheme.OrganisationConnection.FromOrganisation.ReferenceNumber == referenceNumber &&
                selectedScheme.OrganisationConnection.ToOrganisationRoleId == Data.DbConstants.InterOrganisationRole.ComplianceScheme
            )
            .Select(member => new
            {
                ReferenceNumber = member.OrganisationConnection.FromOrganisation.ReferenceNumber.ToString(),
                CompaniesHouseNumber = member.OrganisationConnection.FromOrganisation.CompaniesHouseNumber != null
                ? member.OrganisationConnection.FromOrganisation.CompaniesHouseNumber.ToString()
                : null
            })
            .ToListAsync();

        if (selectedSchemes is null || !selectedSchemes.Any())
        {
            return null;
        }

        List<Organisation> organisations = new List<Organisation>();
        foreach (var scheme in selectedSchemes)
        {
            organisations.Add(new Organisation { ReferenceNumber = scheme.ReferenceNumber, CompaniesHouseNumber = scheme.CompaniesHouseNumber });
        }

        return GenerateCompanyDetailsResponse(organisations);
    }

    public async Task<CompanyDetailsResponse> GetAllProducersCompanyDetails(IEnumerable<string> referenceNumbers)
    {

        var organisations = await _accountsDbContext.Organisations
            .AsNoTracking()
            .Where(organisation => referenceNumbers.Contains(organisation.ReferenceNumber) && !organisation.IsComplianceScheme && (organisation.OrganisationTypeId == 1 || organisation.OrganisationTypeId == 2))
            .ToListAsync();

        if (organisations is null || !organisations.Any())
        {
            return null;
        }

        return GenerateCompanyDetailsResponse(organisations);
    }

    private CompanyDetailsResponse GenerateCompanyDetailsResponse(List<Organisation> organisations)
    {
        var result = new List<CompanyDetailResponse>();

        foreach (var org in organisations)
        {
            result.Add(new CompanyDetailResponse { ReferenceNumber = org.ReferenceNumber, CompaniesHouseNumber = org.CompaniesHouseNumber });

        }

        var organisationsResult = new CompanyDetailsResponse
        {
            Organisations = result
        };

        return organisationsResult;
    }
}