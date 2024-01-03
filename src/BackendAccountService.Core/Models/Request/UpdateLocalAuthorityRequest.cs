using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models.Request
{
    public class UpdateLocalAuthorityRequest
    {
        [Required] public Guid UserId { get; set; }
        [Required][MaxLength(160)] public string Name { get; set; } = string.Empty;
        [Required][MaxLength(54)] public string Nation { get; set; } = string.Empty;
        [Required][MaxLength(100)] public string WasteAuthorityType { get; set; } = string.Empty;
        [MaxLength(36)] public string ExternalId { get; set; } = string.Empty;
        [MaxLength(10)] public string DistrictCode { get; set; } = string.Empty;
    }
}