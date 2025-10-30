using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class PartnerRole
{
    public int Id { get; set; }

    [MaxLength(100)]
    [Required]
    public string Name { get; set; } = null!;
}