using System.Diagnostics.CodeAnalysis;

namespace BackendAccountService.Core.Models.Responses
{
    [ExcludeFromCodeCoverage]
    public class RegulatorUserDetailsUpdateResponse
    {
        public bool HasUserDetailsChangeAccepted { get; set; } = false;
        public bool HasUserDetailsChangeRejected { get; set; } = false;
        public ChangeHistoryModel? ChangeHistory { get; set; }
    }
}
