using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendAccountService.ValidationData.Api.Models;

namespace BackendAccountService.ValidationData.Api.Services;

public interface ICompanyDetailsDataService
{
    Task<CompanyDetailsResponse> GetCompanyDetailsByOrganisationReferenceNumber(string referenceNumber);

    Task<CompanyDetailsResponse> GetCompanyDetailsByOrganisationExternalId(Guid organisationExternalId);

    Task<CompanyDetailsResponse> GetCompanyDetailsByOrganisationReferenceNumberAndComplianceSchemeId(string referenceNumber, Guid? complianceSchemeId);

    Task<CompanyDetailsResponse> GetAllProducersCompanyDetails(IEnumerable<string> referenceNumbers);
}