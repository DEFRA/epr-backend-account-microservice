namespace BackendAccountService.Data.Entities;

public class Enrolment : DataEntity
{
    public int Id { get; set; }

    public int ConnectionId { get; set; }

    public int ServiceRoleId { get; set; }

    public int EnrolmentStatusId { get; set; }

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public PersonOrganisationConnection Connection { get; set; } = null!;

    public ServiceRole ServiceRole { get; set; } = null!;

    public EnrolmentStatus EnrolmentStatus { get; set; } = null!;
    
    public ICollection<RegulatorComment> RegulatorComments { get; set; }

    public DelegatedPersonEnrolment? DelegatedPersonEnrolment { get; set; } = null!;
}
