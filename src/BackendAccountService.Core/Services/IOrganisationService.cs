using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Responses;

namespace BackendAccountService.Core.Services
{
    public interface IOrganisationService
    {
        Task<IReadOnlyCollection<OrganisationResponseModel>> GetOrganisationsByCompaniesHouseNumberAsync(string companiesHouseNumber);
        
        Task<IQueryable<OrganisationUsersResponseModel>> GetUserListForOrganisation(Guid userId, Guid organisationId,  int serviceRoleId);

        Task<Guid> GetOrganisationIdFromEnrolment(Guid enrolmentId);
        
        bool IsUserAssociatedWithMultipleOrganisations(Guid? userId);
        
        Task<OrganisationDetailModel> GetOrganisationByExternalId(Guid organisationExternalId);

        Task<PaginatedResponse<OrganisationSearchResult>> GetOrganisationsBySearchTerm(string query, int nationId, int pageSize, int page);

        Task<List<OrganisationUserOverviewResponseModel>> GetProducerUsers(Guid organisationExternalId);
    }
}