using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class ServiceRole
{
    public int  Id { get; set; }

    public int ServiceId { get; set; }

    [MaxLength(100)]
    public string Key { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public Service Service { get; set; } = null!;
}
