using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class PersonOrganisationConnection : DataEntity
{
    public int Id { get; set; }

    [MaxLength(450)]
    public string? JobTitle { get; set; }

    public int OrganisationId { get; set; }

    public int OrganisationRoleId { get; set; }

    public int PersonId { get; set; }

    public int PersonRoleId { get; set; }

    public Organisation Organisation { get; set; } = null!;

    public OrganisationToPersonRole OrganisationRole { get; set; } = null!;

    public Person Person { get; set; } = null!;

    public PersonInOrganisationRole PersonRole { get; set; } = null!;

    public ICollection<Enrolment> Enrolments { get; } = null!;
}
