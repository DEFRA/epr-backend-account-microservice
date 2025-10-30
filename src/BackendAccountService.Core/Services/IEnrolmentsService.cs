namespace BackendAccountService.Core.Services;

public interface IEnrolmentsService
{
    Task<bool> DeleteEnrolmentsForPersonAsync(Guid userId, Guid enrolledPersonId, Guid organisationId, int serviceRoleId);

    Task<bool> IsUserEnrolledAsync(string userEmail, Guid organisationId);

    public Task<bool> DeletePersonOrgConnectionOrEnrolmentAsync(Guid userId, Guid personExternalId,
        Guid organisationExternalId, int enrolmentId);
}