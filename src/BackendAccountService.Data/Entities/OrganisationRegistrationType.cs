using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities
{
    public class OrganisationRegistrationType
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(10)]
        public string? Key { get; set; }
    }
}
