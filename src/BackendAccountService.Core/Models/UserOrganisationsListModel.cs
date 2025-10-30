using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models;

[ExcludeFromCodeCoverage]
public class UserOrganisationsListModel
{
    public UserDetailsModel User { get; set; }
}