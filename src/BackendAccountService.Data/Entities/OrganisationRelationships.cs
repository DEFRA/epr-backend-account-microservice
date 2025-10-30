namespace BackendAccountService.Data.Entities;

public class OrganisationRelationship : IEditableEntity
{
    public int Id { get; set; }

    public int FirstOrganisationId { get; set; }

    public int SecondOrganisationId { get; set; }

    public int OrganisationRelationshipTypeId { get; set; }

    public int? OrganisationRegistrationTypeId { get; set; } = null;

    public int? LeaverCodeId { get; set; } = null;

    public DateTime RelationFromDate { get; set; }

    public DateTime? RelationToDate { get; set; }

    public string? RelationExpiryReason { get; set; }

    public DateTimeOffset CreatedOn { get; private set; }

    public int LastUpdatedById { get; set; }

    public DateTimeOffset LastUpdatedOn { get; private set; }

    public int LastUpdatedByOrganisationId { get; set; }

    public User LastUpdatedBy { get; set; } = null!;

    public Organisation FirstOrganisation { get; set; }

    public Organisation SecondOrganisation { get; set; }

    public OrganisationRelationshipType OrganisationRelationshipType { get; }

    public OrganisationRegistrationType? OrganisationRegistrationType { get; } = null!;

    public DateTimeOffset? JoinerDate { get; set; }

    public LeaverCode? LeaverCode { get; set; }

    public DateTimeOffset? LeaverDate { get; set; }

    public string? OrganisationChangeReason { get; set; }

    public int? CodeStatusConfigId { get; set; } = null;

    public CodeStatusConfig? CodeStatusConfig { get; set; }
}