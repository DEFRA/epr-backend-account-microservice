using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;

namespace BackendAccountService.Core.Services;

public interface IRegulatorService
{
    Task<PaginatedResponse<OrganisationEnrolments>> GetPendingApplicationsAsync(int nationId, int currentPage, int pageSize, string? organisationName, string applicationType);

    Task<(bool Succeeded, string ErrorMessage)> UpdateEnrolmentStatusForUserAsync(Guid userId, Guid organisationId, Guid enrolmentId, string enrolmentStatus, string regulatorComment);

    int GetRegulatorNationId(Guid userId);

    Task<List<OrganisationNationResponseModel>> GetOrganisationNationsAsync(Guid organisationId);

    Task<List<OrganisationNationResponseModel>> GetCSOrganisationNationsAsync(Guid complianceSchemeId);

    Task<ApplicationEnrolmentDetails> GetOrganisationEnrolmentDetails(Guid organisationId);

    bool DoesRegulatorNationMatchOrganisationNation(Guid userId, Guid organisationId);
    
    bool IsRegulator(Guid userId);
    
    Task<(bool Succeeded, string ErrorMessage)> TransferOrganisationNation(OrganisationTransferNationRequest request);

    Task<IQueryable<OrganisationUsersResponseModel>> GetUserListForRegulator(Guid organisationId, bool getApprovedUsersOnly = false);
    
    Task<List<AssociatedPersonResponseModel>> RemoveApprovedPerson(ApprovedUserRequest request);
    
    Task<AddRemoveApprovedPersonResponseModel> AddRemoveApprovedPerson(AddRemoveApprovedUserRequest request);
    
    Task<CompanySearchDetailsModel> GetCompanyDetailsById(Guid organisationId);

    Task<Result<RegulatorUserDetailsUpdateResponse>> AcceptOrRejectUserDetailsChangeRequestAsync(ManageUserDetailsChangeModel request);

    Task<PaginatedResponse<OrganisationUserDetailChangeRequest>> GetPendingUserDetailChangeRequestsAsync(int nationId, int currentPage, int pageSize, string? organisationName, string applicationType);

    Task<Result<ChangeHistoryModel>> GetUserDetailChangeRequestAsync(Guid externalId);

    Task<Result<RegulatorUserDetailsUpdateByServiceResponse>> AcceptOrRejectUserDetailsChangeRequestByServiceAsync(ManageUserDetailsChangeRequestByService request);

    Task<Result<RegulatorOrganisationUpdateResponse>> UpdateNonCompaniesHouseCompanyByServiceAsync(ManageNonCompaniesHouseCompanyByService request);
    
}