using System.ComponentModel.DataAnnotations;

namespace BackendAccountService.Data.Entities;

public class ReportingType
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(2)]
    public string Key { get; set; }
}