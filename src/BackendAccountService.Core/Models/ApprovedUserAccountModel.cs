using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models;

public class ApprovedUserAccountModel
{
        [Required]
        public PersonModel Person { get; set; } = null!;

        [Required]
        public OrganisationModel Organisation { get; set; } = null!;

        [Required]
        public ConnectionModel Connection { get; set; } = null!;
        
        [Required]
        public Guid UserId { get; set; }
        
}