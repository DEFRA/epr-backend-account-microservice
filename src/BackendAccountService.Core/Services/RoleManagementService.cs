using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Models.Exceptions;
using BackendAccountService.Core.Models.Mappings;
using BackendAccountService.Core.Models.Request;
using BackendAccountService.Core.Models.Responses;
using BackendAccountService.Data.DbConstants;
using BackendAccountService.Data.Entities;
using BackendAccountService.Data.Extensions;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using DbConstants = BackendAccountService.Data.DbConstants;
using EnrolmentStatus = BackendAccountService.Core.Models.EnrolmentStatus;
using PersonRole = BackendAccountService.Core.Models.PersonRole;

namespace BackendAccountService.Core.Services;

public class RoleManagementService : IRoleManagementService
{
    private static readonly int[] ActiveEnrolmentStatuses =
    {
        DbConstants.EnrolmentStatus.Enrolled,
        DbConstants.EnrolmentStatus.Pending,
        DbConstants.EnrolmentStatus.Approved,
        DbConstants.EnrolmentStatus.Invited,
        DbConstants.EnrolmentStatus.Nominated
    };

    private readonly AccountsDbContext _accountsDbContext;
    private readonly IValidationService _validationService;

    public RoleManagementService(
        AccountsDbContext accountsDbContext,
        IValidationService validationService)
    {
        _accountsDbContext = accountsDbContext;
        _validationService = validationService;
    }

    public async Task<ConnectionWithEnrolmentsResponse?> GetConnectionWithEnrolmentsFromOrganisationForServiceAsync(Guid connectionId, Guid organisationId, string serviceKey)
    {
        return await _accountsDbContext.PersonOrganisationConnections
            .Where(connection => connection.ExternalId == connectionId)
            // Ensure that connection belongs to the organisation that user is authorised to
            .Where(connection => connection.Organisation.ExternalId == organisationId)
            .Select(connection => new ConnectionWithEnrolmentsResponse
            {
                PersonRole = MapPersonRole(connection.PersonRoleId),
                UserId = connection.Person.User.UserId.Value,
                Enrolments = connection.Enrolments
                    .Where(enrolment => enrolment.ServiceRole.Service.Key == serviceKey)
                    .Where(enrolment => ActiveEnrolmentStatuses.Contains(enrolment.EnrolmentStatusId))
                    .Select(enrolment => new EnrolmentsFromConnectionResponse
                    {
                        ServiceRoleKey = enrolment.ServiceRole.Key,
                        EnrolmentStatus = MapEnrolmentStatus(enrolment.EnrolmentStatusId)
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();
    }

    public async Task<ConnectionWithPersonResponse?> GetConnectionWithPersonForServiceAsync(Guid connectionId, Guid organisationId, string serviceKey)
    {
        return await _accountsDbContext.PersonOrganisationConnections
            .Where(connection => connection.ExternalId == connectionId)
            // Ensure that connection belongs to the organisation that user is authorised to
            .Where(connection => connection.Organisation.ExternalId == organisationId)
            .Select(connection => new ConnectionWithPersonResponse
            {
                FirstName = connection.Person.FirstName,
                LastName = connection.Person.LastName,
                Email = connection.Person.User.Email,
                OrganisationName = connection.Organisation.Name,
                OrganisationReferenceNumber = connection.Organisation.ReferenceNumber
            })
            .SingleOrDefaultAsync();
    }

    public async Task<UpdatePersonRoleResponse> UpdatePersonRoleAsync(
        Guid connectionId,
        Guid userId,
        Guid organisationId,
        string serviceKey,
        PersonRole personRole)
    {
        if (serviceKey != ServiceKeys.Packaging)
        {
            throw new RoleManagementException($"Unsupported service '{serviceKey}'");
        }

        if (personRole != PersonRole.Employee && personRole != PersonRole.Admin)
        {
            throw new RoleManagementException($"Unsupported person role '{personRole}");
        }

        var connection = await GetConnectionWithEnrolments(connectionId, organisationId, serviceKey);
        if (connection == null)
        {
            throw new RoleManagementException("There is no matching record to update");
        }

        if (connection.Person.User?.UserId == userId)
        {
            throw new RoleManagementException("Updating own record is not permitted");
        }

        var activeEnrolments = ExtractActiveEnrolments(connection, serviceKey);

        if (!activeEnrolments.Any())
        {
            throw new RoleManagementException("Not enrolled user cannot be edited");
        }

        if (activeEnrolments.Any(enrolment => enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Invited))
        {
            throw new RoleManagementException("Invited user cannot be edited");
        }

        if (activeEnrolments.Any(enrolment => enrolment.ServiceRole.Key == DbConstants.ServiceRole.Packaging.ApprovedPerson.Key))
        {
            throw new RoleManagementException("Approved person cannot be edited");
        }

        List<RemovedServiceRole> removedServiceRoles = null;

        if (activeEnrolments.Any(enrolment => enrolment.ServiceRole.Key == DbConstants.ServiceRole.Packaging.DelegatedPerson.Key))
        {
            var isAuthorised = await _validationService.IsAuthorisedToManageDelegatedUsersFromOrganisationForService(userId, organisationId, serviceKey);
            if (!isAuthorised)
            {
                throw new RoleManagementException("Only approved person can edit delegated person enrolment");
            }

            removedServiceRoles = await ReplaceDelegatedPersonWithBasicUser(connection, activeEnrolments);
        }

        connection.PersonRoleId = PersonRoleMappings.GetPersonRoleId(personRole);

        await _accountsDbContext.SaveChangesAsync(userId, organisationId);

        return new UpdatePersonRoleResponse
        {
            RemovedServiceRoles = removedServiceRoles
        };
    }

    private async Task<List<RemovedServiceRole>> ReplaceDelegatedPersonWithBasicUser(PersonOrganisationConnection connection, ICollection<Enrolment> activeEnrolments)
    {
        List<RemovedServiceRole> removedServiceRoles = new();

        foreach (var enrolment in activeEnrolments.Where(enrolment => enrolment.ServiceRole.Key == DbConstants.ServiceRole.Packaging.DelegatedPerson.Key))
        {
            if (enrolment.DelegatedPersonEnrolment != null)
            {
                _accountsDbContext.Remove(enrolment.DelegatedPersonEnrolment);
            }

            _accountsDbContext.Remove(enrolment);

            removedServiceRoles.Add(new RemovedServiceRole
            {
                ServiceRoleKey = enrolment.ServiceRole.Key,
                EnrolmentStatus = enrolment.EnrolmentStatus.Name
            });
        }

        if (!activeEnrolments.Any(enrolment => enrolment.ServiceRole.Key == DbConstants.ServiceRole.Packaging.BasicUser.Key &&
                                               enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Enrolled))
        {
            var basicUserServiceRole = await _accountsDbContext.ServiceRoles
                .SingleAsync(serviceRole => serviceRole.Key == DbConstants.ServiceRole.Packaging.BasicUser.Key);

            connection.Enrolments.Add(new Enrolment
            {
                ServiceRole = basicUserServiceRole,
                EnrolmentStatusId = DbConstants.EnrolmentStatus.Enrolled
            });
        }

        return removedServiceRoles;
    }

    public async Task<(bool Succeeded, string ErrorMessage)> NominateToDelegatedPerson(
        Guid connectionId, Guid userId, Guid organisationId, string serviceKey,
        DelegatedPersonNominationRequest nominationRequest)
    {
        if (serviceKey != ServiceKeys.Packaging)
        {
            return (false, $"Unsupported service '{serviceKey}'");
        }

        var validationResult = ValidateDelegatedPersonNominationRequest(nominationRequest);
        if (!validationResult.Succeeded)
        {
            return (false, $"Invalid nomination request: {validationResult.ErrorMessage}");
        }

        var connection = await GetConnectionWithEnrolments(connectionId, organisationId, serviceKey);
        if (connection == null)
        {
            return (false, "There is no matching record to update");
        }

        if (connection.Person.User?.UserId == userId)
        {
            return (false, "Updating own record is not permitted");
        }

        var activeEnrolments = ExtractActiveEnrolments(connection, serviceKey);

        if (!activeEnrolments.Any())
        {
            return (false, "Not enrolled user cannot be nominated");
        }

        if (activeEnrolments.Any(enrolment => enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Invited))
        {
            return (false, "Invited user cannot be nominated");
        }

        if (activeEnrolments.Any(enrolment => enrolment.ServiceRole.Key == DbConstants.ServiceRole.Packaging.ApprovedPerson.Key))
        {
            return (false, "Approved Person cannot be nominated");
        }

        if (activeEnrolments.Any(enrolment => enrolment.ServiceRole.Key == DbConstants.ServiceRole.Packaging.DelegatedPerson.Key))
        {
            return (false, "Delegated Person cannot be nominated");
        }

        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (!activeEnrolments.Any(enrolment => enrolment.ServiceRole.Key == DbConstants.ServiceRole.Packaging.BasicUser.Key))
        {
            return (false, "Only Basic User can be nominated");
        }

        connection.PersonRoleId = DbConstants.PersonRole.Admin;

        var nominatorEnrolment = await GetApprovedPersonEnrolmentAsync(userId, organisationId, serviceKey);

        await AddDelegatedPersonNomination(connection, nominationRequest, nominatorEnrolment);

        await _accountsDbContext.SaveChangesAsync(userId, organisationId);

        return (true, string.Empty);
    }

    private static void CheckServiceKey(string serviceKey)
    {
        if (serviceKey != ServiceKeys.Packaging)
        {
            throw new ArgumentException($"Unsupported service '{serviceKey}'");
        }
    }

    public async Task<DelegatedPersonNominatorResponse> GetDelegatedPersonNominator(Guid enrolmentId, Guid userId, Guid organisationId, string serviceKey)
    {
        CheckServiceKey(serviceKey);
        
        return await _accountsDbContext.Enrolments
        .Where(enrolment => enrolment.ExternalId == enrolmentId)
        .Where(enrolment => enrolment.Connection.Organisation.ExternalId == organisationId)
        .Where(enrolment => enrolment.Connection.Person.User.UserId == userId)
        .Where(enrolment => enrolment.ServiceRole.Service.Key == serviceKey)
        .Where(enrolment => enrolment.ServiceRole.Key == DbConstants.ServiceRole.Packaging.DelegatedPerson.Key)
        .Where(enrolment => enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Nominated)
        .Select(enrolment => enrolment.DelegatedPersonEnrolment.NominatorEnrolment)
        .Select(nominatorEnrolment => new DelegatedPersonNominatorResponse
        {
            FirstName = nominatorEnrolment.Connection.Person.FirstName,
            LastName = nominatorEnrolment.Connection.Person.LastName,
            OrganisationName = nominatorEnrolment.Connection.Organisation.Name,
        })
        .FirstOrDefaultAsync();
    }

    public static (bool Succeeded, string ErrorMessage) ValidateDelegatedPersonNominationRequest(DelegatedPersonNominationRequest nominationRequest)
    {
        switch (nominationRequest.RelationshipType)
        {
            case Data.Entities.Conversions.RelationshipType.Employment:
                if (string.IsNullOrWhiteSpace(nominationRequest.JobTitle))
                {
                    return (false, "Employment relationship requires Job Title to be specified");
                }
                break;
            case Data.Entities.Conversions.RelationshipType.Consultancy:
                if (string.IsNullOrWhiteSpace(nominationRequest.ConsultancyName))
                {
                    return (false, "Consultancy relationship requires Consultancy Name to be specified");
                }
                break;
            case Data.Entities.Conversions.RelationshipType.ComplianceScheme:
                if (string.IsNullOrWhiteSpace(nominationRequest.ComplianceSchemeName))
                {
                    return (false, "Compliance Scheme relationship requires Compliance Scheme name specified");
                }
                break;
            case Data.Entities.Conversions.RelationshipType.Other:
                if (string.IsNullOrWhiteSpace(nominationRequest.OtherOrganisationName))
                {
                    return (false, "Other Organisation relationship requires Organisation Name to be specified");
                }
                if (string.IsNullOrWhiteSpace(nominationRequest.OtherRelationshipDescription))
                {
                    return (false, "Other Organisation relationship requires Relationship Description to be specified");
                }
                break;
            default:
                return (false, "Unsupported relationship type");
        }

        return (true, string.Empty);
    }

    public async Task<(bool Succeeded, string ErrorMessage)> AcceptNominationToDelegatedPerson(
        Guid enrolmentId,
        Guid userId,
        Guid organisationId,
        string serviceKey,
        AcceptNominationRequest acceptNominationRequest)
    {
        if (serviceKey != "Packaging")
        {
            return (false, $"Unsupported service '{serviceKey}'");
        }

        var enrolment = await _accountsDbContext.Enrolments
            .Where(enrolment => enrolment.ExternalId == enrolmentId)
            .Where(enrolment => enrolment.Connection.Organisation.ExternalId == organisationId)
            .Where(enrolment => enrolment.Connection.Person.User.UserId == userId)
            .Where(enrolment => enrolment.ServiceRole.Service.Key == serviceKey)
            .Where(enrolment => enrolment.ServiceRole.Key == DbConstants.ServiceRole.Packaging.DelegatedPerson.Key)
            .Where(enrolment => enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Nominated)
            .Include(enrolment => enrolment.Connection.Person)
            .Include(enrolment => enrolment.DelegatedPersonEnrolment)
            .FirstOrDefaultAsync();

        if (enrolment == null)
        {
            return (false, "There is no matching enrolment");
        }

        enrolment.EnrolmentStatusId = DbConstants.EnrolmentStatus.Pending;
        enrolment.Connection.Person.Telephone = acceptNominationRequest.Telephone;
        enrolment.DelegatedPersonEnrolment.NomineeDeclaration = acceptNominationRequest.NomineeDeclaration;
        enrolment.DelegatedPersonEnrolment.NomineeDeclarationTime = DateTimeOffset.Now;

        var connection = await GetConnectionWithEnrolments(enrolment.Connection.ExternalId, organisationId, serviceKey);

        var activeEnrolments = ExtractActiveEnrolments(connection, serviceKey);

        foreach (var basicUserEnrolment in activeEnrolments.Where(enrolment => enrolment.ServiceRole.Key == DbConstants.ServiceRole.Packaging.BasicUser.Key))
        {
            _accountsDbContext.Remove(basicUserEnrolment);
        }

        await _accountsDbContext.SaveChangesAsync(userId, organisationId);

        return (true, string.Empty);
    }

    public async Task<(bool Succeeded, string ErrorMessage)> AcceptNominationForApprovedPerson(
       Guid enrolmentId,
       Guid userId,
       Guid organisationId,
       string serviceKey,
       AcceptNominationForApprovedPersonRequest acceptNominationRequest)
    {
        if (serviceKey != "Packaging")
        {
            return (false, $"Unsupported service '{serviceKey}'");
        }

        var enrolment = await _accountsDbContext.Enrolments
            .Where(enrolment => enrolment.ExternalId == enrolmentId)
            .Where(enrolment => enrolment.Connection.Organisation.ExternalId == organisationId)
            .Where(enrolment => enrolment.Connection.Person.User.UserId == userId)
            .Where(enrolment => enrolment.ServiceRole.Service.Key == serviceKey)
            .Where(enrolment => enrolment.ServiceRole.Key == DbConstants.ServiceRole.Packaging.ApprovedPerson.Key)
            .Where(enrolment => enrolment.EnrolmentStatusId == DbConstants.EnrolmentStatus.Nominated)
            .Include(enrolment => enrolment.Connection.Person)
            .Include(enrolment => enrolment.ApprovedPersonEnrolment)
            .FirstOrDefaultAsync();

        if (enrolment == null)
        {
            return (false, "There is no matching enrolment");
        }

        enrolment.EnrolmentStatusId = DbConstants.EnrolmentStatus.Pending;
        enrolment.Connection.Person.Telephone = acceptNominationRequest.Telephone;
        enrolment.Connection.JobTitle = acceptNominationRequest.JobTitle;
        if (enrolment.ApprovedPersonEnrolment == null)
        {
            var approvedPersonEnrolment = new ApprovedPersonEnrolment
            {
                NomineeDeclaration = acceptNominationRequest.DeclarationFullName,
                NomineeDeclarationTime = (DateTimeOffset)acceptNominationRequest.DeclarationTimeStamp,
                EnrolmentId = enrolment.Id
            };
            await _accountsDbContext.AddAsync(approvedPersonEnrolment);
        }
        else
        {
            enrolment.ApprovedPersonEnrolment.NomineeDeclaration = acceptNominationRequest.DeclarationFullName;
            enrolment.ApprovedPersonEnrolment.NomineeDeclarationTime = (DateTimeOffset)acceptNominationRequest.DeclarationTimeStamp;
        }        

        var connection = await GetConnectionWithEnrolments(enrolment.Connection.ExternalId, organisationId, serviceKey);

        if (connection.PersonRole.Name != PersonRole.Admin.ToString())
        {
            var adminRole = await _accountsDbContext.PersonInOrganisationRoles.SingleOrDefaultAsync(pr => pr.Name == PersonRole.Admin.ToString());
            connection.PersonRole = adminRole;
        }

        var activeEnrolments = ExtractActiveEnrolments(connection, serviceKey);

        foreach (var basicUserEnrolment in activeEnrolments.Where(enrolment => enrolment.ServiceRole.Key == DbConstants.ServiceRole.Packaging.BasicUser.Key))
        {
            _accountsDbContext.Remove(basicUserEnrolment);
        }

        await _accountsDbContext.SaveChangesAsync(userId, organisationId);

        return (true, string.Empty);
    }

    private async Task<PersonOrganisationConnection?> GetConnectionWithEnrolments(Guid connectionId, Guid organisationId, string serviceKey)
    {
        return await _accountsDbContext.PersonOrganisationConnections
            .Where(connection => connection.ExternalId == connectionId)
            // Ensure that connection belongs to the organisation that user is authorised to
            .Where(connection => connection.Organisation.ExternalId == organisationId)
            .Include(connection => connection.Enrolments
                .Where(enrolment => enrolment.ServiceRole.Service.Key == serviceKey)
                .Where(enrolment => ActiveEnrolmentStatuses.Contains(enrolment.EnrolmentStatusId)))
                .ThenInclude(enrolment => enrolment.ServiceRole.Service)
            .Include(connection => connection.Enrolments)
                .ThenInclude(enrolment => enrolment.DelegatedPersonEnrolment)
            .Include(connection => connection.Enrolments)
                .ThenInclude(enrolment => enrolment.EnrolmentStatus)
            .Include(connection => connection.Person.User)
            .Include(Connection => Connection.PersonRole)
            .SingleOrDefaultAsync();
    }

    private static List<Enrolment> ExtractActiveEnrolments(PersonOrganisationConnection connection, string serviceKey)
    {
        // While it looks to be redundant, it is not - connection may
        // potentially contain other enrolments loaded by another query
        return connection.Enrolments
            // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            .Where(enrolment => enrolment.ServiceRole?.Service?.Key == serviceKey)
            // ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            .Where(enrolment => ActiveEnrolmentStatuses.Contains(enrolment.EnrolmentStatusId))
            .ToList();
    }

    private async Task<Enrolment> GetApprovedPersonEnrolmentAsync(Guid userId, Guid organisationId, string serviceKey)
    {
        return await _accountsDbContext.Enrolments
            .WhereUserObjectIdIs(userId)
            .WhereOrganisationIdIs(organisationId)
            .WherePersonRoleIn("Admin")
            .WhereServiceIs(serviceKey)
            .WhereServiceRoleIn(DbConstants.ServiceRole.Packaging.ApprovedPerson.Key)
            .WhereEnrolmentStatusIn(
                DbConstants.EnrolmentStatus.Pending,
                DbConstants.EnrolmentStatus.Approved)
            .FirstOrDefaultAsync();
    }

    private async Task AddDelegatedPersonNomination(PersonOrganisationConnection connection, DelegatedPersonNominationRequest nominationRequest, Enrolment nominatorEnrolment)
    {
        var delegatedPersonServiceRole = await _accountsDbContext.ServiceRoles
            .SingleAsync(serviceRole => serviceRole.Key == DbConstants.ServiceRole.Packaging.DelegatedPerson.Key);

        if (nominationRequest.RelationshipType == Data.Entities.Conversions.RelationshipType.Employment)
        {
            connection.JobTitle = nominationRequest.JobTitle;
        }

        _accountsDbContext.Enrolments.Add(new Enrolment
        {
            Connection = connection,
            ServiceRole = delegatedPersonServiceRole,
            EnrolmentStatusId = DbConstants.EnrolmentStatus.Nominated,
            DelegatedPersonEnrolment = new DelegatedPersonEnrolment
            {
                NominatorEnrolment = nominatorEnrolment,
                RelationshipType = nominationRequest.RelationshipType,
                ConsultancyName = nominationRequest.ConsultancyName,
                ComplianceSchemeName = nominationRequest.ComplianceSchemeName,
                OtherOrganisationName = nominationRequest.OtherOrganisationName,
                OtherRelationshipDescription = nominationRequest.OtherRelationshipDescription,
                NominatorDeclaration = nominationRequest.NominatorDeclaration,
                NominatorDeclarationTime = DateTimeOffset.Now
            }
        });
    }

    public static PersonRole MapPersonRole(int connectionPersonRoleId)
    {
        return connectionPersonRoleId switch
        {
            DbConstants.PersonRole.Employee => PersonRole.Employee,
            DbConstants.PersonRole.Admin => PersonRole.Admin,
            DbConstants.PersonRole.NotSet => throw new ArgumentException("No longer supported value NotSet."),
            _ => throw new ArgumentException("Not recognised person role ID.")
        };
    }

    public static EnrolmentStatus MapEnrolmentStatus(int enrolmentEnrolmentStatusId)
    {
        return enrolmentEnrolmentStatusId switch
        {
            DbConstants.EnrolmentStatus.Enrolled => EnrolmentStatus.Enrolled,
            DbConstants.EnrolmentStatus.Pending => EnrolmentStatus.Pending,
            DbConstants.EnrolmentStatus.Approved => EnrolmentStatus.Approved,
            DbConstants.EnrolmentStatus.Rejected => EnrolmentStatus.Rejected,
            DbConstants.EnrolmentStatus.Invited => EnrolmentStatus.Invited,
            DbConstants.EnrolmentStatus.OnHold => EnrolmentStatus.OnHold,
            DbConstants.EnrolmentStatus.Nominated => EnrolmentStatus.Nominated,
            DbConstants.EnrolmentStatus.NotSet => throw new ArgumentException("No longer supported value NotSet."),
            _ => throw new ArgumentException("Not recognised enrolment status ID.")
        };
    }
}
