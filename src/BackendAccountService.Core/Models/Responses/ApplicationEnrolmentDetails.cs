namespace BackendAccountService.Core.Models.Responses;

public class ApplicationEnrolmentDetails
{
    public Guid OrganisationId { get; set; }
    public string OrganisationName { get; set; }
    public string OrganisationReferenceNumber { get; set; }
    public string OrganisationType { get; set; }
    public string CompaniesHouseNumber { get; set; }
    public bool IsComplianceScheme { get; set; }
    public int? NationId { get; set; }
    public string NationName { get; set; }
    public AddressModel BusinessAddress { get; set; }
    public IEnumerable<UserEnrolmentDetails> Users { get; set; }
    public TransferDetails TransferDetails { get; set; }
}

public class UserEnrolmentDetails
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string TelephoneNumber { get; set; }

    public string JobTitle { get; set; }

    public bool IsEmployeeOfOrganisation { get; set; }

    public EnrolmentDetails Enrolments { get; set; }

    public string? TransferComments { get; set; }
}

public class EnrolmentDetails
{
    public string EnrolmentStatus { get; set; }

    public Guid ExternalId { get; set; }

    public string ServiceRole { get; set; }
}

public class TransferDetails
{
    public int OldNationId { get; set; }
    public DateTimeOffset TransferredDate { get; set; }
}
