using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class SubsidiaryOrganisation : IEditableEntity
{
    public int Id { get; set; }

    public int OrganisationId { get; set; }

    [MaxLength(4000)]
    public string? SubsidiaryId { get; set; }

    public DateTimeOffset CreatedOn { get; set; }

    public DateTimeOffset LastUpdatedOn { get; set; }

    public Organisation Organisation { get; set; } = null!;
}
