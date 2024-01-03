using BackendAccountService.Core.Models;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Entities.Conversions;
using BackendAccountService.Data.Extensions;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EnrolmentStatus = BackendAccountService.Data.DbConstants.EnrolmentStatus;
using Nation = BackendAccountService.Data.DbConstants.Nation;
using OrganisationType = BackendAccountService.Data.DbConstants.OrganisationType;
using ServiceRole = BackendAccountService.Data.DbConstants.ServiceRole;

namespace BackendAccountService.Core.Services;

public class RegulatorService: IRegulatorService
{
    private readonly AccountsDbContext _accountsDbContext;
    private readonly IOrganisationService _organisationService;
    private const string RegulatingService = "Regulating";
    private readonly ILogger<RegulatorService> _logger;

    public RegulatorService(AccountsDbContext accountsDbContext, 
                            IOrganisationService organisationService , 
                            ILogger<RegulatorService> logger)
    {
        _accountsDbContext = accountsDbContext;
        _organisationService = organisationService;
        _logger = logger;
    }

    public async Task<PaginatedResponse<OrganisationEnrolments>> GetPendingApplicationsAsync(int nationId, int currentPage, int pageSize, string? organisationName, string applicationType)
    {
        var companiesHouseList = _accountsDbContext.ComplianceSchemes.Where(x => x.NationId == nationId).Select(x=>x.CompaniesHouseNumber).ToList();
        
        var enrolments = _accountsDbContext.Enrolments.Where(
            e => e.EnrolmentStatus.Id == EnrolmentStatus.Pending
            && (e.Connection.Organisation.Nation.Id == nationId || (e.Connection.Organisation.IsComplianceScheme && companiesHouseList.Contains(e.Connection.Organisation.CompaniesHouseNumber)))
            && (e.ServiceRole.Id == ServiceRole.Packaging.ApprovedPerson.Id ||e.ServiceRole.Id == ServiceRole.Packaging.DelegatedPerson.Id)
            && (string.IsNullOrWhiteSpace(organisationName) || e.Connection.Organisation.Name.ToLower().Contains(organisationName.ToLower())))
            .AsNoTracking()
            .GroupBy(e=> new { id = e.Connection.Organisation.ExternalId, name = e.Connection.Organisation.Name})
            .Select(x=>new OrganisationEnrolments
            {
                OrganisationId = x.Key.id,
                OrganisationName = x.Key.name,
                LastUpdate = x.Min(s=> s.LastUpdatedOn.Date),
                Enrolments = new()
                {
                    HasApprovedPending = x.Sum(s=>s.ServiceRole.Id == ServiceRole.Packaging.ApprovedPerson.Id ? 1:0) >0,
                    HasDelegatePending = x.Sum(s=>s.ServiceRole.Id == ServiceRole.Packaging.DelegatedPerson.Id ? 1:0) >0
                }
            }).OrderByDescending(x=>x.Enrolments.HasApprovedPending).ThenBy(x=>x.LastUpdate).ThenBy(x=>x.OrganisationName)
            .AsQueryable();

        if (applicationType.Equals("ApprovedPerson", StringComparison.CurrentCultureIgnoreCase))
        {
            enrolments = enrolments.Where(e => e.Enrolments.HasApprovedPending).AsQueryable();
        }
        else if (applicationType.Equals("DelegatedPerson", StringComparison.CurrentCultureIgnoreCase))
        {
            enrolments = enrolments.Where(e => e.Enrolments.HasDelegatePending).AsQueryable();
        }

        return await PaginatedResponse<OrganisationEnrolments>.CreateAsync(enrolments,currentPage, pageSize);

    }

    public async Task<(bool Succeeded, string ErrorMessage)> UpdateEnrolmentStatusForUserAsync(Guid userId,
        Guid organisationId, Guid enrolmentId,
        string enrolmentStatus, string regulatorComment)
    {
        var enrolment = _accountsDbContext.Enrolments
            .Include(e => e.Connection)
                .ThenInclude(c => c.Person)
                    .ThenInclude(p => p.User)
            .SingleOrDefault(e => e.ExternalId == enrolmentId
        && (e.ServiceRoleId == ServiceRole.Packaging.ApprovedPerson.Id || e.ServiceRoleId == ServiceRole.Packaging.DelegatedPerson.Id));

        if (enrolment == null)
        {
            return (false, "enrolment not found");
        }

        var enrolmentstatusName =  _accountsDbContext.EnrolmentStatuses.Single(status =>
            status.Name.Equals(enrolmentStatus));

        if (enrolmentstatusName.Id != EnrolmentStatus.Approved && enrolmentstatusName.Id != EnrolmentStatus.Rejected)
        {
            return (false, "unsupported enrolment status");
        }
        if (enrolmentstatusName.Id.Equals(EnrolmentStatus.Approved))
        {
            enrolment.EnrolmentStatusId = EnrolmentStatus.Approved;
            await _accountsDbContext.SaveChangesAsync(userId, organisationId);
            return (true, string.Empty);
        }

        if (regulatorComment == null)
        {
            return (false, "regulator comments missing");
        }

        enrolment.EnrolmentStatusId = EnrolmentStatus.Rejected;
        enrolment.IsDeleted = true;

        var regulatorPerson = _accountsDbContext.Persons.AsNoTracking().Single(person => person.User.UserId == userId);
        
        var rejectedComments = new RegulatorComment
        {
            EnrolmentId = enrolment.Id,
            RejectedComments = regulatorComment,
            PersonId = regulatorPerson.Id
        };
        await _accountsDbContext.RegulatorComments.AddAsync(rejectedComments);

        if (enrolment.ServiceRoleId == ServiceRole.Packaging.DelegatedPerson.Id)
        {
            var basicEnrolment = new Enrolment
            {
                ConnectionId = enrolment.ConnectionId,
                ServiceRoleId = ServiceRole.Packaging.BasicUser.Id,
                EnrolmentStatusId = EnrolmentStatus.Enrolled,
            };

            await _accountsDbContext.Enrolments.AddAsync(basicEnrolment);
            await _accountsDbContext.SaveChangesAsync(userId, organisationId);
            return (true, string.Empty);
        }

        SoftDeleteOrganisation(organisationId);
        SetDefaultUserIdForRejectedApprovedPerson(enrolment);

        await _accountsDbContext.SaveChangesAsync(userId, organisationId);
        return (true, string.Empty);
    }

    private void SetDefaultUserIdForRejectedApprovedPerson(Enrolment enrolment)
    {
        if (enrolment.ServiceRoleId == ServiceRole.Packaging.ApprovedPerson.Id)
        {
            enrolment.Connection.Person.User.UserId = Guid.Empty;
        }
    }

    private void SoftDeleteOrganisation(Guid organisationId)
    {
        _accountsDbContext.Enrolments.Where(enrolment =>
            enrolment.Connection.Organisation.ExternalId == organisationId).ToList().ForEach(x =>
        {
            x.IsDeleted = true;
        });

        _accountsDbContext.PersonOrganisationConnections.Where(connection =>
            connection.Organisation.ExternalId == organisationId).ToList().ForEach(x =>
        {
            x.IsDeleted = true;
        });

        _accountsDbContext.Organisations.Where(organisation =>
            organisation.ExternalId == organisationId).ToList().ForEach(x =>
        {
            x.IsDeleted = true;
        });

        _accountsDbContext.OrganisationsConnections.Where(connection =>
            connection.FromOrganisation.ExternalId == organisationId || connection.ToOrganisation.ExternalId == organisationId).ToList().ForEach(x =>
        {
            x.IsDeleted = true;
        });

        _accountsDbContext.SelectedSchemes.Where(scheme =>
            scheme.OrganisationConnection.FromOrganisation.ExternalId == organisationId || scheme.OrganisationConnection.ToOrganisation.ExternalId == organisationId).ToList().ForEach(x =>
        {
            x.IsDeleted = true;
        });

        var organisationUsersList = _accountsDbContext.PersonOrganisationConnections
            .Where(org => org.Organisation.ExternalId == organisationId)
            .Select(org => org.Person.User.UserId).ToList();

        var usersToSoftDelete = new List<Guid?>();

        foreach (var userId in organisationUsersList)
        {
            if (!_organisationService.IsUserAssociatedWithMultipleOrganisations(userId))
            {
                usersToSoftDelete.Add(userId);
            }
        }

        _accountsDbContext.Users.Where(user =>
            usersToSoftDelete.Contains(user.UserId)).ToList().ForEach(x =>
        {
            x.IsDeleted = true;
        });

        _accountsDbContext.Persons.Where(person =>
            usersToSoftDelete.Contains(person.User.UserId)).ToList().ForEach(x =>
        {
            x.IsDeleted = true;
        });
    }

    public int GetRegulatorNationId(Guid userId)
    {
        var regulatorNation = _accountsDbContext.PersonOrganisationConnections.Include(p => p.Organisation)
            .SingleOrDefault(p => p.Person.User.UserId == userId);

        if (regulatorNation == null)
        {
            return 0;
        }

        return regulatorNation.Organisation.NationId ?? 0;
    }

    public async Task<List<int>> GetOrganisationNationIds(Guid organisationId)
    {
        var nationIds = new List<int>();
        
        var organisation = _accountsDbContext.Organisations
            .SingleOrDefault(p => p.ExternalId == organisationId);

        if (organisation is { IsComplianceScheme: true })
        {
            var complianceSchemeNationIds = await _accountsDbContext.ComplianceSchemes
                .Where(cs => 
                    cs.CompaniesHouseNumber.Equals(organisation.CompaniesHouseNumber) 
                    && cs.NationId != null
                    && cs.NationId != 0)
                .Select(cs => (int)cs.NationId)
                .Distinct()
                .ToListAsync();
            nationIds.AddRange(complianceSchemeNationIds);
        }
        
        if (organisation?.NationId != null 
            && organisation.NationId != 0 
            && !nationIds.Contains((int)organisation.NationId))
        {
            nationIds.Add((int)organisation.NationId);
        }

        // return a 0 if nothing is found to maintain original behaviour where
        // null nation ids mean that all nations are valid for this organisation
        if (!nationIds.Any())
        {
            nationIds.Add(0);
        }

        return nationIds;
    }

    public async Task<ApplicationEnrolmentDetails> GetOrganisationEnrolmentDetails(Guid organisationId)
    {
        var organisationEnrolmentsList = _accountsDbContext.Enrolments.Where(
                e => e.Connection.Organisation.ExternalId == organisationId
                     && (e.EnrolmentStatusId == EnrolmentStatus.Approved ||
                         e.EnrolmentStatusId == EnrolmentStatus.Pending)
                     && (e.ServiceRoleId == ServiceRole.Packaging.ApprovedPerson.Id
                         || e.ServiceRoleId == ServiceRole.Packaging.DelegatedPerson.Id)
            )
            .Select(x => new UserEnrolmentDetails
                {
                    FirstName = x.Connection.Person.FirstName,
                    LastName = x.Connection.Person.LastName,
                    Email = x.Connection.Person.Email,
                    TelephoneNumber = x.Connection.Person.Telephone,
                    JobTitle = x.Connection.JobTitle,
                    IsEmployeeOfOrganisation = x.DelegatedPersonEnrolment == null || x.DelegatedPersonEnrolment.RelationshipType == RelationshipType.Employment,
                    Enrolments = new EnrolmentDetails
                    {
                        EnrolmentStatus = x.EnrolmentStatus.Name,
                        ServiceRole = x.ServiceRole.Key,
                        ExternalId = x.ExternalId
                    }
                }
            ).AsEnumerable();

        var result = _accountsDbContext.Organisations.Where(o => o.ExternalId == organisationId).Select(x =>
            new ApplicationEnrolmentDetails
            {
                OrganisationId = x.ExternalId,
                OrganisationType = x.OrganisationType.Id == OrganisationType.CompaniesHouseCompany ?  x.OrganisationType.Name : x.ProducerType.Name,
                OrganisationName = x.Name,
                OrganisationReferenceNumber = x.ReferenceNumber,
                CompaniesHouseNumber = x.CompaniesHouseNumber,
                IsComplianceScheme = x.IsComplianceScheme,
                BusinessAddress = new AddressModel
                {
                    BuildingNumber = x.BuildingNumber,
                    BuildingName = x.BuildingName,
                    Street = x.Street,
                    County = x.County,
                    Town = x.Town,
                    Postcode = x.Postcode,
                    Locality = x.Locality,
                    Country = x.Country
                },
                NationId = x.NationId,
                NationName = x.Nation.Name,
                Users = organisationEnrolmentsList.ToList()
            }).SingleOrDefault();

        var approvedUserEnrolment = _accountsDbContext.Enrolments
            .SingleOrDefault(enrolment => enrolment.Connection.Organisation.ExternalId == organisationId &&
                                          enrolment.ServiceRoleId == ServiceRole.Packaging.ApprovedPerson.Id);

        if ((approvedUserEnrolment == null && result == null) || result == null)
        {
            return new ApplicationEnrolmentDetails();
        }

        var transfer = _accountsDbContext.RegulatorComments.Where(comments =>
                comments.EnrolmentId == approvedUserEnrolment!.Id && !string.IsNullOrWhiteSpace(comments.TransferComments))
            .OrderByDescending(comments => comments.LastUpdatedOn).Take(1).SingleOrDefault();
        
        if (transfer != null)
        {
            var userEnrolmentDetails = result.Users.Single(x => x.Enrolments.ServiceRole.Contains("Approved"));
            userEnrolmentDetails.TransferComments = transfer.TransferComments;

            var organisation = _accountsDbContext.Organisations.AsNoTracking().Single(organisation => organisation.ExternalId == organisationId);

            result.TransferDetails = new TransferDetails
            {
                OldNationId = organisation.TransferNationId.Value,
                TransferredDate = transfer.CreatedOn
            };
        }
        
        return result;
    }

    public bool DoesRegulatorNationMatchOrganisationNation(Guid userId, Guid organisationId)
    {
        var organisation = _accountsDbContext.Organisations.FirstOrDefault(x => x.ExternalId == organisationId);
        
        if (organisation == null)
            return false;
        
        if (!organisation.IsComplianceScheme)
        {
            var organisationEnrolments = _accountsDbContext.Enrolments
                .WhereNationIs(organisation.NationId ?? Nation.NotSet)
                .WhereServiceIs(RegulatingService)
                .WhereUserObjectIdIs(userId)
                .WhereOrganisationIsRegulator()
                .SelectDistinctSingleOrganisation();

            return organisationEnrolments != null;
        }
        
        var regulatorOrganisation = _accountsDbContext.Enrolments
            .WhereServiceIs(RegulatingService)
            .WhereUserObjectIdIs(userId)
            .WhereOrganisationIsRegulator()
            .SelectDistinctSingleOrganisation();

        if (regulatorOrganisation == null)
            return false;
        var isComplianceSchemeNationMatchesRegulatorNation = _accountsDbContext.ComplianceSchemes.Any(x =>
            x.NationId == regulatorOrganisation.NationId && x.CompaniesHouseNumber == organisation.CompaniesHouseNumber);

        return isComplianceSchemeNationMatchesRegulatorNation;
    }
    
    public bool IsRegulator(Guid userId)
    {
        var organisation = _accountsDbContext.Enrolments
            .WhereServiceIs(RegulatingService)
            .WhereUserObjectIdIs(userId)
            .WhereOrganisationIsRegulator()
            .SelectDistinctSingleOrganisation();

        return organisation != null;
    }

    public async Task<(bool Succeeded, string ErrorMessage)> TransferOrganisationNation(
        OrganisationTransferNationRequest request)
    {
        var organisation = _accountsDbContext.Organisations.Single(org => org.ExternalId == request.OrganisationId);

        if (organisation.IsComplianceScheme)
        {
            return (false, "Cannot transfer compliance scheme");
        }
        
        var transferNation = _accountsDbContext.Nations.AsNoTracking().SingleOrDefault(nation => nation.Id == request.TransferNationId);

        if (transferNation == null)
        {
            return (false, "Invalid Nation");
        }
        
        var enrolmentId = _accountsDbContext.Enrolments.AsNoTracking().Single(e =>
            e.ServiceRoleId == ServiceRole.Packaging.ApprovedPerson.Id
            && e.Connection.Organisation.ExternalId == request.OrganisationId).Id;
        
        var regulatorPerson = _accountsDbContext.Persons.AsNoTracking()
            .Single(person => person.User.UserId == request.UserId);

        // move current nationId to transferNationId to preserve as "transferred from" nation
        organisation.TransferNationId = organisation.NationId;
        
        // set NationId to NEW nation - NationId is always current nation
        organisation.NationId = request.TransferNationId;

        var transferComments = new RegulatorComment
        {
            EnrolmentId = enrolmentId,
            TransferComments = request.TransferComments,
            PersonId = regulatorPerson.Id
        };
        
        await _accountsDbContext.RegulatorComments.AddAsync(transferComments);

        await _accountsDbContext.SaveChangesAsync(request.UserId, request.OrganisationId);

        return (true, string.Empty);
    }
    
    public async Task<IQueryable<OrganisationUsersResponseModel>> GetUserListForRegulator(Guid organisationId, bool getApprovedUsersOnly = false)
    {
        var enrolments = _accountsDbContext.Enrolments
            .Include(con => con.Connection)
            .Include(p => p.Connection.Person).AsNoTracking().Where(e =>
                (!getApprovedUsersOnly || e.EnrolmentStatusId == EnrolmentStatus.Approved)
                && (e.ServiceRoleId == ServiceRole.Packaging.ApprovedPerson.Id
                    || e.ServiceRoleId == ServiceRole.Packaging.DelegatedPerson.Id)
                && e.Connection.Organisation.ExternalId == organisationId);

        var result = enrolments.GroupBy(p => new
        {
            p.Connection.Person.FirstName,
            p.Connection.Person.LastName,
            p.Connection.Person.Email,
            p.Connection.PersonRoleId,
            p.Connection.Person.ExternalId,
            ConnectionExternalId = p.Connection.ExternalId
        }).Select(x => new OrganisationUsersResponseModel
        {
            FirstName = x.Key.FirstName,
            LastName = x.Key.LastName,
            Email = x.Key.Email,
            PersonId = x.Key.ExternalId,
            PersonRoleId = x.Key.PersonRoleId,
            ConnectionId = x.Key.ConnectionExternalId,
            Enrolments = x.Select(e => new UserEnrolments
            {
                EnrolmentStatusId = e.EnrolmentStatusId,
                ServiceRoleId = e.ServiceRoleId
            })
        });

        return result;
    }
    public async Task<CompanySearchDetailsModel> GetCompanyDetailsById(Guid organisationId)
    {
        var companyDetails = new CompanySearchDetailsModel();

        var enrolments = _accountsDbContext.Enrolments
            .Include(con => con.Connection)
            .Include(p => p.Connection.Person).AsNoTracking()
            .WhereEnrolmentStatusIn(
                EnrolmentStatus.Enrolled,
                EnrolmentStatus.Approved,
                EnrolmentStatus.Nominated,
                EnrolmentStatus.Pending,
                EnrolmentStatus.OnHold)
            .Where(e => e.Connection.Organisation.ExternalId == organisationId);

        var enrolmentInformation = enrolments.GroupBy(p => new
        {
            p.Connection.Person.FirstName,
            p.Connection.Person.LastName,
            p.Connection.Person.Email,
            p.Connection.Person.Telephone,
            p.Connection.JobTitle,
            p.Connection.Organisation.Postcode,
            p.Connection.PersonRoleId,
            p.Connection.ExternalId,
            p.DelegatedPersonEnrolment.RelationshipType
        }).Select(x => new UserInformation(
            x.Key.FirstName,
            x.Key.LastName,
            x.Key.Email,
            x.Key.ExternalId,
            x.Select(e => new UserEnrolments
            {
                EnrolmentStatusId = e.EnrolmentStatusId,
                ServiceRoleId = e.ServiceRoleId 
            }),
            x.Key.RelationshipType == RelationshipType.Employment,
            x.Key.JobTitle,
            x.Key.Telephone,
            x.Key.PersonRoleId
        ));

        companyDetails.CompanyUserInformation = enrolmentInformation;
        var details = _accountsDbContext.Organisations.Where(x => x.ExternalId == organisationId)
            .Select(p => new Company
            {
                OrganisationName = p.Name,
                CompaniesHouseNumber = p.CompaniesHouseNumber,
                OrganisationId = p.ReferenceNumber,
                OrganisationTypeId = p.OrganisationTypeId,
                IsComplianceScheme = p.IsComplianceScheme,
                RegisteredAddress = new AddressModel
                {
                    Town = p.Town,
                    County = p.County,
                    Postcode = p.Postcode,
                    Street = p.Street,
                    BuildingNumber = p.BuildingNumber
                }
            });
        companyDetails.Company = details.FirstOrDefault();
        return companyDetails;
    }
    
    public async Task<(bool Succeeded, string ErrorMessage)> RemoveApprovedPerson(Guid userId ,Guid connExternalId, Guid organisationId)
    {
        try
        {
           var getAllUsers = await _accountsDbContext.Enrolments
               .Include(enrolment => enrolment.Connection)
               .Include(enrolment => enrolment.Connection.Person)
               .Include(enrolment => enrolment.Connection.Organisation)
               .WhereEnrolmentServiceRoleIn(ServiceRole.Packaging.ApprovedPerson.Id,
                   ServiceRole.Packaging.DelegatedPerson.Id)
               .Where(e => e.Connection.Organisation.ExternalId == organisationId)
               .ToListAsync();

            var approvedPerson = getAllUsers.SingleOrDefault(a => a.Connection.ExternalId == connExternalId);
           if (approvedPerson == null)
           {
               return (false, "Approved person doesnt belong to organisation");
           }
           
           //soft delete approved users 
           approvedPerson.IsDeleted = true;
           approvedPerson.Connection.IsDeleted = true;
           approvedPerson.Connection.Person.IsDeleted = true;
            
            //demote delegated users to basic users
            var delegatedPersons = getAllUsers
                .Where(a => a.ServiceRoleId == ServiceRole.Packaging.DelegatedPerson.Id)
                .ToList();

            foreach (var person in delegatedPersons)
            {
                person.ServiceRoleId = ServiceRole.Packaging.BasicUser.Id;
            }
        
            await _accountsDbContext.SaveChangesAsync(userId, organisationId);
            return (true, "Success");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing the approved person { connExternalId } from organisation {organisationId}", connExternalId, organisationId);
             return (false, "Error removing the approved person");
        }
        
    }
}