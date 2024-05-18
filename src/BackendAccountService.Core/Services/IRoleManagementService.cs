using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;

namespace BackendAccountService.Core.Services;

public interface IRoleManagementService
{
    Task<ConnectionWithEnrolmentsResponse?> GetConnectionWithEnrolmentsFromOrganisationForServiceAsync(Guid connectionId, Guid organisationId, string serviceKey);
    Task<ConnectionWithPersonResponse?> GetConnectionWithPersonForServiceAsync(Guid connectionId, Guid organisationId, string serviceKey);
    Task<UpdatePersonRoleResponse> UpdatePersonRoleAsync(Guid connectionId, Guid userId, Guid organisationId, string serviceKey, PersonRole personRole);
    Task<(bool Succeeded, string ErrorMessage)> NominateToDelegatedPerson(Guid connectionId, Guid userId, Guid organisationId, string serviceKey, DelegatedPersonNominationRequest nominationRequest);
    Task<(bool Succeeded, string ErrorMessage)> AcceptNominationToDelegatedPerson(Guid enrolmentId, Guid userId, Guid organisationId, string serviceKey, AcceptNominationRequest acceptNominationRequest);
    Task<(bool Succeeded, string ErrorMessage)> AcceptNominationForApprovedPerson(Guid enrolmentId, Guid userId, Guid organisationId, string serviceKey, AcceptNominationForApprovedPersonRequest acceptNominationRequest);
    Task<DelegatedPersonNominatorResponse> GetDelegatedPersonNominator(Guid enrolmentId, Guid userId, Guid organisationId, string serviceKey);
}