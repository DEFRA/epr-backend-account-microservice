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
            .Select(org => new CompanyDetailResponse
            {
                ReferenceNumber = org.ReferenceNumber,
                CompaniesHouseNumber = org.CompaniesHouseNumber
            })
            .ToListAsync();

        return organisations is null || !organisations.Any()
            ? null
            : new CompanyDetailsResponse
            {
                Organisations = organisations
            };
    }

    public async Task<CompanyDetailsResponse> GetCompanyDetailsByOrganisationExternalId(Guid organisationExternalId)
    {
        var organisations = await _accountsDbContext.Organisations
            .AsNoTracking()
            .Where(organisation => organisation.ExternalId == organisationExternalId)
            .Select(org => new CompanyDetailResponse
            {
                ReferenceNumber = org.ReferenceNumber,
                CompaniesHouseNumber = org.CompaniesHouseNumber
            })
            .ToListAsync();

        return organisations is null || !organisations.Any()
            ? null
            : new CompanyDetailsResponse
            {
                Organisations = organisations
            };
    }

    public async Task<CompanyDetailsResponse> GetCompanyDetailsByOrganisationReferenceNumberAndComplianceSchemeId(string referenceNumber, Guid? complianceSchemeId)
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

        var selectedSchemes = await _accountsDbContext.SelectedSchemes
            .AsNoTracking()
            .Where(selectedScheme =>
                selectedScheme.ComplianceSchemeId == complianceScheme.Id &&
                selectedScheme.OrganisationConnection.FromOrganisationRoleId == Data.DbConstants.InterOrganisationRole.Producer &&
                selectedScheme.OrganisationConnection.FromOrganisation.ReferenceNumber == referenceNumber &&
                selectedScheme.OrganisationConnection.ToOrganisationRoleId == Data.DbConstants.InterOrganisationRole.ComplianceScheme
            )
            .Select(member => new CompanyDetailResponse
            { 
                ReferenceNumber = member.OrganisationConnection.FromOrganisation.ReferenceNumber.ToString(),
                CompaniesHouseNumber = member.OrganisationConnection.FromOrganisation.CompaniesHouseNumber != null
                ? member.OrganisationConnection.FromOrganisation.CompaniesHouseNumber.ToString()
                : null
            })
            .ToListAsync();

        return selectedSchemes is null || !selectedSchemes.Any()
          ? null
          : new CompanyDetailsResponse
          {
              Organisations = selectedSchemes
          };
    }

    public async Task<CompanyDetailsResponse> GetAllProducersCompanyDetails(IEnumerable<string> referenceNumbers)
    {
        var organisations = await _accountsDbContext.Organisations
            .AsNoTracking()
            .Where(organisation => referenceNumbers.Contains(organisation.ReferenceNumber) && !organisation.IsComplianceScheme && (organisation.OrganisationTypeId == 1 || organisation.OrganisationTypeId == 2))
            .Select(org => new CompanyDetailResponse
            {
                ReferenceNumber = org.ReferenceNumber,
                CompaniesHouseNumber = org.CompaniesHouseNumber
            })
            .ToListAsync();

        return organisations is null || !organisations.Any()
            ? null
            : new CompanyDetailsResponse
            {
                Organisations = organisations
            };
    }

    public async Task<CompanyDetailsResponse> GetAllProducersCompanyDetailsAsProducer(OrganisationReferencesRequest organisationReferences)
    {
        var hasExternalId = Guid.TryParse(organisationReferences.OrganisationExternalId, out var organisationExternalId);

        var organisations = await _accountsDbContext.Organisations
            .AsNoTracking()
            .Where(organisation => organisationReferences.ReferenceNumbers.Contains(organisation.ReferenceNumber) 
                && !organisation.IsComplianceScheme
                && (organisation.OrganisationTypeId == 1 || organisation.OrganisationTypeId == 2) 
                && (hasExternalId && organisation.ExternalId == organisationExternalId))
            .Select(org => new CompanyDetailResponse
            {
                ReferenceNumber = org.ReferenceNumber,
                CompaniesHouseNumber = org.CompaniesHouseNumber
            })
            .ToListAsync();

        return organisations is null || !organisations.Any()
            ? null
            : new CompanyDetailsResponse
            {
                Organisations = organisations
            };
    }
}