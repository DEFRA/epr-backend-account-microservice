using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class Person : DataEntity
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string FirstName { get; set; }

    [MaxLength(50)]
    public string LastName { get; set; }
    
    [MaxLength(100)]
    public string Email { get; set; }

    [MaxLength(50)]
    public string Telephone { get; set; }

    public int? UserId { get; set; }

    public User? User { get; set; }

    public ICollection<PersonOrganisationConnection> OrganisationConnections { get; set; } = null!;

    public ICollection<PersonsConnection> FromPersonConnections { get; set; } = null!;

    public ICollection<PersonsConnection> ToPersonConnections { get; set; } = null!;
    
    public ICollection<RegulatorComment> RegulatorComments { get; set; }
}
