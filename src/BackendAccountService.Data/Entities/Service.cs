using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class Service
{
    public int  Id { get; set; }

    [MaxLength(100)]
    public string Key { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public ICollection<ServiceRole> ServiceRoles { get; set; } = null!;
}
