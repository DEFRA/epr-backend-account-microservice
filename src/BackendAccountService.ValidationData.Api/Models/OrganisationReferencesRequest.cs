using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BackendAccountService.ValidationData.Api.Models
{
    public class OrganisationReferencesRequest
    {
        [JsonPropertyName(nameof(ReferenceNumbers))]
        public IEnumerable<string> ReferenceNumbers { get; set; }

        [JsonPropertyName(nameof(OrganisationExternalId))]
        public string OrganisationExternalId { get; set; }
    }
}
