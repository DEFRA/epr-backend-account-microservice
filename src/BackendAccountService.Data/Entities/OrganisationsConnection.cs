namespace BackendAccountService.Data.Entities;

public class OrganisationsConnection : DataEntity
{
    public int Id { get; set; }

    public int FromOrganisationId { get; set; }

    public int FromOrganisationRoleId { get; set; }

    public int ToOrganisationId { get; set; }

    public int ToOrganisationRoleId { get; set; }

    public Organisation FromOrganisation { get; set; } = null!;

    public InterOrganisationRole FromOrganisationRole { get; set; } = null!;

    public Organisation ToOrganisation { get; set; } = null!;

    public InterOrganisationRole ToOrganisationRole { get; set; } = null!;
}
