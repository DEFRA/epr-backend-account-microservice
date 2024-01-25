using System.Collections.Generic;

namespace BackendAccountService.ValidationData.Api.Models;

public class OrganisationsResponse
{
    public IEnumerable<string> ReferenceNumbers { get; set; }
}