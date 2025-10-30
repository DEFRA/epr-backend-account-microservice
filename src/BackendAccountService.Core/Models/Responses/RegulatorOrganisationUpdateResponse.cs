using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses
{
    [ExcludeFromCodeCoverage]
    public class RegulatorOrganisationUpdateResponse
    {
        public bool HasOrganisationUpdated { get; set; } = false;
    }
}
