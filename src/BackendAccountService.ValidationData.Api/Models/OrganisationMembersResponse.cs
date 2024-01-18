using System.Collections.Generic;

namespace BackendAccountService.ValidationData.Api.Models;

public class OrganisationMembersResponse
{
    public IEnumerable<string> MemberOrganisations { get; set; }
}