using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;

namespace BackendAccountService.Core.Services;

public interface IRegulatorService
{
    Task<PaginatedResponse<OrganisationEnrolments>> GetPendingApplicationsAsync(int nationId, int currentPage, int pageSize, string? organisationName, string applicationType);

    Task<(bool Succeeded, string ErrorMessage)> UpdateEnrolmentStatusForUserAsync(Guid userId, Guid organisationId, Guid enrolmentId, string enrolmentStatus, string regulatorComment);

    int GetRegulatorNationId(Guid userId);

    Task<List<int>> GetOrganisationNationIds(Guid organisationId);

    Task<ApplicationEnrolmentDetails> GetOrganisationEnrolmentDetails(Guid organisationId);

    bool DoesRegulatorNationMatchOrganisationNation(Guid userId, Guid organisationId);
    
    bool IsRegulator(Guid userId);
    
    Task<(bool Succeeded, string ErrorMessage)> TransferOrganisationNation(OrganisationTransferNationRequest request);

    Task<IQueryable<OrganisationUsersResponseModel>> GetUserListForRegulator(Guid organisationId, bool getApprovedUsersOnly = false);
    
    Task<(bool Succeeded, string ErrorMessage)> RemoveApprovedPerson(Guid userId ,Guid personExternalId, Guid organisationId);
    
    Task<CompanySearchDetailsModel> GetCompanyDetailsById(Guid organisationId);
}