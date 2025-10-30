namespace BackendAccountService.Core.Models;

public class EnrolmentModel
{
    public int? EnrolmentId { get; set; }
    
    public string? EnrolmentStatus { get; set; }

    public string? ServiceRole { get; set; }

	public string? ServiceRoleKey { get; set; }

	public string? Service { get; set; }

	public string? ServiceKey { get; set; }

	public int? ServiceRoleId { get; set; }
}