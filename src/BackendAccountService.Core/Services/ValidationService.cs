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

            var regulatorOrganisation  = _accountsDbContext.Enrolments
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

        var regulatorAdmin  = await _accountsDbContext.Enrolments
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
                _logger.LogInformation("User {loggedInUserId} cannot remove their own enrollments (personId {enrolledPersonId}) for the organisation {organisationId}",
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
                _logger.LogInformation("No enrolments to remove for person  {enrolledPersonId} for OrganisationId {organisationId}",
                    enrolledPersonId, organisationId);
                return false;
            }

            var personServiceRoles = personEnrolments.Select(x => x.ServiceRole.Key).ToArray();
            var highestServiceRole = ServiceRoleExtensions.GetHighestServiceRole(personServiceRoles, serviceRole.ServiceId);
            var serviceRolesAuthorized = ServiceRoleExtensions.GetAuthorizedRolesToRemoveUser(highestServiceRole, serviceRole.ServiceId);

            if (!serviceRolesAuthorized.Any())
            {
                _logger.LogInformation("Cannot remove service role {highestServiceRole} from person {enrolledPersonId} as no roles are authorized to do so. User Id {loggedInUserId}. OrganisationId {organisationId}",
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
                _logger.LogInformation("User {loggedInUserId} can remove service role {highestServiceRole} from person {enrolledPersonId} for OrganisationId {organisationId}",
                    loggedInUserId, enrolledPersonId, highestServiceRole, organisationId);
                return true;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error validating the user {loggedInUserId} can remove roles for person {enrolledPersonId} for the organisation {organisationId}",
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
                   && enrolment.ServiceRole.ServiceId == Data.DbConstants.Service.RegulatorEnrolement
                   && enrolment.Connection.Person.User.UserId == userId
                   && enrolment.Connection.Organisation.OrganisationTypeId == Data.DbConstants.OrganisationType.Regulators
                   && (enrolment.ValidTo > DateTimeOffset.Now || enrolment.ValidTo == null)
                   && (enrolment.ValidFrom < DateTimeOffset.Now || enrolment.ValidFrom == null)
                ).FirstOrDefault();

        if (enrolments != null)
        {
            _logger.LogInformation($"Enrolment found for {userId}, token = {enrolments}");

            return enrolments.Connection.Person.User.InviteToken;
        }

        _logger.LogInformation($"Enrolment not found for {userId}");

        return string.Empty;
    }
}
