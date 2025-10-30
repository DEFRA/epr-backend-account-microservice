using BackendAccountService.Data.Extensions;
using BackendAccountService.Data.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Core.Services;

public class EnrolmentsService : IEnrolmentsService
{
    private readonly AccountsDbContext _accountsDbContext;
    private readonly ILogger<EnrolmentsService> _logger;

    public EnrolmentsService(AccountsDbContext accountsDbContext, ILogger<EnrolmentsService> logger)
    {
        _accountsDbContext = accountsDbContext;
        _logger = logger;
    }

    public async Task<bool> DeleteEnrolmentsForPersonAsync(
        Guid userId,
        Guid enrolledPersonId,
        Guid organisationId,
        int serviceRoleId)
    {
        try
        {
            var serviceRole = _accountsDbContext.ServiceRoles.Single(role => role.Id == serviceRoleId);

            var personEnrolments = _accountsDbContext.Enrolments
                .Include(e => e.DelegatedPersonEnrolment)
                .WherePersonIdIs(enrolledPersonId)
                .WhereUserServiceIdIs(serviceRole.ServiceId)
                .WhereOrganisationIdIs(organisationId)
                .ToList();

            var delegatedEnrolments = personEnrolments
                .Select(x => x.DelegatedPersonEnrolment)
                .Where(x => x != null)
                .ToList();

            _accountsDbContext.DelegatedPersonEnrolments.RemoveRange(delegatedEnrolments);
            _accountsDbContext.Enrolments.RemoveRange(personEnrolments);

            await _accountsDbContext.SaveChangesAsync(userId, organisationId);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error removing enrolments for person {personId} for organisation {organisationId}",
                enrolledPersonId, organisationId);
            return false;
        }
    }

    public async Task<bool> IsUserEnrolledAsync(string userEmail, Guid organisationId)
    {
        return await _accountsDbContext
            .Enrolments
            .AnyAsync(enrolment =>
                enrolment.Connection.Organisation.ExternalId == organisationId &&
                enrolment.Connection.Person.User.Email == userEmail);
    }

    public async Task<bool> DeletePersonOrgConnectionOrEnrolmentAsync(
    Guid userId,
    Guid personExternalId,
    Guid organisationExternalId,
    int enrolmentId)
    {
        try
        {
            // Load enrolment with related connection, person and organisation
            var enrolment = await _accountsDbContext.Enrolments
                .Include(e => e.Connection)
                    .ThenInclude(c => c.Person)
                .Include(e => e.Connection)
                    .ThenInclude(c => c.Organisation)
                .FirstOrDefaultAsync(e =>
                    e.Id == enrolmentId &&
                    e.Connection.Person.ExternalId == personExternalId &&
                    e.Connection.Organisation.ExternalId == organisationExternalId);

            if (enrolment == null)
            {
                _logger.LogWarning("No enrolment found with ID {EnrolmentId} for person {PersonExternalId} and organisation {OrganisationExternalId}", enrolmentId, personExternalId, organisationExternalId);
                return false;
            }

            var connectionId = enrolment.ConnectionId;

            // Remove the enrolment
            _accountsDbContext.Enrolments.Remove(enrolment);

            // Check if other enrolments exist for that connection
            var hasOtherEnrolments = await _accountsDbContext.Enrolments
                .AnyAsync(e => e.ConnectionId == connectionId && e.Id != enrolmentId);

            if (!hasOtherEnrolments)
            {
                var connection = await _accountsDbContext.PersonOrganisationConnections
                    .FirstOrDefaultAsync(c => c.Id == connectionId);

                if (connection != null)
                {
                    _accountsDbContext.PersonOrganisationConnections.Remove(connection);
                }
            }

            // Save all changes
            await _accountsDbContext.SaveChangesAsync(userId, organisationExternalId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing enrolment {EnrolmentId} for person {PersonExternalId} and organisation {OrganisationExternalId}", enrolmentId, personExternalId, organisationExternalId);
            return false;
        }
    }
}