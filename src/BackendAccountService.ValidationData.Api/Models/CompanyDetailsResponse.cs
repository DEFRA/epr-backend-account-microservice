using Newtonsoft.Json;
using System.Collections.Generic;

namespace BackendAccountService.ValidationData.Api.Models;

public class CompanyDetailsResponse
{
    [JsonProperty("Organisations")]
    public IEnumerable<CompanyDetailResponse> Organisations { get; set; }
}