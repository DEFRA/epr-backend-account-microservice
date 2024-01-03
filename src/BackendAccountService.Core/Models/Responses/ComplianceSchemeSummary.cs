namespace BackendAccountService.Core.Models.Responses;

public class ComplianceSchemeSummary
{
    public string Name { get; set; }
    public Nation? Nation { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset? MembersLastUpdatedOn { get; set; }
    public int MemberCount { get; set; }
}