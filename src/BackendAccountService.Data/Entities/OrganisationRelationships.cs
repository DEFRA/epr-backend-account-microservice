﻿namespace BackendAccountService.Data.Entities;

public class OrganisationRelationship
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

    public User LastUpdatedBy { get; set; } = null!;

    public Organisation FirstOrganisation { get; set; } = null!;

    public OrganisationRelationshipType OrganisationRelationshipType { get; } = null!;

    public int OrganisationRegistrationTypeId { get; set; }

    public OrganisationRegistrationType OrganisationRegistrationType { get; } = null!;

}


