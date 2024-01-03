using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Data.Entities;

[ExcludeFromCodeCoverage]
public class EnrolmentReport : DataEntity
{
    public int Id { get; set; }
    
    [Column("Org Name", TypeName = "nvarchar(50)")]
    public string? OrganisationName { get; set; }
    
    [Column("PCS or Direct producer?", TypeName = "nvarchar(50)")]
    public string? PcsOrDirectProducer { get; set; }
    
    [Column("Compliance Scheme Name", TypeName = "nvarchar(50)")]
    public string? ComplianceSchemeName { get; set; }
    
    [Column("Org Type", TypeName = "nvarchar(50)")]
    public string? OrganisationType { get; set; }
    
    [Column("Request Type", TypeName = "nvarchar(50)")]
    public string? RequestType { get; set; }
    
    [Column("Submitted Date", TypeName = "date")]
    public DateTime? SubmittedDate { get; set; }
    
    [Column("Organisation ID", TypeName = "nvarchar(50)")]
    public string? OrganisationId { get; set; }
    
    [Column("Companies House Number", TypeName = "nvarchar(50)")]
    public string? CompaniesHouseNumber { get; set; }
    
    [Column("Nation Of Enrolment", TypeName = "nvarchar(50)")]
    public string? NationOfEnrolment { get; set; }
    
    [Column("Due date", TypeName = "date")]
    public DateTime? DueDate { get; set; }
    
    [Column("AP First Name", TypeName = "nvarchar(50)")]
    public string? APFirstName { get; set; }
    
    [Column("AP Last Name", TypeName = "nvarchar(50)")]
    public string? APLastName { get; set; }
    
    [Column("AP Position", TypeName = "nvarchar(50)")]
    public string? APPosition { get; set; }
    
    [Column("AP Contact", TypeName = "nvarchar(50)")]
    public string? APContact { get; set; }
    
    [Column("AP Email", TypeName = "nvarchar(50)")]
    public string? APEmail { get; set; }
    
    [Column("DP First Name", TypeName = "nvarchar(50)")]
    public string? DPFirstName { get; set; }
    
    [Column("DP Last Name", TypeName = "nvarchar(50)")]
    public string? DPLastName { get; set; }
    
    [Column("DP Position", TypeName = "nvarchar(50)")]
    public string? DPPosition { get; set; }
    
    [Column("DP Nature Of Relationship", TypeName = "nvarchar(50)")]
    public string? DPNatureOfRelationship { get; set; }
    
    [Column("DP Contact", TypeName = "nvarchar(50)")]
    public string? DPContact { get; set; }
    
    [Column("DP Email", TypeName = "nvarchar(50)")]
    public string? DPEmail { get; set; }
    
    [Column("Status", TypeName = "nvarchar(50)")]
    public string? Status { get; set; }
    
    [Column("Actioned By", TypeName = "nvarchar(50)")]
    public string? ActionedBy { get; set; }
    
    [Column("Actioned Date", TypeName = "date")]
    public DateTime? ActionedDate { get; set; }

    [Column("Enrolment Id", TypeName = "nvarchar(50)")]
    public string? EnrolmentId { get; set; }
    
    [Column("Regulator Status", TypeName = "nvarchar(50)")]
    public string? RegulatorStatus { get; set; }
    
    [Column("Date", TypeName = "date")]
    public DateTime? RegulatorDecisionDate { get; set; }
    
    [Column("Regulator User Name", TypeName = "nvarchar(50)")]
    public string? RegulatorUserName { get; set; }
    
    [Column("Regulator Rejection Comments", TypeName = "nvarchar(200)")]
    public string? RegulatorRejectionComments { get; set; }
    
    public bool? IsProcessed { get; set; }
    
    public DateTime? ProcessedDate { get; set; }
    
    public DateTime? UploadDate { get; set; }
    
    public bool? IsValid { get; set; }
    
    public int? BatchNo { get; set; }
}