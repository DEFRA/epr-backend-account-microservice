using System.ComponentModel.DataAnnotations;
using BackendAccountService.Data.Entities.Conversions;

namespace BackendAccountService.Data.Entities;

public class DelegatedPersonEnrolment : ISoftDeletableEntity, IEditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// EnrolmentId of the person being elevated to Delegated Person.
    /// </summary>
    public int EnrolmentId { get; set; }

    /// <summary>
    /// EnrolmentId of the Approved Person who is elevating the person enrolment to Delegated Person.
    /// </summary>
    public int NominatorEnrolmentId { get; set; }

    /// <summary>
    /// Relationship of the Invitee with the organisation.
    /// </summary>
    [MaxLength(50)]
    public RelationshipType RelationshipType { get; set; }

    /// <summary>
    /// Organisation name for RelationshipType = Consultancy.
    /// </summary>
    [MaxLength(160)]
    public string? ConsultancyName { get; set; }

    /// <summary>
    /// Organisation name for RelationshipType = ComplianceScheme.
    /// </summary>
    [MaxLength(160)]
    public string? ComplianceSchemeName { get; set; }

    /// <summary>
    /// Organisation name for RelationshipType = Other.
    /// </summary>
    [MaxLength(160)]
    public string? OtherOrganisationName { get; set; }

    /// <summary>
    /// Description explaining the relationship with the organisation when RelationshipType = Other.
    /// </summary>
    [MaxLength(160)]
    public string? OtherRelationshipDescription { get; set; }

    [MaxLength(450)]
    public string? NominatorDeclaration { get; set; }

    public DateTimeOffset? NominatorDeclarationTime { get; set; }

    [MaxLength(450)]
    public string? NomineeDeclaration { get; set; }

    public DateTimeOffset? NomineeDeclarationTime { get; set; }

    public DateTimeOffset CreatedOn { get; private set; }

    public DateTimeOffset LastUpdatedOn { get; private set; }

    public bool IsDeleted { get; set; }

    public Enrolment Enrolment { get; set; }

    public Enrolment NominatorEnrolment { get; set; }
}
