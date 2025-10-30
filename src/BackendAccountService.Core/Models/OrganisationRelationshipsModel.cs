namespace BackendAccountService.Core.Models;

public class OrganisationRelationshipsModel
{
    public int Id { get; set; }

    public int FirstOrganisationId  { get; set; }

    public int SecondOrganisationId  { get; set; }

    public int OrganisationRelationshipTypeId { get; set; }

    public DateTime RelationFromDate { get; set; }

    public DateTime? RelationToDate { get; set; }

    public string? RelationExpiryReason { get; set; }

    public DateTime CreatedOn { get; set; }

    public int LastUpdatedById  { get; set; }

    public DateTime LastUpdatedOn { get; set; }

    public int LastUpdatedByOrganisationId { get; set; }

    public UserDetailsModel LastUpdatedBy { get; set; } = null!;

    public OrganisationDetailModel FirstOrganisation { get; set; } = null!;

    public OrganisationRelationshipTypeModel OrganisationRelationshipType { get; set; } = null!;
}