using BackendAccountService.Core.Constants;
using BackendAccountService.Core.Extensions;
using BackendAccountService.Data.Extensions;
using BackendAccountService.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendAccountService.Core.Services;

public class ValidationService : IValidationService
{
    private readonly AccountsDbContext _accountsDbContext;
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(AccountsDbContext accountsDbContext, ILogger<ValidationService> logger)
    {
        _accountsDbContext = accountsDbContext;
        _logger = logger;
    }

    public async Task<bool> IsUserInvitedAsync(string email)
    {
        return await _accountsDbContext.Users.AnyAsync(user => user.Email == email);
    }

    public bool IsAuthorisedToManageComplianceScheme(Guid userId, Guid organisationId)
    {
        try
        {
            var organisation = _accountsDbContext.Enrolments
                .WhereServiceRoleIn(Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, Data.DbConstants.ServiceRole.Packaging.DelegatedPerson.Key)
                .WhereEnrolmentStatusIn(Data.DbConstants.EnrolmentStatus.Pending, Data.DbConstants.EnrolmentStatus.Approved)
                .WhereOrganisationIsProducer()
                .WhereOrganisationIdIs(organisationId)
                .WhereUserObjectIdIs(userId)
                .SelectDistinctSingleOrganisation();

            if (organisation != null)
            {
                return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error validating the user {UserId} for the organisation {OrganisationId}", userId, organisationId);
        }

        return false;
    }

    public async Task<bool> IsAuthorisedToViewComplianceSchemeMembers(Guid userId, Guid organisationId)
    {
        try
        {
            return await _accountsDbContext.Enrolments
                .WhereServiceRoleIn(Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key,
                    Data.DbConstants.ServiceRole.Packaging.DelegatedPerson.Key,
                    Data.DbConstants.ServiceRole.Packaging.BasicUser.Key)
                .WhereEnrolmentStatusIn(Data.DbConstants.EnrolmentStatus.Pending,
                    Data.DbConstants.EnrolmentStatus.Approved,
                    Data.DbConstants.EnrolmentStatus.Enrolled)
                .WhereOrganisationIsComplianceScheme()
                .WhereOrganisationIdIs(organisationId)
                .WhereUserObjectIdIs(userId)
                .AnyAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error validating the user {UserId} for the organisation {OrganisationId}", userId, organisationId);
        }

        return false;
    }

    public bool IsAuthorisedToManageUsers(Guid userId, Guid organisationId, int serviceRoleId)
    {
        try
        {
            var serviceRole = _accountsDbContext.ServiceRoles.Single(role => role.Id == serviceRoleId);

            var organisation = _accountsDbContext.Enrolments
                .WhereOrganisationIsNotRegulator()
                .WherePersonRoleIn("Admin")
                .WhereUserServiceIdIs(serviceRole.ServiceId)
                .WhereEnrolmentStatusIn(Data.DbConstants.EnrolmentStatus.Pending, Data.DbConstants.EnrolmentStatus.Approved, Data.DbConstants.EnrolmentStatus.Enrolled)
                .WhereOrganisationIdIs(organisationId)
                .WhereUserObjectIdIs(userId)
                .SelectDistinctSingleOrganisation();

            if (organisation != null)
            {
                return true;
            }

            var regulatorOrganisation = _accountsDbContext.Enrolments
                .WhereOrganisationIsRegulator()
                .WhereServiceRoleIn(Data.DbConstants.ServiceRole.Regulator.Admin.Key)
                .WhereEnrolmentStatusIn(Data.DbConstants.EnrolmentStatus.Pending, Data.DbConstants.EnrolmentStatus.Approved, Data.DbConstants.EnrolmentStatus.Enrolled)
                .WhereOrganisationIdIs(organisationId)
                .WhereUserObjectIdIs(userId)
                .SelectDistinctSingleOrganisation();

            if (regulatorOrganisation != null)
            {
                return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error validating the user {UserId} for the organisation {OrganisationId}", userId, organisationId);
        }

        return false;
    }

    public async Task<bool> IsAuthorisedToManageUsersFromOrganisationForService(Guid userId, Guid organisationId, string serviceKey)
    {
        var producerAdmin = await _accountsDbContext.Enrolments
            .WhereOrganisationIsNotRegulator()
            .WhereUserObjectIdIs(userId)
            .WhereOrganisationIdIs(organisationId)
            .WherePersonRoleIn("Admin")
            .WhereServiceIs(serviceKey)
            .WhereEnrolmentStatusIn(
                Data.DbConstants.EnrolmentStatus.Enrolled,
                Data.DbConstants.EnrolmentStatus.Pending,
                Data.DbConstants.EnrolmentStatus.Approved)
            .AnyAsync();

        var regulatorAdmin = await _accountsDbContext.Enrolments
            .WhereOrganisationIsRegulator()
            .WhereServiceRoleIn(Data.DbConstants.ServiceRole.Regulator.Admin.Key)
            .WhereServiceIs(serviceKey)
            .WhereEnrolmentStatusIn(Data.DbConstants.EnrolmentStatus.Pending, Data.DbConstants.EnrolmentStatus.Approved, Data.DbConstants.EnrolmentStatus.Enrolled)
            .WhereOrganisationIdIs(organisationId)
            .WhereUserObjectIdIs(userId)
            .AnyAsync();

        return producerAdmin || regulatorAdmin;
    }

    public async Task<bool> IsAuthorisedToManageDelegatedUsersFromOrganisationForService(Guid userId, Guid organisationId, string serviceKey)
    {
        return await _accountsDbContext.Enrolments
            .WhereUserObjectIdIs(userId)
            .WhereOrganisationIdIs(organisationId)
            .WherePersonRoleIn("Admin")
            .WhereServiceIs(serviceKey)
            .WhereServiceRoleIn(Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key)
            .WhereEnrolmentStatusIn(
                Data.DbConstants.EnrolmentStatus.Pending,
                Data.DbConstants.EnrolmentStatus.Approved)
            .AnyAsync();
    }

    public bool IsAuthorisedToRemoveEnrolledUser(Guid loggedInUserId, Guid organisationId, int serviceRoleId, Guid enrolledPersonId)
    {
        try
        {
            var loggedInUser = _accountsDbContext.Users.Include(x => x.Person).Single(user => user.UserId == loggedInUserId);
            if (loggedInUser.Person.ExternalId == enrolledPersonId)
            {
                _logger.LogInformation("User {LoggedInUserId} cannot remove their own enrollments (personId {EnrolledPersonId}) for the organisation {OrganisationId}",
                    loggedInUserId, enrolledPersonId, organisationId);
                return false;
            }

            var serviceRole = _accountsDbContext.ServiceRoles.Single(role => role.Id == serviceRoleId);

            var personEnrolments = _accountsDbContext.Enrolments
                .WherePersonIdIs(enrolledPersonId)
                .WhereUserServiceIdIs(serviceRole.ServiceId)
                .WhereOrganisationIdIs(organisationId);

            if (!personEnrolments.Any())
            {
                _logger.LogInformation("No enrolments to remove for person  {EnrolledPersonId} for OrganisationId {OrganisationId}",
                    enrolledPersonId, organisationId);
                return false;
            }

            var personServiceRoles = personEnrolments.Select(x => x.ServiceRole.Key).ToArray();
            var highestServiceRole = ServiceRoleExtensions.GetHighestServiceRole(personServiceRoles, serviceRole.ServiceId);
            var serviceRolesAuthorized = ServiceRoleExtensions.GetAuthorizedRolesToRemoveUser(highestServiceRole, serviceRole.ServiceId);

            if (serviceRolesAuthorized.Length == 0)
            {
                _logger.LogInformation("Cannot remove service role {HighestServiceRole} from person {EnrolledPersonId} as no roles are authorized to do so. User Id {LoggedInUserId}. OrganisationId {OrganisationId}",
                    highestServiceRole, enrolledPersonId, loggedInUserId, organisationId);
                return false;
            }

            var loggedInProducerUserEnrollments = _accountsDbContext.Enrolments
                .WhereOrganisationIsNotRegulator()
                .WhereUserObjectIdIs(loggedInUserId)
                .WherePersonRoleIn("Admin")
                .WhereUserServiceIdIs(serviceRole.ServiceId)
                .WhereOrganisationIdIs(organisationId)
                .WhereServiceRoleIn(serviceRolesAuthorized);

            var loggedInRegulatorUserEnrollments = _accountsDbContext.Enrolments
                .WhereOrganisationIsRegulator()
                .WhereUserObjectIdIs(loggedInUserId)
                .WhereUserServiceIdIs(serviceRole.ServiceId)
                .WhereOrganisationIdIs(organisationId)
                .WhereServiceRoleIn(serviceRolesAuthorized);

            if (loggedInProducerUserEnrollments.Any() || loggedInRegulatorUserEnrollments.Any())
            {
                _logger.LogInformation("User {LoggedInUserId} can remove service role {HighestServiceRole} from person {EnrolledPersonId} for OrganisationId {OrganisationId}",
                    loggedInUserId, highestServiceRole, enrolledPersonId, organisationId);
                return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error validating the user {LoggedInUserId} can remove roles for person {EnrolledPersonId} for the organisation {OrganisationId}",
                loggedInUserId, enrolledPersonId, organisationId);
        }

        return false;
    }

    public async Task<string> UserInvitedTokenAsync(Guid? userId)
    {
        var enrolments = _accountsDbContext
               .Enrolments
               .Include(s => s.ServiceRole)
               .Include(c => c.Connection)
               .Include(p => p.Connection.Person)
               .Include(u => u.Connection.Person.User)
               .Include(o => o.Connection.Organisation)
               .Where(enrolment =>
                   enrolment.EnrolmentStatusId == Data.DbConstants.EnrolmentStatus.Invited
                   && enrolment.ServiceRole.ServiceId == Data.DbConstants.Service.RegulatorEnrolment
                   && enrolment.Connection.Person.User.UserId == userId
                   && enrolment.Connection.Organisation.OrganisationTypeId == Data.DbConstants.OrganisationType.Regulators
                   && (enrolment.ValidTo > DateTimeOffset.Now || enrolment.ValidTo == null)
                   && (enrolment.ValidFrom < DateTimeOffset.Now || enrolment.ValidFrom == null)
                ).FirstOrDefault();

        if (enrolments != null)
        {
            _logger.LogInformation("Enrolment found for {UserId}, token = {Enrolments}", userId, enrolments);
            return enrolments.Connection.Person.User.InviteToken;
        }

        _logger.LogInformation("Enrolment not found for {UserId}", userId);
        return string.Empty;
    }

    public async Task<bool> IsAuthorisedToManageSubsidiaries(Guid userId, Guid organisationId, params int[] serviceRoles)
    {
        var enrolments = _accountsDbContext
              .Enrolments
              .Include(s => s.ServiceRole)
              .Include(c => c.Connection)
              .Include(p => p.Connection.Person)
              .Include(u => u.Connection.Person.User)
              .Include(o => o.Connection.Organisation)
              .Where(enrolment =>
                  serviceRoles.Contains(enrolment.ServiceRole.ServiceId)
                  && enrolment.Connection.Person.User.UserId == userId
                  && enrolment.Connection.Organisation.ExternalId == organisationId
                  && (enrolment.ValidTo > DateTimeOffset.Now || enrolment.ValidTo == null)
                  && (enrolment.ValidFrom < DateTimeOffset.Now || enrolment.ValidFrom == null)
               ).FirstOrDefault();

        if (enrolments != null)
        {
            _logger.LogInformation("Enrolment found for {UserId}, token = {Enrolments}", userId, enrolments);

            return true;
        }

        _logger.LogInformation("Enrolment not found for {UserId}", userId);

        return false;
    }

    public async Task<bool> IsApprovedOrDelegatedUserInEprPackaging(Guid userId, Guid organisationId, string serviceKey)
    {
        var approvedOrDelegatedUser = await _accountsDbContext.Enrolments
            .WhereOrganisationIsNotRegulator()
            .WhereUserObjectIdIs(userId)
            .WhereOrganisationIdIs(organisationId)
            .WhereServiceRoleIn(Data.DbConstants.ServiceRole.Packaging.ApprovedPerson.Key, Data.DbConstants.ServiceRole.Packaging.DelegatedPerson.Key)
            .WhereServiceIs(serviceKey)
            .WhereEnrolmentStatusIn(Data.DbConstants.EnrolmentStatus.Enrolled, Data.DbConstants.EnrolmentStatus.Pending, Data.DbConstants.EnrolmentStatus.Approved)
            .AnyAsync();
        return approvedOrDelegatedUser;
    }

    public async Task<bool> IsBasicUserInEprPackaging(Guid userId, Guid organisationId, string serviceKey)
    {
        var approvedOrDelegatedUser = await _accountsDbContext.Enrolments
            .WhereOrganisationIsNotRegulator()
            .WhereUserObjectIdIs(userId)
            .WhereOrganisationIdIs(organisationId)
            .WhereServiceRoleIn(Data.DbConstants.ServiceRole.Packaging.BasicUser.Key)
            .WhereServiceIs(serviceKey)
            .WhereEnrolmentStatusIn(Data.DbConstants.EnrolmentStatus.Enrolled, Data.DbConstants.EnrolmentStatus.Pending, Data.DbConstants.EnrolmentStatus.Approved)
            .AnyAsync();
        return approvedOrDelegatedUser;
    }

    public bool IsExternalIdExists(Guid externalId, string entityTypeCode) =>
        entityTypeCode.ToUpperInvariant() switch
        {
            EntityTypeCode.ComplianceScheme => _accountsDbContext.ComplianceSchemes.Any(x => x.ExternalId == externalId),
            EntityTypeCode.DirectRegistrant => _accountsDbContext.Organisations.Any(x => x.ExternalId == externalId),
            _ => false
        };
}
