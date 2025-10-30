
namespace BackendAccountService.Core.Models;

public class OrganisationRelationshipsDetailModel
{
    public OrganisationDetailModel Organisation { get; set; }

    public List<RelationshipResponseModel> Relationships { get; set; } = null!;
}