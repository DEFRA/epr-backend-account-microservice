using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models;

public class ConnectionModel
{
    public string? JobTitle { get; set; }

    [Required]
    public string ServiceRole { get; set; } = null!;
}
