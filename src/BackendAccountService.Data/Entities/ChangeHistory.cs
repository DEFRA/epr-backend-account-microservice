using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class ChangeHistory : DataEntity
{
    public int Id { get; set; }

    public int PersonId { get; set; }

    public int OrganisationId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(180)]
    public string? ApproverComments { get; set; }

    public int? ApprovedById { get; set; }

    public DateTimeOffset? DecisionDate { get; set; }

    public DateTimeOffset DeclarationDate { get; set; }

    public Organisation Organisation { get; set; } = null!;

    public Person Person { get; set; } = null!;

    public User? ApprovedBy { get; set; }

}
