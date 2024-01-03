using BackendAccountService.Data.Entities.Conversions;
using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models.Request
{
    public class DelegatedPersonNominationRequest
    {
        /// <summary>
        /// Relationship of the Nominee with the organisation.
        /// </summary>
        public RelationshipType RelationshipType { get; set; }

        /// <summary>
        /// JobTitle for RelationshipType = Employment
        /// </summary>
        [MaxLength(450)]
        public string? JobTitle { get; set; }

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
    }
}
