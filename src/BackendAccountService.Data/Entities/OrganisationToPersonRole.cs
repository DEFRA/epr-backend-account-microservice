using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class OrganisationToPersonRole
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }
}
