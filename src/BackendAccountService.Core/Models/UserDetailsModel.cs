using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models;

[ExcludeFromCodeCoverage]
public class UserDetailsModel
{
    public Guid? Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string? RoleInOrganisation { get; set; }

    public string? EnrolmentStatus { get; set; }

    public string? ServiceRole { get; set; }

    public string? Service { get; set; }

    public int ServiceRoleId { get; set; }

    public string? Telephone { get; set; }

    public string? JobTitle { get; set; }

    public bool IsChangeRequestPending { get; set; } = false;

    public int NumberOfOrganisations { get; set; }

	public List<OrganisationDetailModel> Organisations { get; set; }
}