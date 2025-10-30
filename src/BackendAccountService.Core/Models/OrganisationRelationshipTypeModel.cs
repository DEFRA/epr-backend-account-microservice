using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Core.Models;

public class OrganisationRelationshipTypeModel
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }
}