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
            _logger.LogError(e, "Error removing enrolments for person {personId} for organisation {organisationId}", enrolledPersonId, organisationId);
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
}