using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BackendAccountService.ValidationData.Api.Models;

public class OrganisationsRequest
{
    [JsonPropertyName(nameof(ReferenceNumbers))]
    public IEnumerable<string> ReferenceNumbers { get; set; }
}