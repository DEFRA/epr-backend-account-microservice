namespace BackendAccountService.Core.Models.Responses;

public class OrganisationUsersResponseModel
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public string Email { get; set; }
    
    public int PersonRoleId { get; set; }
    
    public Guid PersonId { get; set; }

    public Guid ConnectionId { get; set; }

    public IEnumerable<UserEnrolments> Enrolments { get; set; }
    
}

public class UserEnrolments
{
    public int ServiceRoleId { get; set; }
    
    public int EnrolmentStatusId { get; set; }
}