using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class Nation
{
    public int Id { get; set; }

    [MaxLength(54)]
    public string Name { get; set; }
}
