using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models;

public class PersonModel
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string ContactEmail { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string TelephoneNumber { get; set; } = null!;
}
