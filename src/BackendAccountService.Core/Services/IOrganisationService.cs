using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Data.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BackendAccountService.Core.Services
{
    public interface IOrganisationService
    {
        Task<IReadOnlyCollection<OrganisationResponseModel>> GetOrganisationsByCompaniesHouseNumberAsync(string companiesHouseNumber);

        Task<IQueryable<OrganisationUsersResponseModel>> GetUserListForOrganisation(Guid userId, Guid organisationId, int serviceRoleId);

		Task<IQueryable<TeamMembersResponseModel>> GetTeamMemberListForOrganisation(Guid userId, Guid organisationId, int serviceRoleId);

		Task<Guid> GetOrganisationIdFromEnrolment(Guid enrolmentId);

        bool IsUserAssociatedWithMultipleOrganisations(Guid? userId);

        Task<OrganisationDetailModel> GetOrganisationByExternalId(Guid organisationExternalId);

        Task<PagedOrganisationRelationshipsResponse> GetPagedOrganisationRelationships(int page, int showPerPage, string search = null);

        Task<List<RelationshipResponseModel>> GetUnpagedOrganisationRelationships();

        Task<OrganisationRelationshipResponseModel> GetOrganisationRelationshipsByOrganisationId(Guid organisationExternalId);

        Task<OrganisationResponseModel> GetOrganisationResponseByExternalId(Guid organisationExternalId);

        Task<OrganisationResponseModel> GetOrganisationResponseByReferenceNumber(string organisationRefNumber);

        Task<PaginatedResponse<OrganisationSearchResult>> GetOrganisationsBySearchTerm(string query, int nationId, int pageSize, int page);

        Task<List<OrganisationUserOverviewResponseModel>> GetProducerUsers(Guid organisationExternalId);

        Task<ApprovedPersonOrganisationModel> GetOrganisationNameByInviteTokenAsync(string token);

        Task<Organisation> AddOrganisationAndOrganisationRelationshipsAsync(OrganisationModel organisationModel,
            OrganisationRelationshipModel organisationRelationshipModel, Guid userExternalId);

        Task<OrganisationRelationship> AddOrganisationRelationshipsAsync(OrganisationRelationshipModel organisationRelationshipModel,
            Guid parentExternalId, Guid userExternalId);

        Task<Result> TerminateOrganisationRelationshipsAsync(TerminateSubsidiaryModel requestModel);

        Task<bool> IsOrganisationInRelationship(int parentOrganisationId, int childOrganisationId);

        Task<List<ExportOrganisationSubsidiariesResponseModel>> ExportOrganisationSubsidiaries(Guid organisationExternalId);

        Task<Result> UpdateOrganisationDetails(Guid userId, Guid organisationId, OrganisationUpdateModel organisation);

        Task<List<UpdatedProducersResponseModel>> GetUpdatedProducers(UpdatedProducersRequest request);

        Task<bool> IsOrganisationValidAsync(Guid organisationExternalId);

        Task<List<PersonEmailResponseModel>> GetPersonEmails(Guid organisationId, string entityTypeCode);

        Task<bool> IsCSOrganisationValidAsync(Guid organisationExternalId);

        Task<IReadOnlyCollection<OrganisationResponseModel>> GetOrganisationsByCompaniesHouseNameAsync(string companiesHouseName);

        Task<IReadOnlyCollection<OrganisationResponseModel>> GetOrganisationByReferenceNumber(string referenceNumber);

        Task<OrganisationResponseModel> GetByCompaniesHouseNumberAsync(string companiesHouseNumber);

        Task<OrganisationRelationship> UpdateOrganisationRelationshipsAsync(OrganisationRelationshipModel organisationRelationshipModel,
            Guid parentExternalId, Guid userExternalId);
    }
}