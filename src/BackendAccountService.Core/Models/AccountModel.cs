using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models;

public class AccountModel
{
    [Required]
    public PersonModel Person { get; set; } = null!;

    [Required]
    public OrganisationModel Organisation { get; set; } = null!;

    [Required]
    public ConnectionModel Connection { get; set; } = null!;

    [Required]
    public UserModel User { get; set; } = null!;
}
