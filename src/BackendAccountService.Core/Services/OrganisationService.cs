using System.Collections.Generic;
using System.Linq;
using System.Net;
using BackendAccountService.Core.Extensions;
using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Core.Models.Result;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Extensions;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using InterOrganisationRole = BackendAccountService.Data.DbConstants.InterOrganisationRole;

namespace BackendAccountService.Core.Services;

public class OrganisationService : ServiceBase, IOrganisationService
{
    private readonly AccountsDbContext _accountsDbContext;

    public OrganisationService(
        AccountsDbContext accountsDbContext) : base(accountsDbContext)
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

    public async Task<IReadOnlyCollection<OrganisationResponseModel>> GetOrganisationsByCompaniesHouseNameAsync(string companiesHouseName)
    {
        if (string.IsNullOrWhiteSpace(companiesHouseName))
        {
            throw new ArgumentException($"{nameof(companiesHouseName)} cannot be empty string", nameof(companiesHouseName));
        }

        var organisations = await _accountsDbContext.Organisations
          .Where(organisation => organisation.Name == companiesHouseName)
          .ToListAsync();

        return organisations
            .Select(OrganisationMappings.GetOrganisationModelFromOrganisation)
            .ToList();
    }

    public async Task<OrganisationResponseModel> GetByCompaniesHouseNumberAsync(string companiesHouseNumber)
    {
        if (string.IsNullOrWhiteSpace(companiesHouseNumber))
        {
            throw new ArgumentException($"{nameof(companiesHouseNumber)} cannot be empty string", nameof(companiesHouseNumber));
        }

        var organisation = await _accountsDbContext.Organisations
            .FirstOrDefaultAsync(org => org.CompaniesHouseNumber == companiesHouseNumber);

        var organisations = await _accountsDbContext.Organisations
            .Where(organisation => organisation.CompaniesHouseNumber == companiesHouseNumber)
            .Include(dm => dm.OrganisationRelationships)
            .ToListAsync();

        Organisation? parentOrganisation = null;

        if (organisations.Count > 0)
        {
            var activeRelationships =
                await _accountsDbContext.OrganisationRelationships.Where(org => org.SecondOrganisationId == organisations[0].Id && org.RelationToDate == null).ToListAsync();

            if (activeRelationships.Count > 0)
            {
                organisations[0].OrganisationRelationships = [.. activeRelationships];
            }

            var relationships = organisations[0].OrganisationRelationships;
            if (relationships is not null)
            {
                parentOrganisation = await _accountsDbContext.Organisations
               .FirstOrDefaultAsync(org => org.Id == relationships.FirstOrDefault().FirstOrganisationId);
            }
        }
        return OrganisationMappings.GetOrganisationModelFromOrganisation(organisation, parentOrganisation);
    }

    public async Task<IReadOnlyCollection<OrganisationResponseModel>> GetOrganisationByReferenceNumber(string referenceNumber)
    {
        var organisations = await _accountsDbContext.Organisations
            .Where(org => org.ReferenceNumber == referenceNumber).ToListAsync();

        return organisations
          .Select(OrganisationMappings.GetOrganisationModelFromOrganisation)
          .ToList();
    }

    private async Task<IReadOnlyCollection<OrganisationResponseModel>> GetOrganisationsByCompaniesHouseNumberInternalAsync(string companiesHouseNumber)
    {
        var organisations = await _accountsDbContext.Organisations
            .Where(organisation => organisation.CompaniesHouseNumber == companiesHouseNumber)
            .Include(dm => dm.OrganisationRelationships)
            .ToListAsync();

        if (organisations.Count > 0)
        {
            var activeRelationships = await _accountsDbContext.OrganisationRelationships.Where(org => org.SecondOrganisationId == organisations[0].Id && org.RelationToDate == null).ToListAsync();

            if (activeRelationships.Count > 0)
            {
                organisations[0].OrganisationRelationships = [.. activeRelationships];
            }
        }
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
                ServiceRoleId = e.ServiceRoleId,
                ServiceRoleKey = e.ServiceRole.Key
            })
        });

        return result;
    }

    public async Task<IQueryable<TeamMembersResponseModel>> GetTeamMemberListForOrganisation(Guid userId, Guid organisationId, int serviceRoleId)
    {
        var serviceId = await _accountsDbContext.ServiceRoles
                            .AsNoTracking()
                            .Where(role => role.Id == serviceRoleId)
                            .Select(role => role.Service.Id)
                            .SingleAsync();

        var enrolments = await _accountsDbContext.Enrolments
                            .Include(e => e.Connection)
                                .ThenInclude(c => c.Person)
                                    .ThenInclude(p => p.User)
                            .Include(e => e.ServiceRole)
                                .ThenInclude(sr => sr.Service)
                            .Include(e => e.EnrolmentStatus)
                            .AsNoTracking()
                            .Where(e =>
                                e.EnrolmentStatusId != Data.DbConstants.EnrolmentStatus.NotSet &&
                                e.EnrolmentStatusId != Data.DbConstants.EnrolmentStatus.Rejected &&
                                e.Connection.Person.User.UserId != userId &&
                                e.Connection.Organisation.ExternalId == organisationId &&
                                e.ServiceRole.Service.Id == serviceId && !e.IsDeleted)
                            .ToListAsync();

        if (enrolments.Count == 0)
        {
            return Enumerable.Empty<TeamMembersResponseModel>().AsQueryable();
        }

        var organisationDbId = enrolments[0].Connection.OrganisationId;
        var invites = await _accountsDbContext.PersonOrganisationConnectionInvites
                        .Include(i => i.User)
                            .ThenInclude(u => u.Person)
                        .Include(i => i.Person)
                        .AsNoTracking()
                        .Where(i => !i.IsDeleted && !i.IsUsed &&
                            i.OrganisationId == organisationDbId)
                        .ToListAsync();

        var result = enrolments
            .GroupBy(e => new
            {
                e.Connection.Person.FirstName,
                e.Connection.Person.LastName,
                e.Connection.Person.Email,
                e.Connection.Person.ExternalId,
                ConnectionExternalId = e.Connection.ExternalId
            })
            .Select(group => new TeamMembersResponseModel
            {
                FirstName = group.Key.FirstName,
                LastName = group.Key.LastName,
                Email = group.Key.Email,
                PersonId = group.Key.ExternalId,
                ConnectionId = group.Key.ConnectionExternalId,
                Enrolments = group
                    .GroupBy(e => new
                    {
                        e.Id,
                        e.ServiceRoleId,
                        e.EnrolmentStatusId,
                        e.EnrolmentStatus.Name,
                        e.ServiceRole.Key
                    })
                    .Select(g =>
                    {
                        var personId = g.First().Connection.Person.Id;
                        var invite = invites.Find(i => i.Person.Id == personId);
                        return new TeamMemberEnrolment
                        {
                            EnrolmentId = g.Key.Id,
                            EnrolmentStatusId = g.Key.EnrolmentStatusId,
                            EnrolmentStatusName = g.Key.Name,
                            ServiceRoleId = g.Key.ServiceRoleId,
                            ServiceRoleKey = g.Key.Key,
                            AddedBy = invite != null
                                ? $"{invite.User.Person.FirstName} {invite.User.Person.LastName}"
                                : ""
                        };
                    })
                    .ToList()
            })
            .AsQueryable();

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

    public async Task<OrganisationResponseModel> GetOrganisationResponseByExternalId(Guid organisationExternalId)
    {
        var organisation = await _accountsDbContext.Organisations
            .FirstOrDefaultAsync(org => org.ExternalId == organisationExternalId);

        return OrganisationMappings.GetOrganisationResponseModel(organisation);
    }

    public async Task<PagedOrganisationRelationshipsResponse> GetPagedOrganisationRelationships(int page, int showPerPage, string search = null)
    {
        var relationshipsQuery = GetOrganisationRelationshipsQuery(search);

        var searchTerms = await (from o in _accountsDbContext.Organisations
                                 join rel in _accountsDbContext.OrganisationRelationships on o.Id equals rel.FirstOrganisationId
                                 join sub in _accountsDbContext.Organisations on rel.SecondOrganisationId equals sub.Id
                                 join subOrg in _accountsDbContext.SubsidiaryOrganisations on rel.SecondOrganisationId equals subOrg.OrganisationId into subOrgGroup
                                 from subOrg in subOrgGroup.DefaultIfEmpty()
                                 orderby sub.Name
                                 select new RelationshipResponseModel()
                                 {
                                     OrganisationName = sub.Name,
                                     OrganisationNumber = sub.ReferenceNumber,
                                     CompaniesHouseNumber = sub.CompaniesHouseNumber
                                 }).Distinct()
                                 .ToListAsync();

        return new PagedOrganisationRelationshipsResponse
        {
            Items = await relationshipsQuery.Skip((page - 1) * showPerPage).Take(showPerPage).ToListAsync(),
            CurrentPage = page,
            TotalItems = await relationshipsQuery.CountAsync(),
            PageSize = showPerPage,
            SearchTerms = searchTerms.SelectMany(x => new[] { x.OrganisationNumber, x.OrganisationName, x.CompaniesHouseNumber })
                .Where(x => !string.IsNullOrEmpty(x))
                .OrderBy(x => x)
                .Distinct()
                .ToList()
        };
    }

    public async Task<List<RelationshipResponseModel>> GetUnpagedOrganisationRelationships()
    {
        var query = GetOrganisationRelationshipsQuery();
        return await query.ToListAsync();
    }

    public async Task<OrganisationRelationshipResponseModel> GetOrganisationRelationshipsByOrganisationId(Guid organisationExternalId)
    {
        var relationships = GetRelationships(organisationExternalId).OrderBy(r => r.Id);

        if (!await relationships.AnyAsync())
        {
            return null;
        }

        return CreateOrganisationRelationshipsDetailModel(await relationships.ToListAsync());
    }

    public async Task<PaginatedResponse<OrganisationSearchResult>> GetOrganisationsBySearchTerm(string query, int nationId, int pageSize, int page)
    {
        IOrderedQueryable<OrganisationSearchResult> organisationsInNationQueryable = FetchProducersInNation(query, nationId);

        var organisationsNotInNationQueryable = FetchProducesNotInNation(query, nationId);

        var allorganisationsQueryable = organisationsInNationQueryable.Concat(organisationsNotInNationQueryable);

        var response = await PaginatedResponse<OrganisationSearchResult>.CreateAsync(allorganisationsQueryable, page, pageSize);
        foreach (var item in response.Items)
        {
            var orgType = DetermineOrganisationType(item.IsComplianceScheme, item.CompanyId);
            item.OrganisationType = orgType.OrganisationType;
            item.IsComplianceSchemeMemberSubsidiary = orgType.IsComplianceSchemeMemberSubsidiary;
        }

        return response;
    }

    public async Task<Organisation> AddOrganisationAndOrganisationRelationshipsAsync(OrganisationModel organisationModel,
    OrganisationRelationshipModel organisationRelationshipModel, Guid userExternalId)
    {
        await using var transaction = await _accountsDbContext.Database.BeginTransactionAsync();

        if (organisationModel.Franchisee_Licensee_Tenant == "Y")
        {
            var org = await ProcessFranchisee(organisationModel, organisationRelationshipModel, userExternalId);
            await transaction.CommitAsync();
            return org;
        }

        var organisationToAdd = OrganisationMappings.GetOrganisationFromOrganisationModel(organisationModel);
        var existingSubsidiary = await GetOrganisationsByCompaniesHouseNumberAsync(organisationModel.CompaniesHouseNumber);
        if (existingSubsidiary.Count == 0)
        {
            _accountsDbContext.Organisations.Add(organisationToAdd);
            await _accountsDbContext.SaveChangesAsync(userExternalId, organisationToAdd.ExternalId);
            organisationRelationshipModel.SecondOrganisationId = organisationToAdd.Id;
        }
        else
        {
            organisationRelationshipModel.SecondOrganisationId = existingSubsidiary.First().Id;
            organisationToAdd.ReferenceNumber = existingSubsidiary.First().ReferenceNumber;
            organisationToAdd.ExternalId = existingSubsidiary.First().ExternalId.Value;
            organisationToAdd.Name = existingSubsidiary.First().Name;
        }

        var relationshipAdded = await AddOrganisationRelationshipsAsync(organisationRelationshipModel, organisationToAdd.ExternalId, userExternalId);

        if (!string.IsNullOrWhiteSpace(organisationModel.SubsidiaryOrganisationId))
        {
            var subsidiaryOrganisation = OrganisationMappings.GetSubsidiaryOrganisationFromOrganisationModel(organisationModel.SubsidiaryOrganisationId, organisationToAdd);

            var subsidiaryOrganisations = await _accountsDbContext.SubsidiaryOrganisations.Where(org => org.SubsidiaryId == organisationModel.SubsidiaryOrganisationId && org.OrganisationId == organisationToAdd.Id).ToListAsync();
            if (subsidiaryOrganisations.Count == 0)
            {
                _accountsDbContext.SubsidiaryOrganisations.Add(subsidiaryOrganisation);
                await _accountsDbContext.SaveChangesAsync(userExternalId, organisationToAdd.ExternalId);
            }
        }

        if (relationshipAdded != null)
        {
            organisationToAdd.OrganisationRelationships.Add(relationshipAdded);
        }

        await transaction.CommitAsync();

        return organisationToAdd;
    }

    public async Task<OrganisationRelationship> AddOrganisationRelationshipsAsync(OrganisationRelationshipModel organisationRelationshipModel,
        Guid parentExternalId, Guid userExternalId)
    {
        var isNestedTransaction = _accountsDbContext.Database.CurrentTransaction is not null;
        var transaction = _accountsDbContext.Database.CurrentTransaction ?? await _accountsDbContext.Database.BeginTransactionAsync();

        try
        {
            var existingRelationshipDuplicates = await _accountsDbContext.OrganisationRelationships.FirstOrDefaultAsync(r => r.FirstOrganisationId == organisationRelationshipModel.FirstOrganisationId
            && r.SecondOrganisationId == organisationRelationshipModel.SecondOrganisationId
            && r.RelationToDate == null);

            if (existingRelationshipDuplicates != null)
            {
                return null;
            }

            var organisationRelationship = OrganisationMappings.GetOrganisationRelationshipFromOrganisationRelationshipModel(organisationRelationshipModel);
            if (organisationRelationship.FirstOrganisationId == organisationRelationship.SecondOrganisationId)
            {
                return null;
            }

            var existingRelationship = await _accountsDbContext.OrganisationRelationships
              .FirstOrDefaultAsync(relationship =>
                  relationship.FirstOrganisationId != organisationRelationshipModel.FirstOrganisationId
                  && relationship.SecondOrganisationId == organisationRelationshipModel.SecondOrganisationId
                  && relationship.RelationToDate == null);

            if (existingRelationship != null)
            {
                var terminateModel = new TerminateSubsidiaryModel
                {
                    ParentExternalId = parentExternalId,
                    ParentOrganisationId = existingRelationship.FirstOrganisationId,
                    ChildOrganisationId = existingRelationship.SecondOrganisationId,
                    UserId = organisationRelationshipModel.LastUpdatedById,
                    UserExternalId = userExternalId
                };

                await TerminateOrganisationRelationshipsAsync(terminateModel);
            }

            _accountsDbContext.OrganisationRelationships.Add(organisationRelationship);

            await _accountsDbContext.SaveChangesAsync(userExternalId, parentExternalId);

            return organisationRelationship;
        }
        finally
        {
            if (!isNestedTransaction)
            {
                await transaction.CommitAsync();
            }
        }
    }

    public async Task<OrganisationRelationship> UpdateOrganisationRelationshipsAsync(OrganisationRelationshipModel organisationRelationshipModel,
        Guid parentExternalId, Guid userExternalId)
    {
        var isNestedTransaction = _accountsDbContext.Database.CurrentTransaction is not null;
        var transaction = _accountsDbContext.Database.CurrentTransaction ?? await _accountsDbContext.Database.BeginTransactionAsync();

        try
        {
            var existingRelationshipCheck = await _accountsDbContext
                .OrganisationRelationships.FirstOrDefaultAsync(r => r.FirstOrganisationId == organisationRelationshipModel.FirstOrganisationId &&
                        r.SecondOrganisationId == organisationRelationshipModel.SecondOrganisationId &&
                        r.RelationToDate == null);

            if (existingRelationshipCheck == null)
            {
                await transaction.CommitAsync();
                return null;
            }

            _accountsDbContext.OrganisationRelationships.Update(existingRelationshipCheck);

            await _accountsDbContext.SaveChangesAsync(userExternalId, parentExternalId);

            return existingRelationshipCheck;
        }
        finally
        {
            if (!isNestedTransaction)
            {
                await transaction.CommitAsync();
            }
        }
    }

    public async Task<Result> TerminateOrganisationRelationshipsAsync(TerminateSubsidiaryModel requestModel)
    {

        var isNestedTransaction = _accountsDbContext.Database.CurrentTransaction is not null;
        var transaction = _accountsDbContext.Database.CurrentTransaction ?? await _accountsDbContext.Database.BeginTransactionAsync();

        try
        {
            if (requestModel.ParentOrganisationId == requestModel.ChildOrganisationId)
            {
                return null;
            }
            var organisationRelationship = await _accountsDbContext.OrganisationRelationships
                .FirstOrDefaultAsync(relationship => relationship.FirstOrganisationId == requestModel.ParentOrganisationId
                    && relationship.SecondOrganisationId == requestModel.ChildOrganisationId
                    && relationship.RelationToDate == null);

            if (organisationRelationship is null)
            {
                var message = $"Relationship with Parent {requestModel.ParentOrganisationId} with Child {requestModel.ChildOrganisationId} is not found";
                return Result.FailedResult(message, HttpStatusCode.NotFound);
            }

            organisationRelationship.LastUpdatedById = requestModel.UserId;
            organisationRelationship.RelationToDate = DateTime.UtcNow;
            organisationRelationship.LastUpdatedByOrganisationId = requestModel.ParentOrganisationId;
            _accountsDbContext.OrganisationRelationships.Update(organisationRelationship);
            await _accountsDbContext.SaveChangesAsync(requestModel.UserExternalId, requestModel.ParentExternalId);
            return Result.SuccessResult();
        }
        finally
        {
            if (!isNestedTransaction)
            {
                await transaction.CommitAsync();
            }
        }
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

    public async Task<List<ExportOrganisationSubsidiariesResponseModel>> ExportOrganisationSubsidiaries(Guid organisationExternalId)
    {
        var organisationSubsidiaries = GetOrganisationSubsidiaries(organisationExternalId).OrderBy(r => r.OrganisationId);

        if (!await organisationSubsidiaries.AnyAsync())
        {
            return null;
        }

        return await organisationSubsidiaries.ToListAsync();
    }

    public async Task<bool> IsOrganisationInRelationship(int parentOrganisationId, int childOrganisationId)
    {
        return _accountsDbContext.OrganisationRelationships.Any(o => o.FirstOrganisationId == parentOrganisationId && o.SecondOrganisationId == childOrganisationId && o.RelationToDate == null);
    }

    public async Task<OrganisationResponseModel> GetOrganisationResponseByReferenceNumber(string organisationRefNumber)
    {
        var organisation = await _accountsDbContext.Organisations
            .FirstOrDefaultAsync(org => org.ReferenceNumber == organisationRefNumber);

        return OrganisationMappings.GetOrganisationResponseModel(organisation);
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

    public async Task<Result> UpdateOrganisationDetails(Guid userId, Guid organisationId, OrganisationUpdateModel organisation)
    {
        var organisationEntity = await _accountsDbContext
            .Organisations
            .Where(o => o.ExternalId == organisationId)
        .SingleOrDefaultAsync();

        if (organisationEntity == null)
        {
            var message = $"Organisation {organisationId} not found";
            return Result.FailedResult(message, HttpStatusCode.BadRequest);
        }

        if (organisationEntity.OrganisationTypeId == Data.DbConstants.OrganisationType.NonCompaniesHouseCompany &&
            (string.IsNullOrWhiteSpace(organisation.Name)
            || string.IsNullOrWhiteSpace(organisation.Town)
            || string.IsNullOrWhiteSpace(organisation.Postcode)
            || string.IsNullOrWhiteSpace(organisation.BuildingNumber)
            || string.IsNullOrWhiteSpace(organisation.Street)
            || organisation.NationId == 0)
            )
        {
            var message = $"request to update non companies house company with details " +
                $"OrganisationName : '{organisation.Name}' " +
                $"BuildingNumber : '{organisation.BuildingNumber}' " +
                $"Street :'{organisation.Street}' " +
                $"Town :'{organisation.Town}'" +
                $" Postcode :'{organisation.Postcode}' " +
                $" NationId :'{organisation.NationId}' " +
                $"is not valid.";
            return Result.FailedResult(message, HttpStatusCode.BadRequest);
        }

        organisationEntity.Name = organisation.Name;
        organisationEntity.SubBuildingName = organisation.SubBuildingName;
        organisationEntity.BuildingName = organisation.BuildingName;
        organisationEntity.BuildingNumber = organisation.BuildingNumber;
        organisationEntity.Street = organisation.Street;
        organisationEntity.Postcode = organisation.Postcode;
        organisationEntity.Locality = organisation.Locality;
        organisationEntity.DependentLocality = organisation.DependentLocality;
        organisationEntity.Town = organisation.Town;
        organisationEntity.County = organisation.County;
        organisationEntity.Country = organisation.Country;
        organisationEntity.NationId = organisation.NationId;

        await _accountsDbContext.SaveChangesAsync(userId, organisationId);

        return Result.SuccessResult();
    }

    public async Task<List<UpdatedProducersResponseModel>> GetUpdatedProducers(UpdatedProducersRequest request)
    {
        var organisations = _accountsDbContext.Organisations
            .Where(x => x.LastUpdatedOn >= request.From && x.LastUpdatedOn <= request.To)
            .ToList();

        return OrganisationMappings.GetUpdatedProducers(organisations);
    }

    public async Task<bool> IsOrganisationValidAsync(Guid organisationExternalId)
    {
        return await _accountsDbContext.Organisations
            .AnyAsync(x => x.ExternalId == organisationExternalId);
    }

    public async Task<bool> IsCSOrganisationValidAsync(Guid organisationExternalId)
    {
        return await _accountsDbContext.ComplianceSchemes
            .AnyAsync(x => x.ExternalId == organisationExternalId);
    }

    private IQueryable<RelationshipResponseModel> GetOrganisationRelationshipsQuery(string search = null)
    {
        if (search != null)
        {
            search = search.ToLower();
        }

        return from o in _accountsDbContext.Organisations
               join rel in _accountsDbContext.OrganisationRelationships on o.Id equals rel.FirstOrganisationId
               join sub in _accountsDbContext.Organisations on rel.SecondOrganisationId equals sub.Id
               join ort in _accountsDbContext.OrganisationRelationshipTypes on rel.OrganisationRelationshipTypeId equals ort.Id
               join subOrg in _accountsDbContext.SubsidiaryOrganisations on rel.SecondOrganisationId equals subOrg.OrganisationId into subOrgGroup
               from subOrg in subOrgGroup.DefaultIfEmpty()
               where rel.RelationToDate == null &&
               (
                   search == null ||
                   EF.Functions.Like(sub.Name.ToLower(), $"%{search}%") ||
                   EF.Functions.Like(sub.ReferenceNumber, $"%{search}%") ||
                   EF.Functions.Like(sub.CompaniesHouseNumber.ToLower(), $"%{search}%")
               )
               orderby sub.Name
               select new RelationshipResponseModel()
               {
                   ParentOrganisationExternalId = o.ExternalId,
                   OrganisationName = sub.Name,
                   OrganisationNumber = sub.ReferenceNumber,
                   RelationshipType = ort.Name,
                   CompaniesHouseNumber = sub.CompaniesHouseNumber,
                   JoinerDate = rel.JoinerDate
               };
    }

    private IQueryable<ExportOrganisationSubsidiariesResponseModel> GetOrganisationSubsidiaries(Guid organisationExternalId)
    {
        var parentChildDetails =
            from o in _accountsDbContext.Organisations
            join rel in _accountsDbContext.OrganisationRelationships on o.Id equals rel.FirstOrganisationId
            join sub in _accountsDbContext.Organisations on rel.SecondOrganisationId equals sub.Id
            join ort in _accountsDbContext.OrganisationRelationshipTypes on rel.OrganisationRelationshipTypeId equals ort.Id
            where o.ExternalId == organisationExternalId && (rel.RelationToDate == null || rel.RelationToDate >= DateTime.Now)
            select new ExportOrganisationSubsidiariesQueryModel
            {
                OrganisationId = o.ReferenceNumber,
                SubsidiaryId = sub.ReferenceNumber,
                ParentOrganisationName = o.Name,
                ChildOrganisationName = sub.Name,
                ChildCompaniesHouseNumber = sub.CompaniesHouseNumber,
                ChildJoinerDate = rel.JoinerDate,
                ParentCompaniesHouseNumber = o.CompaniesHouseNumber
            };

        return parentChildDetails.GetCombinedParentChildQuery();
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

    private IQueryable<OrganisationRelationshipsResponseModel> GetRelationships(Guid organisationExternalId)
    {
        return from organisation in _accountsDbContext.Organisations
               join organisationRelationships in _accountsDbContext.OrganisationRelationships on organisation.Id equals organisationRelationships.FirstOrganisationId
               join organisationSub in _accountsDbContext.Organisations on organisationRelationships.SecondOrganisationId equals organisationSub.Id
               join organisationRelationshipTypes in _accountsDbContext.OrganisationRelationshipTypes on organisationRelationships.OrganisationRelationshipTypeId equals organisationRelationshipTypes.Id
               join subsidiaryOrganisation in _accountsDbContext.SubsidiaryOrganisations
                    on organisationRelationships.SecondOrganisationId equals subsidiaryOrganisation.OrganisationId into subsidiaryOrganisationGroup
               from subsidiaryOrganisation in subsidiaryOrganisationGroup.DefaultIfEmpty()
               where organisation.ExternalId == organisationExternalId
                     && organisationRelationships.RelationToDate == null
               select new OrganisationRelationshipsResponseModel
               {
                   Id = organisation.ExternalId,
                   Name = organisation.Name,
                   OrganisationRole = "Producer",
                   OrganisationType = organisation.OrganisationType.Name,
                   OrganisationNumber = organisation.ReferenceNumber,
                   CompaniesHouseNumber = organisation.CompaniesHouseNumber,
                   ProducerType = organisation.ProducerType.Name,
                   NationId = organisation.NationId,
                   Relationships = new OrganisationRelationshipsModel
                   {
                       FirstOrganisation = new OrganisationDetailModel
                       {
                           OrganisationNumber = organisationSub.ReferenceNumber,
                           Name = organisationSub.Name,
                           CompaniesHouseNumber = organisationSub.CompaniesHouseNumber,
                           JoinerDate = organisationRelationships.JoinerDate
                       },
                       OrganisationRelationshipType = new OrganisationRelationshipTypeModel
                       {
                           Name = organisationRelationshipTypes.Name
                       }
                   }
               };
    }

    private static OrganisationRelationshipResponseModel CreateOrganisationRelationshipsDetailModel(List<OrganisationRelationshipsResponseModel> relationships)
    {
        var firstRelationship = relationships[0];

        var organisationDetail = new OrganisationDetailModel
        {
            Id = firstRelationship.Id,
            Name = firstRelationship.Name,
            OrganisationRole = firstRelationship.OrganisationRole,
            OrganisationNumber = firstRelationship.OrganisationNumber,
            CompaniesHouseNumber = firstRelationship.CompaniesHouseNumber,
            OrganisationType = firstRelationship.OrganisationType,
            ProducerType = firstRelationship.ProducerType,
            NationId = firstRelationship.NationId
        };

        var relationshipDetails = relationships.Select(r => new RelationshipResponseModel
        {
            OrganisationNumber = r.Relationships.FirstOrganisation.OrganisationNumber,
            OrganisationName = r.Relationships.FirstOrganisation.Name,
            RelationshipType = r.Relationships.OrganisationRelationshipType.Name,
            CompaniesHouseNumber = r.Relationships.FirstOrganisation.CompaniesHouseNumber,
            JoinerDate = r.Relationships.FirstOrganisation.JoinerDate,
            LeaverCode = r.Relationships.FirstOrganisation.LeaverCode,
            LeaverDate = r.Relationships.FirstOrganisation.LeaverDate,
            OrganisationChangeReason = r.Relationships.FirstOrganisation.OrganisationChangeReason
        }).ToList();

        return new OrganisationRelationshipResponseModel
        {
            Organisation = organisationDetail,
            Relationships = relationshipDetails
        };
    }

    private async Task<Organisation> ProcessFranchisee(OrganisationModel organisationModel, OrganisationRelationshipModel organisationRelationshipModel, Guid userExternalId)
    {
        var isNestedTransaction = _accountsDbContext.Database.CurrentTransaction is not null;
        var transaction = _accountsDbContext.Database.CurrentTransaction ?? await _accountsDbContext.Database.BeginTransactionAsync();

        try
        {
            var organisation = OrganisationMappings.GetOrganisationFromOrganisationModel(organisationModel);

            var franchisee = await GetOrganisationsByCompaniesHouseNameAsync(organisationModel.Name);
            if (franchisee.Count == 0)
            {
                _accountsDbContext.Organisations.Add(organisation);
                await _accountsDbContext.SaveChangesAsync(userExternalId, organisation.ExternalId);

                organisationRelationshipModel.SecondOrganisationId = organisation.Id;

                var franchiseeAdded = await AddOrganisationRelationshipsAsync(organisationRelationshipModel, organisation.ExternalId, userExternalId);

                if (franchiseeAdded != null)
                {
                    if (!string.IsNullOrWhiteSpace(organisationModel.SubsidiaryOrganisationId))
                    {
                        var subsidiaryOrganisation = OrganisationMappings.GetSubsidiaryOrganisationFromOrganisationModel(organisationModel.SubsidiaryOrganisationId, organisation);

                        _accountsDbContext.SubsidiaryOrganisations.Add(subsidiaryOrganisation);

                        await _accountsDbContext.SaveChangesAsync(userExternalId, organisation.ExternalId);
                    }

                    organisation.OrganisationRelationships ??= new List<OrganisationRelationship>();

                    organisation.OrganisationRelationships.Add(franchiseeAdded);
                }
            }
            else
            {
                organisationRelationshipModel.SecondOrganisationId = franchisee.First().Id;
                organisation.ReferenceNumber = franchisee.FirstOrDefault()?.ReferenceNumber;
                organisation.Id = franchisee.First().Id;

                var franchiseeAdded = await AddOrganisationRelationshipsAsync(organisationRelationshipModel, organisation.ExternalId, userExternalId);

                if (franchiseeAdded != null && !string.IsNullOrWhiteSpace(organisationModel.SubsidiaryOrganisationId))
                {
                    var subsidiaryOrganisation = OrganisationMappings.GetSubsidiaryOrganisationFromOrganisationModel(organisationModel.SubsidiaryOrganisationId, organisation);

                    _accountsDbContext.SubsidiaryOrganisations.Add(subsidiaryOrganisation);

                    await _accountsDbContext.SaveChangesAsync(userExternalId, organisation.ExternalId);
                }
            }

            return organisation;
        }
        finally
        {
            if (!isNestedTransaction)
            {
                await transaction.CommitAsync();
            }
        }

    }

    public async Task<List<PersonEmailResponseModel>> GetPersonEmails(Guid organisationId, string entityTypeCode)
    {
        if (entityTypeCode.ToUpper() == "CS")  //CS = Compliance Scheme
        {
            var results = await (from p in _accountsDbContext.Persons
                                 join poc in _accountsDbContext.PersonOrganisationConnections on p.Id equals poc.PersonId
                                 join o in _accountsDbContext.Organisations on poc.OrganisationId equals o.Id
                                 join cs in _accountsDbContext.ComplianceSchemes on o.CompaniesHouseNumber equals cs.CompaniesHouseNumber
                                 join e in _accountsDbContext.Enrolments on poc.Id equals e.ConnectionId
                                 where cs.ExternalId == organisationId
                                 && !o.IsDeleted && !e.IsDeleted
                                 select new PersonEmailResponseModel
                                 {
                                     FirstName = p.FirstName,
                                     LastName = p.LastName,
                                     Email = p.Email,
                                 }).ToListAsync();

            return results;
        }
        else if (entityTypeCode.ToUpper() == "DR")  //DR = Direct Registrant
        {
            var results = await (from p in _accountsDbContext.Persons
                                 join poc in _accountsDbContext.PersonOrganisationConnections on p.Id equals poc.PersonId
                                 join e in _accountsDbContext.Enrolments on poc.Id equals e.ConnectionId
                                 where poc.OrganisationId == (from o in _accountsDbContext.Organisations
                                                              where o.ExternalId == organisationId
                                                              select o.Id).FirstOrDefault()
                                 select new PersonEmailResponseModel
                                 {
                                     FirstName = p.FirstName,
                                     LastName = p.LastName,
                                     Email = p.Email
                                 }).ToListAsync();

            return results;
        }
        return new List<PersonEmailResponseModel>();
    }
}
