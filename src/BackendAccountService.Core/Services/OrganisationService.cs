using BackendAccountService.Core.Extensions;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Extensions;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Core.Services;

public class OrganisationService : IOrganisationService
{
    private readonly AccountsDbContext _accountsDbContext;

    public OrganisationService(AccountsDbContext accountsDbContext)
    {
        _accountsDbContext = accountsDbContext;
    }

    public Task<IReadOnlyCollection<OrganisationResponseModel>> GetOrganisationsByCompaniesHouseNumberAsync(string companiesHouseNumber)
    {
        if (string.IsNullOrWhiteSpace(companiesHouseNumber))
        {
            throw new ArgumentException($"{nameof(companiesHouseNumber)} cannot be empty string", nameof(companiesHouseNumber));
        }

        return GetOrganisationsByCompaniesHouseNumberInternalAsync(companiesHouseNumber);
    }

    private async Task<IReadOnlyCollection<OrganisationResponseModel>> GetOrganisationsByCompaniesHouseNumberInternalAsync(string companiesHouseNumber)
    {
        var organisations = await _accountsDbContext.Organisations
            .Where(organisation => organisation.CompaniesHouseNumber == companiesHouseNumber)
            .ToListAsync();

        return organisations
            .Select(OrganisationMappings.GetOrganisationModelFromOrganisation)
            .ToList();
    }

    public async Task<IQueryable<OrganisationUsersResponseModel>> GetUserListForOrganisation(Guid userId, Guid organisationId, int serviceRoleId)
    {
        var serviceRole = _accountsDbContext.ServiceRoles.Single(role => role.Id == serviceRoleId);
        var enrolments = _accountsDbContext.Enrolments
            .Include(con => con.Connection)
            .Include(p => p.Connection.Person).AsNoTracking().Where(e =>
                e.EnrolmentStatusId != Data.DbConstants.EnrolmentStatus.NotSet &&
                e.EnrolmentStatusId != Data.DbConstants.EnrolmentStatus.Rejected
                && e.Connection.Person.User.UserId != userId
                && e.Connection.Organisation.ExternalId == organisationId
                && e.ServiceRole.Service.Id == serviceRole.ServiceId);

        var result = enrolments.GroupBy(p => new
        {
            p.Connection.Person.FirstName,
            p.Connection.Person.LastName,
            p.Connection.Person.Email,
            p.Connection.PersonRoleId,
            p.Connection.Person.ExternalId,
            ConnectionExternalId = p.Connection.ExternalId
        }).Select(x => new OrganisationUsersResponseModel()
        {
            FirstName = x.Key.FirstName,
            LastName = x.Key.LastName,
            Email = x.Key.Email,
            PersonId = x.Key.ExternalId,
            PersonRoleId = x.Key.PersonRoleId,
            ConnectionId = x.Key.ConnectionExternalId,
            Enrolments = x.Select(e => new UserEnrolments()
            {
                EnrolmentStatusId = e.EnrolmentStatusId,
                ServiceRoleId = e.ServiceRoleId
            })
        });

        return result;
    }

    public async Task<Guid> GetOrganisationIdFromEnrolment(Guid enrolmentId)
    {
        var enrolment = _accountsDbContext.Enrolments
            .Include(enrolment => enrolment.Connection.Organisation)
            .SingleOrDefault(enrolment => enrolment.ExternalId == enrolmentId);

        if (enrolment != null)
        {
            return enrolment.Connection.Organisation.ExternalId;
        }

        return Guid.Empty;
    }

    public bool IsUserAssociatedWithMultipleOrganisations(Guid? userId)
    {
        return _accountsDbContext.PersonOrganisationConnections.Count(con => con.Person.User.UserId == userId) > 1;
    }

    public async Task<OrganisationDetailModel> GetOrganisationByExternalId(Guid organisationExternalId)
    {
        var organisation = await _accountsDbContext.Organisations
            .FirstOrDefaultAsync(org => org.ExternalId == organisationExternalId);
        
        return OrganisationMappings.GetOrganisationDetailModel(organisation);
    }

    public async Task<PaginatedResponse<OrganisationSearchResult>> GetOrganisationsBySearchTerm(string query, int nationId, int pageSize, int page)
    {
        IOrderedQueryable<OrganisationSearchResult> organisationsInNationQueryable = FetchProducersInNation(query, nationId);

        var organisationsNotInNationQueryable = FetchProducesNotInNation(query, nationId);

        var allorganisationsQueryable = organisationsInNationQueryable.Concat(organisationsNotInNationQueryable);

        var response = await PaginatedResponse<OrganisationSearchResult>.CreateAsync(allorganisationsQueryable, page, pageSize);
        foreach (var item in response.Items)
        {
            item.OrganisationType = DetermineOrganisationType(item.IsComplianceScheme, item.CompanyId);
        }

        return response;
    }

    private IOrderedQueryable<OrganisationSearchResult> FetchProducersInNation(string query, int nationId)
    {
        var lowerCaseQuery = query.ToLower();
        var organisationIdQuery = lowerCaseQuery.Replace(" ", String.Empty);
        var organisationsQueryable = _accountsDbContext.Organisations
            .AsNoTracking()
            .GroupJoin(
                _accountsDbContext.ComplianceSchemes,
                org => org.CompaniesHouseNumber,
                scheme => scheme.CompaniesHouseNumber,
                (org, scheme) => new { org, scheme })
            .SelectMany(
                groupJoined => groupJoined.scheme.DefaultIfEmpty(),
                (groupJoined, scheme) => new
                {
                    Organisation = groupJoined.org,
                    ComplianceSchemeIsDeleted = scheme.IsDeleted,
                    ComplianceSchemeNationId = scheme.NationId
                })
            .Where(joined => !joined.Organisation.IsDeleted)
            .Where(joined => joined.Organisation.Name.ToLower().Contains(lowerCaseQuery) || joined.Organisation.ReferenceNumber.ToLower().Contains(organisationIdQuery))
            .Where(joined =>
                joined.Organisation.NationId == nationId || (!joined.ComplianceSchemeIsDeleted && joined.ComplianceSchemeNationId == nationId))
            .Where(joined => joined.Organisation.OrganisationTypeId != Data.DbConstants.OrganisationType.Regulators)
            .GroupBy(joined => new
            {
                OrganisationId = joined.Organisation.ReferenceNumber,
                CompanyId = joined.Organisation.Id,
                CompanyHouseNumber = joined.Organisation.CompaniesHouseNumber,
                joined.Organisation.ExternalId,
                joined.Organisation.IsComplianceScheme,
                OrganisationName = joined.Organisation.Name
            })
            .Select(grouped => new OrganisationSearchResult
            {
                OrganisationId = grouped.Key.OrganisationId,
                CompanyId = grouped.Key.CompanyId,
                CompanyHouseNumber = grouped.Key.CompanyHouseNumber,
                ExternalId = grouped.Key.ExternalId,
                IsComplianceScheme = grouped.Key.IsComplianceScheme,
                OrganisationName = grouped.Key.OrganisationName
            })
            .OrderBy(org => org.OrganisationName);
        return organisationsQueryable;
    }

    private IQueryable<OrganisationSearchResult> FetchProducesNotInNation(string query, int nationId)
    {
        return from n in _accountsDbContext.Nations
               join cs in _accountsDbContext.ComplianceSchemes on n.Id equals cs.NationId
               join sc in _accountsDbContext.SelectedSchemes on cs.Id equals sc.ComplianceSchemeId
               join oc in _accountsDbContext.OrganisationsConnections on sc.OrganisationConnectionId equals oc.Id
               join o in _accountsDbContext.Organisations on oc.FromOrganisationId equals o.Id
               where n.Id == nationId
                     && o.Name.ToLower().Contains(query.ToLower())
                     && o.NationId != nationId
               group o by new
               {
                   o.ReferenceNumber,
                   o.Id,
                   o.CompaniesHouseNumber,
                   o.ExternalId,
                   o.IsComplianceScheme,
                   o.Name
               } into grouped
               orderby grouped.Key.Name
               select new OrganisationSearchResult
               {
                   OrganisationId = grouped.Key.ReferenceNumber,
                   CompanyId = grouped.Key.Id,
                   CompanyHouseNumber = grouped.Key.CompaniesHouseNumber,
                   ExternalId = grouped.Key.ExternalId,
                   IsComplianceScheme = grouped.Key.IsComplianceScheme,
                   OrganisationName = grouped.Key.Name
               };
    }

    public async Task<List<OrganisationUserOverviewResponseModel>> GetProducerUsers(Guid organisationExternalId)
    {
        return await _accountsDbContext.Enrolments
            .Include(enrolment => enrolment.Connection)
            .Include(enrolment => enrolment.Connection.Person)
            .AsNoTracking()
            .WhereEnrolmentStatusIn(Data.DbConstants.EnrolmentStatus.Enrolled,
                Data.DbConstants.EnrolmentStatus.Approved, Data.DbConstants.EnrolmentStatus.Pending)
            .WhereConnectionPersonRoleIdsIn(Data.DbConstants.PersonRole.Admin, Data.DbConstants.PersonRole.Employee)
            .Where(e => e.Connection.Organisation.ExternalId == organisationExternalId)
            .Where(e => e.ServiceRoleId != Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Id)
            .GroupBy(enrolment => new
            {
                enrolment.Connection.Person.ExternalId,
                enrolment.Connection.Person.FirstName,
                enrolment.Connection.Person.LastName,
                enrolment.Connection.Person.Email
            })
            .Select(groupBy => new OrganisationUserOverviewResponseModel
            {
                PersonExternalId = groupBy.Key.ExternalId,
                FirstName = groupBy.Key.FirstName,
                LastName = groupBy.Key.LastName,
                Email = groupBy.Key.Email
            })
            .OrderBy(user => user.FirstName)
            .ToListAsync();

    }

    public async Task<ApprovedPersonOrganisationModel> GetOrganisationNameByInviteTokenAsync(string token)
    {
        var user = await _accountsDbContext.Users.Where(p => p.InviteToken == token).FirstOrDefaultAsync();
        var person = await _accountsDbContext.Persons.Where(p => p.Email == user.Email).FirstOrDefaultAsync();
        var personOrganisationConnections = await _accountsDbContext.PersonOrganisationConnections
            .Where(p => p.PersonId == person.Id)
            .FirstOrDefaultAsync();
        var organisation = await _accountsDbContext.Organisations
            .Where(p => p.Id == personOrganisationConnections.OrganisationId)
            .FirstOrDefaultAsync();
        
        //create new model here to store company house number check and service role id
        var organisationAddress = new ApprovedPersonOrganisationModel
        {
            SubBuildingName = organisation.SubBuildingName,
            BuildingName = organisation.BuildingName,
            BuildingNumber = organisation.BuildingNumber,
            Country = organisation.Country,
            County = organisation.County,
            DependentLocality = organisation.DependentLocality,
            Locality = organisation.Locality,
            Postcode = organisation.Postcode,
            Street = organisation.Street,
            Town = organisation.Town,
            OrganisationName = organisation.Name,
            ApprovedUserEmail = user.Email
        };

        //return new model
        return organisationAddress;

    }
    private string DetermineOrganisationType(bool isComplianceScheme,int companyId)
    {
        if (isComplianceScheme)
        {
            return OrganisationSchemeType.ComplianceScheme.ToString();
        }
        var checkMatchInOrgConn = _accountsDbContext.OrganisationsConnections
            .FirstOrDefault(
                x => !x.IsDeleted &&
                     x.FromOrganisationId == companyId 
                     || x.ToOrganisationId == companyId);
            
        return checkMatchInOrgConn != null 
            ? OrganisationSchemeType.InDirectProducer.ToString()
            : OrganisationSchemeType.DirectProducer.ToString();
    }
}
