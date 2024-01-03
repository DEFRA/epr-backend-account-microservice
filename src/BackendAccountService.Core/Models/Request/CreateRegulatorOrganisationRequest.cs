using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models.Request
{
    public class CreateRegulatorOrganisationRequest
    {

		[Required]
		public string ServiceId { get; set; }

        [Required]
        [MaxLength(160)]
        public string Name { get; set; } = null!;

        [Required]
        public int NationId { get; set; }
    }
}