using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses
{
    [ExcludeFromCodeCoverage]
    public class RegulatorUserDetailsUpdateByServiceResponse
    {
        public bool HasUserDetailsChangeAccepted { get; set; } = false;
        public bool HasUserDetailsChangeRejected { get; set; } = false;
    }
}
