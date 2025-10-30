using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Data.Entities;

[ExcludeFromCodeCoverage]
public class PersonOrganisationConnectionInvite : DataEntity
{
    public int Id { get; set; }

    public int InviteePersonId { get; set; }
    public Person Person { get; set; } = null!;

    public int OrganisationId { get; set; }
    public Organisation Organisation { get; set; } = null!;

    public int InvitedByUserId { get; set; }
    public User User { get; set; } = null!;

    public string InviteToken { get; set; } = string.Empty;

    public bool IsUsed { get; set; }

    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;
}
