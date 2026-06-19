using System.Text.Json.Serialization;

namespace BackendAccountService.ValidationData.Api.Models;

public class CompanyDetailResponse
{
    [JsonPropertyName("RN")]
    public string ReferenceNumber { get; set; }

    [JsonPropertyName("CHN")]
    public string CompaniesHouseNumber { get; set; }
}
