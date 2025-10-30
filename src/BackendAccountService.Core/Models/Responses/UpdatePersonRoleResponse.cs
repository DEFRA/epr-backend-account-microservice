using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses;

[ExcludeFromCodeCoverage]
public class UpdatePersonRoleResponse
{
    public List<RemovedServiceRole> RemovedServiceRoles { get; set; }
}