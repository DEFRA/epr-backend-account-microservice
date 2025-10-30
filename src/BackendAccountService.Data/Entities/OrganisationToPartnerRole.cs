
using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class OrganisationToPartnerRole
{
    public int Id { get; set; }

    public int OrganisationId { get; set; }

    public int PartnerRoleId { get; set; }

    [MaxLength(100)]
    [Required]
    public string Name { get; set; } = null!;

    public Organisation Organisation { get; set; } = null!;
    public PartnerRole PartnerRole { get; set; } = null!;
}