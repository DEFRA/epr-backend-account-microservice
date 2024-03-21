using Newtonsoft.Json;

namespace BackendAccountService.ValidationData.Api.Models;

public class CompanyDetailResponse
{
    [JsonProperty("RN")]
    public string ReferenceNumber { get; set; }

    [JsonProperty("CHN")]
    public string CompaniesHouseNumber { get; set; }
}