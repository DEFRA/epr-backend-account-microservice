using System;
using System.Threading.Tasks;
using BackendAccountService.ValidationData.Api.Models;

namespace BackendAccountService.ValidationData.Api.Services;

public interface IOrganisationDataService
{
    Task<OrganisationResponse> GetOrganisationByExternalId(Guid organisationExternalId);
}