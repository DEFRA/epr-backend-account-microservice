namespace BackendAccountService.Core.Models.Responses;

public record CreateAccountResponse
{
    public Guid OrganisationId { get; set; }
    public string ReferenceNumber { get; set; } = null!;
}
