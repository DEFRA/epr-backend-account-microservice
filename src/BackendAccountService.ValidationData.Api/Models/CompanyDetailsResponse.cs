using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BackendAccountService.ValidationData.Api.Models;

public class CompanyDetailsResponse
{
    [JsonPropertyName(nameof(Organisations))]
    public IEnumerable<CompanyDetailResponse> Organisations { get; set; }
}
