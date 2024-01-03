using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class RegulatorComment : DataEntity
{
    public int Id { get; set; }
    
    public int PersonId { get; set; }
    
    public int EnrolmentId { get; set; }
    
    [MaxLength(200)]
    public string? RejectedComments { get; set; }
    
    [MaxLength(200)]
    public string? TransferComments { get; set; }
    
    [MaxLength(200)]
    public string? OnHoldComments { get; set; }

    public bool IsDeleted { get; set; }
    
    public DateTimeOffset CreatedOn { get; }
    
    public DateTimeOffset LastUpdatedOn { get; }
    
    public Person Person { get; set; }
    public Enrolment Enrolment { get; set; }
}