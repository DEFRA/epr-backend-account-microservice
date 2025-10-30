using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public class EmailRecipient
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}