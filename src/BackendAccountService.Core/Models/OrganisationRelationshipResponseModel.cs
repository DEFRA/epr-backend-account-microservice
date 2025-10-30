
namespace BackendAccountService.Core.Models;

public class OrganisationRelationshipResponseModel
{
    public OrganisationDetailModel Organisation { get; set; }

    public List<RelationshipResponseModel> Relationships { get; set; } = null!;
}