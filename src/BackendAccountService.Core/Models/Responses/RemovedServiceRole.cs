using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public class RemovedServiceRole
{
    public string ServiceRoleKey { get; set; }

    public string EnrolmentStatus { get; set; }
}
