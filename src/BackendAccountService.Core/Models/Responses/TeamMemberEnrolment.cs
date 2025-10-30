using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public class TeamMemberEnrolment
{
	public int EnrolmentId { get; set; }

	public int ServiceRoleId { get; set; }

	public int EnrolmentStatusId { get; set; }

	public string EnrolmentStatusName { get; set; }

	public string ServiceRoleKey { get; set; }

	public string AddedBy { get; set; }
}