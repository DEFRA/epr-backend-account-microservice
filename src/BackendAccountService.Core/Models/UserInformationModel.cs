namespace BackendAccountService.Core.Models;

using Responses;

public class UserInformation
{
    public UserInformation(string firstName, string lastName, string email, Guid externalId, IEnumerable<UserEnrolments> userEnrolments, bool? isEmployee, string? jobTitle, string? phoneNumber, int personRoleId)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UserEnrolments = userEnrolments;
        IsEmployee = isEmployee;
        JobTitle = jobTitle;
        PhoneNumber = phoneNumber;
        PersonRoleId = personRoleId;
        ExternalId = externalId;
    }
    
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public Guid ExternalId { get; set; }
    public IEnumerable<UserEnrolments> UserEnrolments { get; set; }
    public bool? IsEmployee { get; set; }
    public string? JobTitle { get; set; }
    public string? PhoneNumber { get; set; }
    public int PersonRoleId { get; set; }
}