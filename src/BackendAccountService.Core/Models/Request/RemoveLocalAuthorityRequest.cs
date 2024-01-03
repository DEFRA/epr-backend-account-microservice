using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models.Request
{
    public class RemoveLocalAuthorityRequest
    {
        [Required] public Guid UserId { get; set; }
        [MaxLength(36)] public string ExternalId { get; set; } = string.Empty;
        [MaxLength(10)] public string DistrictCode { get; set; } = string.Empty;
    }
}