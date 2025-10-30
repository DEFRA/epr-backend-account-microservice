using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models;

public class OrganisationRelationshipsResponseModel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string OrganisationRole { get; set; }

    public string OrganisationType { get; set; }

    public string OrganisationNumber { get; set; }
    public string CompaniesHouseNumber { get; set; }

    public string ProducerType { get; set; }

    public int? NationId { get; set; }

    public OrganisationRelationshipsModel Relationships { get; set; } = null!;
}