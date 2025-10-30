using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public class TeamMembersResponseModel
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public Guid PersonId { get; set; }

    public Guid ConnectionId { get; set; }

    public IEnumerable<TeamMemberEnrolment> Enrolments { get; set; }
}