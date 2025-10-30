namespace BackendAccountService.Core.Models;

public class PersonsDetailsRequestDto
{
    public Guid? OrgId { get; set; }
    public List<Guid> UserIds { get; set; }
}
