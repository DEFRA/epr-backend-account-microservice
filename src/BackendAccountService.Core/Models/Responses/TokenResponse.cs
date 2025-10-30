using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public class TokenResponse
{
    public string InviteToken { get; set; }
}