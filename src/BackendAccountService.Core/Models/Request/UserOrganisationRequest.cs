using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models.Request;

public class UserOrganisationRequest
{
    [Required]
    public Guid UserOid { get; set; }
    
    [Required]
    public Guid OrganisationId { get; set; }
}