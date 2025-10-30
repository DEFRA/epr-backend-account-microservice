using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Data.Entities;

[ExcludeFromCodeCoverage]
public class OfflineApprovalErrorLog : DataEntity
{
    public int Id { get; set; }
    
    public int Error_Number { get; set; }
    
    public string Error_Message { get; set; }
    
    public int Error_Line { get; set; }
    
    public string Error_Procedure { get; set; }
    
    public int Error_Severity { get; set; }
    
    public int Error_State { get; set; }
    
    public DateTime Error_Date { get; set; }
}