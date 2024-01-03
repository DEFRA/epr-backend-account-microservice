using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class ComplianceSchemeMemberRemovalReason
{
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string Code { get; set; }

    [MaxLength(2000)]
    public string Name { get; set; }

    public bool RequiresReason { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }   
}