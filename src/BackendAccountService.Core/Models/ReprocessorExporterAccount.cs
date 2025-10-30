using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models;

[ExcludeFromCodeCoverage]
public class ReprocessorExporterAccount
{
    [Required]
    public required PersonModel Person { get; set; }

    [Required]
    public required UserModel User { get; set; }
}
