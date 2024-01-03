using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models.Request
{
    public class UpdatePersonRoleRequest
    {
        [Required]
        public PersonRole PersonRole { get; set; }
    }
}