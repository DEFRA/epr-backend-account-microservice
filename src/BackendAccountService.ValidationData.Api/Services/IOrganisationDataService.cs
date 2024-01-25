using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackendAccountService.ValidationData.Api.Models;

namespace BackendAccountService.ValidationData.Api.Services;

public interface IOrganisationDataService
{
    Task<OrganisationResponse> GetOrganisationByExternalId(Guid organisationExternalId);

    Task<OrganisationMembersResponse> GetOrganisationMembersByComplianceSchemeId(Guid organisationExternalId,
        Guid? complianceSchemeId);

    Task<OrganisationsResponse> GetExistingOrganisationsByReferenceNumber(IEnumerable<string> referenceNumbers);
}