using BackendAccountService.Data.Entities;

namespace BackendAccountService.Core.Models.Mappings;

/// <remarks>
/// We deviate her from the existing pattern in the service, where mappers use static classes/methods.
/// Using static mappers means that you have to unit tests the mappers at least twice,
/// once at the mapper level and again for every consumer, as you can't mock static methods.
/// </remarks>
public interface IReExEnrolmentMaps
{
    Enrolment GetAdminEnrolmentForCurrentUser(
        ReprocessorExporterAddOrganisation account,
        Person person);

    Enrolment GetApprovedPersonEnrolmentForCurrentUser(
        ReprocessorExporterAddOrganisation account,
        Person person,
        PersonOrganisationConnection connection);

    Enrolment GetEnrolmentForInvitedApprovedUser(
        Organisation organisation,
        InvitedApprovedUserModel invitedApprovedUser,
        string inviterEmail,
        Person personToInvite);
}